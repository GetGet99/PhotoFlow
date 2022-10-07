#nullable enable
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace PhotoFlow.Layers;

public class TextLayer : Layer
{
    public override Types LayerType { get; } = Types.Text;
    public TextBlock TextBlock { get; private set; }
    string? _Text;
    public string? Text
    {
        get
        {
            _Text ??= TextBlock.Text;
            return TextBlock.Text;
        }
        set
        {
            _Text = value;
            if (TextBlock != null) TextBlock.Text = value;
        }
    }
    FontFamily? _Font;

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

    public TextLayer(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime);
        OnCreate();

    }
    public TextLayer(Windows.Foundation.Point Where, string Text)
    {
        this.Text = Text;
        OnCreate();
        X = Where.X;
        Y = Where.Y;
    }
    [MemberNotNull(nameof(TextBlock))]
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
#pragma warning disable CS8774
    }
#pragma warning restore CS8774
    public override void Dispose() { }
    protected override JObject OnDataSaving()
    {
        string? Text = "", FontFamily = "";
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