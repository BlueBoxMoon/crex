using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

using Crex.Extensions;

namespace Crex.tvOS.Templates
{
    public class ImageViewController : CrexBaseViewController
    {
        #region Views

        /// <summary>
        /// Gets the background image view.
        /// </summary>
        /// <value>The background image view.</value>
        protected UIImageView BackgroundImageView { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded, now we can create any child views.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BackgroundImageView = new UIImageView( new CGRect( 0, 0, 1920, 1080 ) );
            View.AddSubview( BackgroundImageView );
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            EnsureView();

            //
            // Load the image.
            //
            var urlSet = Data.FromJson<Rest.UrlSet>();
            BackgroundImageView.Image = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( urlSet.BestMatch ) );
        }

        #endregion
    }
}