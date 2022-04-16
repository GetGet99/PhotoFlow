using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using CSUI;
using Windows.UI.Xaml;

namespace PhotoFlow.CommandButton.Controls
{
    public abstract class CommandButtonCommandBar : StackPanel
    {
        public CommandButtonCommandBar()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            Orientation = Orientation.Horizontal;
            Padding = new Thickness(10);
            Height = 60;
            //this.RegisterCSUIReload();
        }

        //protected override IEnumerable<UIElement> OnLoadUI()
        //{
        //    HorizontalAlignment = HorizontalAlignment.Left;
        //    Orientation = Orientation.Horizontal;
        //    Padding = new Thickness(10);
        //    return Array.Empty<UIElement>();
        //}
    }
}
