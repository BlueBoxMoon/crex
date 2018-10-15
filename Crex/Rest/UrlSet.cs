using System.Collections.Generic;
using System.Linq;

namespace Crex.Rest
{
    /// <summary>
    /// Defines a URL set that contains multiple resolutions of the same URL.
    /// </summary>
    public class UrlSet
    {
        /// <summary>
        /// Gets or sets the HD url (720p).
        /// </summary>
        /// <value>
        /// The HD url (720p).
        /// </value>
        public string HD { get; set; }

        /// <summary>
        /// Gets or sets the FHD url (1080p).
        /// </summary>
        /// <value>
        /// The FHD url (1080p).
        /// </value>
        public string FHD { get; set; }

        /// <summary>
        /// Gets or sets the UHD url (2160p).
        /// </summary>
        /// <value>
        /// The UHD url (2160p).
        /// </value>
        public string UHD { get; set; }

        /// <summary>
        /// Gets the best available quality url.
        /// </summary>
        /// <value>
        /// The best available quality url.
        /// </value>
        public virtual string BestQuality
        {
            get
            {
                return new List<string> { UHD, FHD, HD }
                    .FirstOrDefault( i => !string.IsNullOrWhiteSpace( i ) );
            }
        }

        /// <summary>
        /// Gets the best match url.
        /// </summary>
        /// <value>
        /// The best match url.
        /// </value>
        public virtual string BestMatch
        {
            get
            {
                List<string> images;

                if ( Crex.Application.Current.Resolution.Height >= 2160 )
                {
                    images = new List<string> { UHD, FHD, HD };
                }
                else if ( Crex.Application.Current.Resolution.Height >= 1080 )
                {
                    images = new List<string> { FHD, UHD, HD };
                }
                else /* 720 */
                {
                    images = new List<string> { HD, FHD, UHD };
                }

                return images.FirstOrDefault( i => !string.IsNullOrWhiteSpace( i ) );
            }
        }
    }
}