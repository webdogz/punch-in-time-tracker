using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Webdogz.UI.Presentation
{
    /// <summary>
    /// Represents an observable collection of links.
    /// </summary>
    public class TitleLinkCollection
        : ObservableCollection<TitleLink>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TitleLinkCollection"/> class.
        /// </summary>
        public TitleLinkCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleLinkCollection"/> class that contains specified links.
        /// </summary>
        /// <param name="links">The links that are copied to this collection.</param>
        public TitleLinkCollection(IEnumerable<TitleLink> links)
        {
            if (links == null) {
                throw new ArgumentNullException("links");
            }
            foreach (var link in links) {
                Add(link);
            }
        }
    }
}
