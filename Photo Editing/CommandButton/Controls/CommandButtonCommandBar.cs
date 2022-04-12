using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PhotoEditing.CommandButton.Controls
{
    public class CommandButtonCommandBar : StackPanel
    {
        public CommandButtonCommandBar()
        {
            HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            Orientation = Orientation.Horizontal;
            Padding = new Windows.UI.Xaml.Thickness(10);
        }
    }
}
