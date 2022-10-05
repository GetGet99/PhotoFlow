#nullable enable
using CSharpUI;
using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using PhotoFlow.CommandButton.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Linq;
using PhotoFlow.Layer;

namespace PhotoFlow;

public class ImageCommandButton : CommandButtonBase
{

    private readonly ImageBar ImageCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => ImageCommandBar;
    PhotoToysImportDialog ImportDialog = new();
    //Layer.MatLayer MatLayer => CurrentLayer.LayerType == Layer.Types.Mat ? (Layer.MatLayer)CurrentLayer : null;
    
    public ImageCommandButton(ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Pictures, CommandBarPlace, LayerContainer, MainScrollViewer)
    {
        ImageCommandBar.Invert.Click += (s, e) =>
        {
            if (CurrentLayer is not MatLayer MatLayer) return;
            MatLayer.Mat?.Invert(InPlace: true);
            MatLayer.UpdateImage();
            LayerContainer.History.NewAction(new HistoryAction<(LayerContainer LayerContainer, uint LayerId)>(
                (LayerContainer, MatLayer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (LayerContainer, LayerId) = x;
                    if (LayerContainer.GetLayerFromId(LayerId) is not MatLayer MatLayer) return;
                    MatLayer.Mat?.Invert(InPlace: true);
                    MatLayer.UpdateImage();
                },
                Redo: x =>
                {
                    var (LayerContainer, LayerId) = x;
                    if (LayerContainer.GetLayerFromId(LayerId) is not MatLayer MatLayer) return;
                    MatLayer.Mat?.Invert(InPlace: true);
                    MatLayer.UpdateImage();
                }
            ));
        };
        ImageCommandBar.PhotoToysImport.Click += async delegate
        {
            await ImportDialog.ShowAsync();
        };
        ImportDialog.ImportRequested += x => AddNewLayer(new Layer.MatLayer(x) {
            LayerName = {
                Value = "Imported Layer"
            }
        });
    }
    class PhotoToysImportDialog : ThemeContentDialog
    {
        public event Action<Mat>? ImportRequested;
        public PhotoToysImportDialog()
        {
            Title = "PhotoToys Import";
            PrimaryButtonText = "Done!";
            Content = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Style = App.CardBorderStyle,
                Child = new SimpleUI.FluentVerticalStack
                {
                    Children =
                    {
                        new Border
                        {
                            Height = 300,
                            AllowDrop = true,
                            Padding = new Thickness(16),
                            CornerRadius = new CornerRadius(8),
                            Style = App.CardBorderStyle,
                            Child = new Grid
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition(),
                                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star)}
                                },
                                Children =
                                {
                                    new Rectangle
                                    {
                                        Margin = new Thickness(-16),
                                        RadiusX = 8,
                                        RadiusY = 8,
                                        HorizontalAlignment = HorizontalAlignment.Stretch,
                                        VerticalAlignment = VerticalAlignment.Stretch,
                                        StrokeDashCap = PenLineCap.Flat,
                                        StrokeDashOffset = 1.5,
                                        StrokeDashArray = new DoubleCollection {3},
                                        Stroke = new SolidColorBrush(Colors.Gray),
                                        StrokeThickness = 3,
                                    }.Edit(x => Grid.SetColumnSpan(x, 2)),
                                    new SimpleUI.FluentVerticalStack
                                    {
                                        //VerticalAlignment = VerticalAlignment.Center,
                                        Children =
                                        {
                                            new TextBlock
                                            {
                                                TextAlignment = TextAlignment.Center,
                                                FontSize = 20,
                                                Text = "Drop PhotoToys Image Here!"
                                            },
                                            new TextBlock
                                            {
                                                TextAlignment = TextAlignment.Center,
                                                FontSize = 16,
                                                Text = "or"
                                            },
                                            new StackPanel
                                            {
                                                HorizontalAlignment = HorizontalAlignment.Center,
                                                Children =
                                                {
                                                    new Button
                                                    {
                                                        HorizontalAlignment = HorizontalAlignment.Stretch,
                                                        Content = "Paste Image",
                                                        Margin = new Thickness(0, 10, 0, 0)
                                                    }.Assign(out var FromClipboard)
                                                }
                                            }
                                        }
                                    }
                                    .Assign(out var UIStack)
                                    .Edit(x => Grid.SetColumnSpan(x, 2))
                                }
                            }
                        }.Edit(border =>
                        {
                            border.DragOver += async (o, e) =>
                            {
                                var d = e.GetDeferral();
                                try
                                {
                                    if (e.DataView.AvailableFormats.Contains(StandardDataFormats.StorageItems))
                                    {
                                        var files = (await e.DataView.GetStorageItemsAsync()).ToArray();
                                        if (files.All(f => f is StorageFile sf && sf.ContentType.ToLower().Contains("image")))
                                            e.AcceptedOperation = DataPackageOperation.Copy;
                                    }
                                    if (e.DataView.AvailableFormats.Contains(StandardDataFormats.Bitmap))
                                    {
                                        e.AcceptedOperation = DataPackageOperation.Copy;
                                    }
                                } catch (Exception ex)
                                {
                                    Hide();
                                    ContentDialog c = new()
                                    {
                                        Title = "Unhandled Error (Drag Item Over)",
                                        Content = ex.Message,
                                        PrimaryButtonText = "Okay",
                                        XamlRoot = border.XamlRoot
                                    };
                                    _ = c.ShowAsync();
                                }
                                d.Complete();
                            };
                            async Task ReadFile(StorageFile sf, string action)
                            {
                                if (sf.ContentType.Contains("image"))
                                {
                                    // It's an image!
                                    var stream = await sf.OpenStreamForReadAsync();
                                    var bytes = new byte[stream.Length];
                                    await stream.ReadAsync(bytes, 0, bytes.Length);
                                    var decoded = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
                                    if (decoded is not null) ImportRequested?.Invoke(decoded);
                                    else await DropError(
                                        ErrorTitle: "File Error",
                                        ErrorContent: $"There is an error reading the file you {action}. Make sure the file is the image file!"
                                    );
                                } else
                                {
                                    await DropError(
                                        ErrorTitle: "File Error",
                                        ErrorContent: $"There is an error reading the file you {action}. Make sure the file is the image file!"
                                    );
                                }
                            }
                            async Task ReadData(DataPackageView DataPackageView, string action)
                            {
                                if (DataPackageView.Contains(StandardDataFormats.StorageItems))
                                {
                                    var a = await DataPackageView.GetStorageItemsAsync();
                                    if (a[^1] is StorageFile sf && sf.ContentType.ToLower().Contains("image"))
                                        await ReadFile(sf, action);
                                    return;
                                }
                                if (DataPackageView.Contains(StandardDataFormats.Bitmap))
                                {
                                    var a = await DataPackageView.GetBitmapAsync();
                                    var b = await a.OpenReadAsync();
                                    var stream = b.AsStream();
                                    var bytes = new byte[stream.Length];
                                    await stream.ReadAsync(bytes, 0, bytes.Length);
                                    var decoded = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
                                    if (decoded is not null) ImportRequested?.Invoke(decoded);
                                    else await DropError("Error", $"There is an error reading the file you {action}. Make sure the file is the image file!");
                                    return;
                                }
                                Hide();
                                ContentDialog c = new()
                                {
                                    Title = "Error",
                                    Content = $"The content you {action} is not supported",
                                    PrimaryButtonText = "Okay",
                                    XamlRoot = border.XamlRoot
                                };
                                await c.ShowAsync();
                                return;
                            }
                            async Task DropError(string ErrorTitle, string ErrorContent)
                            {
                                Hide();
                                ContentDialog c = new()
                                {
                                    Title = ErrorTitle,
                                    Content = ErrorContent,
                                    PrimaryButtonText = "Okay",
                                    XamlRoot = border.XamlRoot
                                };
                                await c.ShowAsync();
                                return;
                            }
                            border.Drop += async (o, e) => {
                                try
                                {
                                    await ReadData(e.DataView, "dropped");
                                } catch (Exception ex)
                                {
                                    Hide();
                                    ContentDialog c = new()
                                    {
                                        Title = "Unhandled Error (Drop Item)",
                                        Content = ex.Message,
                                        PrimaryButtonText = "Okay",
                                        XamlRoot = border.XamlRoot
                                    };
                                    _ = c.ShowAsync();
                                }
                            };
                            FromClipboard.Click += async delegate
                            {
                                await ReadData(Clipboard.GetContent(), "pasted");
                            };
                        })
                    }
                }
            };
        }
    }
    class ImageBar : CommandButtonCommandBar
    {
        public Button PhotoToysImport, Invert;
        public ImageBar()
        {
            Children.Add(new Button
            {
                Content = "PhotoToys Import",
                Margin = new Thickness(0, 0, 10, 0)
            }.Assign(out PhotoToysImport));
            Children.Add(new Button
            {
                Content = "Invert",
                Margin = new Thickness(0, 0, 10, 0)
            }.Assign(out Invert));
        }
    }
}
