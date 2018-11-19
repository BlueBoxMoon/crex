using CoreGraphics;
using Crex.tvOS.Extensions;
using Foundation;
using UIKit;

namespace Crex.tvOS.Views
{
    [Register("MenuButton")]
    public class MenuButton : UIButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.MenuButton"/> class.
        /// </summary>
        public MenuButton()
        {
            SetTitleColor( Crex.Application.Current.Config.Buttons.FocusedTextColor.AsUIColor(), UIControlState.Focused );
            SetTitleColor( Crex.Application.Current.Config.Buttons.UnfocusedTextColor.AsUIColor(), UIControlState.Normal );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.MenuButton"/> class.
        /// </summary>
        /// <param name="frame">Initial frame of the button.</param>
        public MenuButton( CGRect frame )
            : this()
        {
            Frame = frame;
        }

        /// <summary>
        /// Sizes the control to fit the width required by it's content. Does
        /// not affect the height of the control.
        /// </summary>
        public void SizeWidthToFit()
        {
            var frame = Frame;

            SizeToFit();

            Frame = new CGRect( Frame.Location.X, Frame.Location.Y, Frame.Size.Width, frame.Size.Height );
        }

        /// <summary>
        /// Sizes the control to fit the width and height required by it's content.
        /// </summary>
        public override void SizeToFit()
        {
            base.SizeToFit();

            Frame = new CGRect( Frame.Location.X, Frame.Location.Y, Frame.Size.Width + 40, Frame.Size.Height );
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="forState">For state.</param>
        public override void SetTitle( string title, UIControlState forState )
        {
            base.SetTitle( title.ToUpper(), forState );
        }

        /// <summary>
        /// Focus has changed for this control. Update the background color to
        /// match the style in the config.
        /// </summary>
        /// <param name="context">Focus update context.</param>
        /// <param name="coordinator">Focus animation coordinator.</param>
        public override void DidUpdateFocus( UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator )
        {
            base.DidUpdateFocus( context, coordinator );

            if ( context.NextFocusedView == this )
            {
                coordinator.AddCoordinatedAnimations( () =>
                {
                    BackgroundColor = Crex.Application.Current.Config.Buttons.FocusedBackgroundColor.AsUIColor();
                }, null );
            }
            else
            {
                coordinator.AddCoordinatedAnimations( () =>
                {
                    BackgroundColor = Crex.Application.Current.Config.Buttons.UnfocusedBackgroundColor.AsUIColor();
                }, null );
            }
        }
    }
}
