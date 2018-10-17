﻿using Foundation;
using UIKit;

namespace Sample.AppleTV
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register( "AppDelegate" )]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching( UIApplication application, NSDictionary launchOptions )
        {
            Window = new UIWindow( UIScreen.MainScreen.Bounds );

            new Crex.tvOS.Application().Run( Window );

            return true;
        }
    }
}
