#nullable enable
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
using Windows.UI.Xaml.Input;
using PhotoFlow.Layers;

namespace PhotoFlow
{
    partial class Constants
    {
        public static readonly Brush LayerFillColorDefaultBrush = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
        public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    }
    partial class MainPage
    {
        readonly CompositeTransform LayerContaineCompositeTransformr = new();
        void UpdateLayerContainerSizeAndRotation()
        {
            if (LayerContaineCompositeTransformr.Rotation > 360) LayerContaineCompositeTransformr.Rotation -= 360;
            else if (LayerContaineCompositeTransformr.Rotation < 0) LayerContaineCompositeTransformr.Rotation += 360;
            const double twoPiOver360 = 2 * Math.PI / 360;
            var rot = LayerContaineCompositeTransformr.Rotation;
            if (rot > 180) rot = 360 - rot;
            if (rot > 90) rot = 180 - rot;
            rot *= twoPiOver360;
            var sin = Math.Sin(rot);
            var cos = Math.Cos(rot);
            var ow = LayerContainer.Width;
            var oh = LayerContainer.Height;
            LayerContainerSizeMaintainer.Width = ow * cos + oh * sin;
            LayerContainerSizeMaintainer.Height = ow * sin + oh * cos;
            var rotdeg = LayerContaineCompositeTransformr.Rotation;
            RotationText.Value = (rotdeg > 180 ? rotdeg - 360 : rotdeg);
        }
        void ImplementingLayersThing()
        {
            LayerContainer.SetScrollViewer(MainScrollView);
            
            LayerContainer.SizeUpdate += () =>
            {
                LayerContainerMasker.Width = LayerContainer.Width;
                LayerContainerMasker.Height = LayerContainer.Height;
                UpdateLayerContainerSizeAndRotation();
            };
            LayerContainer.SelectionUpdate += (_, idx) =>
                {
                    if (idx == -1)
                    {
                        LayerContainerBackground.ManipulationMode = ManipulationModes.Rotate | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia | ManipulationModes.Scale | ManipulationModes.ScaleInertia;
                    }
                    else
                    {
                        LayerContainerBackground.ManipulationMode = ManipulationModes.System;
                    }
                };
            LayerContainerBackground.RenderTransform = LayerContaineCompositeTransformr;
            LayerContainerBackground.ManipulationDelta += (_, e) =>
            {
                var tran = e.Delta.Translation;
                MainScrollView.ChangeView(MainScrollView.HorizontalOffset - tran.X, MainScrollView.VerticalOffset - tran.Y, ZoomFactor * e.Delta.Scale);
                LayerContaineCompositeTransformr.CenterX = LayerContainerBackground.ActualWidth / 2;
                LayerContaineCompositeTransformr.CenterY = LayerContainerBackground.ActualHeight / 2;
                LayerContaineCompositeTransformr.Rotation += e.Delta.Rotation;
                if (LayerContaineCompositeTransformr.Rotation > 360) LayerContaineCompositeTransformr.Rotation -= 360;
                else if (LayerContaineCompositeTransformr.Rotation < 0) LayerContaineCompositeTransformr.Rotation += 360;
                e.Handled = true;
                UpdateLayerContainerSizeAndRotation();
            };
            var LayerPreviewPanel = new ReverseStackPanel();
            LayerPanel.PropertiesPane.Content = LayerPreviewPanel;


            var Layers = LayerContainer.Layers;
            void FinalizeUpdate()
            {
                foreach (var (i, layer) in Layers.Enumerate())
                    layer.LayerPreview.Background = Constants.TransparentBrush;
                LayerContainer.SelectionIndex.InvokeUpdate();
            }
            void Layers_Cleared(Layers.Layer[] Values)
            {
                Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Clear());
                FinalizeUpdate();
            }

            void Layers_Replaced(int Index, Layers.Layer OldItem, Layers.Layer NewItem)
            {
                Extension.RunOnUIThread(() => LayerPreviewPanel.Children[Index] = Layers[Index].LayerPreview);
                FinalizeUpdate();
            }

            void Layers_Moved(int Index1, int Index2, Layers.Layer Item1, Layers.Layer Item2)
            {
                Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Move((uint)Index1, (uint)Index2));
                FinalizeUpdate();
            }

            void Layers_Removed(int Index, Layers.Layer Item)
            {
                Extension.RunOnUIThread(() => LayerPreviewPanel.Children.RemoveAt(Index));
                FinalizeUpdate();
            }

            void Layers_Added(int Index, Layers.Layer Item)
            {
                Extension.RunOnUIThread(() => LayerPreviewPanel.Children.Insert(Index, Item.LayerPreview));
                FinalizeUpdate();
            }

            Layers.Added += Layers_Added;
            Layers.Removed += Layers_Removed;
            Layers.Moved += Layers_Moved;
            Layers.Replaced += Layers_Replaced;
            Layers.Cleared += Layers_Cleared;
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
            selection.CutNoWait();
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            var selection = LayerContainer.Selection;
            if (selection == null) return;
            selection.CopyNoWait();
        }

        private async void Paste(object sender, RoutedEventArgs e)
        {
            if (LayerContainer.Selection is Layer Layer && Layer.RequestPaste()) 
                return;
            var data = Clipboard.GetContent();
            var layer = await data.ReadAsLayerAsync();
            if (layer != null)
            {
                LayerContainer.AddNewLayer(layer);
                layer.FinalizeLoad();
            }
        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            if (LayerContainer.Selection is not Layer Layer) return;
            if (Layer.RequestDelete()) return;
            LayerContainer.Selection?.DeleteSelf();
        }
        private void Duplicate(object sender, RoutedEventArgs e)
        {
            if (LayerContainer.Selection is not Layer Layer) return;
            if (Layer.RequestDuplicate()) return;
            Layer.Duplicate();
        }
    }
    partial class Extension
    {
        
        
        public static async Task<T> GetAsAsync<T>(this DataPackageView dataObject, string format) => (T)(await dataObject.GetDataAsync(format));
        public static Task<MemoryStream> GetStreamAsync(this DataPackageView dataObject, string format) => dataObject.GetAsAsync<MemoryStream>(format);
        public static Task<string> GetStringAsync(this DataPackageView dataObject, string format) => dataObject.GetAsAsync<string>(format);
        public static async Task<Layers.Layer?> ReadAsLayerAsync(this DataPackageView data)
        {
            System.Diagnostics.Debug.WriteLine(string.Join(", ", data.AvailableFormats));
            if (data.Contains("GPE"))
            {
                return LayerContainer.LoadLayer(JObject.Parse(await data.GetStringAsync("GPE")), Runtime: true);
            }
            else if (data.Contains("PNG"))
            {
                var pngdata = (await data.GetAsAsync<Windows.Storage.Streams.IRandomAccessStream>("PNG")).AsStream();
                var bytes = new byte[pngdata.Length];
                pngdata.Read(bytes, 0, bytes.Length);
                var layer = new Layers.MatLayer(OpenCvSharp.Cv2.ImDecode(bytes, OpenCvSharp.ImreadModes.Unchanged));
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
                var layer = new Layers.MatLayer(OpenCvSharp.Cv2.ImDecode(bytes, OpenCvSharp.ImreadModes.Unchanged));
                layer.LayerName.Value = "Pasted Image";
                return layer;
            }
            else return null;
        }
    }
}
