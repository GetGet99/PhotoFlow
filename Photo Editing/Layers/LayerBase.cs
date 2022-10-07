#nullable enable
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage.Streams;
using System.IO;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.ViewManagement;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using Cv2 = OpenCvSharp.Cv2;

namespace PhotoFlow.Layers;

public enum Types
{
    Background,
    Mat,
    Inking,
    Text,
    RectangleShape,
    EllipseShape
}
public interface ILayerTyping
{
    Types LayerType { get; }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class RunOnUIThreadAttribute : Attribute
{

}

public record struct LayerTransform(double X, double Y, double CenterX, double CenterY, double Rotation, double ScaleX, double ScaleY, double Width, double Height)
{

}

public abstract class Layer : ISaveable, ILayerTyping, IDisposable, INotifyPropertyChanged
{
    private static uint NextId = 0;
    public uint LayerId { get; private set; }
    protected static readonly UISettings UISettings = new();
    public Grid LayerUIElement { get; private set; }
    public History? History => LayerContainer?.History;
    static Layer()
    {
        static void UpdateBorderColor()
        {
            if (UISettings.GetColorValue(UIColorType.Background).R < 255 / 2)
            {
                // Dark Mode
                BorderColor.Color = UISettings.GetColorValue(UIColorType.AccentLight2);
            }
            else
            {
                // Light Mode
                BorderColor.Color = UISettings.GetColorValue(UIColorType.AccentDark2);
            }
        }
        UpdateBorderColor();
        UISettings.ColorValuesChanged += delegate
        {
            UpdateBorderColor();
        };
    }
    protected static readonly SolidColorBrush BorderColor = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public abstract Types LayerType { get; }
    public VariableUpdateAlert<string> LayerName { get; } = new("Unnamed Layer");
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public bool Visible
    {
        get => LayerUIElement.Visibility == Visibility.Visible;
        set
        {
            LayerUIElement.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            NotifyPropertyChanged();
        }
    }
    public double CenterX
    {
        get => RenderTransform.CenterX;
        set
        {
            RenderTransform.CenterX = value;
            NotifyPropertyChanged();
        }
    }
    public double CenterY
    {
        get => RenderTransform.CenterY;
        set
        {
            RenderTransform.CenterY = value;
            NotifyPropertyChanged();
        }
    }
    public CompositeTransform RenderTransform => (CompositeTransform)LayerUIElement.RenderTransform;
    public double X
    {
        set => RenderTransform.TranslateX = value;
        get => RenderTransform.TranslateX;
    }
    public double Y
    {
        set => RenderTransform.TranslateY = value;
        get => RenderTransform.TranslateY;
    }
    public double Rotation
    {
        get => RenderTransform.Rotation;
        set => RenderTransform.Rotation = value;
    }
    public double ScaleX
    {
        get => RenderTransform.ScaleX;
        set => RenderTransform.ScaleX = value;
    }
    public double ScaleY
    {
        get => RenderTransform.ScaleY;
        set => RenderTransform.ScaleY = value;
    }
    public double Width
    {
        get => LayerUIElement.Width;//(LayerUIElement.Children[0] as FrameworkElement).Width;
        set
        {
            LayerUIElement.Width = value;
            //foreach (var child in LayerUIElement.Children)
            //    if (child is FrameworkElement element)
            //        element.Width = value;
        }
    }
    public double Height
    {
        get => LayerUIElement.Height;//(LayerUIElement.Children[0] as FrameworkElement).Height;
        set
        {
            LayerUIElement.Height = value;
            //foreach (var child in LayerUIElement.Children)
            //    if (child is FrameworkElement element)
            //        element.Height = value;
        }
    }
    public LayerTransform Transform
    {
        get => new(X, Y, CenterX, CenterY, Rotation, ScaleX, ScaleY, Width, Height);
        set => (X, Y, CenterX, CenterY, Rotation, ScaleX, ScaleY, Width, Height) = value;
    }
    public JObject SaveData(bool Runtime = false)
    {
        var save = OnDataSaving();
        double X = 0, Y = 0,
            Width = 0, Height = 0,
            Rotation = 0,
            ScaleX = 0, ScaleY = 0,
            CenterX = 0, CenterY = 0;
        bool Visible = true;
        Extension.RunOnUIThread(delegate
        {
            X = this.X;
            Y = this.Y;
            Width = this.Width;
            Height = this.Height;
            Rotation = this.Rotation;
            ScaleX = this.ScaleX;
            ScaleY = this.ScaleY;
            CenterX = this.CenterX;
            CenterY = this.CenterY;
            Visible = this.Visible;
        });
        return new JObject(
            new JProperty("LayerName", LayerName.Value),
            new JProperty("LayerType", LayerType),
            new JProperty("CenterPoint", new double[] { CenterX, CenterY }),
            new JProperty("X", X),
            new JProperty("Y", Y),
            new JProperty("Width", Width),
            new JProperty("Height", Height),
            new JProperty("Rotation", Rotation),
            new JProperty("Scale", new double[] { ScaleX, ScaleY }),
            new JProperty("Visible", Visible),
            new JProperty("Runtime", LayerId),
            new JProperty("AdditionalData", save)
        );
    }

