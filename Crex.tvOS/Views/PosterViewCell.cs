using System;
using Crex.tvOS.Extensions;
using UIKit;

namespace Crex.tvOS.Views
{
    public class PosterViewCell : UITableViewCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.PosterViewCell"/> class.
        /// </summary>
        /// <param name="style">Style.</param>
        /// <param name="reuseIdentifier">Reuse identifier.</param>
        public PosterViewCell( UITableViewCellStyle style, string reuseIdentifier )
            : base( style, reuseIdentifier )
        {
            FocusStyle = UITableViewCellFocusStyle.Custom;
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        /// <summary>
        /// The focus has updated. Update this cell to reflect whether it has
        /// focus or not.
        /// </summary>
        /// <param name="context">The focus context.</param>
        /// <param name="coordinator">The animation coordinator.</param>
        public override void DidUpdateFocus( UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator )
        {
            base.DidUpdateFocus( context, coordinator );

            if ( context.NextFocusedView == this )
            {
                coordinator.AddCoordinatedAnimations( () =>
                {
                    ContentView.BackgroundColor = "#32ffffff".AsUIColor();
                }, null );
            }
            else
            {
                coordinator.AddCoordinatedAnimations( () =>
                {
                    ContentView.BackgroundColor = UIColor.Clear;
                }, null );
            }
        }
    }
}
