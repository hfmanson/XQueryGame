using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xslt2Game
{
    public class ExtendedContentControl : ContentControl
    {
        public new double Rotation
        {
            get => (double)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(
                nameof(Rotation),
                typeof(double),
                typeof(ExtendedContentControl),
                new PropertyMetadata(0)
            );
    }
}
