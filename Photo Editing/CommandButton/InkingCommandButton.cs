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
    static readonly UISettings UISettings = new();
    private readonly Inking InkingCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => InkingCommandBar;

    Layer.InkingLayer? InkLayer;

    public InkingCommandButton(Border CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Edit, CommandBarPlace, LayerContainer, MainScrollViewer)
    {
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
        
        
    }
    void RotateRuler(double degree)
    {
        var radian = degree / 180 * Math.PI;
        var ruler = InkingCommandBar.StencilButton.Ruler;
        var originaltransform = ruler.Transform;
        var transform = originaltransform;
        var COS = (float)Math.Cos(radian);
        var SIN = (float)Math.Sin(radian);
        transform.Translation += new System.Numerics.Vector2((float)ruler.Length * COS, (float)ruler.Length * SIN);
        InkingCommandBar.StencilButton.Ruler.Transform = transform;
    }
    protected override void Selected() => base.Selected();
    protected override void Deselected()
    {
        base.Deselected();
        InkingCommandBar.StencilButton.IsChecked = false;
    }
    protected override void LayerChanged(Layer.Layer Layer)
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
    List<Windows.Foundation.Point> Lasso = new();
    void UnprocessedPressed(InkUnprocessedInput o, PointerEventArgs ev)
    {
        if (InkingCommandBar.LassoTool.IsChecked ?? false)
        {
            Lasso.Clear();
            var pos = ev.CurrentPoint.Position;
            Lasso.Add(pos);
            if (InkLayer is not null)
            {
                InkLayer.ClearInkSlection();
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
        public readonly Button CreateNewLayer;
        public readonly ToggleButton TouchDraw;
        public readonly InkToolbar InkControl;
        public readonly InkToolbarCustomToolButton LassoTool;
        public readonly PropertiesButton PropertiesButton;
        public InkToolbarStencilButton StencilButton;
        public Inking()
        {
            Children.Add(CreateNewLayer = new Button
            {
                Content = "Create New Inking Layer",
                Margin = DefaultMargin
            });
            Children.Add(TouchDraw = new ToggleButton
            {
                Content = "Touch Drawing",
                Margin = DefaultMargin
            });
            var TransparentBrush = new SolidColorBrush(Colors.Transparent);
            Children.Add(InkControl = new InkToolbar
            {
                Margin = DefaultMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 100,
                Children =
                {
                    new InkToolbarBallpointPenButton { VerticalAlignment = VerticalAlignment.Center },
                    new InkToolbarPencilButton(),
                    new InkToolbarHighlighterButton(),
                    (LassoTool = new InkToolbarCustomToolButton
                    {
                        Content = new SymbolIcon((Symbol)0xF408),
                        Background = TransparentBrush
                    }.Edit(x => {
                        RoutedEventHandler r = (_, _1) => x.Background = x.IsChecked ?? false ? (Brush)App.Current.Resources["CardBackgroundFillColorDefaultBrush"] : TransparentBrush;
                        x.Checked += r;
                        x.Unchecked += r;
                    })),
                    new InkToolbarEraserButton(),
                    new InkToolbarStencilButton().Assign(out StencilButton)
                }
            });

            Children.Add(PropertiesButton = new PropertiesButton
            {
                Margin = new Thickness(0, 0, 10, 0)
            }.Edit(x => LayerChanged += () => x.Layer = Layer));
        }
    }
}
