using CoreGraphics;
using UIKit;

using Crex.tvOS.Views;

namespace Crex.tvOS.ViewControllers
{
    public class NavigationController : UINavigationController
    {
        #region Views

        /// <summary>
        /// Gets the loading spinner view.
        /// </summary>
        /// <value>The loading spinner view.</value>
        protected LoadingSpinnerView LoadingSpinnerView { get; private set; }

        /// <summary>
        /// Gets the overlay view.
        /// </summary>
        /// <value>The overlay view.</value>
        protected FocusView OverlayView { get; private set; }

        #endregion

        #region Properties

        protected bool IsWorking { get; private set; }

        /// <summary>
        /// Gets the preferred focus environments.
        /// </summary>
        /// <value>The preferred focus environments.</value>
        public override IUIFocusEnvironment[] PreferredFocusEnvironments
        {
            get
            {
                return OverlayView.FocusEnabled ? new[] { OverlayView } : base.PreferredFocusEnvironments;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded and is ready for final initialization.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            OverlayView = new FocusView
            {
                Frame = new CGRect( 0, 0, 1920, 1080 ),
                Hidden = true,
                FocusEnabled = false
            };
            View.AddSubview( OverlayView );

            LoadingSpinnerView = new Views.LoadingSpinnerView( new CGRect( 880, 460, 160, 160 ) );
            OverlayView.AddSubview( LoadingSpinnerView );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the loading overlay.
        /// </summary>
        public void ShowLoading()
        {
            if ( !IsWorking && ViewIfLoaded != null )
            {
                OverlayView.Hidden = false;
                OverlayView.FocusEnabled = true;
                LoadingSpinnerView.Start();

                IsWorking = true;
                SetNeedsFocusUpdate();
            }
        }

        /// <summary>
        /// Hides the loading overlay.
        /// </summary>
        public void HideLoading()
        {
            if ( IsWorking )
            {
                LoadingSpinnerView.Stop( () =>
                {
                    OverlayView.Hidden = true;
                } );

                IsWorking = false;
                OverlayView.FocusEnabled = false;
                SetNeedsFocusUpdate();
            }
        }

        #endregion
    }
}
