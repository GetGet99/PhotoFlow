#nullable enable
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotoFlow;

internal class ReverseStackPanel : StackPanel
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        double YValue = 0;

        foreach (UIElement child in Reverse(Children))
        {
            var desiredHeight = child.DesiredSize.Height;
            Point anchorPoint = new(0, YValue);
            child.Arrange(new Rect(anchorPoint, new Size(finalSize.Width, desiredHeight)));
            YValue += desiredHeight;
        }
        return finalSize;
    }
    static IEnumerable<UIElement> Reverse(UIElementCollection collection)
    {
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            yield return collection[i];
        }
    }
}
