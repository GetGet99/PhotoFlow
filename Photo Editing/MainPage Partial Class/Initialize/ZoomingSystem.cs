#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PhotoFlow;

partial class MainPage
{
    readonly LambdaCommand RotateCommand;
    float ZoomFactor => MainScrollView.ZoomFactor;
    void ImplementZoomFunctionality()
    {
        MainScrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
        {
            var ZoomFactor = this.ZoomFactor;
            Zoom.Text = (ZoomFactor * 100).ToString();
        });
    }
    void ZoomIn(object _, RoutedEventArgs _1)
    {
        MainScrollView.ChangeView(null, null, ZoomFactor + 0.1f);
    }
    void ZoomOut(object _, RoutedEventArgs _1)
    {
        MainScrollView.ChangeView(null, null, Math.Max(ZoomFactor - 0.1f, 0.1f));
    }
    void ResetZoom(object _, RoutedEventArgs _1)
    {
        MainScrollView.ChangeView(null, null, 1);
    }
    void ResetRotation(object _, RoutedEventArgs _1)
    {
        LayerContaineCompositeTransformr.CenterX = LayerContainerBackground.ActualWidth / 2;
        LayerContaineCompositeTransformr.CenterY = LayerContainerBackground.ActualHeight / 2;
        LayerContaineCompositeTransformr.Rotation = 0;
        UpdateLayerContainerSizeAndRotation();
    }
}
