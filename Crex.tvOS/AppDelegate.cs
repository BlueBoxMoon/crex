using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Crex.iOS
{
    [Register( "CrexAppDelegate" )]
    [Preserve]
    public class CrexAppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching( UIApplication application, NSDictionary launchOptions )
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            Window = new UIWindow( UIScreen.MainScreen.Bounds );

            UIViewController vc = new ViewControllers.MainMenuViewController();
            Window.RootViewController = vc;

            Window.MakeKeyAndVisible();

            return true;
        }
    }
}
