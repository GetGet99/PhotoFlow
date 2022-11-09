#nullable enable
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation.Metadata;
using OpenCvSharp;

namespace PhotoFlow.Layers;

public class MatLayer : Layer
{
    public override UIElement UIElementDirect => Image;
    public override Types LayerType { get; } = Types.Mat;

    public Mat? Mat { get; set; }
    [Deprecated("Will be removed", DeprecationType.Deprecate, 1)]
    public Mat? SoftSelectedPartEdit { get => Mat; set => Mat = value; }
    [Deprecated("Will be removed", DeprecationType.Deprecate, 1)]
    public Mat? HardSelectedPartEdit { get => Mat; set => Mat = value; }

    Image Image;

    public MatLayer(Mat m)
    {
        Mat = m;
        OnCreate();
    }
    public MatLayer(Rect r)
    {
        var m = new Mat(r.Size, MatType.CV_8UC4);
        Mat = m;
        X = r.X;
        Y = r.Y;
        OnCreate();
    }
    public MatLayer(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime);
        OnCreate();
    }
    [MemberNotNull(nameof(Image))]
    protected override void OnCreate()
    {
        var m = Mat;
        var width = m?.Width ?? 0;
        var height = m?.Height ?? 0;
        Extension.RunOnUIThread(() =>
        {
            Image = new Image
            {
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            UpdateImage();
            LayerUIElement.Children.Add(Image);
            Width = width;
            Height = height;
        });
        CompleteCreate();
#pragma warning disable CS8774
    }
#pragma warning restore CS8774

    protected override JObject OnDataSaving(bool Runtime)
    {
        return Mat is null ? new JObject() : new JObject(
            new JProperty("Image", Mat.ToBytes())
        );
    }
    protected override void OnDataLoading(JObject json, Task _)
    {
        Mat = json["Image"]?.ToObject<byte[]>()?.ToMat();
        if (Image != null) Extension.RunOnUIThread(UpdateImage);
    }
    [RunOnUIThread]
    public void UpdateImage()
    {
        Image.Source = Mat?.ToBitmapImage(DisposeMat: false);
        UpdatePreview();
    }
    public override void Dispose()
    {
        Mat?.Dispose();
    }
}