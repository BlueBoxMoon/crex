using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

using Crex.tvOS.Extensions;
using CoreGraphics;

namespace Crex.tvOS.Views
{
    public class MenuBarView : UIView
    {
        #region Published Events

        /// <summary>
        /// Occurs when button is clicked.
        /// </summary>
        public event EventHandler<ButtonClickEventArgs> ButtonClicked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the focused button.
        /// </summary>
        /// <value>The index of the focused button.</value>
        protected int FocusedButtonIndex { get; private set; } = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.MenuBarView"/> class.
        /// </summary>
        public MenuBarView()
            : base()
        {
            BackgroundColor = Crex.Application.Current.Config.MenuBarBackgroundColor.AsUIColor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.MenuBarView"/> class.
        /// </summary>
        /// <param name="frame">The initial frame to use.</param>
        public MenuBarView( CGRect frame )
            : base( frame )
        {
            BackgroundColor = Crex.Application.Current.Config.MenuBarBackgroundColor.AsUIColor();
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Gets the preferred focus environments.
        /// </summary>
        /// <value>The preferred focus environments.</value>
        public override IUIFocusEnvironment[] PreferredFocusEnvironments
        {
            get
            {
                var buttons = Subviews.Cast<MenuButton>().ToList();

                return FocusedButtonIndex >= buttons.Count ? base.PreferredFocusEnvironments : ( new[] { buttons[FocusedButtonIndex] } );
            }
        }

        /// <summary>
        /// The focus has moved to a new control. Track the focused button
        /// if it's one of ours.
        /// </summary>
        /// <param name="context">Focus update context.</param>
        /// <param name="coordinator">Focus animation coordinator.</param>
        public override void DidUpdateFocus( UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator )
        {
            base.DidUpdateFocus( context, coordinator );

            var buttons = Subviews.Cast<MenuButton>().ToList();

            for ( int i = 0; i < buttons.Count;  i++ )
            {
                if ( context.NextFocusedView == buttons[i])
                {
                    FocusedButtonIndex = i;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets teh button list to the indicated titles.
        /// </summary>
        /// <param name="titles">The titles of the buttons.</param>
        public void SetButtons( IEnumerable<string> titles )
        {
            //
            // Remove existing buttons.
            //
            while ( Subviews.Length > 0 )
            {
                Subviews[0].RemoveFromSuperview();
            }

            //
            // Add each of the new buttons.
            //
            int buttonIndex = 0;
            foreach ( var title in titles )
            {
                var button = new MenuButton()
                {
                    Tag = buttonIndex++
                };

                button.TitleLabel.Font = UIFont.SystemFontOfSize( 34 );
                button.SetTitle( title.ToUpper(), UIControlState.Normal );
                button.SizeToFit();

                button.PrimaryActionTriggered += ( sender, e ) =>
                {
                    ButtonClicked?.Invoke( this, new ButtonClickEventArgs( ( int ) ( ( MenuButton ) sender ).Tag ) );
                };

                AddSubview( button );
            }

            LayoutButtons();
        }

        /// <summary>
        /// Layouts the buttons so they are centered horizontally and vertically.
        /// </summary>
        private void LayoutButtons()
        {
            var buttons = Subviews.Cast<MenuButton>().ToList();
            nfloat totalWidth = 0;

            if ( buttons.Count == 0 )
            {
                return;
            }

            //
            // Determine the total width we will be using.
            //
            buttons.ForEach( b =>
            {
                b.SizeToFit();
                totalWidth += b.Frame.Width;
            } );

            //
            // Get our starting position of the first button.
            //
            nfloat x = ( Bounds.Width - totalWidth ) / 2.0f;

            //
            // Position each button sequentially.
            //
            buttons.ForEach( b =>
            {
                nfloat y = Bounds.Size.Height / 4.0f;

                b.Frame = new CGRect( x, y, b.Frame.Size.Width, Bounds.Size.Height / 2.0f );

                x += b.Frame.Size.Width;
            } );
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
        /// The position of the button that was clicked.
        /// </summary>
        /// <value>The position of the button that was clicked.</value>
        public int Position { get; private set; }

        public ButtonClickEventArgs( int buttonIndex )
        {
            Position = buttonIndex;
        }
    }
}
