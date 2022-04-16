using Microsoft.Graphics.Canvas;
using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI;
using System;
namespace PhotoFlow
{
    using CommandButton.Controls;
    public class InkingCommandButton : CommandButtonBase
    {
        private readonly Inking InkingCommandBar = new Inking();
        protected override CommandButtonCommandBar CommandBar => InkingCommandBar;


        Layer.InkingLayer InkLayer;

        public InkingCommandButton(Border CommandBarPlace) : base(Symbol.Edit, CommandBarPlace)
        {
            InkingCommandBar.CreateNewLayer.Click += (s, e) =>
            {
                InkLayer = new Layer.InkingLayer(new Rect(x: -CanvasPadding.Width, y: -CanvasPadding.Height, width: CanvasSize.Width, height: CanvasSize.Height));
                InkLayer.LayerName.Value = "Inking Layer";
                InkLayer.DrawingAllowed.Value = true;
                InkLayer.TouchAllowed.Value = InkingCommandBar.TouchDraw.IsChecked.Value;
                AddNewLayer(InkLayer);
            };

            InkingCommandBar.TouchDraw.Click += (s, e) =>
            {
                if (InkLayer != null) InkLayer.TouchAllowed.Value = InkingCommandBar.TouchDraw.IsChecked.Value;
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
            //transform.M11 = COS;
            //transform.M12 = -SIN;
            //transform.M21 = SIN;
            //transform.M22 = COS;
            transform.Translation += new System.Numerics.Vector2((float)ruler.Length * COS, (float)ruler.Length * SIN);
            InkingCommandBar.StencilButton.Ruler.Transform = transform;
        }
        protected override void Selected()
        {
            base.Selected();
            //InvokeLayerChange();
        }
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
            }
            else
            {
                InkingCommandBar.InkControl.Visibility = Visibility.Collapsed;
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
            }
        }
        protected override void RequestRemoveLayerEvent(Layer.Layer Layer)
        {
            base.RequestRemoveLayerEvent(Layer);
            if (Layer != null && Layer.LayerType == PhotoFlow.Layer.Types.Inking) ((Layer.InkingLayer)Layer).DrawingAllowed.Value = false;
        }
    }
}
