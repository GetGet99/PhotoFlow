using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PhotoEditing
{
    partial class MainPage
    {
        float ZoomFactor => MainScrollView.ZoomFactor;
        void ImplementZoomFunctionality()
        {
            MainScrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
            {
                var ZoomFactor = this.ZoomFactor;
                Zoom.Text = (ZoomFactor * 100).ToString();
            });
        }
    }
}
