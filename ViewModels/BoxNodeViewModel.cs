using Windows.UI.Xaml.Media;

namespace Xslt2Game.ViewModels
{
    public class BoxNodeViewModel : NodeViewModelBase
    {
        private double _left;
        private double _top;
        private double _boxRotation;
        private PointCollection _points;
        private Brush _foreground;

        // --- UI Target Bindings (What the Box UserControl looks at) ---
        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(); }
        }

        public double BoxRotation
        {
            get => _boxRotation;
            set { _boxRotation = value; OnPropertyChanged(); }
        }

        public PointCollection Points
        {
            get => _points;
            set { _points = value; OnPropertyChanged(); }
        }

        public Brush Foreground
        {
            get => _foreground;
            set { _foreground = value; OnPropertyChanged(); }
        }

        // --- Data Calculations ---
        public void UpdateColum(int column)
        {
            Left = column - 1;
        }

        public void UpdateRow(int row)
        {
            Top = row - 1;
        }

        public void UpdateRotation(double dx, double dy)
        {
            BoxRotation = dy == 1.0 ? 0 : dy == -1.0 ? 180.0 : dx == 1 ? -90.0 : 90.0;
        }
    }
}
