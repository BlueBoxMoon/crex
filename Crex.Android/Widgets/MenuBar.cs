using System;
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

        #region Fields

        Color unfocusedColor;
        Color focusedColor;

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
            SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.MenuBar.BackgroundColor ) );

            //
            // Ensure that our internal gravity is centered so the buttons line up correctly.
            //
            SetHorizontalGravity( GravityFlags.Center );
            SetVerticalGravity( GravityFlags.Center );

            //
            // Save a reference to the actual color objects so we don't have to keep
            // creating new color objects each time the selection changes.
            //
            unfocusedColor = Color.ParseColor( Crex.Application.Current.Config.MenuBar.UnfocusedTextColor );
            focusedColor = Color.ParseColor( Crex.Application.Current.Config.MenuBar.FocusedTextColor );
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
                var button = new Button( Context )
                {
                    Text = buttonTitles[i],
                    Tag = i
                };

                button.SetBackgroundColor( Color.Transparent );
                button.SetTextColor( unfocusedColor );
                button.SetTextSize( ComplexUnitType.Dip, 17 );
                button.SetPadding( 20, 0, 20, 0 );
                button.FocusChange += button_FocusChange;
                button.Click += button_Click;

                AddView( button );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void button_Click( object sender, EventArgs e )
        {
            ButtonClicked( this, new ButtonClickEventArgs( ( int ) ( ( Button ) sender ).Tag ) );
        }

        /// <summary>
        /// Handles the FocusChange event of the button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FocusChangeEventArgs"/> instance containing the event data.</param>
        private void button_FocusChange( object sender, FocusChangeEventArgs e )
        {
            if ( e.HasFocus )
            {
                ( ( Button ) sender ).SetTextColor( focusedColor );
            }
            else
            {
                ( ( Button ) sender ).SetTextColor( unfocusedColor );
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