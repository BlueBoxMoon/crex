﻿using System;
using System.Collections.Generic;

using Android.Content;
using Android.Graphics;
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
        }

        #endregion

        #region Base Method Overrides

        protected override void OnSizeChanged( int w, int h, int oldw, int oldh )
        {
            base.OnSizeChanged( w, h, oldw, oldh );

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
        /// Look for a descendant to call <c><see cref="M:Android.Views.View.RequestFocus" /></c> on.
        /// </summary>
        /// <param name="direction">One of FOCUS_UP, FOCUS_DOWN, FOCUS_LEFT, and FOCUS_RIGHT</param>
        /// <param name="previouslyFocusedRect">The rectangle (in this View's coordinate system)
        /// to give a finer grained hint about where focus is coming from.  May be null
        /// if there is no hint.</param>
        /// <returns>true to indicate the event has been handled.</returns>
        protected override bool OnRequestFocusInDescendants( int direction, Rect previouslyFocusedRect )
        {
            if ( LastFocusedButton < ChildCount )
            {
                GetChildAt( LastFocusedButton ).RequestFocus();

                return true;
            }

            return base.OnRequestFocusInDescendants( direction, previouslyFocusedRect );
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