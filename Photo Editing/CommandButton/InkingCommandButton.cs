#nullable enable
using CSharpUI;
using OpenCvSharp;
using PhotoFlow.CommandButton.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Input.Inking;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;
using Window = Windows.UI.Xaml.Window;

namespace PhotoFlow;


public class InkingCommandButton : CommandButtonBase
{
    private readonly Inking InkingCommandBar;
    protected override CommandButtonCommandBar CommandBar => InkingCommandBar;

    Layer.InkingLayer? InkLayer;

    public InkingCommandButton(ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Edit, CommandBarPlace, LayerContainer, MainScrollViewer)
    {
        InkingCommandBar = new(LayerContainer.History);
        InkingCommandBar.CreateNewLayer.Click += (s, e) =>
        {
            InkLayer = new Layer.InkingLayer(new Rect(x: -CanvasPadding.Width, y: -CanvasPadding.Height, width: CanvasSize.Width, height: CanvasSize.Height));
            InkLayer.LayerName.Value = "Inking Layer";
            InkLayer.DrawingAllowed.Value = true;
            if (InkingCommandBar.TouchDraw.IsChecked != null)
                InkLayer.TouchAllowed.Value = InkingCommandBar.TouchDraw.IsChecked.Value;
            AddNewLayer(InkLayer);
        };

        InkingCommandBar.TouchDraw.Click += (s, e) =>
        {
            if (InkLayer != null)
                if (InkingCommandBar.TouchDraw.IsChecked != null)
                    InkLayer.TouchAllowed.Value = InkingCommandBar.TouchDraw.IsChecked.Value;
        };
        LayerContainer.History.PropertyChanged += delegate
        {
            var history = LayerContainer.History;
            InkingCommandBar.Undo.IsEnabled = history.CanUndo && history.NextUndo?.Tag == InkLayer;
            InkingCommandBar.Redo.IsEnabled = history.CanRedo && history.NextRedo?.Tag == InkLayer;
        };
        InkingCommandBar.Undo.Click += delegate
        {
            var history = LayerContainer.History;
            if (history.CanUndo && history.NextUndo?.Tag == InkLayer)
                history.Undo();
        };
        InkingCommandBar.Redo.Click += delegate
        {
            var history = LayerContainer.History;
            if (history.CanRedo && history.NextRedo?.Tag == InkLayer)
                history.Redo();
        };


    }
    //void RotateRuler(double degree)
    //{
    //    var radian = degree / 180 * Math.PI;
    //    var ruler = InkingCommandBar.StencilButton.Ruler;
    //    var originaltransform = ruler.Transform;
    //    var transform = originaltransform;
    //    var COS = (float)Math.Cos(radian);
    //    var SIN = (float)Math.Sin(radian);
    //    transform.Translation += new System.Numerics.Vector2((float)ruler.Length * COS, (float)ruler.Length * SIN);
    //    InkingCommandBar.StencilButton.Ruler.Transform = transform;
    //}
    protected override void Selected() => base.Selected();
    protected override void Deselected()
    {
        base.Deselected();
        InkingCommandBar.StencilButton.IsChecked = false;
    }
    protected override void LayerChanged(Layer.Layer? Layer)
    {
        base.LayerChanged(Layer);
        if (Layer != null && Layer.LayerType == PhotoFlow.Layer.Types.Inking)
        {
            InkLayer = (Layer.InkingLayer)Layer;
            InkingCommandBar.InkControl.Visibility = Visibility.Visible;
            InkingCommandBar.PropertiesButton.Visibility = Visibility.Visible;
        }
        else
        {
            InkingCommandBar.InkControl.Visibility = Visibility.Collapsed;
            InkingCommandBar.PropertiesButton.Visibility = Visibility.Collapsed;
        }
    }
    protected override void RequestAddLayerEvent(Layer.Layer Layer)
    {
        base.RequestAddLayerEvent(Layer);
        if (Layer != null && Layer.LayerType == PhotoFlow.Layer.Types.Inking)
        {
            var InkLayer = (Layer.InkingLayer)Layer;
            InkLayer.DrawingAllowed.Value = true;
            InkingCommandBar.InkControl.TargetInkCanvas = InkLayer.InkCanvas;
            InkingCommandBar.PropertiesButton.Layer = Layer;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedPressed;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedMove;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedReleased;
        }
    }
    protected override void RequestRemoveLayerEvent(Layer.Layer Layer)
    {
        base.RequestRemoveLayerEvent(Layer);
        if (Layer != null && Layer.LayerType == PhotoFlow.Layer.Types.Inking)
        {
            var InkLayer = (Layer.InkingLayer)Layer;
            InkLayer.DrawingAllowed.Value = false;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerPressed -= UnprocessedPressed;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerMoved -= UnprocessedMove;
            InkLayer.InkCanvas!.InkPresenter.UnprocessedInput.PointerReleased -= UnprocessedReleased;
        }
        InkingCommandBar.PropertiesButton.Layer = null;
    }
    readonly List<Windows.Foundation.Point> Lasso = new();
    void UnprocessedPressed(InkUnprocessedInput o, PointerEventArgs ev)
    {
        if (InkingCommandBar.LassoTool.IsChecked ?? false)
        {
            Lasso.Clear();
            var pos = ev.CurrentPoint.Position;
            Lasso.Add(pos);
            if (InkLayer is not null)
            {
                InkLayer.ClearInkSelection();
                InkLayer.SelectionPreviewClear();
                InkLayer.SelectionPreviewAdd(pos);
            }
        }
    }
    void UnprocessedMove(InkUnprocessedInput o, PointerEventArgs ev)
    {
        if (!ev.CurrentPoint.IsInContact) return;
        if (InkingCommandBar.LassoTool.IsChecked ?? false)
        {
            var pos = ev.CurrentPoint.Position;
            Lasso.Add(pos);
            if (InkLayer is not null)
            {
                InkLayer.SelectionPreviewAdd(pos);
            }
        }
    }
    void UnprocessedReleased(InkUnprocessedInput o, PointerEventArgs ev)
    {
        if (InkingCommandBar.LassoTool.IsChecked ?? false)
        {
            if (InkLayer is not null)
            {
                InkLayer.SelectionPreviewClear();
                InkLayer.SelectInkWithPolyline(Lasso);
            }
        }
    }
    class Inking : CommandButtonCommandBar
    {
        public void ForceUpdateLayer() => LayerChanged?.Invoke();
        public event Action? LayerChanged;
        Layer.Layer? _Layer;
        public Layer.Layer? Layer
        {
            get => _Layer; set
            {
                _Layer = value;
                LayerChanged?.Invoke();
            }
        }
        static Thickness DefaultMargin = new (0, 0, 10, 0);
        public readonly Button CreateNewLayer, Undo, Redo;
        public readonly ToggleButton TouchDraw;
        public readonly InkToolbar InkControl;
        public readonly InkToolbarCustomToolButton LassoTool;
        public readonly PropertiesButton PropertiesButton;
        public InkToolbarStencilButton StencilButton;
        public Inking(History History)
        {
            Children.Add(CreateNewLayer = new Button
            {
                Content = new SymbolIcon(Symbol.Add),
                Margin = DefaultMargin
            }.Edit(x => ToolTipService.SetToolTip(x, "Add New Drawing Layer")));
            var TransparentBrush = new SolidColorBrush(Colors.Transparent);
            Children.Add(InkControl = new InkToolbar
            {
                Margin = DefaultMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 100,
                Children =
                {
                    new InkToolbarBallpointPenButton { VerticalAlignment = VerticalAlignment.Center }
                    .Assign(out var BalloonPen),
                    new InkToolbarPencilButton(),
                    new InkToolbarHighlighterButton().Assign(out var Highlight),
                    (LassoTool = new InkToolbarCustomToolButton
                    {
                        Content = new SymbolIcon((Symbol)0xF408),
                        Background = TransparentBrush
                    }.Edit(x => {
                        void r (object _, RoutedEventArgs _1) => x.Background = x.IsChecked ?? false ? (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] : TransparentBrush;
                        x.Checked += r;
                        x.Unchecked += r;
                    })),
                    new InkToolbarEraserButton(),
                    new InkToolbarStencilButton().Assign(out StencilButton),
                    new InkToolbarCustomToggleButton
                    {
                        Content = new SymbolIcon(Symbol.TouchPointer)
                    }.Edit(x => ToolTipService.SetToolTip(x, "Touch Drawing")).Assign(out TouchDraw)
                }
            });
            BalloonPen.Loaded += delegate
            {
                Highlight.Palette.Clear();
                foreach (var brush in BalloonPen.Palette) Highlight.Palette.Add(brush);
            };
            Button CreateColorButton(int index)
            {
                var btn = new Button
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    Visibility = Visibility.Collapsed,
                    Width = 30,
                    Height = 30,
                    CornerRadius = new CornerRadius(16)
                };
                BalloonPen.Loaded += delegate
                {
                    var colorBrush = BalloonPen.Palette[index].CastOrThrow<SolidColorBrush>();
                    var noFullcolorBrush = new SolidColorBrush(colorBrush.Color) { Opacity = 0.8 };
                    btn.Background = noFullcolorBrush;
                    btn.Resources.ThemeDictionaries["Dark"] = new ResourceDictionary
                    {
                        ["ButtonBackground"] = noFullcolorBrush,
                        ["ButtonBackgroundPointerPressed"] = colorBrush,
                        ["ButtonBackgroundPointerOver"] = colorBrush,
                    };
                    btn.Resources.ThemeDictionaries["Light"] = new ResourceDictionary
                    {
                        ["ButtonBackground"] = noFullcolorBrush,
                        ["ButtonBackgroundPointerPressed"] = colorBrush,
                        ["ButtonBackgroundPointerOver"] = colorBrush,
                    };
                    btn.Command = new LambdaCommand(() =>
                    {
                        if (InkControl.ActiveTool is InkToolbarPenButton i) i.SelectedBrushIndex = index;
                    });
                };
                
                InkControl.RegisterPropertyChangedCallback(VisibilityProperty, delegate
                {
                    btn.Visibility = InkControl.Visibility;
                });
                return btn;
            }
            Children.Add(CreateColorButton(0));
            Children.Add(CreateColorButton(1));
            Children.Add(CreateColorButton(7));
            Children.Add(CreateColorButton(10));
            Children.Add(CreateColorButton(13));
            Children.Add(CreateColorButton(15));
            Children.Add(Undo = new Button
            {
                Margin = new Thickness(0, 0, 10, 0),
                Content = new SymbolIcon(Symbol.Undo)
            });

            Children.Add(Redo = new Button
            {
                Margin = new Thickness(0, 0, 10, 0),
                Content = new SymbolIcon(Symbol.Redo)
            });

            Children.Add(PropertiesButton = new PropertiesButton(History)
            {
                Margin = new Thickness(0, 0, 10, 0)
            }.Edit(x => LayerChanged += () => x.Layer = Layer));
        }
    }
}
