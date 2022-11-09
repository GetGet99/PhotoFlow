#nullable enable
using Windows.UI.Xaml.Media;
using Windows.UI;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Shapes;
using System.Diagnostics.CodeAnalysis;
using Windows.UI.Xaml;

namespace PhotoFlow.Layers;

public class EllipseLayer : ShapeLayer
{
    public override UIElement UIElementDirect => Ellipse;
    public override Types LayerType { get; } = Types.EllipseShape;
    Ellipse Ellipse { get; set; }
    Brush? _BackgroundBrush;
    [DisallowNull]
    public override Brush BackgroundBrush
    {
        get => Ellipse.Fill;
        set
        {
            if (Ellipse == null) _BackgroundBrush = value;
            else Ellipse.Fill = value;
        }
    }
    public EllipseLayer() { OnCreate(); }
    public EllipseLayer(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime);
        OnCreate();
    }
    [MemberNotNull(nameof(Ellipse))]
    protected override void OnCreate()
    {
        Extension.RunOnUIThread(() =>
        {
            var BackgroundBrush = _BackgroundBrush ?? new SolidColorBrush(Colors.Transparent);
            Ellipse = new Ellipse()
            {
                Fill = BackgroundBrush
            };
            LayerUIElement.Children.Add(Ellipse);
        });

#pragma warning disable CS8774
    }
#pragma warning restore CS8774
}
