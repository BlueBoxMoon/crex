using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Crex.iOS
{
    public class Application : Crex.Application
    {
        public Application()
            : base( System.IO.File.OpenRead( NSBundle.MainBundle.PathForResource( "config", "json" ) ), new Resolution( 1920, 1080 ) )
        {
        }

        public override void Run( object sender )
        {
            UIWindow window = ( UIWindow ) sender;

            UIViewController vc = new ViewControllers.MainMenuViewController();
            window.RootViewController = vc;

            window.MakeKeyAndVisible();
        }

        public override void StartAction( object sender, string template, object data )
        {
            throw new NotImplementedException();
        }
    }
}