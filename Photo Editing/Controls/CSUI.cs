using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace CSUI
{
    public interface ICSUI
    {
        void OnLoadUI();
    }
    public abstract class CSUIStackPanel : StackPanel, ICSUI
    {
        void ICSUI.OnLoadUI()
        {
            Children.Clear();
            foreach (var element in OnLoadUI())
                Children.Add(element);
        }
        protected abstract IEnumerable<UIElement> OnLoadUI();
    }
    public abstract class CSUIUserControl : UserControl, ICSUI
    {
        void ICSUI.OnLoadUI()
        {
            Content = OnLoadUI();
        }
        protected abstract UIElement OnLoadUI();
    }

    public static class Extension
    {
        public static void RegisterCSUIReload(this ICSUI ICSUIElement)
        {
            ICSUIElement.OnLoadUI();
            Window.Current.CoreWindow.KeyDown += (_, e) =>
            {
                switch (e.VirtualKey)
                {
                    case VirtualKey.R:
                        ICSUIElement.OnLoadUI();
                        break;
                }
            };
        }

        public static T SetAsVariable<T>(this T Item, out T Variable)
        {
            Variable = Item;
            return Item;
        }
        public static T Edit<T>(this T Item, Action<T> Action)
        {
            Action?.Invoke(Item);
            return Item;
        }
        public static T SetContent<T>(this T Item, object Object) where T : ContentControl
        {
            Item.Content = Object;
            return Item;
        }
        public static T SetContent<T>(this T Item, UIElement UIElement) where T : UserControl
        {
            Item.Content = UIElement;
            return Item;
        }
        public static T AddChild<T>(this T Item, UIElement UIElement) where T : Panel
        {
            Item.Children.Add(UIElement);
            return Item;
        }
    }
}
