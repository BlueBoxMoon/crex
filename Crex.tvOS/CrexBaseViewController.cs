using System;
using UIKit;

namespace Crex.tvOS
{
    public class CrexBaseViewController : UIViewController
    {
        public string Data { get; set; }

        /// <summary>
        /// Creates the view needed for this controller.
        /// </summary>
        public override void LoadView()
        {
            View = new UIView()
            {
                BackgroundColor = new UIColor( 0.07f, 1 )
            };
        }

        protected void ShowUpdateRequiredDialog()
        {
            var alertController = UIAlertController.Create( "Update Required",
                                                  "An update is required to view this content.",
                                                  UIAlertControllerStyle.Alert );

            var action = UIAlertAction.Create( "Close", UIAlertActionStyle.Cancel, ( alert ) =>
            {
                Console.WriteLine( "Close" );
            } );
            alertController.AddAction( action );

            InvokeOnMainThread( () =>
            {
                PresentViewController( alertController, true, null );
            } );
        }

        protected void ShowDataErrorDialog( Action retry )
        {
            InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Error loading data",
                                                      "An error occurred trying to laod the content. Please try again later.",
                                                      UIAlertControllerStyle.Alert );

                var action = UIAlertAction.Create( "Retry", UIAlertActionStyle.Default, ( alert ) =>
                {
                    Console.WriteLine( "Retry" );
                    retry();
                } );
                alertController.AddAction( action );

                if ( NavigationController.ViewControllers[0] != this )
                {
                    action = UIAlertAction.Create( "Cancel", UIAlertActionStyle.Cancel, ( alert ) =>
                    {
                        Console.WriteLine( "Cancel" );
                        NavigationController.PopViewController( true );
                    } );
                    alertController.AddAction( action );
                }

                PresentViewController( alertController, true, null );
            } );
        }
    }
}
