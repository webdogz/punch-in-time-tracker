using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PunchIn.Controls
{
    public class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ImageBrushProperty =
            DependencyProperty.Register("ImageBrush", typeof(Brush), typeof(ImageButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageButton), new UIPropertyMetadata(20D));
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageButton), new UIPropertyMetadata(20D));
        
        public ImageButton()
        {
            this.DefaultStyleKey = typeof(ImageButton);
        }
        /// <summary>
        /// Gets or sets the Image
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        /// <summary>
        /// Gets or sets the Brush
        /// </summary>
        public Brush ImageBrush
        {
            get { return (Brush)GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }
        /// <summary>
        /// Gets or sets the width of the Image and Brush
        /// </summary>
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        /// <summary>
        /// Gets or sets the height of the Image and Brush
        /// </summary>
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }
    }
}
