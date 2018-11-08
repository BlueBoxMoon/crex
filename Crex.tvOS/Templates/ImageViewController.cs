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

        protected UIImageView BackgroundImageView { get; private set; }

        protected Views.LoadingSpinnerView LoadingSpinnerView { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded, now we can create any child views.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BackgroundImageView = new UIImageView( new CGRect( 0, 0, 1920, 1080 ) )
            {
                Alpha = 0
            };
            View.AddSubview( BackgroundImageView );

            LoadingSpinnerView = new Views.LoadingSpinnerView( new CGRect( 880, 460, 160, 160 ) );
            View.AddSubview( LoadingSpinnerView );

            LoadingSpinnerView.Start();
        }

        /// <summary>
        /// The view is about to appear. Start loading the content.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            LoadContentInBackground();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content for the menu.
        /// </summary>
        private void LoadContentInBackground()
        {
            Task.Run( async () =>
            {
                //
                // Load the image.
                //
                var urlSet = Data.FromJson<Rest.UrlSet>();
                var image = await Utility.LoadImageFromUrlAsync( urlSet.BestMatch );

                InvokeOnMainThread( () =>
                {
                    //
                    // Update the UI with the image and buttons.
                    //
                    BackgroundImageView.Image = image;

                    UIView.Animate( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
                    {
                        BackgroundImageView.Alpha = 1;
                    } );

                    LoadingSpinnerView.Stop();
                } );
            } )
            .ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    ShowDataErrorDialog( LoadContentInBackground );
                }
            } );
        }

        #endregion
    }
}