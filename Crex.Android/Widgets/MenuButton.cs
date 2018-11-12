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
        public MenuButton( Context context )
            : base( context )
        {
            SetBackgroundColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedBackgroundColor ) );
            SetTextColor( Color.ParseColor( Crex.Application.Current.Config.Buttons.UnfocusedTextColor ) );
            SetTextSize( ComplexUnitType.Dip, 17 );
            SetPadding( 20, 0, 20, 0 );
        }

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
