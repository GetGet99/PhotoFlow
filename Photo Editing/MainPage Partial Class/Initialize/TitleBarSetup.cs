using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace PhotoFlow
{
    partial class MainPage
    {
        void SetUpTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            void SetCaptionColor()
            {
                var Background = Colors.Transparent;
                if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    Background.R = 0;
                    Background.G = 0;
                    Background.B = 0;
                    Background.A = 0;
                }
                else
                {
                    Background.R = 255;
                    Background.G = 255;
                    Background.B = 255;
                    Background.A = 0;
                }
                titleBar.ButtonBackgroundColor = Background;
                titleBar.ButtonInactiveBackgroundColor = Background;
            }
            SetCaptionColor();
            new UISettings().ColorValuesChanged += delegate
            {
                SetCaptionColor();
            };

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            Window.Current.SetTitleBar(AppTitleBar);

            void UpdateTitleBarLayout(CoreApplicationViewTitleBar cTitleBar)
            {
                EntireTitleBar.Height = cTitleBar.Height;
                Thickness currMargin = AppTitleBar.Margin;
                AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, cTitleBar.SystemOverlayRightInset, currMargin.Bottom);
            }
            UpdateTitleBarLayout(coreTitleBar);

            coreTitleBar.LayoutMetricsChanged += (o, e) => UpdateTitleBarLayout(o);

            coreTitleBar.IsVisibleChanged += (o, e) => AppTitleBar.Visibility = o.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
