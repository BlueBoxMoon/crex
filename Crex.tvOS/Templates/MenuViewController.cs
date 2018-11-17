using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

using Crex.Extensions;
using Crex.tvOS.ViewControllers;

namespace Crex.tvOS.Templates
{
    public class MenuViewController : CrexBaseViewController
    {
        #region Views

        /// <summary>
        /// Gets the background image view.
        /// </summary>
        /// <value>The background image view.</value>
        protected UIImageView BackgroundImageView { get; private set; }

        /// <summary>
        /// Gets the menu bar view.
        /// </summary>
        /// <value>The menu bar view.</value>
        protected Views.MenuBarView MenuBarView { get; private set; }

        /// <summary>
        /// Gets the notification view controller.
        /// </summary>
        /// <value>The notification view controller.</value>
        protected NotificationViewController NotificationViewController { get; private set; }

        #endregion

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

        /// <summary>
        /// Gets the preferred focus environments.
        /// </summary>
        /// <value>The preferred focus environments.</value>
        public override IUIFocusEnvironment[] PreferredFocusEnvironments => new[] { MenuBarView };

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

            MenuBarView = new Views.MenuBarView( new CGRect( 0, 960, 1920, 120 ) );
            MenuBarView.ButtonClicked += MenuBarView_ButtonClicked;
            View.AddSubview( MenuBarView );

            //
            // Setup the notification view controller.
            //
            NotificationViewController = new NotificationViewController();
            AddChildViewController( NotificationViewController );
            View.AddSubview( NotificationViewController.View );
            NotificationViewController.DidMoveToParentViewController( this );
            NotificationViewController.NotificationWasDismissed += ( sender, arg ) =>
            {
                View.SetNeedsFocusUpdate();

                Task.Run( async () =>
                {
                    //
                    // Slight pause to let the focus animations settle.
                    //
                    await Task.Delay( 500 );

                    InvokeOnMainThread( ShowNextNotification );
                } );
            };
            NotificationViewController.FocusGuide.PreferredFocusEnvironments = new[] { MenuBarView };

            //
            // Setup the focus guide for when the users moves up from the menu bar.
            //
            var menuBarUpFocusGuide = new UIFocusGuide();
            View.AddLayoutGuide( menuBarUpFocusGuide );
            menuBarUpFocusGuide.LeftAnchor.ConstraintEqualTo( MenuBarView.LeftAnchor ).Active = true;
            menuBarUpFocusGuide.RightAnchor.ConstraintEqualTo( MenuBarView.RightAnchor ).Active = true;
            menuBarUpFocusGuide.TopAnchor.ConstraintEqualTo( MenuBarView.TopAnchor, -64 ).Active = true;
            menuBarUpFocusGuide.BottomAnchor.ConstraintEqualTo( MenuBarView.TopAnchor ).Active = true;
            menuBarUpFocusGuide.PreferredFocusEnvironments = new[] { NotificationViewController.View };
        }

        /// <summary>
        /// The view is about to appear. If we have not loaded our content
        /// in a while then load the content in the background.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear( bool animated )
        {
            // TODO: Remove!
            NSUserDefaults.StandardUserDefaults.SetString( "", "Crex.LastSeenNotification" );
            base.ViewWillAppear( animated );

            if ( DateTime.Now.Subtract( LastLoadedDate ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
            {
                Task.Run( async () =>
                {
                    try
                    {
                        await LoadContentAsync();
                    }
                    catch
                    {
                        LastLoadedDate = DateTime.Now;
                    }
                } );
            }
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            //
            // Load the menu data.
            //
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
            // Load the image and get the button titles.
            //
            UIImage image;
            try
            {
                image = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( MenuData.BackgroundImage.BestMatch ) );
            }
            catch
            {
                image = null;
            }
            var buttons = MenuData.Buttons.Select( b => b.Title ).ToList();

            LastLoadedDate = DateTime.Now;

            //
            // Update the UI with the image and buttons.
            //
            InvokeOnMainThread( () =>
            {
                EnsureView();
                BackgroundImageView.Image = image;
                MenuBarView.SetButtons( buttons );
                SetNeedsFocusUpdate();

                ShowNextNotification();
            } );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the next notification that needs to be displayed.
        /// </summary>
        protected void ShowNextNotification()
        {
            if ( MenuData.Notifications == null )
            {
                return;
            }

            //
            // Get the current time and the last notification date we saw.
            //
            var now = DateTime.Now;
            if ( !DateTime.TryParse( NSUserDefaults.StandardUserDefaults.StringForKey( "Crex.LastSeenNotification" ), out DateTime lastSeenNotification ) )
            {
                lastSeenNotification = DateTime.MinValue;
            }

            //
            // Find the next notification.
            //
            var notification = MenuData.Notifications
                                       .Where( n => n.StartDateTime > lastSeenNotification && n.StartDateTime <= now )
                                       .OrderBy( n => n.StartDateTime )
                                       .FirstOrDefault();

            //
            // If we have another notification, show it.
            //
            if (notification != null )
            {
                NotificationViewController.ShowNotification( notification );
            }

        }

        #endregion

        #region Events

        /// <summary>
        /// Menus the bar view button clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void MenuBarView_ButtonClicked( object sender, Views.ButtonClickEventArgs e )
        {
            if ( MenuData.Buttons[e.Position].Action != null )
            {
                await Crex.Application.Current.StartAction( this, MenuData.Buttons[e.Position].Action );
            }
            else
            {
                await Crex.Application.Current.StartAction( this, MenuData.Buttons[e.Position].ActionUrl );
            }
        }

        #endregion
    }
}
