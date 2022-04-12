using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotoEditing
{
    internal class ReverseStackPanel : StackPanel
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            double YValue = 0;
            //loop through each Child, call Arrange on each
            foreach (UIElement child in Reverse(Children))
            {
                var desiredHeight = child.DesiredSize.Height;
                Point anchorPoint = new Point(0, YValue);
                child.Arrange(new Rect(anchorPoint, new Size(finalSize.Width, desiredHeight)));
                YValue += desiredHeight;
            }
            return finalSize;//new Size(finalSize.Width, YValue); //OR, return a different Size, but that's rare
        }
        IEnumerable<UIElement> Reverse(UIElementCollection collection)
        {
            for (var i = collection.Count - 1; i >= 0; i--)
            {
                yield return collection[i];
            }
        }
    }
}