    public void LoadData(JObject json, bool Runtime = false)
    {
        LayerName.Value = json["LayerName"]?.ToObject<string>() ?? "Unnamed Layer";
        var CenterPoint = json["CenterPoint"]?.ToObject<double[]>();
        var Scale = json["Scale"]?.ToObject<double[]>();

        var X = json["X"]?.ToObject<double>();
        var Y = json["Y"]?.ToObject<double>();
        var Rotation = json["Rotation"]?.ToObject<double>();
        var Width = json["Width"]?.ToObject<double>();
        var Height = json["Height"]?.ToObject<double>();
        var Visible = json["Visible"]?.ToObject<bool>();
        if (Runtime)
        {
            var RuntimeValue = json["Runtime"]?.ToObject<uint>();
            if (RuntimeValue is uint val) LayerId = val;
        }
        var Task = Extension.RunOnUIThreadAsync(() =>
        {
            if (CenterPoint != null)
            {
                CenterX = CenterPoint[0];
                CenterY = CenterPoint[1];
            }
            if (X != null) this.X = X.Value;
            if (Y != null) this.Y = Y.Value;
            if (Rotation != null) this.Rotation = Rotation.Value;
            if (Scale != null)
            {
                ScaleX = Scale[0];
                ScaleY = Scale[1];
            }
            if (Width != null) this.Width = Width.Value;
            if (Height != null) this.Height = Height.Value;
            if (Visible != null) this.Visible = Visible.Value;
        });

        var additionalData = json["AdditionalData"]?.ToObject<JObject>();
        if (additionalData != null) OnDataLoading(additionalData, Task);
        if (!Task.IsCompleted) Task.Wait();
    }

    public double ActualWidth => LayerUIElement.ActualWidth;
    public double ActualHeight => LayerUIElement.ActualHeight;


    public VariableUpdateAlert<int>? SelectionIndexUpdateTarget => LayerContainer?.SelectionIndex;
    public int Index { get; private set; }
    public void UpdateThisIndex(int newIndex)
    {
        Index = newIndex;
    }

    protected abstract JObject OnDataSaving();
    protected abstract void OnDataLoading(JObject storage, Task MainLoadingTask);

    public Control? Control { get; private set; }

    public LayerPreview LayerPreview { get; private set; }
    protected LayerContainer? LayerContainer => (LayerUIElement.Parent is LayerContainer L) ? L : null;

    public VariableUpdateAlert<bool> Selecting { get; } = new(false);


