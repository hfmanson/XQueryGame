using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xslt2Game.ViewModels;

namespace Xslt2Game.Views
{
    public sealed partial class SmallBoxControl : UserControl
    {
        public SmallBoxControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(BoxNodeViewModel),
                typeof(SmallBoxControl),
                new PropertyMetadata(null));

        public BoxNodeViewModel ViewModel
        {
            get => (BoxNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}
