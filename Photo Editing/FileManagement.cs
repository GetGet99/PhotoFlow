#nullable enable
using OpenCvSharp;
using PhotoFlow.Layers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace PhotoFlow
{
    public partial class Constants
    {
        public const string GetPhotoEditingFileFormat = ".gpe";
    }
    static class FileManagement
    {
        public static async Task<Mat> OpenNewImageFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            ulong size = stream.Size;
            using var inputStream = stream.GetInputStreamAt(0);
            using var dataReader = new Windows.Storage.Streams.DataReader(inputStream);
            uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
            byte[] bytes = new byte[numBytesLoaded];
            dataReader.ReadBytes(bytes);
            return bytes.ToMat();
        }
        public static async Task<(string, byte[]?)> OpenFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gpe");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return ("<Canceled>", null);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            ulong size = stream.Size;
            using var inputStream = stream.GetInputStreamAt(0);
            using var dataReader = new Windows.Storage.Streams.DataReader(inputStream);
            uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
            byte[] bytes = new byte[numBytesLoaded];
            dataReader.ReadBytes(bytes);


            return (file.FileType, bytes);
        }
        public static async Task<bool> ExportLayer(Layer Layer)
        {
            Layer.DisablePreviewEffects();
            if (Layer is InkingLayer inkingLayer) return await ExportInkingLayer(inkingLayer);
            var @out = await SaveFile(await Layer.LayerUIElement.ToByteArrayImaegPngAsync() ?? Array.Empty<byte>());
            Layer.EnablePreviewEffects();
            return @out;
        }
        static async Task<bool> ExportInkingLayer(InkingLayer Layer)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            picker.FileTypeChoices.Add("SVG", new List<string>() { ".svg" });
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file == null) return false;
            else
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                switch (file.FileType)
                {
                    case ".png":
                        await Windows.Storage.FileIO.WriteBytesAsync(file, await Layer.LayerUIElement.ToByteArrayImaegPngAsync());
                        break;
                    case ".svg":
                        var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                        stream.Size = 0;
                        await Ink2Svg.ConvertInkToSVG(Layer.InkCanvas.InkPresenter, (float)Layer.Width, (float)Layer.Height).SaveAsync(stream);
                        stream.Dispose();
                        break;
                }
                GC.Collect();
                return true;
            }
        }
        public static async Task<bool> SaveFile(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out var bytes);
            mat.Dispose();
            return await SaveFile(bytes);
        }
        public static async Task<bool> SaveFile(byte[] bytes)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file == null) return false;
            else
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
                GC.Collect();
                return true;
            }
        }
        public static async Task<bool> SaveFile(LayerContainer LayerContainer, Windows.UI.Xaml.Controls.ScrollViewer LayerContainerMasker, Windows.UI.Xaml.Controls.Border LayerContainerBackground)
        {
            bool ToReturn;
            var background = LayerContainer.Background;
            LayerContainer.Background = null;
            var padding = LayerContainer.PaddingPixel;
            LayerContainer.PaddingPixel = 0;
            var bg = LayerContainerBackground.Background;
            LayerContainerBackground.Background = null;

            foreach (var Layer in LayerContainer.Layers) Layer.DisablePreviewEffects();

            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeChoices.Add("Get Photo Editing", new List<string>() { Constants.GetPhotoEditingFileFormat });
            picker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                ToReturn = false;
                goto End;
            }
            else
            {
                switch (file.FileType)
                {
                    case ".png":
                        Windows.Storage.CachedFileManager.DeferUpdates(file);
                        await Windows.Storage.FileIO.WriteBytesAsync(file, await LayerContainerMasker.ToByteArrayImaegPngAsync() ?? Array.Empty<byte>());
                        GC.Collect();
                        ToReturn = true;
                        goto End;
                    case ".gpe":
                        Windows.Storage.CachedFileManager.DeferUpdates(file);
                        await Windows.Storage.FileIO.WriteBytesAsync(file, Encoding.UTF8.GetBytes((await LayerContainer.Save()).ToString()));
                        GC.Collect();
                        ToReturn = true;
                        goto End;
                    default:
                        ToReturn = false;
                        goto End;
                }
            }

        End:
            foreach (var Layer in LayerContainer.Layers) Layer.EnablePreviewEffects();
            LayerContainerBackground.Background = bg;
            LayerContainer.Background = background;
            LayerContainer.PaddingPixel = padding;
            return ToReturn;
        }
    }
}
