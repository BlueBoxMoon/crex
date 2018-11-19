using System;
using System.Threading.Tasks;
using CoreGraphics;
using Crex.tvOS.Extensions;
using Crex.tvOS.Views;
using UIKit;
using Foundation;

namespace Crex.tvOS.ViewControllers
{
    public class NotificationViewController : UIViewController
    {
        #region Published Events

        /// <summary>
        /// Occurs when notification is dismissed.
        /// </summary>
        public event EventHandler NotificationWasDismissed;

        /// <summary>
        /// Occurs when notification was shown.
        /// </summary>
        public event EventHandler NotificationWasShown;

        #endregion

        #region Views

        /// <summary>
        /// Gets the image view.
        /// </summary>
        /// <value>The image view.</value>
        protected UIImageView ImageView { get; private set; }

        /// <summary>
        /// Gets the message label.
        /// </summary>
        /// <value>The message label.</value>
        protected UILabel MessageLabel { get; private set; }

        /// <summary>
        /// Gets the dismiss button.
        /// </summary>
        /// <value>The dismiss button.</value>
        protected MenuButton DismissButton { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notification.
        /// </summary>
        /// <value>The notification.</value>
        public Rest.Notification Notification { get; private set; }

        /// <summary>
        /// Gets the focus guide. This can be used to set the target
        /// focus control when the user presses the down button while the
        /// notification is currently focused.
        /// </summary>
        /// <value>The focus guide.</value>
        public UIFocusGuide FocusGuide { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view needs to be created.
        /// </summary>
        public override void LoadView()
        {
            base.LoadView();

            View.AutoresizingMask = UIViewAutoresizing.None;
            View.AutosizesSubviews = false;
            View.Frame = new CGRect( 0, -240, 1920, 240 );
            View.BackgroundColor = "#d0323232".AsUIColor();
        }

        /// <summary>
        /// The view has loaded and is ready for final preparation.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //
            // Create the image view.
            //
            ImageView = new UIImageView( new CGRect( 40, 40, 288, 162 ) )
            {
                ContentMode = UIViewContentMode.ScaleAspectFit
            };
            View.AddSubview( ImageView );

            //
            // Create the message text area.
            //
            MessageLabel = new UILabel( new CGRect( 368, 40, 1184, 162 ) )
            {
                Lines = 4,
                LineBreakMode = UILineBreakMode.TailTruncation,
                TextAlignment = UITextAlignment.Center,
                TextColor = "#ffdddddd".AsUIColor(),
                Font = UIFont.SystemFontOfSize( 33 )
            };
            View.AddSubview( MessageLabel );

            //
            // Create the dismiss button.
            //
            DismissButton = new MenuButton( new CGRect( 1594, 136, 288, 64 ) );
            DismissButton.SetTitle( "Dismiss", UIControlState.Normal );
            DismissButton.PrimaryActionTriggered += DismissButton_PrimaryActionTriggered;
            View.AddSubview( DismissButton );

            //
            // Setup the focus guide for when the users moves down from dismiss
            // button.
            //
            FocusGuide = new UIFocusGuide();
            View.AddLayoutGuide( FocusGuide );
            FocusGuide.LeftAnchor.ConstraintEqualTo( DismissButton.LeftAnchor ).Active = true;
            FocusGuide.RightAnchor.ConstraintEqualTo( DismissButton.RightAnchor ).Active = true;
            FocusGuide.TopAnchor.ConstraintEqualTo( DismissButton.BottomAnchor ).Active = true;
            FocusGuide.BottomAnchor.ConstraintEqualTo( View.BottomAnchor ).Active = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the current notification.
        /// </summary>
        private void ShowCurrentNotification()
        {
            UIView.AnimateNotify( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
            {
                View.Frame = new CGRect( 0, 0, View.Frame.Size.Width, View.Frame.Size.Height );
            }, ( bool finished ) =>
            {
                NotificationWasShown?.Invoke( this, new EventArgs() );
            } );
        }

        /// <summary>
        /// Hides the current notification.
        /// </summary>
        private void HideCurrentNotification()
        {
            UIView.AnimateNotify( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
            {
                View.Frame = new CGRect( 0, -View.Frame.Size.Height, View.Frame.Size.Width, View.Frame.Size.Height );
            }, ( bool completed ) =>
            {
                NotificationWasDismissed?.Invoke( this, new EventArgs() );
            } );
        }

        /// <summary>
        /// Shows the notification.
        /// </summary>
        /// <param name="notification">The notification to be shown.</param>
        public void ShowNotification( Rest.Notification notification )
        {
            View.Frame = new CGRect( 0, -View.Frame.Size.Height, View.Frame.Size.Width, View.Frame.Size.Height );
            Notification = notification;

            if ( notification == null )
            {
                return;
            }

            MessageLabel.Text = notification.Message;

            if ( notification.Image != null )
            {
                Task.Run( async () =>
                {
                    UIImage image;

                    try
                    {
                        image = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( notification.Image.BestMatch ) );
                    }
                    catch
                    {
                        image = null;
                    }

                    InvokeOnMainThread( () =>
                    {
                        ImageView.Image = image;

                        ShowCurrentNotification();
                    } );
                } );
            }
            else
            {
                ImageView.Image = null;
                ShowCurrentNotification();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the PrimaryActionTriggered event for the DismissButton control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments that describe this event.</param>
        void DismissButton_PrimaryActionTriggered( object sender, EventArgs e )
        {
            NSUserDefaults.StandardUserDefaults.SetString( Notification.StartDateTime.ToString( "s" ), "Crex.LastSeenNotification" );
            HideCurrentNotification();
        }

        #endregion
    }
}
