using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Collections.Specialized;

namespace PhotoEditing
{
    partial class Constants
    {
        public static readonly Brush LayerFillColorDefaultBrush = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
        public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    }
    partial class MainPage
    {
        void ImplementingLayersThing()
        {
            LayerContainer.SizeUpdate += () =>
            {
                LayerContainerMasker.Width = LayerContainer.Width;
                LayerContainerMasker.Height = LayerContainer.Height;
            };
            var LayerPreviewPanel = new ReverseStackPanel();
            LayerPanel.PropertiesPane.Content = LayerPreviewPanel;
            var Layers = LayerContainer.Layers;
            Layers.CollectionChanged += (o, e) =>
            {
                var newindex = e.NewStartingIndex;
                var oldindex = e.OldStartingIndex;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        var lp = Layers[newindex].LayerPreview;
                        Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Insert(newindex, lp));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        Extension.RunOnUIThread(() => LayerPreviewPanel.Children.RemoveAt(oldindex));
                        break;
                    case NotifyCollectionChangedAction.Move:
                        Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Move((uint)oldindex, (uint)newindex));
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        Extension.RunOnUIThread(() => LayerPreviewPanel.Children[oldindex] = Layers[oldindex].LayerPreview);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Clear());
                        break;
                }
                foreach (var (i, layer) in Layers.Enumerate())
                    layer.LayerPreview.Background = Constants.TransparentBrush;
                LayerContainer.SelectionIndex.InvokeUpdate();
            };
            LayerContainer.SelectionUpdate += (oldVal, newVal) =>
            {
                var newcount = LayerContainer.Children.Count;
                if (newcount != 0)
                {
                    if (oldVal != -1 && oldVal < newcount) Layers[oldVal].LayerPreview.Background = Constants.TransparentBrush;
                    if (newVal != -1 && newVal < newcount) Layers[newVal].LayerPreview.Background = Constants.LayerFillColorDefaultBrush;
                }
            };

            NewImage(Color.FromArgb(0, 0, 0, 0), new OpenCvSharp.Size(800, 450));
        }
    }
}
