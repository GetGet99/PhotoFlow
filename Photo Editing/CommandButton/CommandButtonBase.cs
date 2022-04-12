using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using PhotoEditing.CommandButton.Controls;
namespace PhotoEditing
{
    public abstract class CommandButtonBase
    {
        public Symbol Icon { get; private set; }
        protected Size CanvasSize => new Size(LayerContainer.Width, LayerContainer.Height);
        protected Size CanvasPadding => new Size(LayerContainer.PaddingPixel, LayerContainer.PaddingPixel);
        protected readonly Border CommandBarPlace;
        protected Layer.Layer CurrentLayer { get; private set; }
        protected abstract CommandButtonCommandBar CommandBar { get; }
        public void SetLayerContainer(LayerContainer LayerContainer)
        {
            this.LayerContainer = LayerContainer;
        }
        protected LayerContainer LayerContainer { get; private set; }
        public CommandButtonBase(Symbol Icon, Border CommandBarPlace)
        {
            this.CommandBarPlace = CommandBarPlace;
            this.Icon = Icon;
        }
        public void Select() => Selected();
        protected virtual void Selected()
        {
            CommandBarPlace.Child = CommandBar;
            InvokeLayerChange();
        }
        public void Deselect() => Deselected();
        protected virtual void Deselected()
        {
            CommandBarPlace.Child = null;
            if (CurrentLayer != null) {
                RequestRemoveLayerEvent(CurrentLayer);
                CurrentLayer = null;
            }
        }
        protected virtual void RequestRemoveLayerEvent(Layer.Layer Layer)
        {

        }
        protected virtual void RequestAddLayerEvent(Layer.Layer Layer)
        {

        }
        public void InvokeLayerChange() => LayerChanged(LayerContainer.Selection);
        protected virtual void LayerChanged(Layer.Layer Layer)
        {
            if (CurrentLayer != null) RequestRemoveLayerEvent(CurrentLayer);
            CurrentLayer = Layer;
            if (CurrentLayer != null) RequestAddLayerEvent(Layer);
        }
        protected void AddNewLayer(Layer.Layer Layer)
        {
            LayerContainer.AddNewLayer(Layer);
        }
    }
}