    protected Layer()
    {
        Init();
    }
    [MemberNotNull(nameof(LayerUIElement), nameof(LayerPreview))]
    void Init()
    {
        LayerId = NextId++;
        InitUIThread();
        LayerName.Update += (oldVal, newVal) =>
        {
            Extension.RunOnUIThread(() => LayerPreview.LayerName = newVal);
        };
        Selecting.Update += (oldVal, newVal) =>
        {
            Extension.RunOnUIThread(() =>
            {
                LayerUIElement.IsHitTestVisible = newVal;
            });
            if (newVal) Select(); else Deselect();
        };
    }
    [MemberNotNull(nameof(LayerUIElement), nameof(LayerPreview))]
    void InitUIThread()
    {
        Extension.RunOnUIThread(() =>
        {
            LayerUIElement = new Grid
            {
                BorderThickness = new Thickness(2),
                CanBeScrollAnchor = false,
                IsHitTestVisible = false,
                RenderTransform = new CompositeTransform(),
                Background = new SolidColorBrush(Colors.Transparent)
            };
            LayerPreview = new LayerPreview(this);
        });
#pragma warning disable CS8774
    }
#pragma warning restore CS8774
    protected abstract void OnCreate();
    protected void CompleteCreate()
    {
        UpdatePreview();
    }
    [RunOnUIThread]
    protected async Task UpdatePreviewAsync()
    {
        try
        {
            await Task.Run(
                () => Extension.RunOnUIThread(async delegate
                {
                    try
                    {
                        LayerPreview.PreviewImage = await LayerUIElement.ToRenderTargetBitmapAsync();
                    }
                    catch
                    {

                    }
                })
            );
        }
        catch
        {

        }
    }
    protected async void UpdatePreview()
    {
        await UpdatePreviewAsync();
    }

    protected virtual void SelectedIndexChange(int oldIdx, int newIdx) { }
    protected virtual void Select()
    {
        Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = BorderColor);
    }
    protected virtual void Deselect()
    {
        Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = null);
    }

    public virtual void DisablePreviewEffects() => Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = null);
    public virtual void EnablePreviewEffects()
    {
        if (Selecting) Extension.RunOnUIThread(() => LayerUIElement.BorderBrush = BorderColor);
    }

    protected void ReplaceSelf(Layer NewLayer)
    {
        var lc = LayerContainer;
        if (lc != null)
        {
            var index = lc.Layers.IndexOf(this);
            if (index != -1) lc.Layers[index] = NewLayer;
            Dispose();
        }
    }
    protected void RemoveSelf()
    {
        var lc = LayerContainer;
        if (lc != null)
        {
            lc.Layers.Remove(this);
            Dispose();
        }
    }
    public void DeleteSelf() => RemoveSelf();
    public void Duplicate() => LayerContainer?.AddNewLayer(this.DeepClone());
    async Task CopyAsync(DataPackageOperation Operation)
    {
        var data = new DataPackage
        {
            RequestedOperation = Operation
        };
        data.SetData("GPE", SaveData(Runtime: true).ToString());
        DisablePreviewEffects();
        var mat = await LayerUIElement.ToMatAsync();
        EnablePreviewEffects();
        Cv2.ImEncode(".png", mat, out var bytes);
        var ms = new MemoryStream(bytes);
        var memref = RandomAccessStreamReference.CreateFromStream(ms.AsRandomAccessStream());
        data.SetData("PNG", ms.AsRandomAccessStream());
        data.SetBitmap(memref);
        GC.KeepAlive(ms);

        Clipboard.SetContent(data);

        void HistoryChanged(object _, ClipboardHistoryChangedEventArgs e)
        {
            ms.Dispose();
        }
        Clipboard.HistoryChanged += HistoryChanged;
    }
    public async Task CopyAsync()
    {
        if (RequestCopy()) return;
        await CopyAsync(DataPackageOperation.Copy);
    }
    public async void CopyNoWait() => await CopyAsync();
    public async Task CutAsync()
    {
        if (RequestCut()) return;
        await CopyAsync(DataPackageOperation.Move);
        DeleteSelf();
    }
    public async void CutNoWait() => await CutAsync();
    public async void ConvertToMatLayerAsync()
    {
        ReplaceSelf(await ConvertToMatLayerAsync(this));
    }
    public static async Task<MatLayer> ConvertToMatLayerAsync(Layer layer)
    {
        layer.DisablePreviewEffects();
        var Toreturn = new MatLayer(await layer.LayerUIElement.ToMatAsync())
        {
            X = layer.X,
            Y = layer.Y
        };
        layer.EnablePreviewEffects();
        return Toreturn;
    }
    protected virtual bool RequestCut() => false;
    protected virtual bool RequestCopy() => false;
    public virtual bool RequestPaste() => false;
    public virtual bool RequestDuplicate() => false;
    public virtual bool RequestDelete() => false;

    public abstract void Dispose();
    public override string ToString()
    {
        return $"[{GetType().Name}] Id = {LayerId}";
    }
}
