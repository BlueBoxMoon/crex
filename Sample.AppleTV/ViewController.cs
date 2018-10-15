using System;
using Foundation;
using UIKit;

namespace Sample.AppleTV
{
    public partial class ViewController : UIViewController
    {
        //public ViewController(IntPtr handle) : base(handle)
        //{
        //}

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
            label.Text = "Hello World";

            View.AddSubview( label );
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

