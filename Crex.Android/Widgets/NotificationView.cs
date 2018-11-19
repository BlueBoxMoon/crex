using System;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Crex.Android.Widgets
{
    public class NotificationView : FrameLayout
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
        /// Gets the container view.
        /// </summary>
        /// <value>
        /// The container view.
        /// </value>
        protected FrameLayout ContainerView { get; private set; }

        /// <summary>
        /// Gets the imageview.
        /// </summary>
        /// <value>
        /// The imageview.
        /// </value>
        protected ImageView ImageView { get; private set; }

        /// <summary>
        /// Gets the message view.
        /// </summary>
        /// <value>
        /// The message view.
        /// </value>
        protected TextView MessageView { get; private set; }

        /// <summary>
        /// Gets the dismiss button.
        /// </summary>
        /// <value>
        /// The dismiss button.
        /// </value>
        protected MenuButton DismissButton { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notification.
        /// </summary>
        /// <value>
        /// The notification.
        /// </value>
        public Rest.Notification Notification { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuBar"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public NotificationView( Context context )
            : base( context )
        {
            //
            // Configure the container view that will be animated.
            //
            ContainerView = new FrameLayout( context )
            {
                LayoutParameters = new LayoutParams( ViewGroup.LayoutParams.MatchParent, Utility.DipToPixel( 120 ) )
                {
                    Gravity = GravityFlags.Top
                },
                TranslationY = Utility.DipToPixel( -120 ),
                Visibility = ViewStates.Invisible
            };
            ContainerView.SetBackgroundColor( Color.ParseColor( "#d0323232" ) );
            AddView( ContainerView );

            //
            // Configure the image view.
            //
            ImageView = new ImageView( context )
            {
                LayoutParameters = new LayoutParams( Utility.DipToPixel( 144 ), Utility.DipToPixel( 81 ) )
                {
                    LeftMargin = Utility.DipToPixel( 20 ),
                    TopMargin = Utility.DipToPixel( 20 ),
                    Gravity = GravityFlags.Top | GravityFlags.Left
                }
            };
            ImageView.SetScaleType( ImageView.ScaleType.FitCenter );
            ContainerView.AddView( ImageView );

            //
            // Configure the message text view.
            //
            MessageView = new TextView( context )
            {
                LayoutParameters = new LayoutParams( ViewGroup.LayoutParams.MatchParent, Utility.DipToPixel( 81 ) )
                {
                    TopMargin = Utility.DipToPixel( 20 ),
                    LeftMargin = Utility.DipToPixel( 184 ),
                    RightMargin = Utility.DipToPixel( 184 ),
                    Gravity = GravityFlags.Top
                },
                TextSize = 16,
                TextAlignment = TextAlignment.Center,
                Ellipsize = global::Android.Text.TextUtils.TruncateAt.End,
            };
            MessageView.SetTextColor( Color.ParseColor( "#ffdddddd" ) );
            MessageView.SetMaxLines( 4 );
            MessageView.SetSingleLine( false );
            ContainerView.AddView( MessageView );

            //
            // Configure the dismiss button.
            //
            DismissButton = new MenuButton( context )
            {
                LayoutParameters = new LayoutParams( Utility.DipToPixel( 144 ), Utility.DipToPixel( 32 ) )
                {
                    BottomMargin = Utility.DipToPixel( 20 ),
                    RightMargin = Utility.DipToPixel( 19 ),
                    Gravity = GravityFlags.Bottom | GravityFlags.Right
                },
                Title = "Dismiss"
            };
            DismissButton.Click += DismissButton_Click;
            ContainerView.AddView( DismissButton );
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Look for a descendant to call <c><see cref="M:Android.Views.View.RequestFocus" /></c> on.
        /// </summary>
        /// <param name="direction">One of FOCUS_UP, FOCUS_DOWN, FOCUS_LEFT, and FOCUS_RIGHT</param>
        /// <param name="previouslyFocusedRect">The rectangle (in this View's coordinate system)
        /// to give a finer grained hint about where focus is coming from.  May be null
        /// if there is no hint.</param>
        /// <returns>true to indicate the event has been handled.</returns>
        protected override bool OnRequestFocusInDescendants( int direction, Rect previouslyFocusedRect )
        {
            DismissButton.RequestFocus();

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the current notification.
        /// </summary>
        private void ShowCurrentNotification()
        {
            var animation = ObjectAnimator.OfFloat( ContainerView, "translationY", Utility.DipToPixel( 0 ) );

            animation.SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
            animation.AnimationEnd += ( sender, args ) =>
            {
                NotificationWasShown?.Invoke( this, new EventArgs() );
            };

            ContainerView.ClearAnimation();
            ContainerView.TranslationY = Utility.DipToPixel( -120 );
            ContainerView.Visibility = ViewStates.Visible;
            DismissButton.Focusable = true;

            animation.Start();
        }

        /// <summary>
        /// Hides the current notification.
        /// </summary>
        private void HideCurrentNotification()
        {
            var animation = ObjectAnimator.OfFloat( ContainerView, "translationY", Utility.DipToPixel( -120 ) );

            animation.SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
            animation.AnimationEnd += ( sender, args ) =>
            {
                ContainerView.Visibility = ViewStates.Invisible;
                NotificationWasDismissed?.Invoke( this, new EventArgs() );
            };

            ContainerView.ClearAnimation();
            DismissButton.ClearFocus();
            DismissButton.Focusable = false;

            animation.Start();
        }

        /// <summary>
        /// Shows the notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        public void ShowNotification( Rest.Notification notification )
        {
            Notification = notification;

            //
            // If no notification, then clear the notification view immediately.
            //
            if ( notification == null )
            {
                ContainerView.ClearAnimation();
                ContainerView.Visibility = ViewStates.Invisible;
                ContainerView.TranslationY = Utility.DipToPixel( -120 );

                DismissButton.Focusable = false;
                DismissButton.ClearFocus();

                return;
            }

            MessageView.Text = notification.Message;

            if ( notification.Image != null )
            {
                Task.Run( async () =>
                {
                    Bitmap image;

                    try
                    {
                        image = await Utility.LoadImageFromUrlAsync( notification.Image.BestMatch );
                    }
                    catch
                    {
                        image = null;
                    }

                    Post( () =>
                    {
                        ImageView.SetImageBitmap( image );
                        ShowCurrentNotification();
                    } );
                } );
            }
            else
            {
                ImageView.SetImageBitmap( null );
                ShowCurrentNotification();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the DismissButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void DismissButton_Click( object sender, EventArgs e )
        {
            Crex.Application.Current.Preferences.SetDateTimeValue( "Crex.LastSeenNotification", Notification.StartDateTime );
            HideCurrentNotification();
        }

        #endregion
    }
}