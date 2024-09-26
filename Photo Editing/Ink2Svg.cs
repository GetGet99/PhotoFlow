using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Input.Inking;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Svg;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
namespace PhotoFlow;

static class Ink2Svg
{
    public static CanvasSvgDocument ConvertInkToSVG(InkPresenter InkPresenter, float width, float height)
    {
        var sharedDevice = CanvasDevice.GetSharedDevice();
        using (var offscreen = new CanvasRenderTarget(sharedDevice, width, height, 96))
        {
            using (var session = offscreen.CreateDrawingSession())
            {
                var svgDocument = new CanvasSvgDocument(sharedDevice);

                svgDocument.Root.SetStringAttribute("viewBox", $"0 0 {width} {height}");

                foreach (var stroke in InkPresenter.StrokeContainer.GetStrokes())
                {
                    var canvasGeometry = CanvasGeometry.CreateInk(session, new[] { stroke }).Outline();

                    var pathReceiver = new CanvasGeometryToSvgPathReader();
                    canvasGeometry.SendPathTo(pathReceiver);
                    var element = svgDocument.Root.CreateAndAppendNamedChildElement("path");
                    element.SetStringAttribute("d", pathReceiver.Path);
                    var color = stroke.DrawingAttributes.Color;
                    element.SetColorAttribute("fill", color);

                }

                return svgDocument;
            }
        }
    }
    class CanvasGeometryToSvgPathReader : ICanvasPathReceiver
    {
        private readonly Vector2 _ratio;
        private List<string> Parts { get; }
        public string Path => string.Join(" ", Parts);
        StringBuilder sb = new("""

            """);
        public CanvasGeometryToSvgPathReader() : this(Vector2.One)
        { }

        public CanvasGeometryToSvgPathReader(Vector2 ratio)
        {
            _ratio = ratio;
            Parts = new List<string>();
        }

        public void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill)
        {
            Parts.Add($"M{startPoint.X / _ratio.X} {startPoint.Y / _ratio.Y}");
        }

        public void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
        {

        }

        public void AddCubicBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            Parts.Add($"C{controlPoint1.X / _ratio.X},{controlPoint1.Y / _ratio.Y} {controlPoint2.X / _ratio.X},{controlPoint2.Y / _ratio.Y} {endPoint.X / _ratio.X},{endPoint.Y / _ratio.Y}");
        }

        public void AddLine(Vector2 endPoint)
        {
            Parts.Add($"L {endPoint.X / _ratio.X} {endPoint.Y / _ratio.Y}");
        }

        public void AddQuadraticBezier(Vector2 controlPoint, Vector2 endPoint)
        {
            //
        }

        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination)
        {
            //
        }

        public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions)
        {
            //
        }

        public void EndFigure(CanvasFigureLoop figureLoop)
        {
            Parts.Add("Z");
        }
    }
}