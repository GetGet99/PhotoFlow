#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoFlow.CommandButton.Controls;

public sealed partial class Shape : CommandButtonCommandBar
{
    public readonly PropertiesButton PropertiesButton;
    public Shape(History History)
    {
        InitializeComponent();
        LayerEditorControls.Children.Add(PropertiesButton = new PropertiesButton(History)
        {
            Margin = new Thickness(10, 0, 0, 0)
        });
        ColorPicker.ColorChanged += delegate
        {
            ColorPickerButton.Background = new SolidColorBrush(ColorPicker.Color);
        };
        void ev(object _, RoutedEventArgs _1)
            => AcrylicEditor.Visibility = Acrylic.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
        Acrylic.Checked += ev;
        Acrylic.Unchecked += ev;
    }
}
