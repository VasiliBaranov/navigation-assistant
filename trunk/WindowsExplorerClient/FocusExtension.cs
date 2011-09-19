using System.Windows;

namespace WindowsExplorerClient
{
    //This attached property allows to set focus on controls from view model,
    //e.g. <TextBox local:FocusExtension.IsFocused="{Binding IsUserNameFocused}" />
    //Code is taken from http://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model-c-wpf
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
                DependencyProperty.RegisterAttached(
                 "IsFocused", typeof(bool), typeof(FocusExtension),
                 new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject d,
                DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }

}
