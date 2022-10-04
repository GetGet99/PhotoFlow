using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace PhotoFlow.CommandButton.Controls;

public abstract class CommandButtonCommandBar : StackPanel
{
    public CommandButtonCommandBar()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        Orientation = Orientation.Horizontal;
        Padding = new Thickness(10);
        Height = 60;
    }
}
