using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Crex.Extensions;

namespace Crex.Android.Templates
{
    public class MenuFragment : CrexBaseFragment
    {
        #region Views

        /// <summary>
        /// Gets the background image view.
        /// </summary>
        /// <value>
        /// The background image view.
        /// </value>
        protected ImageView BackgroundImageView { get; private set; }

        /// <summary>
        /// Gets the menu bar view.
        /// </summary>
        /// <value>
        /// The menu bar view.
        /// </value>
        protected Widgets.MenuBar MenuBarView { get; private set; }

        /// <summary>
        /// Gets the notification view.
        /// </summary>
        /// <value>
        /// The notification view.
        /// </value>
        protected Widgets.NotificationView NotificationView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the menu data.
        /// </summary>
        /// <value>
        /// The menu data.
        /// </value>
        protected Rest.Menu MenuData { get; private set; }

        /// <summary>
        /// Gets the date we last loaded our content.
        /// </summary>
        /// <value>
        /// The date we last loaded our content.
        /// </value>
        protected DateTime LastLoadedDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the background image that was loaded on the background thread.
        /// </summary>
        /// <value>
        /// The background image that was loaded on the background thread.
        /// </value>
        protected Bitmap BackgroundImage { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called immediately after <c><see cref="M:Android.App.Fragment.OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)" /></c>
        /// has returned, but before any saved state has been restored in to the view.
        /// </summary>
        /// <param name="view">The View returned by <c><see cref="M:Android.App.Fragment.OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)" /></c>.</param>
        /// <param name="savedInstanceState">If non-null, this fragment is being re-constructed
        /// from a previous saved state as given here.</param>
        public override void OnViewCreated( View view, Bundle savedInstanceState )
        {
            base.OnViewCreated( view, savedInstanceState );

            var layout = ( FrameLayout ) view;
            Crex.Application.Current.Preferences.RemoveValue( "Crex.LastSeenNotification" );

            //
            // Setup the background image view.
            //
            BackgroundImageView = new ImageView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent )
            };
            BackgroundImageView.SetScaleType( ImageView.ScaleType.CenterCrop );
            layout.AddView( BackgroundImageView );

            //
            // Setup the Menu Bar
            //
            MenuBarView = new Widgets.MenuBar( Activity, null )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, Utility.DipToPixel( 60 ) )
                {
                    Gravity = GravityFlags.Bottom
                }
            };
            MenuBarView.ButtonClicked += MenuBar_ButtonClicked;
            layout.AddView( MenuBarView );

            NotificationView = new Widgets.NotificationView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent )
                {
                    Gravity = GravityFlags.Top
                }
            };
            NotificationView.NotificationWasDismissed += ( sender, arg ) =>
            {
                ShowNextNotification();
            };
            layout.AddView( NotificationView );

            //
            // Set our initial data if we have already loaded.
            //
            if ( MenuData != null )
            {
                UpdateUserInterfaceFromContent();
            }
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            var menu = Data.FromJson<Rest.Menu>();

            //
            // If the menu content hasn't actually changed, then ignore.
            //
            if ( menu.ToJson().ComputeHash() == MenuData.ToJson().ComputeHash() )
            {
                return;
            }

            MenuData = menu;

            //
            // Load the background image and prepate the menu buttons.
            //
            BackgroundImage = await Utility.LoadImageFromUrlAsync( MenuData.BackgroundImage.BestMatch );

            if ( Activity != null )
            {
                Activity.RunOnUiThread( UpdateUserInterfaceFromContent );
            }

            LastLoadedDate = DateTime.Now;
        }

        /// <summary>
        /// Called when the fragment has been fully hidden.
        /// </summary>
        public override void OnFragmentDidHide()
        {
            NotificationView.ShowNotification( null );
        }

        /// <summary>
        /// Called when the fragment has fully appeared on screen.
        /// </summary>
        public override void OnFragmentDidShow()
        {
            ShowNextNotification();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the user interface to match the content.
        /// </summary>
        private void UpdateUserInterfaceFromContent()
        {
            BackgroundImageView.SetImageBitmap( BackgroundImage );

            MenuBarView.SetButtons( MenuData.Buttons.Select( b => b.Title ).ToList() );
            MenuBarView.RequestFocus();
        }

        /// <summary>
        /// Shows the next notification.
        /// </summary>
        protected void ShowNextNotification()
        {
            var notification = MenuData.Notifications.GetNextNotification();

            if ( notification != null )
            {
                NotificationView.ShowNotification( notification );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ButtonClicked event of the menuBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Widgets.ButtonClickEventArgs"/> instance containing the event data.</param>
        private void MenuBar_ButtonClicked( object sender, Widgets.ButtonClickEventArgs e )
        {
            var button = MenuData.Buttons[e.Position];

            if ( button.Action != null )
            {
                Crex.Application.Current.StartAction( this, button.Action );
            }
            else
            {
                Crex.Application.Current.StartAction( this, button.ActionUrl );
            }
        }

        #endregion
    }
}
