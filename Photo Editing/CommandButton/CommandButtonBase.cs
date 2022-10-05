#nullable enable
using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using PhotoFlow.CommandButton.Controls;
namespace PhotoFlow;

public abstract class CommandButtonBase
{
    public Symbol Icon { get; private set; }
    protected Size CanvasSize => new(LayerContainer.Width, LayerContainer.Height);
    protected Size CanvasPadding => new(LayerContainer.PaddingPixel, LayerContainer.PaddingPixel);
    protected readonly ScrollViewer CommandBarPlace;
    protected Layer.Layer? CurrentLayer { get; private set; }
    protected float? ZoomFactor => ScrollViewer?.ZoomFactor;
    protected abstract CommandButtonCommandBar CommandBar { get; }
    public void SetLayerContainer(LayerContainer LayerContainer)
        => this.LayerContainer = LayerContainer;
    public void SetScrollViewer(ScrollViewer ScrollViewer)
        => this.ScrollViewer = ScrollViewer;
    protected LayerContainer LayerContainer { get; private set; }
    protected ScrollViewer ScrollViewer { get; private set; }
    public CommandButtonBase(Symbol Icon, ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer)
    {
        this.CommandBarPlace = CommandBarPlace;
        this.Icon = Icon;
        this.LayerContainer = LayerContainer;
        this.ScrollViewer = MainScrollViewer;
    }
    public void Select() => Selected();
    protected virtual void Selected()
    {
        CommandBarPlace.Content = CommandBar;
        InvokeLayerChange();
    }
    public void Deselect() => Deselected();
    protected virtual void Deselected()
    {
        CommandBarPlace.Content = null;
        if (CurrentLayer != null)
        {
            RequestRemoveLayerEvent(CurrentLayer);
            CurrentLayer = null;
        }
    }
    protected virtual void RequestRemoveLayerEvent(Layer.Layer Layer) { }
    protected virtual void RequestAddLayerEvent(Layer.Layer Layer) { }
    public void InvokeLayerChange() => LayerChanged(LayerContainer.Selection);
    protected virtual void LayerChanged(Layer.Layer? Layer)
    {
        if (CurrentLayer != null) RequestRemoveLayerEvent(CurrentLayer);
        CurrentLayer = Layer;
        if (Layer != null) RequestAddLayerEvent(Layer);
    }
    protected void AddNewLayer(Layer.Layer Layer)
        => LayerContainer.AddNewLayer(Layer);
}
