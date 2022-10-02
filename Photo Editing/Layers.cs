#nullable enable
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OpenCvSharp;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using System.IO;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Shapes;
using Windows.ApplicationModel.DataTransfer;
using System.Collections;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using Window = Windows.UI.Xaml.Window;
using System.Linq;
using System.Diagnostics;
using CSUI;
using Windows.UI.ViewManagement;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PhotoFlow.Layer
{
    public enum Types
    {
        Background,
        Mat,
        Inking,
        Text,
        RectangleShape,
        EllipseShape
    }
    public interface ILayerTyping
    {
        Types LayerType { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class RunOnUIThreadAttribute : Attribute
    {

    }

    public abstract class Layer : ISaveable, ILayerTyping, IDisposable, INotifyPropertyChanged
    {
        protected static readonly UISettings UISettings = new();
        public Grid LayerUIElement { get; private set; }
        static Layer()
        {
            static void UpdateBorderColor()
            {
                if (UISettings.GetColorValue(UIColorType.Background).R < 255 / 2)
                {
                    // Dark Mode
                    BorderColor.Color = UISettings.GetColorValue(UIColorType.AccentLight2);
                } else
                {
                    // Light Mode
                    BorderColor.Color = UISettings.GetColorValue(UIColorType.AccentDark2);
                }
            }
            UpdateBorderColor();
            UISettings.ColorValuesChanged += delegate
            {
                UpdateBorderColor();
            };
        }
        protected static readonly SolidColorBrush BorderColor = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract Types LayerType { get; }
        public VariableUpdateAlert<string> LayerName { get; } = new VariableUpdateAlert<string>() { Value = "Unnamed Layer" };
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public bool Visible
        {
            get => LayerUIElement.Visibility == Visibility.Visible;
            set
            {
                LayerUIElement.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                NotifyPropertyChanged();
            }
        }
        public double CenterX
        {
            get => RenderTransform.CenterX;
            set {
                RenderTransform.CenterX = value;
                NotifyPropertyChanged();
            }
        }
        public double CenterY
        {
            get => RenderTransform.CenterY;
            set
            {
                RenderTransform.CenterY = value;
                NotifyPropertyChanged();
            }
        }
        public CompositeTransform RenderTransform => (CompositeTransform)LayerUIElement.RenderTransform;
        public double X
        {
            set => RenderTransform.TranslateX = value;
            get => RenderTransform.TranslateX;
        }
        public double Y
        {
            set => RenderTransform.TranslateY = value;
            get => RenderTransform.TranslateY;
        }
        public double Rotation
        {
            get => RenderTransform.Rotation;
            set => RenderTransform.Rotation = value;
        }
        public double ScaleX
        {
            get => RenderTransform.ScaleX;
            set => RenderTransform.ScaleX = value;
        }
        public double ScaleY
        {
            get => RenderTransform.ScaleY;
            set => RenderTransform.ScaleY = value;
        }
        public double Width
        {
            get => LayerUIElement.Width;//(LayerUIElement.Children[0] as FrameworkElement).Width;
            set
            {
                LayerUIElement.Width = value;
                //foreach (var child in LayerUIElement.Children)
                //    if (child is FrameworkElement element)
                //        element.Width = value;
            }
        }
        public double Height
        {
            get => LayerUIElement.Height;//(LayerUIElement.Children[0] as FrameworkElement).Height;
            set
            {
                LayerUIElement.Height = value;
                //foreach (var child in LayerUIElement.Children)
                //    if (child is FrameworkElement element)
                //        element.Height = value;
            }
        }

        public JObject SaveData()
        {
            var save = OnDataSaving();
            double X = 0, Y = 0,
                Width = 0, Height = 0,
                Rotation = 0,
                ScaleX = 0, ScaleY = 0,
                CenterX = 0, CenterY = 0;
            bool Visible = true;
            Extension.RunOnUIThread(delegate
            {
                X = this.X;
                Y = this.Y;
                Width = this.Width;
                Height = this.Height;
                Rotation = this.Rotation;
                ScaleX = this.ScaleX;
                ScaleY = this.ScaleY;
                CenterX = this.CenterX;
                CenterY = this.CenterY;
                Visible = this.Visible;
            });
            return new JObject(
                new JProperty("LayerName", LayerName.Value),
                new JProperty("LayerType", LayerType),
                new JProperty("CenterPoint", new double[] { CenterX, CenterY }),
                new JProperty("X", X),
                new JProperty("Y", Y),
                new JProperty("Width", Width),
                new JProperty("Height", Height),
                new JProperty("Rotation", Rotation),
                new JProperty("Scale", new double[] { ScaleX, ScaleY }),
                new JProperty("Visible", Visible),
                new JProperty("AdditionalData", save)
            );
        }

        public void LoadData(JObject json)
        {
            LayerName.Value = json["LayerName"]?.ToObject<string>();
            var CenterPoint = json["CenterPoint"]?.ToObject<double[]>();
            var Scale = json["Scale"]?.ToObject<double[]>();

            var X = json["X"]?.ToObject<double>();
            var Y = json["Y"]?.ToObject<double>();
            var Rotation = json["Rotation"]?.ToObject<double>();
            var Width = json["Width"]?.ToObject<double>();
            var Height = json["Height"]?.ToObject<double>();
            var Visible = json["Visible"]?.ToObject<bool>();

            var Task = Extension.RunOnUIThreadAsync(() =>
            {
                if (CenterPoint != null)
                {
                    CenterX = CenterPoint[0];
                    CenterY = CenterPoint[1];
                }
                if (X != null) this.X = X.Value;
                if (Y != null) this.Y = Y.Value;
                if (Rotation != null) this.Rotation = Rotation.Value;
                if (Scale != null)
                {
                    ScaleX = Scale[0];
                    ScaleY = Scale[1];
                }
                if (Width != null) this.Width = Width.Value;
                if (Height != null) this.Height = Height.Value;
                if (Visible != null) this.Visible = Visible.Value;
            });

            var additionalData = json["AdditionalData"]?.ToObject<JObject>();
            if (additionalData != null) OnDataLoading(additionalData, Task);
            if (!Task.IsCompleted) Task.Wait();
        }

        public double ActualWidth => LayerUIElement.ActualWidth;
        public double ActualHeight => LayerUIElement.ActualHeight;


        public VariableUpdateAlert<int> SelectionIndexUpdateTarget => LayerContainer.SelectionIndex;
        public int Index { get; private set; }
        public void UpdateThisIndex(int newIndex)
        {
            Index = newIndex;
        }

        protected abstract JObject OnDataSaving();
        protected abstract void OnDataLoading(JObject storage, Task MainLoadingTask);

        public Control Control { get; private set; }

        public LayerPreview LayerPreview { get; private set; }
        protected LayerContainer? LayerContainer => (LayerUIElement.Parent is LayerContainer L) ? L : null;

        public VariableUpdateAlert<bool> Selecting { get; } = new VariableUpdateAlert<bool>() { Value = false };


        protected Layer()
        {
            Init();
        }

        void Init()
        {
            Extension.RunOnUIThread(() =>
            {
                LayerUIElement = new Grid
                {
                    BorderThickness = new Thickness(2),
                    CanBeScrollAnchor = false,
                    IsHitTestVisible = false,
                    RenderTransform = new CompositeTransform(),
                    Background = new SolidColorBrush(Colors.Transparent)
                };
                LayerPreview = new LayerPreview(this);
            });
            LayerName.Update += (oldVal, newVal) =>
            {
                Extension.RunOnUIThread(() => LayerPreview.LayerName = newVal);
            };
            Selecting.Update += (oldVal, newVal) =>
            {
                Extension.RunOnUIThread(() =>
                {
                    LayerUIElement.IsHitTestVisible = newVal;
                });
                if (newVal) Select(); else Deselect();
            };
        }
        protected abstract void OnCreate();
        protected void CompleteCreate()
        {
            UpdatePreview();
        }
        [RunOnUIThread]
        protected async Task UpdatePreviewAsync()
        {
            await Task.Run(
                () => Extension.RunOnUIThread(async () => LayerPreview.PreviewImage = await LayerUIElement.ToRenderTargetBitmapAsync())
            );
        }
        protected async void UpdatePreview()
        {
            await UpdatePreviewAsync();
        }

        protected virtual void SelectedIndexChange(int oldIdx, int newIdx) { }
        protected virtual void Select()
        {
            Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = BorderColor);
        }
        protected virtual void Deselect()
        {
            Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = null);
        }

        public virtual void DisablePreviewEffects() => Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = null);
        public virtual void EnablePreviewEffects()
        {
            if (Selecting) Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = BorderColor);
        }

        protected void ReplaceSelf(Layer NewLayer)
        {
            var lc = LayerContainer;
            if (lc != null)
            {
                var index = lc.Layers.IndexOf(this);
                if (index != -1) lc.Layers[index] = NewLayer;
                Dispose();
            }
        }
        protected void RemoveSelf()
        {
            var lc = LayerContainer;
            if (lc != null)
            {
                lc.Layers.Remove(this);
                Dispose();
            }
        }
        public void DeleteSelf() => RemoveSelf();
        public void Duplicate() => LayerContainer?.AddNewLayer(this.DeepClone());
        async Task CopyAsync(DataPackageOperation Operation)
        {
            var data = new DataPackage
            {
                RequestedOperation = Operation
            };
            data.SetData("GPE", SaveData().ToString());
            DisablePreviewEffects();
            var mat = await LayerUIElement.ToMatAsync();
            EnablePreviewEffects();
            Cv2.ImEncode(".png", mat, out var bytes);
            var ms = new MemoryStream(bytes);
            var memref = RandomAccessStreamReference.CreateFromStream(ms.AsRandomAccessStream());
            data.SetData("PNG", ms.AsRandomAccessStream());
            data.SetBitmap(memref);
            GC.KeepAlive(ms);

            Clipboard.SetContent(data);

            void HistoryChanged(object _, ClipboardHistoryChangedEventArgs e)
            {
                ms.Dispose();
            }
            Clipboard.HistoryChanged += HistoryChanged;
        }
        public async Task CopyAsync()
        {
            if (RequestCopy()) return;
            await CopyAsync(DataPackageOperation.Copy);
        }
        public async void CopyNoWait() => await CopyAsync();
        public async Task CutAsync()
        {
            if (RequestCut()) return;
            await CopyAsync(DataPackageOperation.Move);
            DeleteSelf();
        }
        public async void CutNoWait() => await CutAsync();
        public async void ConvertToMatLayerAsync()
        {
            ReplaceSelf(await ConvertToMatLayerAsync(this));
        }
        public static async Task<MatLayer> ConvertToMatLayerAsync(Layer layer)
        {
            layer.DisablePreviewEffects();
            var Toreturn = new MatLayer(await layer.LayerUIElement.ToMatAsync())
            {
                X = layer.X,
                Y = layer.Y
            };
            layer.EnablePreviewEffects();
            return Toreturn;
        }
        protected virtual bool RequestCut() => false;
        protected virtual bool RequestCopy() => false;
        public virtual bool RequestPaste() => false;
        public virtual bool RequestDuplicate() => false;
        public virtual bool RequestDelete() => false;

        public abstract void Dispose();
    }
    public class MatLayer : Layer, Features.Mat.IMatFeatureApplyable<Mat>, Features.ICanBecomeFeatureDataType<Mat>
    {

        public override Types LayerType { get; } = Types.Mat;

        public Mat Mat { get; set; }
        public Mat SoftSelectedPartEdit { get => Mat; set => Mat = value; }
        public Mat HardSelectedPartEdit { get => Mat; set => Mat = value; }
        public Features.IFeatureDataTypes<Mat> GetFeatureDataType => Mat;

        Image Image;

        public MatLayer(Mat m)
        {
            Mat = m;
            OnCreate();
        }
        public MatLayer(Rect r)
        {
            var m = new Mat(r.Size, MatType.CV_8UC4);
            Mat = m;
            X = r.X;
            Y = r.Y;
            OnCreate();
        }
        public MatLayer(JObject json)
        {
            LoadData(json);
            OnCreate();
        }
        protected override void OnCreate()
        {
            var m = Mat;
            var width = m.Width;
            var height = m.Height;
            Extension.RunOnUIThread(() =>
            {
                Image = new Image
                {
                    Stretch = Stretch.Fill,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                UpdateImage();
                LayerUIElement.Children.Add(Image);
                Width = width;
                Height = height;
            });
            CompleteCreate();
        }

        protected override JObject OnDataSaving()
        {
            return new JObject(
                new JProperty("Image", Mat.ToBytes())
            );
        }
        protected override void OnDataLoading(JObject json, Task _)
        {
            Mat = json["Image"].ToObject<byte[]>().ToMat();
            if (Image != null) Extension.RunOnUIThread(UpdateImage);
        }
        [RunOnUIThread]
        public void UpdateImage()
        {
            Image.Source = Mat.ToBitmapImage(DisposeMat: false);
            UpdatePreview();
        }
        public override void Dispose()
        {
            Mat.Dispose();
        }

        public void ApplyFeature(Features.Mat.MatBasedFeature<Mat> Feature)
        {
            HardSelectedPartEdit = Feature.ForwardApply(Mat);
            UpdateImage();
        }
    }
    public class InkingLayer : Layer
    {
        public override Types LayerType { get; } = Types.Inking;
        public readonly VariableUpdateAlert<bool> TouchAllowed = new();
        public readonly VariableUpdateAlert<bool> DrawingAllowed = new();
        static readonly SolidColorBrush SelectionColor = BorderColor;
        readonly Canvas Canvas = new()
        {
            //IsHitTestVisible = false
        };
        readonly Polygon SelectionPolygon = new()
        {
            Stroke = SelectionColor,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection() { 5, 2 },
            IsHitTestVisible = false
        };
        readonly Rectangle SelectionRectangle = new()
        {
            Visibility = Visibility.Collapsed,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection() { 5, 2 },
            Fill = new SolidColorBrush(Colors.Transparent),
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
            IsHitTestVisible = true
        };
        CompositeTransform SelectionRenderTransform = new CompositeTransform();
        public InkCanvas? InkCanvas { get; private set; }
        public InkingLayer(Rect Where)
        {
            OnCreate();
            CompleteCreate();
            X = Where.X;
            Y = Where.Y;
            Width = Where.Width;
            Height = Where.Height;
        }
        public InkingLayer(JObject json)
        {
            OnCreate();
            LoadData(json);
            CompleteCreate();
        }
        Windows.Foundation.Point LatestMouse = new();
        protected override void OnCreate()
        {
            TouchAllowed.Update += (oldValue, newValue) => UpdateInkingDeviceOnUIThread();
            DrawingAllowed.Update += (oldValue, newValue) => UpdateInkingDeviceOnUIThread();
            Extension.RunOnUIThread(() =>
            {

                InkCanvas = new InkCanvas()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CanBeScrollAnchor = false
                };
                LayerUIElement.Children.Add(InkCanvas);
                Canvas.Children.Add(SelectionPolygon);
                Canvas.Children.Add(SelectionRectangle);
                LayerUIElement.Children.Add(Canvas);
                //LayerUIElement.Children.Add(SelectionRectangle);

                InkCanvas.InkPresenter.StrokesCollected += (o, e) => { ClearInkSlection(); UpdatePreview(); };
                InkCanvas.InkPresenter.StrokesErased += (o, e) => { ClearInkSlection(); UpdatePreview(); };
                InkCanvas.PointerMoved += (_, e) => LatestMouse = e.GetCurrentPoint(InkCanvas).Position;
                SelectionRectangle.RenderTransform = SelectionRenderTransform;
                SelectionRectangle.Stroke = SelectionColor;
                //SelectionRectangle.PointerEntered += delegate
                //{
                //    //Debugger.Break();
                //};
                //SelectionRectangle.PointerPressed += delegate
                //{
                //    Debugger.Break();
                //};
                //SelectionRectangle.PointerExited += delegate
                //{
                //    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                //};
                SelectionRectangle.ManipulationStarted += (_, e) => e.Handled = true;
                SelectionRectangle.ManipulationDelta += (_, e) =>
                {
                    e.Handled = true;
                    float ZoomFactor = LayerContainer?.ZoomFactor ?? 1;
                    Windows.Foundation.Point d = e.Delta.Translation;
                    double TwoPiOver360 = 2 * Math.PI / 360;
                    var sin = Math.Sin(Rotation * TwoPiOver360);
                    var cos = Math.Cos(Rotation * TwoPiOver360);
                    d = new Windows.Foundation.Point(
                        d.X * cos + d.Y * sin,
                        d.X * sin + d.Y * cos
                    );
                    d.X /= ZoomFactor;
                    d.Y /= ZoomFactor;

                    var SelectionBounds = this.SelectionBounds;
                    Windows.Foundation.Point newPos = new(SelectionBounds.X + d.X, SelectionBounds.Y + d.Y);

                    var newBounds = new Windows.Foundation.Rect(newPos,
                        new Windows.Foundation.Size(SelectionBounds.Width, SelectionBounds.Height));
                    InkCanvas.InkPresenter.StrokeContainer.MoveSelected(d);
                    UpdateInkSelectionRectangle(newBounds);
                };
                CommandBarFlyout? C = null;
                InkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
                InkCanvas.InkPresenter.UnprocessedInput.PointerPressed += (_, e) =>
                {
                    if (e.CurrentPoint.Properties.IsRightButtonPressed)
                    {
                        if (CanPasteInk)
                            new MenuFlyout
                            {
                                Items =
                                {
                                    new MenuFlyoutItem
                                    {
                                        Text = "Paste",
                                        Icon = new SymbolIcon(Symbol.Paste),
                                        Command = new LambdaCommand(() => PasteInkAt(e.CurrentPoint.Position))
                                    }
                                }
                            }.ShowAt(InkCanvas, e.CurrentPoint.Position);
                    }
                };
                SelectionRectangle.ContextFlyout = C = new CommandBarFlyout
                {
                    PrimaryCommands =
                    {
                        new AppBarButton
                        {
                            Label = "Cut",
                            Icon = new SymbolIcon(Symbol.Cut),
                            Command = new LambdaCommand(() => {
                                RequestCut();
                                C?.Hide();
                            })
                        },
                        new AppBarButton
                        {
                            Label = "Copy",
                            Icon = new SymbolIcon(Symbol.Copy),
                            Command = new LambdaCommand(() => {
                                RequestCopy();
                                C?.Hide();
                            })
                        },
                        new AppBarButton
                        {
                            Label = "Duplicate",
                            Icon = new SymbolIcon(Symbol.Copy),
                            Command = new LambdaCommand(() => {
                                RequestDuplicate();
                                C?.Hide();
                            })
                        },
                        new AppBarButton
                        {
                            Label = "Delete",
                            Icon = new SymbolIcon(Symbol.Delete),
                            Command = new LambdaCommand(() =>
                            {
                                RequestDelete();
                                C?.Hide();
                            })
                        }
                    }
                };

                LayerUIElement.SizeChanged += delegate
                {
                    Canvas.Width = Width;
                    Canvas.Height = Height;
                };
                Canvas.Width = Width;
                Canvas.Height = Height;

                SelectionRectangle.RenderTransform = SelectionRenderTransform;
                SelectionRectangle.PointerEntered += delegate
                {
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
                };
                SelectionRectangle.PointerEntered += delegate
                {
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                };
            });
        }
        protected override void Deselect()
        {
            ClearInkSlection();
            UpdateInkingDeviceOnUIThread();
            base.Deselect();
        }
        protected override void Select()
        {
            UpdateInkingDeviceOnUIThread();
            base.Select();
        }
        public void SelectionPreviewClear()
        {
            SelectionPolygon.Points.Clear();
        }
        public void SelectionPreviewAdd(Windows.Foundation.Point pt)
        {
            SelectionPolygon.Points.Add(pt);
        }
        public void SelectInkWithPolyline(IEnumerable<Windows.Foundation.Point> Polyline)
        {
            UpdateInkSelectionRectangle(InkCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(Polyline));
        }
        
        public Windows.Foundation.Rect SelectionBounds { get; private set; }
        public void UpdateInkSelectionRectangle(Windows.Foundation.Rect Bounds)
        {
            SelectionBounds = Bounds;
            if (Bounds.Width is 0 && Bounds.Height is 0)
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                SelectionRectangle.Visibility = Visibility.Visible;
                SelectionColor.Color =
                    SelectionRectangle.ActualTheme == ElementTheme.Dark ? UISettings.GetColorValue(UIColorType.AccentLight2)
                    : UISettings.GetColorValue(UIColorType.AccentDark2);
                SelectionRenderTransform.TranslateX = Bounds.X; //-(Width - Bounds.Width) / 2 + Bounds.X;
                SelectionRenderTransform.TranslateY = Bounds.Y; //-(Height - Bounds.Height) / 2 + Bounds.Y;
                SelectionRectangle.Width = Bounds.Width;
                SelectionRectangle.Height = Bounds.Height;

            }
        }
        public void ClearInkSlection()
        {
            foreach (var s in InkCanvas.InkPresenter.StrokeContainer.GetStrokes())
                s.Selected = false;
            UpdateInkSelectionRectangle(default);
        }
        protected override bool RequestCut()
        {
            if (RequestCopy())
            {
                InkCanvas?.InkPresenter.StrokeContainer.DeleteSelected();
                UpdateInkSelectionRectangle(default);
                return true;
            }
            return false;
        }
        protected override bool RequestCopy()
        {
            if (SelectionBounds.Width == 0) return false;
            if (InkCanvas is null) return false;
            InkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            return true;
        }
        public bool CanPasteInk => InkCanvas?.InkPresenter.StrokeContainer.CanPasteFromClipboard() ?? false;
        public override bool RequestPaste()
        {
            if (InkCanvas is null) return false;
            if (!CanPasteInk)
                return false;
            PasteInkAt(LatestMouse);
            return true;
        }
        public void PasteInkAt(Windows.Foundation.Point pt)
        {
            InkCanvas?.InkPresenter.StrokeContainer.PasteFromClipboard(pt);
        }
        public override bool RequestDuplicate()
        {
            if (SelectionBounds.Width == 0) return false;
            if (InkCanvas is null) return false;
            InkCanvas.InkPresenter.StrokeContainer.AddStrokes(
                (from x in InkCanvas.InkPresenter.StrokeContainer.GetStrokes()
                 where x.Selected
                 select x.Clone()).ToArray()
            );
            return true;
        }
        public override bool RequestDelete()
        {
            if (SelectionBounds.Width == 0) return false;
            if (InkCanvas is null) return false;
            InkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            UpdateInkSelectionRectangle(default);
            return true;
        }
        public override void DisablePreviewEffects()
        {
            ClearInkSlection();
            base.DisablePreviewEffects();
        }
        void UpdateInkingDeviceOnUIThread() => Extension.RunOnUIThread(() => UpdateInkingDevice());
        [RunOnUIThread]
        void UpdateInkingDevice()
        {
            CoreInputDeviceTypes InputTypes;
            if (DrawingAllowed && Selecting)
            {
                InputTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
                if (TouchAllowed) InputTypes |= CoreInputDeviceTypes.Touch;
            }
            else
            {
                InputTypes = CoreInputDeviceTypes.None;
            }
            InkCanvas!.InkPresenter.InputDeviceTypes = InputTypes;
        }
        public override void Dispose() => Extension.RunOnUIThread(() => InkCanvas!.InkPresenter.StrokeContainer.Clear());
        protected override JObject OnDataSaving()
        {
            async Task<JObject> func()
            {
                var ms = new InMemoryRandomAccessStream();
                await Extension.RunOnUIThreadAsync(async () =>
                    await InkCanvas!.InkPresenter.StrokeContainer.SaveAsync(ms, InkPersistenceFormat.Isf)
                );

                byte[] bytes = new byte[ms.Size];
                var readstream = ms.AsStreamForRead();
                readstream.Position = 0;
                await readstream.ReadAsync(bytes, 0, bytes.Length);
                return new JObject(new JProperty("Ink", bytes));
            }
            Task<JObject> t = func();
            t.Wait();
            return t.Result;
        }

        protected override void OnDataLoading(JObject json, Task _)
        {
            var ms = new MemoryStream(json["Ink"].ToObject<byte[]>());
            Extension.RunOnUIThread(async () => await InkCanvas.InkPresenter.StrokeContainer.LoadAsync(ms.AsRandomAccessStream()));
            ms.Dispose();
        }
    }
    public class TextLayer : Layer
    {
        public override Types LayerType { get; } = Types.Text;
        public TextBlock TextBlock { get; private set; }
        string _Text;
        public string Text
        {
            get => TextBlock.Text;
            set
            {
                _Text = value;
                if (TextBlock != null) TextBlock.Text = value;
            }
        }
        FontFamily _Font;
        public FontFamily Font
        {
            get => TextBlock.FontFamily;
            set
            {
                _Font = value;
                if (TextBlock != null) TextBlock.FontFamily = value;
            }
        }
        double? _FontSize;
        public double? FontSize
        {
            get => TextBlock.FontSize;
            set
            {
                _FontSize = value;
                if (TextBlock != null && value.HasValue) TextBlock.FontSize = value.Value;
            }
        }
        Color _TextColor = Colors.White;
        public Color TextColor
        {
            get => (TextBlock.Foreground as SolidColorBrush ?? throw new InvalidCastException()).Color;
            set
            {
                _TextColor = value;
                if (TextBlock != null) TextBlock.Foreground = new SolidColorBrush(value);
            }
        }

        public TextLayer(JObject json)
        {
            LoadData(json);
            OnCreate();

        }
        public TextLayer(Windows.Foundation.Point Where, string Text)
        {
            this.Text = Text;
            OnCreate();
            X = Where.X;
            Y = Where.Y;
        }
        protected override void OnCreate()
        {
            Extension.RunOnUIThread(() =>
            {
                TextBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CanBeScrollAnchor = false,
                    Text = _Text,
                    Foreground = new SolidColorBrush(_TextColor)
                };
                if (_Font != null) TextBlock.FontFamily = _Font;
                if (_FontSize.HasValue) TextBlock.FontSize = _FontSize.Value;

                LayerUIElement.Children.Add(TextBlock);
            });
        }
        public override void Dispose() { }
        protected override JObject OnDataSaving()
        {
            string Text = "", FontFamily = "";
            double? FontSize = default;
            Color TextColor = default;
            Extension.RunOnUIThread(() =>
            {
                Text = this.Text;
                FontFamily = Font.Source;
                FontSize = this.FontSize;
                TextColor = this.TextColor;
            });
            return new JObject(
                new JProperty("Text", Text),
                new JProperty("FontFamily", FontFamily),
                new JProperty("FontSize", FontSize),
                new JProperty("TextColor", new JObject(
                    new JProperty("R", TextColor.R),
                    new JProperty("G", TextColor.G),
                    new JProperty("B", TextColor.B),
                    new JProperty("A", TextColor.A)
                ))
            );
        }

        protected override void OnDataLoading(JObject json, Task _)
        {
            Extension.RunOnUIThread(() =>
            {
                Text = json["Text"]?.ToObject<string>() ?? "";
                var f = json["FontFamily"]?.ToObject<string>();
                if (f != null) Font = new FontFamily(f);
                FontSize = json["FontSize"]?.ToObject<double>();
                var TextColor = json["TextColor"];
                if (TextColor is null) return;
                var r = TextColor["R"]?.ToObject<byte>();
                var g = TextColor["G"]?.ToObject<byte>();
                var b = TextColor["B"]?.ToObject<byte>();
                var a = TextColor["A"]?.ToObject<byte>();
                if (r is null || g is null || b is null || a is null) return;
                this.TextColor = Color.FromArgb(a.Value, r.Value, g.Value, b.Value);
            });
        }
    }
    public abstract class ShapeLayer : Layer
    {
        public abstract Brush BackgroundBrush { get; set; }
        public bool Acrylic
        {
            get => BackgroundBrush is AcrylicBrush;
            set
            {
                var IsAlreadyAcrylic = Acrylic;
                if (IsAlreadyAcrylic != value)
                {
                    var Opacity = this.Opacity;
                    var BackColor = this.Color;
                    BackgroundBrush = value ? new AcrylicBrush() as Brush : new SolidColorBrush();
                    this.Opacity = Opacity;
                    this.Color = BackColor;
                }
            }
        }
        public double Opacity
        {
            get => BackgroundBrush.Opacity;
            set => BackgroundBrush.Opacity = value;
        }
        public Color Color
        {
            get
            {
                if (BackgroundBrush is AcrylicBrush AcrylicBrush) return AcrylicBrush.TintColor;
                else if (BackgroundBrush is SolidColorBrush SolidColorBrush) return SolidColorBrush.Color;
                else return default;
            }
            set
            {
                if (BackgroundBrush is AcrylicBrush AcrylicBrush) AcrylicBrush.TintColor = value;
                else if (BackgroundBrush is SolidColorBrush SolidColorBrush) SolidColorBrush.Color = value;
            }
        }
        public double TintOpacity
        {
            get => (BackgroundBrush as AcrylicBrush)?.TintOpacity ?? 1;
            set
            {
                if (BackgroundBrush is AcrylicBrush AcrylicBrush) AcrylicBrush.TintOpacity = value;
            }
        }

        public ShapeLayer(JObject json)
        {
            LoadData(json);
            OnCreate();

        }
        public ShapeLayer()
        {
            OnCreate();
        }
        public override void Dispose() { }
        protected override JObject OnDataSaving()
        {
            bool Acrylic = false;
            Color Color = default;
            double Opacity = default, TintOpacity = default;
            Extension.RunOnUIThread(() =>
            {
                var BackgroundBrush = this.BackgroundBrush;
                Opacity = BackgroundBrush.Opacity;
                if (BackgroundBrush is AcrylicBrush AcrylicBrush)
                {
                    Acrylic = true;
                    Color = AcrylicBrush.TintColor;
                    TintOpacity = AcrylicBrush.TintOpacity;
                }
                else if (BackgroundBrush is SolidColorBrush SolidColorBrush)
                {
                    Color = SolidColorBrush.Color;
                }
            });
            return new JObject(
                new JProperty(nameof(Acrylic), Acrylic),
                new JProperty(nameof(Color), new byte[] { Color.A, Color.R, Color.G, Color.B }),
                new JProperty(nameof(Opacity), Opacity),
                new JProperty(nameof(TintOpacity), TintOpacity)
            );
        }

        protected override void OnDataLoading(JObject json, Task _)
        {
            var Acrylic = json["Acrylic"]?.ToObject<bool>() ?? false;
            var Opacity = json["Opacity"]?.ToObject<double>() ?? 1;
            var TintOpacity = json["TintOpacity"]?.ToObject<double>() ?? 1;
            var c = json["Color"]?.ToObject<byte[]>() ?? new byte[] { 0, 0, 0, 0 };
            var Color = new Color { A = c[0], R = c[1], G = c[2], B = c[3] };
            Extension.RunOnUIThread(() =>
            {
                if (Acrylic)
                {
                    BackgroundBrush = new AcrylicBrush
                    {
                        TintColor = Color,
                        Opacity = Opacity,
                        TintOpacity = TintOpacity
                    };
                }
                else
                {
                    BackgroundBrush = new SolidColorBrush
                    {
                        Color = Color,
                        Opacity = Opacity
                    };
                }
            });
        }
    }
    public class RectangleLayer : ShapeLayer
    {
        public override Types LayerType { get; } = Types.RectangleShape;
        Rectangle Rectangle { get; set; }
        Brush _BackgroundBrush;
        public override Brush BackgroundBrush
        {
            get => Rectangle == null ? _BackgroundBrush : Rectangle.Fill;
            set
            {
                if (Rectangle == null) _BackgroundBrush = value;
                else Rectangle.Fill = value;
            }
        }
        public RectangleLayer() : base() { }
        public RectangleLayer(JObject json) : base(json) { }
        protected override void OnCreate()
        {
            Extension.RunOnUIThread(() =>
            {
                var BackgroundBrush = _BackgroundBrush;
                if (BackgroundBrush == null) BackgroundBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                Rectangle = new Rectangle()
                {
                    Fill = BackgroundBrush
                };
                LayerUIElement.Children.Add(Rectangle);
            });
        }
    }
    public class EllipseLayer : ShapeLayer
    {
        public override Types LayerType { get; } = Types.EllipseShape;
        Ellipse Ellipse { get; set; }
        Brush _BackgroundBrush;
        public override Brush BackgroundBrush
        {
            get => Ellipse == null ? _BackgroundBrush : Ellipse.Fill;
            set
            {
                if (Ellipse == null) _BackgroundBrush = value;
                else Ellipse.Fill = value;
            }
        }
        public EllipseLayer() : base() { }
        public EllipseLayer(JObject json) : base(json) { }
        protected override void OnCreate()
        {
            Extension.RunOnUIThread(() =>
            {
                var BackgroundBrush = _BackgroundBrush;
                if (BackgroundBrush == null) BackgroundBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                Ellipse = new Ellipse()
                {
                    Fill = BackgroundBrush
                };
                LayerUIElement.Children.Add(Ellipse);
            });
        }
    }

}
namespace PhotoFlow
{
    public static partial class Extension
    {
        public static T SetName<T>(this T Layer, string Name) where T : Layer.Layer
        {
            Layer.LayerName.Value = Name;
            return Layer;
        }
        public static T DeepClone<T>(this T Layer) where T : Layer.Layer
        {
            return (T)LayerContainer.LoadLayer(Layer.SaveData());
        }
    }
}
