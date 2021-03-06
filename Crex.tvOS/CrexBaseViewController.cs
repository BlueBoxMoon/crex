﻿using System;
using System.Threading.Tasks;
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
                BackgroundColor = new UIColor( 0.196f, 1 )
            };
        }

        /// <summary>
        /// Ensures that the view has loaded correctly.
        /// </summary>
        public void EnsureView()
        {
            if ( View == null )
            {
                throw new Exception( "Could not load view" );
            }
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public virtual async Task LoadContentAsync()
        {
            //
            // Silence compiler warnings.
            //
            await Task.Delay( 0 );
        }

        /// <summary>
        /// Shows the update required dialog. This displays a message to the
        /// user that an update is required to view the content. If the view
        /// controller is not the root one then a Close button will be available.
        /// </summary>
        protected void ShowUpdateRequiredDialog()
        {
            InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Update Required",
                                                  "An update is required to view this content.",
                                                  UIAlertControllerStyle.Alert );

                if ( NavigationController.ViewControllers[0] != this )
                {
                    var action = UIAlertAction.Create( "Close", UIAlertActionStyle.Cancel, ( alert ) =>
                    {
                        NavigationController.PopViewController( true );
                    } );
                    alertController.AddAction( action );
                }

                PresentViewController( alertController, true, null );
            } );
        }

        /// <summary>
        /// Shows an error to the user indicating that we could not load the
        /// data correctly. They can Retry or, if not the root view controller,
        /// they can Cancel.
        /// </summary>
        /// <param name="retry">The action to be performed when Retry is pressed.</param>
        protected void ShowDataErrorDialog( Action retry )
        {
            InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Error Loading Data",
                                                      "An error occurred trying to laod the content. Please try again later.",
                                                      UIAlertControllerStyle.Alert );

                if ( retry != null )
                {
                    var action = UIAlertAction.Create( "Retry", UIAlertActionStyle.Default, ( alert ) =>
                    {
                        Console.WriteLine( "Retry" );
                        retry();
                    } );
                    alertController.AddAction( action );
                }

                if ( NavigationController.ViewControllers[0] != this )
                {
                    var action = UIAlertAction.Create( "Cancel", UIAlertActionStyle.Cancel, ( alert ) =>
                    {
                        NavigationController.PopViewController( true );
                    } );
                    alertController.AddAction( action );
                }

                PresentViewController( alertController, true, null );
            } );
        }
    }
}
