using CSharpUI;
using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using PhotoFlow.CommandButton.Controls;

namespace PhotoFlow;

public class ImageCommandButton : CommandButtonBase
{

    private readonly ImageBar ImageCommandBar = new ();
    protected override CommandButtonCommandBar CommandBar => ImageCommandBar;

    //Layer.MatLayer MatLayer => CurrentLayer.LayerType == Layer.Types.Mat ? (Layer.MatLayer)CurrentLayer : null;
    Features.Mat.IMatFeatureApplyable<Mat> MatFeatureApplyable
        => CurrentLayer.Cast<Features.Mat.IMatFeatureApplyable<Mat>>();

    public ImageCommandButton(Border CommandBarPlace) : base(Symbol.Pictures, CommandBarPlace)
    {
        ImageCommandBar.CreateNewLayer.Click += (s, e) =>
        {
            var NewMatLayer = new Layer.MatLayer(new Rect(x: -CanvasPadding.Width, y: -CanvasPadding.Height, width: CanvasSize.Width, height: CanvasSize.Height));
            NewMatLayer.LayerName.Value = "Blank Image Layer";
            AddNewLayer(NewMatLayer);
        };
        ImageCommandBar.Invert.Click += (s, e) =>
        {
            var MatFeatureApplyable = this.MatFeatureApplyable;
            var feat = new Features.Mat.Invert();
            MatFeatureApplyable?.ApplyFeature(feat);
            LayerContainer.History.Add(feat);
        };
    }

    class ImageBar : CommandButtonCommandBar
    {
        public Button CreateNewLayer, Invert, Crop;
        public ImageBar()
        {
            Children.Add(new Button
            {
                Content = "Create New Blank Layer",
                Margin = new Thickness(0, 0, 10, 0)
            }.Assign(out CreateNewLayer));
            Children.Add(new Button
            {
                Content = "Invert",
                Margin = new Thickness(0, 0, 10, 0)
            }.Assign(out Invert));
            Children.Add(new Button
            {
                Content = "Crop",
                Margin = new Thickness(0, 0, 10, 0)
            }.Assign(out Crop));
        }
    }
}
