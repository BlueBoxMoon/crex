using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

using Crex.Extensions;

namespace Crex.tvOS.ViewControllers
{
    public class MenuViewController : CrexBaseViewController
    {
        #region Properties

        /// <summary>
        /// Gets the menu data.
        /// </summary>
        /// <value>The menu data.</value>
        protected Rest.Menu MenuData { get; private set; }

        /// <summary>
        /// Gets the date we last loaded our content.
        /// </summary>
        /// <value>The date we last loaded our content.</value>
        protected DateTime LastLoadedDate { get; private set; } = DateTime.MinValue;

        #endregion

        #region Views

        protected UIImageView BackgroundImageView { get; private set; }
        protected Views.MenuBarView MenuBarView { get; private set; }
        protected Views.LoadingSpinnerView LoadingSpinnerView { get; private set; }

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

            MenuBarView = new Views.MenuBarView( new CGRect( 0, 960, 1920, 120 ) )
            {
                Alpha = 0
            };
            MenuBarView.ButtonClicked += MenuBarView_ButtonClicked;
            View.AddSubview( MenuBarView );

            LoadingSpinnerView = new Views.LoadingSpinnerView( new CGRect( 880, 460, 160, 160 ) );
            View.AddSubview( LoadingSpinnerView );

            LoadingSpinnerView.Start();
        }

        /// <summary>
        /// The view is about to appear. If we have not loaded our content
        /// in a while then load the content in the background.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            if ( DateTime.Now.Subtract( LastLoadedDate ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
            {
                LoadContentInBackground();
            }
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
                // Load the menu data.
                //
                string url = Data.FromJson<string>();
                var json = await new System.Net.Http.HttpClient().GetStringAsync( url );
                var menu = json.FromJson<Rest.Menu>();

                //
                // If the menu content hasn't actually changed, then ignore.
                //
                if ( menu.ToJson().ComputeHash() == MenuData.ToJson().ComputeHash() )
                {
                    return;
                }
                MenuData = menu;

                //
                // Check if an update is required to show this menu.
                //
                if ( MenuData.RequiredCrexVersion.HasValue && MenuData.RequiredCrexVersion.Value > Crex.Application.Current.CrexVersion )
                {
                    ShowUpdateRequiredDialog();

                    return;
                }

                //
                // Load the image and get the button titles.
                //
                var image = await Utility.LoadImageFromUrlAsync( MenuData.BackgroundImage.BestMatch );
                var buttons = MenuData.Buttons.Select( b => b.Title ).ToList();

                InvokeOnMainThread( () =>
                {
                    //
                    // Update the UI with the image and buttons.
                    //
                    BackgroundImageView.Image = image;
                    MenuBarView.SetButtons( buttons );
                    SetNeedsFocusUpdate();

                    UIView.Animate( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
                    {
                        BackgroundImageView.Alpha = 1;
                        MenuBarView.Alpha = 1;
                    } );

                    LoadingSpinnerView.Stop();
                } );

                LastLoadedDate = DateTime.Now;
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

        #region Events

        /// <summary>
        /// Menus the bar view button clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void MenuBarView_ButtonClicked( object sender, Views.ButtonClickEventArgs e )
        {
            Crex.Application.Current.StartAction( this, MenuData.Buttons[e.Position].Action );
        }

        #endregion
    }
}