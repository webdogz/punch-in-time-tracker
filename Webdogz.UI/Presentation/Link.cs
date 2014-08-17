﻿using System;

namespace Webdogz.UI.Presentation
{
    /// <summary>
    /// Represents a displayable link.
    /// </summary>
    public class Link
        : Displayable
    {
        private Uri source;

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
    }
}
