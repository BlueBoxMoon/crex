using System;

using Foundation;
using UIKit;

using Crex.Extensions;
using AVFoundation;

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
            AVAudioSession.SharedInstance().SetCategory( AVAudioSession.CategoryPlayback );
        }

        /// <summary>
        /// Runs the Crex application.
        /// </summary>
        /// <param name="sender">Thew UIWindow object created for us.</param>
        public override void Run( object sender )
        {
            UIWindow window = ( UIWindow ) sender;

            var rootViewController = GetViewControllerForTemplate( Current.Config.ApplicationRootTemplate );
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

            Console.WriteLine( $"Navigation to { action.Template }" );

            //
            // Check if we can display this action.
            //
            if ( action.RequiredCrexVersion.HasValue && action.RequiredCrexVersion.Value > CrexVersion )
            {
                ShowUpdateRequiredDialog( viewController );

                return;
            }

            var newViewController = GetViewControllerForTemplate( action.Template );
            newViewController.Data = action.Data.ToJson();

            viewController.NavigationController.PushViewController( newViewController, true );
        }

        /// <summary>
        /// Gets the view controller for template.
        /// </summary>
        /// <returns>The view controller for template.</returns>
        /// <param name="template">Template.</param>
        private CrexBaseViewController GetViewControllerForTemplate(string template)
        {
            var type = Type.GetType( $"Crex.tvOS.Templates.{ template }ViewController" );

            if ( type == null )
            {
                Console.WriteLine( $"Unknown template specified: { template }" );
                return null;
            }

            return ( CrexBaseViewController ) Activator.CreateInstance( type );
        }

        /// <summary>
        /// Shows the update required dialog. This displays a message to the
        /// user that an update is required to view the content.
        /// </summary>
        protected void ShowUpdateRequiredDialog( UIViewController viewController )
        {
            viewController.InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Update Required",
                                                  "An update is required to view this content.",
                                                  UIAlertControllerStyle.Alert );

                var action = UIAlertAction.Create( "Close", UIAlertActionStyle.Cancel, ( alert ) => { } );
                alertController.AddAction( action );

                viewController.PresentViewController( alertController, true, null );
            } );
        }
    }
}