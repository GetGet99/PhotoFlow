using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PhotoEditing
{
    partial class MainPage
    {
        void ImplementZoomFunctionality()
        {
            MainScrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
            {
                Zoom.Text = (MainScrollView.ZoomFactor * 100).ToString();
            });
        }
    }
}
