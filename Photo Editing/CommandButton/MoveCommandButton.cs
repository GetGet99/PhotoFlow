#nullable enable
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PhotoFlow.CommandButton.Controls;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using System.Diagnostics;
using PhotoFlow.Layers;
using Windows.UI.Xaml.Media;
namespace PhotoFlow
{
    public class MoveCommandButton : CommandButtonBase
    {
        private readonly Move MoveCommandBar = new();
        protected override CommandButtonCommandBar CommandBar => MoveCommandBar;

        public MoveCommandButton(ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.TouchPointer, CommandBarPlace, LayerContainer, MainScrollViewer)
        {
            var ilc = new LambdaCommand(InvokeLayerChange);
            MoveCommandBar.EnableMove.Content = new SymbolIcon((Symbol)0xe7c2);
            MoveCommandBar.EnableMove.Command = ilc;
            MoveCommandBar.EnableScale.Content = new SymbolIcon((Symbol)0xe740);
            MoveCommandBar.EnableScale.Command = ilc;
            MoveCommandBar.EnableResize.Content = new SymbolIcon((Symbol)0xe744);
            MoveCommandBar.EnableResize.Command = ilc;
            MoveCommandBar.EnableRotate.Content = new SymbolIcon(Symbol.Rotate);
            MoveCommandBar.EnableRotate.Command = ilc;

            void UpdateNumberFromTBEV(NumberBox _, NumberBoxValueChangedEventArgs _1)
                => UpdateNumberFromTB();
            MoveCommandBar.TB_X.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_Y.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_R.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_S.ValueChanged += UpdateNumberFromTBEV;

            LayerContainer.History.UndoCompleted += UpdateNumberFromValue;
            LayerContainer.History.RedoCompleted += UpdateNumberFromValue;
        }

