using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PhotoEditing.CommandButton.Controls;

namespace PhotoEditing
{
    public class MoveCommandButton : CommandButtonBase
    {
        private readonly Move MoveCommandBar = new Move();
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
            
            void UpdateFromKey(object o, KeyRoutedEventArgs e)
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    UpdateNumberFromTB();
                }
            };
            MoveCommandBar.TB_X.KeyDown += UpdateFromKey;
            MoveCommandBar.TB_Y.KeyDown += UpdateFromKey;
            MoveCommandBar.TB_R.KeyDown += UpdateFromKey;
            MoveCommandBar.TB_S.KeyDown += UpdateFromKey;
            void UpdateFromLostFocus(object o, RoutedEventArgs e)
            {
                UpdateNumberFromTB();
            }
            MoveCommandBar.TB_X.LostFocus += UpdateFromLostFocus;
            MoveCommandBar.TB_Y.LostFocus += UpdateFromLostFocus;
            MoveCommandBar.TB_R.LostFocus += UpdateFromLostFocus;
            MoveCommandBar.TB_S.LostFocus += UpdateFromLostFocus;
        }
        void UpdateNumberFromTB()
        {
            var layer = CurrentLayer;
            if (layer == null) return;
            layer.CenterPoint = new System.Numerics.Vector3((float)(layer.X + layer.ActualWidth / 2), (float)(layer.Y + layer.ActualHeight / 2), 1);
            layer.X = Convert.ToDouble(MoveCommandBar.TB_X.Text);
            layer.Y = Convert.ToDouble(MoveCommandBar.TB_Y.Text);
            layer.Scale = new System.Numerics.Vector3(Convert.ToSingle(MoveCommandBar.TB_S.Text));
            layer.Rotation = Convert.ToSingle(MoveCommandBar.TB_R.Text);
            LayerContainer.InvalidateArrange();
        }
        void UpdateNumberFromValue()
        {
            MoveCommandBar.TB_X.Text = CurrentLayer.X.ToString("G");
            MoveCommandBar.TB_Y.Text = CurrentLayer.Y.ToString("G");
            MoveCommandBar.TB_S.Text = ((CurrentLayer.Scale.X + CurrentLayer.Scale.Y) / 2).ToString("G");
            MoveCommandBar.TB_R.Text = CurrentLayer.Rotation.ToString("G");
        }
        private void ManipulationDeltaEvent(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var layer = CurrentLayer;
            //var posX = e.Position.X;
            //var posY = e.Position.Y;


            ////// FIRST GET TOUCH POSITION RELATIVE TO THIS ELEMENT ///
            if (e.Container != null)
            {
                //var p = e.Container.TransformToVisual(layer.LayerUIElement).TransformPoint(new Windows.Foundation.Point(posX, posY)); //transform touch point position relative to this element
                //posX = p.X;
                //posY = p.Y;
            }

            var delta = e.Delta.Translation;
            layer.CenterPoint = new System.Numerics.Vector3((float)(layer.X + layer.ActualWidth / 2), (float)(layer.Y + layer.ActualHeight / 2), 1); //new System.Numerics.Vector3((float)posX, (float)posY, 1);
            layer.X += delta.X;
            layer.Y += delta.Y;
            layer.Rotation += e.Delta.Rotation;
            var scaledelta = e.Delta.Scale;
            layer.Scale *= new System.Numerics.Vector3(scaledelta, scaledelta, 1);
            e.Handled = true;
            UpdateNumberFromValue();
        }
        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
            //LayerContainer.CancelDirectManipulations();
        }
        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }
        protected override void RequestAddLayerEvent(Layer.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            var Element = Layer.LayerUIElement;
            if (MoveCommandBar.EnableMove.IsChecked.Value)
            {
                Element.ManipulationMode |= ManipulationModes.TranslateX;
                Element.ManipulationMode |= ManipulationModes.TranslateY;
            }
            if (MoveCommandBar.EnableResize.IsChecked.Value)
            {
                Element.ManipulationMode &= ~ManipulationModes.System;
                Element.ManipulationMode |= ManipulationModes.Scale;
            }
            if (MoveCommandBar.EnableRotate.IsChecked.Value)
            {
                Element.ManipulationMode |= ManipulationModes.Rotate;
            }
            Element.ManipulationDelta += ManipulationDeltaEvent;
            Element.PointerEntered += PointerEntered;
            Element.PointerExited += PointerExited;
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
            Element.ManipulationDelta -= ManipulationDeltaEvent;
            Element.PointerEntered -= PointerEntered;
            Element.PointerExited -= PointerExited;
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
            Action = (_) => a();
        }
        public LambdaCommand()
        {

        }
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            Action?.Invoke(parameter);
        }
        public static implicit operator LambdaCommand(Action a) => new LambdaCommand(a);
    }
}
