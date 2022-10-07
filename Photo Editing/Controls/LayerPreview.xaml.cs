#nullable enable
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PhotoFlow;

public sealed partial class LayerPreview : IDisposable
{
    readonly Symbol Eye = (Symbol)0xe7b3;
    private readonly Layers.Layer Layer;
    public LayerPreview(Layers.Layer YourLayer)
    {
        Layer = YourLayer;
        InitializeComponent();
        VisibleButton.IsChecked = YourLayer.Visible;
        Image.Source = null;

        if (YourLayer is Layers.MatLayer)
            SendToPhotoToys.Visibility = Visibility.Visible;
        ButtonOverlay.Click += delegate {
            var target = Layer.SelectionIndexUpdateTarget;
            if (target is null) return;
            target.Value = target.Value == Layer.Index ? -1 : Layer.Index;
        };
    }
    public string LayerName { get => LayerNameTextBlock.Text; set => LayerNameTextBlock.Text = value; }
    public ImageSource? PreviewImage { get => Image.Source; set => Image.Source = value; }

    private async void Rename(object sender, RoutedEventArgs e)
    {
        RightClickCommand.Hide();
        var RenameDialog = new RenameDialog(Layer.LayerName);

        await RenameDialog.ShowAsync();
        if (RenameDialog.Success)
        {
            Layer.LayerName.Value = RenameDialog.NewName;
        }
    }
    private async void SaveLayerAsImage(object sender, RoutedEventArgs e)
    {
        RightClickCommand.Hide();
        Layer.DisablePreviewEffects();
        await FileManagement.SaveFile(await Layer.LayerUIElement.ToByteArrayImaegPngAsync() ?? Array.Empty<byte>());
        Layer.EnablePreviewEffects();
    }
    private void ToMatLayer(object sender, RoutedEventArgs e)
    {
        RightClickCommand.Hide();
        Layer.ConvertToMatLayerAsync();
    }
    private void Delete(object sender, RoutedEventArgs e)
    {
        RightClickCommand.Hide();
        Layer.DeleteSelf();
    }
    private async void Properties(object sender, RoutedEventArgs e)
    {
        RightClickCommand.Hide();
        await new ContentDialog
        {
            Title = "Layer Properties",
            Content = $"Name = {Layer.LayerName.Value}\nType = {Layer.LayerType}",
            PrimaryButtonText = "Okay",
            Background = Constants.DefaultAcrylicBackground
        }.ShowAsync();
    }
    public void Dispose()
    {
        
    }

    private void Duplicate(object _, RoutedEventArgs _1)
    {
        RightClickCommand.Hide();
        Layer.Duplicate();
    }

    private void Copy(object _, RoutedEventArgs _1)
    {
        RightClickCommand.Hide();
        Layer.CopyNoWait();
    }

    private void Cut(object _, RoutedEventArgs _1)
    {
        RightClickCommand.Hide();
        Layer.CutNoWait();
    }

    private async void Send2PhotoToys(object _, RoutedEventArgs _1)
    {
        if (Layer is Layers.MatLayer matLayer && matLayer.Mat is OpenCvSharp.Mat m)
        {
            await m.ImShow("Send To PhotoToys (Drag And Drop The Image)", XamlRoot);
        }
    }

    private void ShowLayer(object sender, RoutedEventArgs e)
    {
        Layer.Visible = true;
        RightClickCommand.Hide();
    }
    private void HideLayer(object sender, RoutedEventArgs e)
    {
        Layer.Visible = false;
        RightClickCommand.Hide();
    }
}
