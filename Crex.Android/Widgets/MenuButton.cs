using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Crex.Android.Widgets
{
    public class MenuButton : Button
    {
        /// <summary>
        /// Gets or sets the title of the button.
        /// </summary>
        /// <value>The title of the button.</value>
        public string Title
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = ( value ?? string.Empty ).ToUpper();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.Android.Widgets.MenuButton"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public MenuButton( Context context )
            : base( context )
        {
            SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedBackgroundColor ) );
            SetTextColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedTextColor ) );
            SetTextSize( ComplexUnitType.Dip, 17 );
            SetPadding( 20, 0, 20, 0 );
        }

        /// <summary>
        /// Called when the focus has changed on this view.
        /// </summary>
        /// <param name="gainFocus">If set to <c>true</c> gain focus.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="previouslyFocusedRect">Previously focused rect.</param>
        protected override void OnFocusChanged( bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect )
        {
            base.OnFocusChanged( gainFocus, direction, previouslyFocusedRect );

            if ( gainFocus )
            {
                SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.FocusedBackgroundColor ) );
                SetTextColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.FocusedTextColor ) );
            }
            else
            {
                SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedBackgroundColor ) );
                SetTextColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedTextColor ) );
            }
        }
    }
}
