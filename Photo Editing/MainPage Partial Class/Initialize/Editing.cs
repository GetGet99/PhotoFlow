#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoFlow
{
    partial class MainPage
    {
        Size CanvasDimension;

    }
    public static partial class Extension
    {
        const bool InPlaceDefault = false;
        [return: NotNullIfNotNull(nameof(byteArray))]
        public static BitmapImage? AsBitmapImage(this byte[]? byteArray)
        {
            if (byteArray == null) return null;

            using var stream = new InMemoryRandomAccessStream();
            stream.WriteAsync(byteArray.AsBuffer()).GetResults();
            
            var image = new BitmapImage();
            stream.Seek(0);
            image.SetSource(stream);
            return image;
        }
        public static Mat Merge(this Mat[] Mats, bool DisposeEverything = false)
        {
            Mat mat = new();
            Cv2.Merge(Mats, mat);
            if (DisposeEverything) Mats.DisposeAndDelete();
            return mat;
        }
        public static void DisposeAndDelete(this ICollection<Mat> Mats)
        {
            IEnumerator<Mat> en = Mats.GetEnumerator();
            while (en.MoveNext())
            {
                en.Current.Dispose();
            }
            if (!Mats.IsReadOnly) Mats.Clear();
        }
        static Mat InPlaceOperations(this Mat oldMat, Mat newMat, bool InPlace = InPlaceDefault)
        {
            if (InPlace)
            {
                newMat.CopyTo(oldMat);
                newMat.Dispose();
                return oldMat;
            }
            else return newMat;
        }

        public static Mat Invert(this Mat m, bool InPlace = InPlaceDefault)
        {
            return m.InPlaceOperations(new Mat[] { 255 - m.ExtractChannel(0), 255 - m.ExtractChannel(1), 255 - m.ExtractChannel(2), m.ExtractChannel(3) }.Merge(DisposeEverything: true), InPlace: InPlace);
        }
        public static BitmapImage ToBitmapImage(this Mat m, bool DisposeMat = false)
        {

            BitmapImage bmp = m.ToBytes().AsBitmapImage();

            if (DisposeMat) m.Dispose();

            return bmp;
        }
    }
}
