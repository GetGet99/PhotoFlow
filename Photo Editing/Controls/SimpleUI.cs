#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using Size = Windows.Foundation.Size;
using Rect = Windows.Foundation.Rect;

namespace PhotoFlow;

static class SimpleUI
{
    public class FluentVerticalStack : Panel
    {
        public FluentVerticalStack(int ElementPadding = 16)
        {
            this.ElementPadding = ElementPadding;
        }
        public int ElementPadding { get; }
        protected override Size MeasureOverride(Size availableSize)
        {
            double UsedHeight = 0;
            foreach (var child in Children)
            {
                if (child.Visibility == Visibility.Collapsed) continue;
                child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                UsedHeight += child.DesiredSize.Height + ElementPadding;
            }
            UsedHeight -= ElementPadding;
            return new Size(availableSize.Width, Math.Max(UsedHeight, 0));
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double UsedHeight = 0;
            foreach (var child in Children)
            {
                if (child.Visibility == Visibility.Collapsed) continue;
                child.Measure(new Size(finalSize.Width, double.PositiveInfinity)); // finalSize.Height - UsedHeight
                child.Arrange(new Rect(0, UsedHeight, finalSize.Width, child.DesiredSize.Height));
                UsedHeight += child.DesiredSize.Height + ElementPadding;
            }
            UsedHeight -= ElementPadding;
            return new Size(finalSize.Width, Math.Max(UsedHeight, 0));
        }
    }
}