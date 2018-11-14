using CoreGraphics;
using UIKit;

using Crex.tvOS.Views;
using System.Threading.Tasks;
using System.Threading;
using System;
using Crex.Extensions;
using Foundation;

namespace Crex.tvOS.ViewControllers
{
    public class NavigationController : UINavigationController
    {
        #region Views

        /// <summary>
        /// Gets the loading spinner view.
        /// </summary>
        /// <value>The loading spinner view.</value>
        protected LoadingSpinnerView LoadingSpinnerView { get; private set; }

        /// <summary>
        /// Gets the overlay view.
        /// </summary>
        /// <value>The overlay view.</value>
        protected FocusView OverlayView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the preferred focus environments.
        /// </summary>
        /// <value>The preferred focus environments.</value>
        public override IUIFocusEnvironment[] PreferredFocusEnvironments
        {
            get
            {
                return OverlayView.FocusEnabled ? new[] { OverlayView } : base.PreferredFocusEnvironments;
            }
        }

        /// <summary>
        /// Gets the loading cancellation token source.
        /// </summary>
        /// <value>The loading cancellation token source.</value>
        protected CancellationTokenSource LoadingCancellationTokenSource { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded and is ready for final initialization.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            OverlayView = new FocusView
            {
                Frame = new CGRect( 0, 0, 1920, 1080 ),
                Hidden = true,
                FocusEnabled = false
            };
            View.AddSubview( OverlayView );

            LoadingSpinnerView = new LoadingSpinnerView( new CGRect( 880, 460, 160, 160 ) );
            OverlayView.AddSubview( LoadingSpinnerView );
        }

        /// <summary>
        /// A button has been pressed, ignore it if we are currently loading.
        /// </summary>
        /// <param name="presses">Presses.</param>
        /// <param name="evt">Evt.</param>
        public override void PressesBegan( NSSet<UIPress> presses, UIPressesEvent evt )
        {
            if ( LoadingCancellationTokenSource != null )
            {
                return;
            }

            base.PressesBegan( presses, evt );
        }

        /// <summary>
        /// A button press has changed, ignore it if we are loading.
        /// </summary>
        /// <param name="presses">Presses.</param>
        /// <param name="evt">Evt.</param>
        public override void PressesChanged( NSSet<UIPress> presses, UIPressesEvent evt )
        {
            if ( LoadingCancellationTokenSource != null )
            {
                return;
            }

            base.PressesChanged( presses, evt );
        }

        /// <summary>
        /// A button press has ended. Check if we need to handle it.
        /// </summary>
        /// <param name="presses">Presses.</param>
        /// <param name="evt">Evt.</param>
        public override void PressesEnded( NSSet<UIPress> presses, UIPressesEvent evt )
        {
            //
            // Check for menu button while loading.
            //
            if ( LoadingCancellationTokenSource != null )
            {
                foreach ( UIPress press in presses )
                {
                    if ( press.Type == UIPressType.Menu )
                    {
                        LoadingCancellationTokenSource.Cancel();
                        LoadingCancellationTokenSource = null;
                        HideLoading();
                    }
                }

                Console.WriteLine( "Ignored press ended" );
                return;
            }

            base.PressesBegan( presses, evt );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the action specified by the URL.
        /// </summary>
        /// <param name="url">URL.</param>
        public async Task StartAction( string url )
        {
            LoadingCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartAction( url, LoadingCancellationTokenSource.Token );
            }
            catch ( Exception e)
            {
                Console.WriteLine( e.Message );
            }
        }

        /// <summary>
        /// Starts the action specified by the URL
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task StartAction( string url, CancellationToken cancellationToken )
        {
            ShowLoading();

            //
            // Retrieve the data from the server.
            //
            var json = await new System.Net.Http.HttpClient().GetStringAsync( url );
            var action = json.FromJson<Rest.CrexAction>();

            cancellationToken.ThrowIfCancellationRequested();

            await StartAction( action, cancellationToken );
        }

        /// <summary>
        /// Starts the action.
        /// </summary>
        /// <param name="action">Action.</param>
        public async Task StartAction( Rest.CrexAction action )
        {
            LoadingCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartAction( action, LoadingCancellationTokenSource.Token );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
        }

        /// <summary>
        /// Starts the action.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task StartAction( Rest.CrexAction action, CancellationToken cancellationToken )
        {
            //
            // Check if we were able to load the data.
            //
            if ( action == null )
            {
                HideLoading();
                ShowDataErrorDialog( null );

                return;
            }

            //
            // Check if we can display this action.
            //
            if ( action.RequiredCrexVersion.HasValue && action.RequiredCrexVersion.Value > Crex.Application.Current.CrexVersion )
            {
                HideLoading();
                ShowUpdateRequiredDialog();

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            ShowLoading();

            //
            // Load the new view controller from the template.
            //
            var newViewController = GetViewControllerForTemplate( action.Template );
            newViewController.Data = action.Data.ToJson();
            try
            {
                await Task.Delay( 2000 );
                await newViewController.LoadContentAsync();
            }
            catch
            {
                HideLoading();
                ShowDataErrorDialog( null );

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            LoadingCancellationTokenSource = null;

            if (ViewControllers.Length == 0)
            {
                ViewControllers = new[] { newViewController };
            }
            else
            {
                PushViewController( newViewController, true );
            }

            HideLoading();
        }

        /// <summary>
        /// Shows the loading overlay.
        /// </summary>
        public void ShowLoading()
        {
            OverlayView.Hidden = false;
            OverlayView.FocusEnabled = true;
            LoadingSpinnerView.Start();

            SetNeedsFocusUpdate();
        }

        /// <summary>
        /// Hides the loading overlay.
        /// </summary>
        public void HideLoading()
        {
            LoadingSpinnerView.Stop( () =>
            {
                OverlayView.Hidden = true;
            } );

            OverlayView.FocusEnabled = false;
            SetNeedsFocusUpdate();
        }

        /// <summary>
        /// Gets the view controller for template.
        /// </summary>
        /// <returns>The view controller for template.</returns>
        /// <param name="template">Template.</param>
        private CrexBaseViewController GetViewControllerForTemplate( string template )
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
        protected void ShowUpdateRequiredDialog()
        {
            InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Update Required",
                                                  "An update is required to view this content.",
                                                  UIAlertControllerStyle.Alert );

                if ( ViewControllers.Length > 0 )
                {
                    var action = UIAlertAction.Create( "Close", UIAlertActionStyle.Cancel, ( alert ) => { } );
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
                                                      "An error occurred trying to load the content. Please try again later.",
                                                      UIAlertControllerStyle.Alert );

                //
                // If they specified a retry action, include it.
                //
                if ( retry != null )
                {
                    var action = UIAlertAction.Create( "Retry", UIAlertActionStyle.Default, ( alert ) =>
                    {
                        retry.Invoke();
                    } );
                    alertController.AddAction( action );
                }

                if ( ViewControllers.Length > 0 )
                {
                    var action = UIAlertAction.Create( "Cancel", UIAlertActionStyle.Cancel, ( alert ) => { } );
                    alertController.AddAction( action );
                }

                PresentViewController( alertController, true, null );
            } );
        }

        #endregion
    }
}
