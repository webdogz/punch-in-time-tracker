using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Webdogz.UI.Controls
{
    public class IconButton : Button
    {
        /// <summary>
        /// Identifies the IconToolTip property
        /// </summary>
        public static readonly DependencyProperty IconToolTipProperty = DependencyProperty.Register("IconToolTip", typeof(string), typeof(IconButton));
        /// <summary>
        /// Identifies the IconData property.
        /// </summary>
        public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register("IconData", typeof(Geometry), typeof(IconButton));
        /// <summary>
        /// Identifies the IconHeight property.
        /// </summary>
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(IconButton), new PropertyMetadata(12D));
        /// <summary>
        /// Identifies the IconWidth property.
        /// </summary>
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(IconButton), new PropertyMetadata(12D));
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernButton"/> class.
        /// </summary>
        public IconButton()
        {
            this.DefaultStyleKey = typeof(IconButton);
        }

        /// <summary>
        /// Gets or sets the icon path data.
        /// </summary>
        /// <value>
        /// The icon path data.
        /// </value>
        public Geometry IconData
        {
            get { return (Geometry)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }

        /// <summary>
        /// Gets or sets the tooltip displayed when hovering over the icon.
        /// </summary>
        /// <value>
        /// The icon tooltip.
        /// </value>
        public string IconToolTip
        {
            get { return (string)GetValue(IconToolTipProperty); }
            set { SetValue(IconToolTipProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon height.
        /// </summary>
        /// <value>
        /// The icon height.
        /// </value>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon width.
        /// </summary>
        /// <value>
        /// The icon width.
        /// </value>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
    }
}
