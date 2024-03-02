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
    //public static string ConvertInkToSVG(InkPresenter inkPresenter)
    //{
    //    StringBuilder svgBuilder = new();

    //    // SVG header
    //    svgBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
    //    svgBuilder.AppendLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
    //    svgBuilder.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">");

    //    // Convert each ink stroke to SVG path
    //    int i = 0;
    //    var strokes = inkPresenter.StrokeContainer.GetStrokes();
    //    foreach (var stroke in strokes)
    //    {
    //        var attr = stroke.DrawingAttributes;
    //        var color = stroke.DrawingAttributes.Color;
    //        var size = stroke.DrawingAttributes.Size;
    //        svgBuilder.AppendLine($"<path fill=\"none\" stroke=\"#{color.R:X2}{color.G:X2}{color.B:X2}\" stroke-width=\"{size.Width}\" stroke-linecap=\"{(attr.PenTip is PenTipShape.Circle ? "round" : "square")}\" d=\"{ConvertStrokeToSvgPath(stroke)}\"/>");
    //    }

    //    // SVG footer
    //    svgBuilder.AppendLine("</svg>");

    //    return svgBuilder.ToString();
    //}

    //static string ConvertStrokeToSvgPath(Windows.UI.Input.Inking.InkStroke stroke)
    //{
    //    StringBuilder pathBuilder = new();

    //    // Get the points of the stroke
    //    var points = stroke.GetInkPoints();

    //    if (points.Count > 0)
    //    {
    //        // Convert points to SVG path using Bézier curves
    //        var bezierPoints = GetBezierPoints(points, stroke.PointTransform);

    //        // Move to the first point
    //        var startPoint = bezierPoints[0];
    //        pathBuilder.Append($"M {startPoint.X},{startPoint.Y} ");

    //        // Generate the path data using Bézier curves
    //        for (int i = 1; i < bezierPoints.Count; i += 3)
    //        {
    //            var controlPoint1 = bezierPoints[i];
    //            var controlPoint2 = bezierPoints[i + 1];
    //            var endPoint = bezierPoints[i + 2];

    //            pathBuilder.Append($"C {controlPoint1.X},{controlPoint1.Y} {controlPoint2.X},{controlPoint2.Y} {endPoint.X},{endPoint.Y} ");
    //        }
    //    }

    //    return pathBuilder.ToString();
    //}


    //static List<Windows.Foundation.Point> GetBezierPoints(IReadOnlyList<Windows.UI.Input.Inking.InkPoint> points, Matrix3x2 transformMatrix)
    //{
    //    List<Windows.Foundation.Point> bezierPoints = new List<Windows.Foundation.Point>();

    //    if (points.Count < 2)
    //    {
    //        // If there are fewer than two points, return the original points
    //        foreach (var point in points)
    //        {
    //            bezierPoints.Add(TransformPoint(point.Position, transformMatrix));
    //        }
    //        return bezierPoints;
    //    }

    //    // Add the first point
    //    bezierPoints.Add(TransformPoint(points[0].Position, transformMatrix));

    //    // Calculate control points and end points for Bézier curves
    //    for (int i = 0; i < points.Count - 1; i++)
    //    {
    //        var p0 = i > 0 ? points[i - 1].Position : points[i].Position;
    //        var p1 = points[i].Position;
    //        var p2 = points[i + 1].Position;
    //        var p3 = i < points.Count - 2 ? points[i + 2].Position : p2;

    //        // Calculate control points
    //        var controlPoint1 = new Windows.Foundation.Point(
    //            p1.X + (p2.X - p0.X) / 6.0,
    //            p1.Y + (p2.Y - p0.Y) / 6.0
    //        );

    //        var controlPoint2 = new Windows.Foundation.Point(
    //            p2.X - (p3.X - p1.X) / 6.0,
    //            p2.Y - (p3.Y - p1.Y) / 6.0
    //        );

    //        // Add control points and end point
    //        bezierPoints.Add(TransformPoint(controlPoint1, transformMatrix));
    //        bezierPoints.Add(TransformPoint(controlPoint2, transformMatrix));
    //        bezierPoints.Add(TransformPoint(p2, transformMatrix));
    //    }

    //    return bezierPoints;
    //}

    //static Windows.Foundation.Point TransformPoint(Windows.Foundation.Point point, Matrix3x2 transformMatrix)
    //{
    //    // Apply transformation matrix to the point
    //    var transformed = Vector2.Transform(new Vector2((float)point.X, (float)point.Y), transformMatrix);
    //    return new(transformed.X, transformed.Y);
    //}
}