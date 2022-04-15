using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Hosting;
using Windows.UI.WindowManagement;
using Windows.UI;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PhotoEditing
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            SetValue(Microsoft.UI.Xaml.Controls.BackdropMaterial.ApplyToRootOrPageBackgroundProperty, true);
            InitializeComponent();
            SetUpTitleBar();
            InitializeCommandButtons();
            ImplementZoomFunctionality();
            ImplementingLayersThing();
            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (o, e) =>
            {
                var deferral = e.GetDeferral();
                var dialog = new ThemeContentDialog
                {
                    Title = "Warning",
                    Content = "Are you sure you want to quit?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No"
                };
                if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                {
                    //cancel close by handling the event
                    e.Handled = true;
                }
                deferral.Complete();

            };
        }
        
        

        private void Invert(object sender, RoutedEventArgs e)
        {
            var layer = LayerContainer.Selection;
            if (layer.LayerType == Layer.Types.Mat)
            {
                var matLayer = (Layer.MatLayer)layer;
                matLayer.Mat.Invert(InPlace: true);
                matLayer.UpdateImage();
            }
        }

        private async void ReloadWindow(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            Frame.Navigate(GetType());
        }
        private async void NewWindow(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            //Frame.Navigate(GetType());
            var appWindow = await AppWindow.TryCreateAsync();
            var titleBar = appWindow.TitleBar;
            var Background = Colors.Transparent;
            var Foreground = Colors.White;
            titleBar.ButtonBackgroundColor = Background;
            titleBar.ButtonInactiveBackgroundColor = Background;
            titleBar.ButtonForegroundColor = Foreground;
            titleBar.ButtonInactiveForegroundColor = Foreground;
            titleBar.ExtendsContentIntoTitleBar = true;
            Frame appWindowContentFrame = new Frame();
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            await appWindow.TryShowAsync();
            await Task.Delay(2000);
            appWindowContentFrame.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Transparent);
            appWindowContentFrame.Navigate(GetType());
            //appWindowContentFrame
        }

        
    }
    
}