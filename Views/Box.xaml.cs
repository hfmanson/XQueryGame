// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xslt2Game.ViewModels;

namespace Xslt2Game
{
    public sealed partial class Box : UserControl
    {
        public Box()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(Box),
                new PropertyMetadata(0.0));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty BoxRotationProperty =
            DependencyProperty.Register(
                nameof(BoxRotation),
                typeof(double),
                typeof(Box),
                new PropertyMetadata(0.0));

        public double BoxRotation
        {
            get => (double)GetValue(BoxRotationProperty);
            set => SetValue(BoxRotationProperty, value);
        }

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(
                nameof(Points),
                typeof(PointCollection),
                typeof(Box),
                new PropertyMetadata(null));

        public PointCollection Points
        {
            get => (PointCollection)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(
                nameof(Fill),
                typeof(Brush),
                typeof(Box),
                new PropertyMetadata(null));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }
    }
}
