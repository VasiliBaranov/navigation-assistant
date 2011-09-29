using System;
using System.Windows;

namespace NavigationAssistant.Utilities
{
    //Declares an attached property to bind ActualSize to ViewModel.
    //Code taken from http://stackoverflow.com/questions/1083224/pushing-read-only-gui-properties-back-into-viewmodel
    public static class SizeExtension
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeExtension),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeExtension));

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeExtension));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            // WPF 4.0 onwards
            //frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            //frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);

             // WPF 3.5 and prior
            SetObservedWidth(frameworkElement, frameworkElement.ActualWidth);
            SetObservedHeight(frameworkElement, frameworkElement.ActualHeight);
        }


    }
}
