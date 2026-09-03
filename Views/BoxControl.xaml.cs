using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xslt2Game.ViewModels;

namespace Xslt2Game
{
    public sealed partial class BoxControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(BoxNodeViewModel),
                typeof(BoxControl),
                new PropertyMetadata(null));

        public BoxNodeViewModel ViewModel
        {
            get => (BoxNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public BoxControl()
        {
            this.InitializeComponent();
        }
    }
}
