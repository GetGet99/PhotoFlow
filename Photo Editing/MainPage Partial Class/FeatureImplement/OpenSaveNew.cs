using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.Foundation;
using Newtonsoft.Json.Linq;
namespace PhotoEditing
{
    partial class MainPage
    {
        async void OpenFile(object sender, RoutedEventArgs e)
        {
            var (fileType, bytes) = await FileManagement.OpenFile();
            switch (fileType)
            {
                case "<Canceled>":
                    break;
                case Constants.GetPhotoEditingFileFormat:
                    await LayerContainer.LoadAndReplace(JObject.Parse(Encoding.UTF8.GetString(bytes)));
                    break;
                default:
                    SetNewImage(bytes.ToMat());
                    break;
            }
        }
        void SaveFile(object sender, RoutedEventArgs e)
        {
            _ = FileManagement.SaveFile(LayerContainer, LayerContainerMasker, LayerContainerBackground);
        }
        private async void InsertFile(object sender, RoutedEventArgs e)
        {
            var mat = await FileManagement.OpenNewImageFile();
            var layer = new Layer.MatLayer(mat);
            layer.LayerName.Value = "Inserted Image";
            LayerContainer.AddNewLayer(layer);
        }

        async void New(object sender, RoutedEventArgs e)
        {
            var dialog = new NewDialog();
            await dialog.ShowAsync();
            if (!dialog.Success) return;
            var width = dialog.ImageWidth;
            var height = dialog.ImageHeight;
            var CanvasDimension = new OpenCvSharp.Size(width, height);
            OpenCvSharp.Scalar Color;
            switch (dialog.InitBackground)
            {
                case "Transparent":
                    Color = new OpenCvSharp.Scalar(0, 0, 0, 0);
                    break;
                case "White":
                    Color = new OpenCvSharp.Scalar(255, 255, 255, 255);
                    break;
                case "Black":
                    Color = new OpenCvSharp.Scalar(0, 0, 0, 255);
                    break;
                default:
                    throw new Exception("impossible!");
            }

            NewImage(Color, CanvasDimension);
        }
        void NewImage(OpenCvSharp.Scalar Color, OpenCvSharp.Size CanvasDimension)
        {
            var a = new OpenCvSharp.Mat(CanvasDimension, OpenCvSharp.MatType.CV_8UC4);
            a.SetTo(Color);
            SetNewImage(a);
        }
        void SetNewImage(OpenCvSharp.Mat Image)
        {
            if (Image.Channels() == 3) Image = Image.CvtColor(OpenCvSharp.ColorConversionCodes.BGR2BGRA);

            var CvSize = new OpenCvSharp.Size(Image.Width, Image.Height);
            CanvasDimension = CvSize;
            
            Size Size = new Size(CanvasDimension.Width, CanvasDimension.Height);
            LayerContainer.ImageSize = Size;

            LayerContainer.Clear();
            //LayerContainer.AddNewLayer(new Layer.BackgroundLayer(CanvasDimension));
            var layer = new Layer.MatLayer(Image);
            layer.LayerName.Value = "Background";
            LayerContainer.AddNewLayer(layer);
        }
    }
}
