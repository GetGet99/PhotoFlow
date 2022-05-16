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

namespace PhotoFlow
{
    public class MoveCommandButton : CommandButtonBase
    {
        private readonly Move MoveCommandBar = new ();
        protected override CommandButtonCommandBar CommandBar => MoveCommandBar;

        public MoveCommandButton(Border CommandBarPlace) : base(Symbol.TouchPointer, CommandBarPlace)
        {

            MoveCommandBar.ResetSize.Command = new LambdaCommand(() =>
            {
                CurrentLayer.X = 0;
                CurrentLayer.Y = 0;
                UpdateNumberFromValue();
            });
            var ilc = new LambdaCommand(InvokeLayerChange);
            MoveCommandBar.EnableMove.Command = ilc;
            MoveCommandBar.EnableResize.Command = ilc;
            MoveCommandBar.EnableRotate.Command = ilc;

            void UpdateNumberFromTBEV(NumberBox _, NumberBoxValueChangedEventArgs _1)
                => UpdateNumberFromTB();
            MoveCommandBar.TB_X.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_Y.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_R.ValueChanged += UpdateNumberFromTBEV;
            MoveCommandBar.TB_S.ValueChanged += UpdateNumberFromTBEV;
        }

        void UpdateNumberFromTB()
        {
            if (IsUpdatingNumberFromValue) return;
            var layer = CurrentLayer;
            if (layer == null) return;
            layer.X = MoveCommandBar.TB_X.Value;
            layer.Y = MoveCommandBar.TB_Y.Value;
            layer.CenterX = layer.ActualWidth / 2;
            layer.CenterY = layer.ActualHeight / 2;
            var scale = MoveCommandBar.TB_S.Value;
            layer.ScaleX = scale;
            layer.ScaleY = scale;
            layer.Rotation = MoveCommandBar.TB_R.Value;
        }
        bool IsUpdatingNumberFromValue = false;
        void UpdateNumberFromValue()
        {
            IsUpdatingNumberFromValue = true;
            MoveCommandBar.TB_X.Value = CurrentLayer.X;
            MoveCommandBar.TB_Y.Value = CurrentLayer.Y;
            MoveCommandBar.TB_S.Value = (CurrentLayer.ScaleX + CurrentLayer.ScaleY) / 2;
            MoveCommandBar.TB_R.Value = CurrentLayer.Rotation;
            IsUpdatingNumberFromValue = false;
        }
        private void ManipulationDeltaEvent(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var layer = CurrentLayer;

            if (e.Container != null)
            {
                //var p = e.Container.TransformToVisual(layer.LayerUIElement).TransformPoint(new Windows.Foundation.Point(posX, posY)); //transform touch point position relative to this element
                //posX = p.X;
                //posY = p.Y;
            }
            var ZoomFactor = this.ZoomFactor;
            var scale = (layer.ScaleX + layer.ScaleY) / 2 * e.Delta.Scale;
            layer.ScaleX = scale;
            layer.ScaleY = scale;
            var delta = e.Delta.Translation;
            layer.X += delta.X / ZoomFactor;
            layer.Y += delta.Y / ZoomFactor;
            layer.CenterX = layer.ActualWidth / 2;
            layer.CenterY = layer.ActualHeight / 2;
            layer.Rotation += e.Delta.Rotation;
            e.Handled = true;
            UpdateNumberFromValue();
        }
        private void PointerEntered(object? _, PointerRoutedEventArgs? _1)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        }
        private void PointerExited(object? _, PointerRoutedEventArgs? _1)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }
        //int NotSameCount = 0;
        //bool PreviouslyScrollUp = false;
        private void PointerWheel(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentLayer is Layer.Layer Layer)
            {
                double dblDelta_Scroll = e.GetCurrentPoint(Layer.LayerUIElement).Properties.MouseWheelDelta;
                switch (e.KeyModifiers)
                {
                    case VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift:
                        //bool ScrollUp = dblDelta_Scroll > 0;
                        Debug.WriteLine(dblDelta_Scroll);
                        //if (PreviouslyScrollUp != ScrollUp)
                        //{
                        //    NotSameCount++;
                        //    if (NotSameCount > 2) PreviouslyScrollUp = ScrollUp;
                        //    break;
                        //}
                        //else NotSameCount = 0;
                        //dblDelta_Scroll = dblDelta_Scroll > 0 ? (dblDelta_Scroll * 0.0005) : ((1 / -dblDelta_Scroll - 1) * 0.0325);
                        //var sign = dblDelta_Scroll > 0 ? 1 : -1;
                        //dblDelta_Scroll = sign;
                        //dblDelta_Scroll = Math.Abs(dblDelta_Scroll);
                        //dblDelta_Scroll = dblDelta_Scroll > 100 ? 100 : dblDelta_Scroll;
                        dblDelta_Scroll = dblDelta_Scroll > 0 ? 5 : -5;
                        //dblDelta_Scroll *= sign;
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
            ScrollViewer.PointerReleased += delegate
            {
                ScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                ScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                ScrollViewer.ZoomMode = ZoomMode.Disabled;
                Layer.LayerUIElement.IsHitTestVisible = true;
            };
            Layer.LayerUIElement.ReleasePointerCapture(e.Pointer);
            ScrollViewer.CapturePointer(e.Pointer);
            e.Handled = false;
            return;
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
            var Element = CurrentLayer.LayerUIElement;
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
                CurrentLayer.LayerUIElement.ManipulationMode |= ManipulationModes.System;
                CancelWheelIfKeyUp();
            }
        }
        protected override void RequestAddLayerEvent(Layer.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            var Element = Layer.LayerUIElement;
            if (MoveCommandBar.EnableMove.IsChecked ?? false)
            {
                Element.ManipulationMode |= ManipulationModes.TranslateX;
                Element.ManipulationMode |= ManipulationModes.TranslateY;
            }
            Element.ManipulationMode &= ~ManipulationModes.System;
            if (MoveCommandBar.EnableResize.IsChecked ?? false)
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
            //Element.PointerEntered += PointerEntered;
            //Element.PointerExited += PointerExited;
            //Element.PointerCaptureLost += PointerExited;
            Window.Current.CoreWindow.KeyDown += KeyDownHandle;
            Window.Current.CoreWindow.KeyUp += KeyUpHandle;
            UpdateNumberFromValue();
        }


        protected override void RequestRemoveLayerEvent(Layer.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            var Element = Layer.LayerUIElement;
            Element.ManipulationMode &= ~ManipulationModes.TranslateX;
            Element.ManipulationMode &= ~ManipulationModes.TranslateY;
            Element.ManipulationMode &= ~ManipulationModes.Scale;
            Element.ManipulationMode &= ~ManipulationModes.Rotate;
            Element.ManipulationMode |= ManipulationModes.System;
            Element.ManipulationDelta -= ManipulationDeltaEvent;
            //Element.PointerEntered -= PointerEntered;
            //Element.PointerExited -= PointerExited;
            //Element.PointerCaptureLost -= PointerExited;
            //PointerExited(null, null);
            Window.Current.CoreWindow.KeyDown -= KeyDownHandle;
            Window.Current.CoreWindow.KeyUp -= KeyUpHandle;
            //CancelWheelIfKeyUp();
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
        public static implicit operator LambdaCommand(Action a) => new (a);
    }
}
