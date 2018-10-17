using System;

using Foundation;
using UIKit;

using Crex.Extensions;

namespace Crex.tvOS
{
    public class Application : Crex.Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Application"/> class.
        /// </summary>
        public Application()
            : base( System.IO.File.OpenRead( NSBundle.MainBundle.PathForResource( "config", "json" ) ), new Resolution( ( int ) UIScreen.MainScreen.NativeBounds.Size.Width, ( int ) UIScreen.MainScreen.NativeBounds.Size.Height ) )
        {
        }

        /// <summary>
        /// Runs the Crex application.
        /// </summary>
        /// <param name="sender">Thew UIWindow object created for us.</param>
        public override void Run( object sender )
        {
            UIWindow window = ( UIWindow ) sender;

            var type = Type.GetType( $"Crex.tvOS.ViewControllers.{ Current.Config.ApplicationRootTemplate }ViewController" );

            if ( type == null )
            {
                Console.WriteLine( $"Unknown root template specified: { Current.Config.ApplicationRootTemplate }" );
                return;
            }

            var rootViewController = ( CrexBaseViewController ) Activator.CreateInstance( type );
            rootViewController.Data = Current.Config.ApplicationRootUrl.ToJson();

            window.RootViewController = new UINavigationController( rootViewController );
            window.MakeKeyAndVisible();
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="action">The action to be started.</param>
        public override void StartAction( object sender, Rest.CrexAction action )
        {
            UIViewController viewController = ( UIViewController ) sender;
            var type = Type.GetType( $"Crex.tvOS.ViewControllers.{ action.Template }ViewController" );

            if ( type == null )
            {
                Console.WriteLine( $"Unknown template specified: { action.Template }" );
                return;
            }

            Console.WriteLine( $"Navigation to { action.Template }" );

            var newViewController = (CrexBaseViewController)Activator.CreateInstance( type );
            newViewController.Data = action.Data.ToJson();

            viewController.NavigationController.PushViewController( newViewController, true );
        }
    }
}