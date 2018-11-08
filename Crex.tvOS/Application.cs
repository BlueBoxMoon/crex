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

                await StartAction( navigationController, Current.Config.ApplicationRootUrl );
                window.MakeKeyAndVisible();
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
            UIViewController viewController = ( UIViewController ) sender;
            NavigationController navigationController = viewController is NavigationController
                ? ( NavigationController ) viewController
                : ( NavigationController ) viewController.NavigationController;

            Console.WriteLine( $"Navigation to { url }" );

            navigationController.ShowLoading();

            //
            // Retrieve the data from the server.
            //
            var json = await new System.Net.Http.HttpClient().GetStringAsync( url );
            var action = json.FromJson<Rest.CrexAction>();

            //
            // Check if we were able to load the data.
            //
            if ( action == null )
            {
                navigationController.HideLoading();
                ShowDataErrorDialog( navigationController, null );

                return;
            }

            await StartAction( sender, action );
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="action">The action to be started.</param>
        public override async Task StartAction( object sender, Rest.CrexAction action )
        {
            UIViewController viewController = ( UIViewController ) sender;
            NavigationController navigationController = viewController is NavigationController
                ? ( NavigationController ) viewController
                : ( NavigationController ) viewController.NavigationController;

            navigationController.ShowLoading();

            //
            // Check if we can display this action.
            //
            if ( action.RequiredCrexVersion.HasValue && action.RequiredCrexVersion.Value > CrexVersion )
            {
                navigationController.HideLoading();
                ShowUpdateRequiredDialog( viewController, null );

                return;
            }

            //
            // Load the new view controller from the template.
            //
            var newViewController = GetViewControllerForTemplate( action.Template );
            newViewController.Data = action.Data.ToJson();
            try
            {
                await newViewController.LoadContentAsync();
            }
            catch
            {
                navigationController.HideLoading();
                ShowDataErrorDialog( navigationController, null );

                return;
            }

            //
            // Either make this new view the root view controller or push it
            // onto the stack.
            //
            if ( viewController is UINavigationController )
            {
                ( ( UINavigationController ) viewController ).ViewControllers = new[] { newViewController };
            }
            else
            {
                viewController.NavigationController.PushViewController( newViewController, true );
            }

            navigationController.HideLoading();
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
        public static void ShowUpdateRequiredDialog( UIViewController viewController, Action close )
        {
            viewController.InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Update Required",
                                                  "An update is required to view this content.",
                                                  UIAlertControllerStyle.Alert );

                if ( viewController is UINavigationController && ( ( UINavigationController ) viewController ).ViewControllers.Length > 0 )
                {
                    var action = UIAlertAction.Create( "Close", UIAlertActionStyle.Cancel, ( alert ) => { close?.Invoke(); } );
                    alertController.AddAction( action );
                }

                viewController.PresentViewController( alertController, true, null );
            } );
        }

        /// <summary>
        /// Shows an error to the user indicating that we could not load the
        /// data correctly. They can Retry or, if not the root view controller,
        /// they can Cancel.
        /// </summary>
        /// <param name="retry">The action to be performed when Retry is pressed.</param>
        public static void ShowDataErrorDialog( UIViewController viewController, Action retry )
        {
            viewController.InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Error Loading Data",
                                                      "An error occurred trying to load the content. Please try again later.",
                                                      UIAlertControllerStyle.Alert );

                //
                // If they specified a retry action, include it.
                //
                if ( retry != null )
                {
                    var action = UIAlertAction.Create( "Retry", UIAlertActionStyle.Default, ( alert ) =>
                    {
                        retry?.Invoke();
                    } );
                    alertController.AddAction( action );
                }

                if ( viewController is UINavigationController && ( ( UINavigationController ) viewController ).ViewControllers.Length > 0 )
                {
                    var action = UIAlertAction.Create( "Cancel", UIAlertActionStyle.Cancel, ( alert ) =>
                    {
                        ( ( UINavigationController ) viewController ).PopViewController( true );
                    } );
                    alertController.AddAction( action );
                }

                viewController.PresentViewController( alertController, true, null );
            } );
        }
    }
}