#nullable enable
using Microsoft.UI.Xaml.Controls;
using PhotoFlow.CommandButton.Controls;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using CSharpUI;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;
using Windows.UI.Xaml.Media;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Windows.Security.Cryptography.Certificates;
using Windows.UI;

namespace PhotoFlow;

public class TextCommandButton : CommandButtonBase
{
    private readonly Text TextCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => TextCommandBar;

    HistoryActionMutable<(string Original, string New, LayerContainer LayerContainer, uint LayerId)>? TextChangingHistoryAction = null;
    public TextCommandButton(ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Font, CommandBarPlace, LayerContainer, MainScrollViewer)
    {
        TextCommandBar.CreateNewLayer.Click += (_, _1) =>
        {
            var newLayer = new Layers.TextLayer(new Windows.Foundation.Point(0, 0), "Text");
            newLayer.LayerName.Value = "Text Layer";
            AddNewLayer(newLayer);
        };

        TextCommandBar.TextBox.GotFocus += (_, _1) =>
        {
            if (CurrentLayer is not Layers.TextLayer Layer) return;

        };
        TextCommandBar.TextBox.TextChanged += (_, _1) =>
        {
            if (CurrentLayer is not Layers.TextLayer Layer) return;
            Layer.Text = TextCommandBar.TextBox.Text;
            if (TextChangingHistoryAction is null) CreateNewTextChangingHistoryAction(Layer);
            var (a, b, c, d) = TextChangingHistoryAction.Param;
            TextChangingHistoryAction.Param = (a, Layer.Text, c, d);
        };
        TextCommandBar.Font.TextChanged += (_, e) =>
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                if (CurrentLayer is not Layers.TextLayer Layer) return;
                var newFont = new FontFamily(TextCommandBar.Font.Text);
                LayerContainer.History.NewAction(new HistoryAction<(FontFamily Old, FontFamily New, LayerContainer LayerContainer, uint LayerId)>(
                    (Layer.Font, newFont, LayerContainer, Layer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (OldFont, _, LayerContainer, LayerId) = x;
                        if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                        {
                            TextLayer.Font = OldFont;
                        }
                    },
                    Redo: x =>
                    {
                        var (_, NewFont, LayerContainer, LayerId) = x;
                        if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                        {
                            TextLayer.Font = NewFont;
                        }
                    }
                ));
                Layer.Font = newFont;
            }
        };
        TextCommandBar.FontSize.ValueChanged += (_, e) =>
        {
            if (CurrentLayer is not Layers.TextLayer Layer) return;
            var newSize = TextCommandBar.FontSize.Value;
            LayerContainer.History.NewAction(new HistoryAction<(double? Old, double? New, LayerContainer LayerContainer, uint LayerId)>(
                (Layer.FontSize, newSize, LayerContainer, Layer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (OldSize, _, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                    {
                        TextLayer.FontSize = OldSize;
                    }
                },
                Redo: x =>
                {
                    var (_, NewSize, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                    {
                        TextLayer.FontSize = NewSize;
                    }
                }
            ));
            Layer.FontSize = newSize;
        };
        TextCommandBar.ColorPicker.ColorChanged += delegate
        {
            if (CurrentLayer is not Layers.TextLayer Layer) return;
            var newColor = TextCommandBar.ColorPicker.Color;
            LayerContainer.History.NewAction(new HistoryAction<(Color Old, Color New, LayerContainer LayerContainer, uint LayerId)>(
                (Layer.TextColor, newColor, LayerContainer, Layer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (OldColor, _, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                    {
                        TextLayer.TextColor = OldColor;
                    }
                },
                Redo: x =>
                {
                    var (_, NewColor, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                    {
                        TextLayer.TextColor = NewColor;
                    }
                }
            ));
            Layer.TextColor = newColor;
        };
    }
    [MemberNotNull(nameof(TextChangingHistoryAction))]
    void CreateNewTextChangingHistoryAction(Layers.TextLayer Layer)
    {
        TextChangingHistoryAction = new(
            (Layer.Text ?? "", Layer.Text ?? "", LayerContainer, Layer.LayerId),
            Tag: this,
            Undo: x =>
            {
                var (oldText, newText, LayerContainer, LayerId) = x;
                if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                {
                    TextLayer.Text = oldText;
                }
            },
            Redo: x =>
            {
                var (oldText, newText, LayerContainer, LayerId) = x;
                if (LayerContainer?.GetLayerFromId(LayerId) is Layers.TextLayer TextLayer)
                {
                    TextLayer.Text = newText;
                }
            }
        );
        LayerContainer.History.NewAction(TextChangingHistoryAction);


    }
    protected override void LayerChanged(Layers.Layer? Layer)
    {
        base.LayerChanged(Layer);
        if (Layer == null) return;
        TextCommandBar.LayerEditorControls.Visibility =
            Layer.LayerType == PhotoFlow.Layers.Types.Text ? Visibility.Visible : Visibility.Collapsed;
        if (Layer is Layers.TextLayer TextLayer)
        {
            TextCommandBar.TextBox.Text = TextLayer.Text;
            TextCommandBar.Font.Text = TextLayer.Font.Source;
            TextCommandBar.ColorPicker.Color = TextLayer.TextColor;
            if (TextLayer.FontSize != null) TextCommandBar.FontSize.Value = TextLayer.FontSize.Value;
        }
    }
}
class Text : CommandButtonCommandBar
{
    static readonly Thickness DefaultThickness = new(16, 0, 16, 0);
    public Button CreateNewLayer, ColorPickerButton;
    public ColorPicker ColorPicker;
    public StackPanel LayerEditorControls;
    public TextBox TextBox;
    public AutoSuggestBox Font;
    public NumberBox FontSize;
    public Text()
    {
        const VerticalAlignment Center = VerticalAlignment.Center;
        Children.Add(new Button
        {
            Content = new SymbolIcon(Symbol.Add),
            Margin = new Thickness(0, 0, 10, 0)
        }.Edit(x => ToolTipService.SetToolTip(x, "Add New Text Layer")).Assign(out CreateNewLayer));
        Children.Add(
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Text:",
                        VerticalAlignment = Center
                    },
                    new TextBox
                    {
                        Margin = DefaultThickness,
                        Width = 300
                    }.Assign(out TextBox),
                    new TextBlock
                    {
                        Text = "Font:",
                        VerticalAlignment = Center
                    },
                    new AutoSuggestBox
                    {
                        Margin = DefaultThickness,
                        Width = 200
                    }.Assign(out Font),
                    new TextBlock
                    {
                        Text = "Font Size:",
                        VerticalAlignment = Center
                    },
                    new NumberBox
                    {
                        Margin = DefaultThickness,
                        Width = 100
                    }.Assign(out FontSize),
                    new Button
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(16, 0, 16, 0),
                        Flyout = new Flyout
                        {
                            Content = new ColorPicker
                            {
                                IsAlphaEnabled = true
                            }
                            .Edit(x =>
                            {
                                x.ColorChanged += delegate
                                {
                                    if (ColorPickerButton != null)
                                        ColorPickerButton.Background = new SolidColorBrush(x.Color);
                                };
                            })
                            .Assign(out ColorPicker)
                        }
                    }
                    .Assign(out ColorPickerButton)
                }
            }.Assign(out LayerEditorControls)
        );
        var original = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
        Font.ItemsSource = original;
        Font.TextChanged += (_, e) =>
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                Font.ItemsSource = from x in original where x.ToLower().Contains(Font.Text.ToLower()) select x;
            }
        };

    }
}
