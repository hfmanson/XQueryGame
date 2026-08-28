using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xslt2Game
{
    public class PriegelControl : ContentControl
    {
        public double OffsetX
        {
            get => (double)GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(PriegelControl), new PropertyMetadata(0));

        public double OffsetY
        {
            get => (double)GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(PriegelControl), new PropertyMetadata(0));

    }
}
