#nullable enable
using CSharpUI;
using OpenCvSharp;
using PhotoFlow.CommandButton.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
namespace PhotoFlow;


public class InkingCommandButton : CommandButtonBase
{
    private readonly Inking InkingCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => InkingCommandBar;

    Layer.InkingLayer? InkLayer;

    public InkingCommandButton(Border CommandBarPlace) : base(Symbol.Edit, CommandBarPlace)
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
        }
    }
    protected override void RequestRemoveLayerEvent(Layer.Layer Layer)
    {
        base.RequestRemoveLayerEvent(Layer);
        if (Layer != null && Layer.LayerType == PhotoFlow.Layer.Types.Inking)
        {
            ((Layer.InkingLayer)Layer).DrawingAllowed.Value = false;
        }
        InkingCommandBar.PropertiesButton.Layer = null;
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
        public readonly PropertiesButton PropertiesButton;
        public InkToolbarStencilButton StencilButton;
        public Inking()
        {
            Children.Add(CreateNewLayer = new Button
            {
                Content = "Create New Inking Layer",
                Margin = DefaultMargin
            });
            Children.Add(new ToggleButton
            {
                Content = "Touch Drawing",
                Margin = DefaultMargin
            }.Assign(out TouchDraw));
            Children.Add(new InkToolbar
            {
                Margin = DefaultMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 100
            }
            .Edit(x => {
                x.Children.Add(new InkToolbarBallpointPenButton { VerticalAlignment = VerticalAlignment.Center });
                x.Children.Add(new InkToolbarPencilButton());
                x.Children.Add(new InkToolbarHighlighterButton());
                x.Children.Add(new InkToolbarEraserButton());
                x.Children.Add(StencilButton = new InkToolbarStencilButton());
            })
            .Assign(out InkControl));

            Children.Add(PropertiesButton = new PropertiesButton
            {
                Margin = new Thickness(0, 0, 10, 0)
            }.Edit(x => LayerChanged += () => x.Layer = Layer));

            if (StencilButton == null)
                // just to make nuablle check happy
                throw new NullReferenceException();
        }
    }
}
