#nullable enable
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace PhotoFlow;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        SetValue(Microsoft.UI.Xaml.Controls.BackdropMaterial.ApplyToRootOrPageBackgroundProperty, true);
        RotateCommand = new(x =>
        {
            LayerContaineCompositeTransformr.CenterX = LayerContainerBackground.ActualWidth / 2;
            LayerContaineCompositeTransformr.CenterY = LayerContainerBackground.ActualHeight / 2;
            LayerContaineCompositeTransformr.Rotation = Convert.ToDouble((string)x);
            UpdateLayerContainerSizeAndRotation();
        });
        InitializeComponent();
        SetUpTitleBar();
        InitializeCommandButtons();
        ImplementZoomFunctionality();
        ImplementingLayersThing();
        SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (o, e) =>
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



    private void Invert(object _, RoutedEventArgs _1)
    {
        var layer = LayerContainer.Selection;
        if (layer == null) return;
        if (layer.LayerType == Layer.Types.Mat)
        {
            var matLayer = (Layer.MatLayer)layer;
            matLayer.Mat?.Invert(InPlace: true);
            matLayer.UpdateImage();
        }
    }

    private async void ReloadWindow(object _, RoutedEventArgs _1)
    {
        await Task.Delay(100);
        Frame.Navigate(GetType());
    }
    private async void NewWindow(object _, RoutedEventArgs _1)
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
        Frame appWindowContentFrame = new();
        ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
        await appWindow.TryShowAsync();
        await Task.Delay(2000);
        appWindowContentFrame.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Transparent);
        appWindowContentFrame.Navigate(GetType());
        //appWindowContentFrame
    }

    private void Undo(object _, RoutedEventArgs _1)
    {
        LayerContainer.History.Undo();
    }

    private void Redo(object _, RoutedEventArgs _1)
    {
        LayerContainer.History.Redo();
    }
}

