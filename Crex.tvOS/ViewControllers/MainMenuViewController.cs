using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Crex.iOS.ViewControllers
{
    public class MainMenuViewController : UIViewController
    {
        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.Blue;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UILabel label = new UILabel( new CoreGraphics.CGRect( 0, 0, 960, 50 ) );
            label.BackgroundColor = UIColor.Red;
            label.Text = "Goodbye World";

            View.AddSubview( label );
        }
    }
}