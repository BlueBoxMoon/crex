using System;
using System.Threading.Tasks;

using AVFoundation;
using Crex.Extensions;
using Crex.tvOS.ViewControllers;
using Foundation;
using UIKit;


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
            Preferences = new ApplePreferences();
            AVAudioSession.SharedInstance().SetCategory( AVAudioSession.CategoryPlayback );
        }

        /// <summary>
        /// Runs the Crex application.
        /// </summary>
        /// <param name="sender">Thew UIWindow object created for us.</param>
        public override void Run( object sender )
        {
            UIWindow window = ( UIWindow ) sender;

            async void doRun()
            {
                var navigationController = new NavigationController();
                window.RootViewController = navigationController;
                window.MakeKeyAndVisible();

                await StartAction( navigationController, Current.Config.ApplicationRootUrl );
            }

            window.InvokeOnMainThread( doRun );
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="url">The url to the action to be started.</param>
        public override async Task StartAction( object sender, string url )
        {
            NavigationController navigationController = sender is NavigationController
                ? ( NavigationController ) sender
                : ( NavigationController ) ( ( UIViewController ) sender ).NavigationController;

            url = GetAbsoluteUrl( url );

            Console.WriteLine( $"Navigation to { url }" );

            await navigationController.StartAction( url );
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="action">The action to be started.</param>
        public override async Task StartAction( object sender, Rest.CrexAction action )
        {
            NavigationController navigationController = sender is NavigationController
                ? ( NavigationController ) sender
                : ( NavigationController ) ( ( UIViewController ) sender ).NavigationController;

            await navigationController.StartAction( action );
        }
    }
}