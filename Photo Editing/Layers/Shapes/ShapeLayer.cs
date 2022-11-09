#nullable enable
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Shapes;
using System.Diagnostics.CodeAnalysis;

namespace PhotoFlow.Layers;

public abstract class ShapeLayer : Layer
{
    [DisallowNull]
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

    public ShapeLayer()
    {
    }
    public override void Dispose() { }
    protected override JObject OnDataSaving(bool Runtime)
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