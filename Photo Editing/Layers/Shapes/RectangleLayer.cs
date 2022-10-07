#nullable enable
using Windows.UI.Xaml.Media;
using Windows.UI;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Shapes;
using System.Diagnostics.CodeAnalysis;

namespace PhotoFlow.Layers;

public class RectangleLayer : ShapeLayer
{
    public override Types LayerType { get; } = Types.RectangleShape;
    Rectangle Rectangle { get; set; }
    Brush? _BackgroundBrush;
    public override Brush BackgroundBrush
    {
        get => Rectangle.Fill;
        set
        {
            if (Rectangle == null) _BackgroundBrush = value;
            else Rectangle.Fill = value;
        }
    }
    public RectangleLayer() { OnCreate(); }
    public RectangleLayer(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime);
        OnCreate();
    }
    [MemberNotNull(nameof(Rectangle))]
    protected override void OnCreate()
    {
        Extension.RunOnUIThread(() =>
        {
            var BackgroundBrush = _BackgroundBrush;
            BackgroundBrush ??= new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            Rectangle = new Rectangle()
            {
                Fill = BackgroundBrush
            };
            LayerUIElement.Children.Add(Rectangle);
        });
#pragma warning disable CS8774
    }
#pragma warning restore CS8774
}
