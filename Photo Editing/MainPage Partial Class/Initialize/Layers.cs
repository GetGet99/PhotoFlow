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
using Windows.ApplicationModel.DataTransfer;
using Newtonsoft.Json.Linq;
using System.IO;

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
            LayerPanel.MoveLayerUp.Click += delegate
            {
                var idx = LayerContainer.SelectionIndex.Value;
                if (idx + 1 < Layers.Count) Layers.Move(idx, idx + 1);
            };
            LayerPanel.MoveLayerDown.Click += delegate
            {
                var idx = LayerContainer.SelectionIndex.Value;
                if (idx - 1 >= 0) Layers.Move(idx, idx - 1);
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

        private void Cut(object sender, RoutedEventArgs e)
        {
            var selection = LayerContainer.Selection;
            if (selection == null) return;
            var data = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Move
            };
            var dataasstring = selection.SaveData(OnMainThread: true).ToString();
            data.SetData("GPE", dataasstring);
            data.SetText(dataasstring);
            selection.DeleteSelf();
            Clipboard.SetContent(data);
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            var data = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            var selection = LayerContainer.Selection;
            if (selection == null) return;
            data.SetData("GPE", selection.SaveData(OnMainThread: true).ToString());
            Clipboard.SetContent(data);
        }

        private async void Paste(object sender, RoutedEventArgs e)
        {
            var data = Clipboard.GetContent();
            var layer = await data.ReadAsLayerAsync();
            if (layer != null) LayerContainer.AddNewLayer(layer);
        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            LayerContainer.Selection?.DeleteSelf();
        }
        private void Duplicate(object sender, RoutedEventArgs e)
        {
            var selection = LayerContainer.Selection;
            if (selection == null) return;
            LayerContainer.AddNewLayer(selection.DeepClone(OnMainThread: true));
        }
    }
    partial class Extension
    {
        public static async Task<T> GetAsAsync<T>(this DataPackageView dataObject, string format) => (T)(await dataObject.GetDataAsync(format));
        public static Task<MemoryStream> GetStreamAsync(this DataPackageView dataObject, string format) => dataObject.GetAsAsync<MemoryStream>(format);
        public static Task<string> GetStringAsync(this DataPackageView dataObject, string format) => dataObject.GetAsAsync<string>(format);
        public static async Task<Layer.Layer> ReadAsLayerAsync(this DataPackageView data)
        {
            System.Diagnostics.Debug.WriteLine(string.Join(", ", data.AvailableFormats));
            if (data.Contains("GPE"))
            {
                return LayerContainer.LoadLayer(JObject.Parse(await data.GetStringAsync("GPE")));
            }
            else if (data.Contains("PNG"))
            {
                var pngdata = (await data.GetAsAsync<Windows.Storage.Streams.IRandomAccessStream>("PNG")).AsStream();
                var bytes = new byte[pngdata.Length];
                pngdata.Read(bytes, 0, bytes.Length);
                var layer = new Layer.MatLayer(OpenCvSharp.Cv2.ImDecode(bytes, OpenCvSharp.ImreadModes.Unchanged));
                layer.LayerName.Value = "Pasted Image";
                return layer;
            }
            else if (data.Contains("Bitmap"))
            {
                var getbmp = await data.GetBitmapAsync();
                var openread = await getbmp.OpenReadAsync();
                var bmpdata = openread.AsStream();
                var bytes = new byte[bmpdata.Length];
                bmpdata.Read(bytes, 0, bytes.Length);
                var layer = new Layer.MatLayer(OpenCvSharp.Cv2.ImDecode(bytes, OpenCvSharp.ImreadModes.Unchanged));
                layer.LayerName.Value = "Pasted Image";
                return layer;
            }
            else return null;
        }
    }
}
