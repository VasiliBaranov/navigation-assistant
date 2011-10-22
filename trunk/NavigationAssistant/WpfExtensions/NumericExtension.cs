using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NavigationAssistant.WpfExtensions
{
    /// <summary>
    /// Implements an extension that allows restriction of textboxes to numeric input.
    /// </summary>
    /// <remarks>
    /// Code taken from http://stackoverflow.com/questions/1346707/validation-in-textbox-in-wpf
    /// </remarks>
    public class NumericExtension
    {
        /// <summary>
        /// TextBox Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsNumericProperty = DependencyProperty.RegisterAttached(
            "IsNumeric",
            typeof (bool),
            typeof(NumericExtension),
            new UIPropertyMetadata(false, HandleIsNumericChanged));

        /// <summary>
        /// Gets the IsNumeric property.  This dependency property indicates the text box only allows numeric or not.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the StatusBarContent property</returns>
        public static bool GetIsNumeric(DependencyObject d)
        {
            return (bool) d.GetValue(IsNumericProperty);
        }

        /// <summary>
        /// Sets the IsNumeric property.  This dependency property indicates the text box only allows numeric or not.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetIsNumeric(DependencyObject d, bool value)
        {
            d.SetValue(IsNumericProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsNumeric property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void HandleIsNumericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isNumeric = (bool) e.NewValue;

            TextBox textBox = (TextBox) d;

            if (isNumeric)
            {
                textBox.PreviewTextInput += HandlePreviewTextInput;
                textBox.PreviewKeyDown += HandlePreviewKeyDown;
            }
            else
            {
                textBox.PreviewTextInput -= HandlePreviewTextInput;
                textBox.PreviewKeyDown -= HandlePreviewKeyDown;
            }
        }

        /// <summary>
        /// Disallows non-digit characters.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="TextCompositionEventArgs"/> that contains the event data.</param>
        private static void HandlePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!Char.IsDigit(ch))
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Disallows a space key.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
        private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Disallow the space key, which doesn't raise a PreviewTextInput event.
                e.Handled = true;
            }
        }
    }
}
