using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MusicPlayerWPF
{
    public class RoundThumb : Thumb
    {
        public RoundThumb() 
        {
            Width = 20;
            Height = 20;
            Background = new SolidColorBrush(Colors.Black);

            Template = new ControlTemplate(typeof(Thumb));
            Template.VisualTree = new FrameworkElementFactory(typeof(Border));
            Template.VisualTree.SetValue(Border.BackgroundProperty, Brushes.Black);
            Template.VisualTree.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
        }
    }
}