        void UpdateNumberFromTB()
        {
            if (IsUpdatingNumberFromValue) return;
            var layer = CurrentLayer;
            if (layer == null) return;
            var oldTransform = layer.Transform;
            layer.X = MoveCommandBar.TB_X.Value;
            layer.Y = MoveCommandBar.TB_Y.Value;
            layer.CenterX = layer.ActualWidth / 2;
            layer.CenterY = layer.ActualHeight / 2;
            var scale = MoveCommandBar.TB_S.Value;
            layer.ScaleX = scale;
            layer.ScaleY = scale;
            layer.Rotation = MoveCommandBar.TB_R.Value;
            var newTransform = layer.Transform;
            RecordNewTransformAction(oldTransform, newTransform);
        }
        void RecordNewTransformAction(LayerTransform Old, LayerTransform New)
        {
            if (!CurrentLayer.IsNotNull(out var layer)) return;
            layer.NewHistoryAction(LayerContainer.History, new HistoryAction<(LayerTransform Old, LayerTransform New)>(
                (Old, New),
                Tag: this,
                Undo: x =>
                {
                    layer.Transform = x.Old;
                },
                Redo: x =>
                {
                    layer.Transform = x.New;
                }
            ));
        }
        bool IsUpdatingNumberFromValue = false;
        void UpdateNumberFromValue()
        {
            var layer = CurrentLayer;
            if (layer == null) return;
            IsUpdatingNumberFromValue = true;
            MoveCommandBar.TB_X.Value = layer.X;
            MoveCommandBar.TB_Y.Value = layer.Y;
            MoveCommandBar.TB_S.Value = (layer.ScaleX + layer.ScaleY) / 2;
            MoveCommandBar.TB_R.Value = layer.Rotation;
            IsUpdatingNumberFromValue = false;
        }
        bool ResizingXStart = false;
        bool ResizingYStart = false;
        bool ResizingXEnd = false;
        bool ResizingYEnd = false;
        LayerTransform StartManipulationTransform;
        private void ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!CurrentLayer.IsNotNull(out var layer)) return;
            StartManipulationTransform = layer.Transform;
            if ((MoveCommandBar.EnableResize.IsChecked ?? false))
            {
                var ZoomFactor = this.ZoomFactor;
                var pos = e.Position;
                var pixelThreshold = 30 / ZoomFactor;
                ResizingXStart = pos.X <= pixelThreshold;
                ResizingYStart = pos.Y <= pixelThreshold;
                ResizingXEnd = pos.X >= layer.Width - pixelThreshold;
                ResizingYEnd = pos.Y >= layer.Height - pixelThreshold;
            }
            else
            {
                ResizingXStart = false;
                ResizingYStart = false;
                ResizingXEnd = false;
                ResizingYEnd = false;
            }
        }
        private void ManipulationDeltaEvent(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var layer = CurrentLayer;
            if (layer == null) return;
            var ZoomFactor = this.ZoomFactor ?? 1;

            if (e.Container != null)
            {
                //var p = e.Container.TransformToVisual(layer.LayerUIElement).TransformPoint(new Windows.Foundation.Point(posX, posY)); //transform touch point position relative to this element
                //posX = p.X;
                //posY = p.Y;
            }
            var deltaTranslation = e.Delta.Translation;
            {
                var doStuff = false;
                if (ResizingXStart)
                {
                    var dx = deltaTranslation.X / ZoomFactor;
                    layer.Width -= dx / layer.ScaleX;
                    layer.X += dx;
                    doStuff = true;
                }
                if (ResizingYStart)
                {
                    var dy = deltaTranslation.Y / ZoomFactor;
                    layer.Height -= dy / layer.ScaleY;
                    layer.Y += dy;
                    doStuff = true;
                }
                if (ResizingXEnd)
                {
                    var dx = deltaTranslation.X / ZoomFactor / layer.ScaleX;
                    layer.Width += dx;
                    doStuff = true;
                }
                if (ResizingYEnd)
                {
                    var dy = deltaTranslation.Y / ZoomFactor / layer.ScaleY;
                    layer.Height += dy;
                    doStuff = true;
                }
                if (doStuff)
                    goto End;
            }
            var scale = (layer.ScaleX + layer.ScaleY) / 2 * e.Delta.Scale;
            layer.ScaleX = scale;
            layer.ScaleY = scale;
            layer.X += deltaTranslation.X / ZoomFactor;
            layer.Y += deltaTranslation.Y / ZoomFactor;
            layer.CenterX = layer.ActualWidth / 2;
            layer.CenterY = layer.ActualHeight / 2;
            layer.Rotation += e.Delta.Rotation;
        End:
            e.Handled = true;
            UpdateNumberFromValue();
        }
        private void ManipulationCompletedEvent(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!CurrentLayer.IsNotNull(out var layer)) return;
            RecordNewTransformAction(StartManipulationTransform, layer.Transform);
        }
        //private void PointerEntered(object? _, PointerRoutedEventArgs? _1)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        //}
        //private void PointerExited(object? _, PointerRoutedEventArgs? _1)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        //}
        private void PointerWheel(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentLayer is Layers.Layer Layer)
            {
                double dblDelta_Scroll = e.GetCurrentPoint(Layer.LayerUIElement).Properties.MouseWheelDelta;
                switch (e.KeyModifiers)
                {
                    case VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift:
                        Debug.WriteLine(dblDelta_Scroll);
                        
                        dblDelta_Scroll = dblDelta_Scroll > 0 ? 5 : -5;
                        dblDelta_Scroll = dblDelta_Scroll > 0 ? (dblDelta_Scroll * 0.0002) : ((1 / -dblDelta_Scroll - 1) * 0.002);
                        dblDelta_Scroll += 1;
                        Layer.CenterX = Layer.ActualWidth / 2;
                        Layer.CenterY = Layer.ActualHeight / 2;
                        Layer.ScaleX *= dblDelta_Scroll;
                        Layer.ScaleY *= dblDelta_Scroll;
                        UpdateNumberFromValue();
                        break;
                    case VirtualKeyModifiers.Shift:
                        Layer.CenterX = Layer.ActualWidth / 2;
                        Layer.CenterY = Layer.ActualHeight / 2;
                        Layer.Rotation += dblDelta_Scroll * 0.01;
                        UpdateNumberFromValue();
                        break;
                    default:
                        goto ReleasePointerToScrollView;
                }
            }
            return;
        ReleasePointerToScrollView:
            ScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            ScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
            ScrollViewer.ZoomMode = ZoomMode.Enabled;
            Layer.LayerUIElement.IsHitTestVisible = false;
            
            ScrollViewer.PointerReleased += SpacialScrollPointerReleased;
            Layer.LayerUIElement.ReleasePointerCapture(e.Pointer);
            ScrollViewer.CapturePointer(e.Pointer);
            e.Handled = false;
            return;
        }
        void SpacialScrollPointerReleased(object _1, PointerRoutedEventArgs _2)
        {
            ScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            ScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
            ScrollViewer.ZoomMode = ZoomMode.Disabled;
            if (CurrentLayer is not null)
                CurrentLayer.LayerUIElement.IsHitTestVisible = true;
        }
        void KeyDownHandle(CoreWindow C, KeyEventArgs e)
        {
            if (C.GetKeyState(VirtualKey.Shift) != CoreVirtualKeyStates.None && CurrentLayer != null)
            {
                var Element = CurrentLayer.LayerUIElement;
                Element.PointerWheelChanged += PointerWheel;
                Element.ManipulationMode &= ~ManipulationModes.System;
                ScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                ScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                ScrollViewer.ZoomMode = ZoomMode.Disabled;
            }
        }
        void CancelWheelIfKeyUp()
        {
            var Element = CurrentLayer?.LayerUIElement;
            if (Element == null) return;
            Element.PointerWheelChanged -= PointerWheel;
            ScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            ScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
            ScrollViewer.ZoomMode = ZoomMode.Enabled;
        }
        void KeyUpHandle(CoreWindow C, KeyEventArgs e)
        {
            if (C.GetKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.None && CurrentLayer != null)
            {
                try
                {
                    CurrentLayer.LayerUIElement.ManipulationMode |= ManipulationModes.System;
                }
                catch
                {

                }
                CancelWheelIfKeyUp();
            }
        }
        protected override void RequestAddLayerEvent(Layers.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            var Element = Layer.LayerUIElement;
            if ((MoveCommandBar.EnableMove.IsChecked ?? false) || (MoveCommandBar.EnableResize.IsChecked ?? false))
            {
                Element.ManipulationMode |= ManipulationModes.TranslateX;
                Element.ManipulationMode |= ManipulationModes.TranslateY;
            }
            Element.ManipulationMode &= ~ManipulationModes.System;
            if (MoveCommandBar.EnableScale.IsChecked ?? false)
            {
                Element.ManipulationMode &= ~ManipulationModes.System;
                Element.ManipulationMode |= ManipulationModes.Scale;
            }
            if (MoveCommandBar.EnableRotate.IsChecked ?? false)
            {
                Element.ManipulationMode &= ~ManipulationModes.System;
                Element.ManipulationMode |= ManipulationModes.Rotate;
            }
            Element.ManipulationDelta += ManipulationDeltaEvent;
            Element.ManipulationStarted += ManipulationStarted;
            Element.ManipulationCompleted += ManipulationCompletedEvent;

            //Element.PointerEntered += PointerEntered;
            //Element.PointerExited += PointerExited;
            //Element.PointerCaptureLost += PointerExited;
            Window.Current.CoreWindow.KeyDown += KeyDownHandle;
            Window.Current.CoreWindow.KeyUp += KeyUpHandle;
            UpdateNumberFromValue();
        }


        protected override void RequestRemoveLayerEvent(Layers.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            var Element = Layer.LayerUIElement;
            Element.ManipulationMode &= ~ManipulationModes.TranslateX;
            Element.ManipulationMode &= ~ManipulationModes.TranslateY;
            Element.ManipulationMode &= ~ManipulationModes.Scale;
            Element.ManipulationMode &= ~ManipulationModes.Rotate;
            Element.ManipulationMode |= ManipulationModes.System;
            Element.ManipulationDelta -= ManipulationDeltaEvent;
            Element.ManipulationStarted -= ManipulationStarted;
            Element.ManipulationCompleted -= ManipulationCompletedEvent;
            ScrollViewer.PointerReleased -= SpacialScrollPointerReleased;
            //Element.PointerEntered -= PointerEntered;
            //Element.PointerExited -= PointerExited;
            //Element.PointerCaptureLost -= PointerExited;
            //PointerExited(null, null);
            Window.Current.CoreWindow.KeyDown -= KeyDownHandle;
            Window.Current.CoreWindow.KeyUp -= KeyUpHandle;
            //CancelWheelIfKeyUp();
            ScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            ScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
            ScrollViewer.ZoomMode = ZoomMode.Enabled;
        }
    }
    class LambdaCommand : System.Windows.Input.ICommand
    {
        public event Action<object> Action;
        public LambdaCommand(Action<object> a)
        {
            Action = a;
        }
        public LambdaCommand(Action a)
        {
            Action = _ => a?.Invoke();
        }
        public LambdaCommand()
        {
            Action = delegate { };
        }
#pragma warning disable
        public event EventHandler? CanExecuteChanged;
#pragma warning restore
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            Action?.Invoke(parameter);
        }
        public static implicit operator LambdaCommand(Action a) => new(a);
    }
}
