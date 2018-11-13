using System;
using System.Collections.Generic;

using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Crex.Android.Widgets
{
    public class MenuBar : LinearLayout
    {
        #region Published Events

        /// <summary>
        /// Occurs when a button is clicked.
        /// </summary>
        public event EventHandler<ButtonClickEventArgs> ButtonClicked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the last focused button.
        /// </summary>
        /// <value>
        /// The last focused button.
        /// </value>
        protected int LastFocusedButton { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuBar"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public MenuBar( Context context, IAttributeSet attrs )
            : base( context, attrs )
        {
            //
            // Set our background color to what is configured.
            //
            SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.MenuBarBackgroundColor ) );

            //
            // Ensure that our internal gravity is centered so the buttons line up correctly.
            //
            SetHorizontalGravity( GravityFlags.Center );
            SetVerticalGravity( GravityFlags.Center );

            Focusable = true;
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        protected override void OnSizeChanged( int w, int h, int oldw, int oldh )
        {
            base.OnSizeChanged( w, h, oldw, oldh );

            //
            // Adjust the size of all the menu buttons.
            //
            for ( int i = 0; i < ChildCount; i++ )
            {
                if ( GetChildAt( i ) is MenuButton button )
                {
                    button.LayoutParameters = new LayoutParams( ViewGroup.LayoutParams.WrapContent, h / 2 );
                }
            }

            Measure( MeasureSpec.MakeMeasureSpec( MeasuredWidth, MeasureSpecMode.Exactly ), MeasureSpec.MakeMeasureSpec( MeasuredHeight, MeasureSpecMode.Exactly ) );
        }

        /// <summary>
        /// Call this to try to give focus to a specific view or to one of its descendants
        /// and give it hints about the direction and a specific rectangle that the focus
        /// is coming from.
        /// </summary>
        /// <param name="direction">One of FOCUS_UP, FOCUS_DOWN, FOCUS_LEFT, and FOCUS_RIGHT</param>
        /// <param name="previouslyFocusedRect">The rectangle (in this View's coordinate system)
        /// to give a finer grained hint about where focus is coming from.  May be null
        /// if there is no hint.</param>
        public override bool RequestFocus( [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect )
        {
            if ( LastFocusedButton < ChildCount )
            {
                GetChildAt( LastFocusedButton ).RequestFocus();

                return true;
            }

            return base.RequestFocus( direction, previouslyFocusedRect );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the button list to the indicated titles.
        /// </summary>
        /// <param name="buttonTitles">The button titles.</param>
        public void SetButtons( List<string> buttonTitles )
        {
            RemoveAllViews();

            for ( int i = 0; i < buttonTitles.Count; i++ )
            {
                var button = new MenuButton( Context )
                {
                    Text = buttonTitles[i],
                    Tag = i,
                    LayoutParameters = new LayoutParams( ViewGroup.LayoutParams.WrapContent, Height / 2 )
                };

                button.Click += (sender, e) =>
                {
                    ButtonClicked?.Invoke( this, new ButtonClickEventArgs( ( int ) ( ( Button ) sender ).Tag ) );
                };

                button.FocusChange += ( sender, e ) =>
                {
                    if ( e.HasFocus )
                    {
                        LastFocusedButton = ( int ) ( ( Button ) sender ).Tag;
                    }
                };

                AddView( button );
            }
        }

        #endregion
    }

    /// <summary>
    /// Event arguments that identify which button was clicked.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ButtonClickEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the button position.
        /// </summary>
        /// <value>
        /// The button position.
        /// </value>
        public int Position { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonClickEventArgs"/> class.
        /// </summary>
        /// <param name="buttonIndex">Index of the button.</param>
        public ButtonClickEventArgs( int buttonIndex )
        {
            Position = buttonIndex;
        }
    }
}