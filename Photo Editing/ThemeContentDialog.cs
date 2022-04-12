using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
namespace PhotoEditing
{
    public class ThemeContentDialog : ContentDialog
    {
        public ThemeContentDialog()
        {
            Background = Constants.DefaultAcrylicBackground;
        }
    }
    public static partial class Constants
    {
        public static AcrylicBrush DefaultAcrylicBackground => (AcrylicBrush)Windows.UI.Xaml.Application.Current.Resources["DefaultAcrylicBackground"];
    }
}
