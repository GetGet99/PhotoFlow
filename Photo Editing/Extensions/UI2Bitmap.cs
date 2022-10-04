#nullable enable
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoFlow;

public partial class Extension
{
    public static async Task<RenderTargetBitmap?> ToRenderTargetBitmapAsync(this UIElement element)
    {
        try
        {
            RenderTargetBitmap rtb = new();
            System.Numerics.Vector2 ActualSize = new();
            await element.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var scale = element.Scale;
                var rotation = element.Rotation;
                var center = element.CenterPoint;
                ActualSize = element.ActualSize;
                //await rtb.RenderAsync(element);
                //await rtb.RenderAsync(element, (int)element.ActualSize.X, (int)element.ActualSize.Y);
            });
            if (ActualSize.X == 0 || ActualSize.Y == 0) return null;
            await rtb.RenderAsync(element);
            return rtb;
        } catch (Exception ex)
        {
            System.Diagnostics.Debugger.Break();
            throw ex;
        }
        
    }
    public static async Task<byte[]?> ToByteArrayImaegPngAsync(this UIElement element)
    {
        var displayInformation = DisplayInformation.GetForCurrentView();

        var rtb = await element.ToRenderTargetBitmapAsync();
        if (rtb == null) return null;

        var pixels = (await rtb.GetPixelsAsync()).ToArray();
        
        var ms = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms);

        encoder.SetPixelData(
            pixelFormat: BitmapPixelFormat.Bgra8,
            alphaMode: BitmapAlphaMode.Straight,
            width: (uint)rtb.PixelWidth,
            height: (uint)rtb.PixelHeight,
            dpiX: displayInformation.RawDpiX,
            dpiY: displayInformation.RawDpiY,
            pixels: pixels);

        await encoder.FlushAsync();

        byte[] bytes = new byte[ms.Size];
        await ms.AsStream().ReadAsync(bytes, 0, bytes.Length);
        ms.Dispose();

        return bytes;
    }
    public static async Task<Mat> ToMatAsync(this UIElement element) => Cv2.ImDecode(await element.ToByteArrayImaegPngAsync() ?? new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x1, 0x0, 0x1, 0x0, 0x80, 0x0, 0x0, 0xff, 0xff, 0xff, 0x0, 0x0, 0x0, 0x2c, 0x0, 0x0, 0x0, 0x0, 0x1, 0x0, 0x1, 0x0, 0x0, 0x2, 0x2, 0x44, 0x1, 0x0, 0x3b }, ImreadModes.Unchanged);
}
