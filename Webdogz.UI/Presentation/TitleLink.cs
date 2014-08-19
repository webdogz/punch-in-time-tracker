using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Webdogz.UI.Presentation
{
    /// <summary>
    /// Represents a displayable link.
    /// </summary>
    public class TitleLink
        : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the LogoData dependency property.
        /// </summary>
        public static readonly DependencyProperty LogoDataProperty = DependencyProperty.Register("IconData", typeof(Geometry), typeof(TitleLink));
        
        private string displayName;
        private Uri source;
        private string iconToolTip;

        /// <summary>
        /// Gets or sets the Title of the link.
        /// </summary>
        /// <value>The title</value>
        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                if (this.displayName != value)
                {
                    this.displayName = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Geometry data.
        /// </summary>
        /// <value>The icon Geometry data</value>
        public Geometry IconData
        {
            get { return (Geometry)GetValue(LogoDataProperty); }
            set { SetValue(LogoDataProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icons tooltip.
        /// </summary>
        /// <value>The tooltip.</value>
        public string IconToolTip
        {
            get { return this.iconToolTip; }
            set
            {
                if (this.iconToolTip != value)
                {
                    this.iconToolTip = value;
                    OnPropertyChanged("IconToolTip");
                }
            }
        }

        /// <summary>
        /// Gets or sets the source uri.
        /// </summary>
        /// <value>The source.</value>
        public Uri Source
        {
            get { return this.source; }
            set
            {
                if (this.source != value) {
                    this.source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        #region Notification Impl
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
