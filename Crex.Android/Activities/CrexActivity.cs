using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "Crex")]
	public class CrexActivity : Activity
    {
        #region Views

        /// <summary>
        /// Gets the loading spinner view.
        /// </summary>
        /// <value>
        /// The loading spinner view.
        /// </value>
        protected Widgets.LoadingSpinner LoadingSpinnerView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the main activity.
        /// </summary>
        /// <value>
        /// The main activity.
        /// </value>
        public static CrexActivity MainActivity { get; private set; }

        /// <summary>
        /// Gets the fragments.
        /// </summary>
        /// <value>
        /// The fragments.
        /// </value>
        protected List<CrexBaseFragment> Fragments { get; private set; } = new List<CrexBaseFragment>();

        /// <summary>
        /// Gets the loading cancellation token source.
        /// </summary>
        /// <value>
        /// The loading cancellation token source.
        /// </value>
        protected CancellationTokenSource LoadingCancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets a value indicating whether we are currently transitioning fragments.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we are currently transitioning fragments; otherwise, <c>false</c>.
        /// </value>
        protected bool InFragmentTransition { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            var view = new FrameLayout( this );
            SetContentView( view );

            //
            // Initialize the loading spinner.
            //
            LoadingSpinnerView = new Widgets.LoadingSpinner( this, null )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ( int ) ( 80 * Resources.DisplayMetrics.Density ), ( int ) ( 80 * Resources.DisplayMetrics.Density ) )
                {
                    Gravity = GravityFlags.Center
                },
                Visibility = ViewStates.Invisible
            };
            view.SetZ( 1 );
            view.AddView( LoadingSpinnerView );

            MainActivity = this;

            //
            // Start the default action.
            //
            Crex.Application.Current.StartAction( this, Crex.Application.Current.Config.ApplicationRootUrl );
        }

        /// <summary>
        /// Called when the activity has detected the user's press of the back
        /// key.
        /// </summary>
        public override void OnBackPressed()
        {
            //
            // Either pop the topmost fragment if we have more than one, if not end the activity.
            //
            if ( Fragments.Count > 1 )
            {
                PopTopFragment();
            }
            else
            {
                Finish();
            }
        }

        /// <summary>
        /// Called to process key events.
        /// </summary>
        /// <param name="e">The key event.</param>
        public override bool DispatchKeyEvent( KeyEvent e )
        {
            //
            // If we are in the middle of a transition, ignore all keys.
            //
            if ( InFragmentTransition == true )
            {
                return true;
            }

            //
            // If we are currently loading a new action then disable everything
            // except the Back button.
            //
            if ( LoadingCancellationTokenSource != null )
            {
                if ( e.KeyCode == Keycode.Back )
                {
                    if ( !Fragments.Any() )
                    {
                        return base.DispatchKeyEvent( e );
                    }

                    LoadingCancellationTokenSource.Cancel();
                    LoadingCancellationTokenSource = null;
                    HideLoading();
                }

                return true;
            }

            return base.DispatchKeyEvent( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="url">The url to the action to be started.</param>
        public async Task StartAction( string url )
        {
            LoadingCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartAction( url, LoadingCancellationTokenSource.Token );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="url">The url to the action to be started.</param>
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
        /// Starts the view template.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action that should be loaded.</param>
        public async Task StartAction( Rest.CrexAction action )
        {
            LoadingCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartAction( action, LoadingCancellationTokenSource.Token );
            }
            catch (Exception e)
            {
                Console.WriteLine( e.Message );
            }
        }

        /// <summary>
        /// Starts the view template.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action that should be loaded.</param>
        private async Task StartAction( Rest.CrexAction action, CancellationToken cancellationToken )
        { 
            //
            // Check if we were able to load the data.
            //
            if ( action == null )
            {
                HideLoading();
                ShowDataErrorDialog( null, () =>
                {
                    if ( Fragments.Count == 0 )
                    {
                        Finish();
                    }
                } );

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

            var fragment = GetFragmentForTemplate( action.Template );
            fragment.Data = action.Data.ToJson();

            try
            {
                await fragment.LoadContentAsync();
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
                HideLoading();
                ShowDataErrorDialog( null, () =>
                {
                    if ( Fragments.Count == 0 )
                    {
                        Finish();
                    }
                } );

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            LoadingCancellationTokenSource = null;
            PushFragment( fragment );
            HideLoading();
        }

        /// <summary>
        /// Pushes the fragment onto the view stack.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        public void PushFragment(CrexBaseFragment fragment)
        {
            var oldFragment = Fragments.LastOrDefault();
            var tx = FragmentManager.BeginTransaction();

            tx.SetCustomAnimations( global::Android.Resource.Animator.FadeIn, global::Android.Resource.Animator.FadeOut, global::Android.Resource.Animator.FadeIn, global::Android.Resource.Animator.FadeOut );
            tx.Add( global::Android.Resource.Id.Content, fragment );
            Fragments.Add( fragment );

            InFragmentTransition = true;
            tx.Commit();
        }

        /// <summary>
        /// Pops the top-most fragment from the view stack.
        /// </summary>
        public void PopTopFragment()
        {
            if ( Fragments.Count > 1 )
            {
                var oldFragment = Fragments.Last();
                var tx = FragmentManager.BeginTransaction();

                tx.SetCustomAnimations( global::Android.Resource.Animator.FadeIn, global::Android.Resource.Animator.FadeOut, global::Android.Resource.Animator.FadeIn, global::Android.Resource.Animator.FadeOut );
                tx.Remove( oldFragment );

                InFragmentTransition = true;
                tx.Commit();
            }
        }

        /// <summary>
        /// A fragment transition animation has started.
        /// </summary>
        /// <param name="enter">if set to <c>true</c> a new fragment is entering.</param>
        public void FragmentAnimationStarted( bool enter )
        {
            if ( enter )
            {
                var oldFragment = Fragments.Count > 1 ? Fragments[Fragments.Count - 2] : null;
                var newFragment = Fragments.Last();

                oldFragment?.OnFragmentWillHide();
                newFragment.OnFragmentWillShow();
            }
            else
            {
                var newFragment = Fragments.Count > 1 ? Fragments[Fragments.Count - 2] : null;
                var oldFragment = Fragments.Last();

                oldFragment.OnFragmentWillHide();

                if ( newFragment != null )
                {
                    newFragment.OnFragmentWillShow();
                    newFragment.View.Visibility = ViewStates.Visible;
                }
            }
        }

        /// <summary>
        /// A fragment transition animation has ended.
        /// </summary>
        /// <param name="enter">if set to <c>true</c> a new fragment has entered.</param>
        public void FragmentAnimationEnded( bool enter )
        {
            if ( enter )
            {
                var oldFragment = Fragments.Count > 1 ? Fragments[Fragments.Count - 2] : null;
                var newFragment = Fragments.Last();

                if ( oldFragment != null )
                {
                    oldFragment.OnFragmentDidHide();
                    oldFragment.View.Visibility = ViewStates.Invisible;
                }

                newFragment.OnFragmentDidShow();
            }
            else
            {
                var newFragment = Fragments.Count > 1 ? Fragments[Fragments.Count - 2] : null;
                var oldFragment = Fragments.Last();

                oldFragment.OnFragmentDidHide();
                newFragment?.OnFragmentDidShow();

                Fragments.Remove( Fragments.Last() );
            }

            InFragmentTransition = false;
        }

        /// <summary>
        /// Shows the loading spinner.
        /// </summary>
        public void ShowLoading()
        {
            LoadingSpinnerView.Start();
            LoadingSpinnerView.Visibility = ViewStates.Visible;
            LoadingSpinnerView.RequestFocus();
        }

        /// <summary>
        /// Hides the loading spinner.
        /// </summary>
        public void HideLoading()
        {
            LoadingSpinnerView.Stop( () =>
            {
                LoadingSpinnerView.Visibility = ViewStates.Invisible;
            } );
        }
        
        /// <summary>
        /// Gets the activity type for template.
        /// </summary>
        /// <returns>The type for template.</returns>
        /// <param name="template">Template.</param>
        protected CrexBaseFragment GetFragmentForTemplate( string template )
        {
            var type = Type.GetType( $"Crex.Android.Templates.{ template }Fragment" );

            if ( type == null )
            {
                Log.Debug( "Crex", $"Unknown template specified: { template }" );
                return null;
            }

            return ( CrexBaseFragment ) Activator.CreateInstance( type );
        }

        /// <summary>
        /// Shows the update required dialog, does not pop the current activity.
        /// </summary>
        protected void ShowUpdateRequiredDialog()
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Update Required" )
                .SetMessage( "An update is required to view this content." )
                .SetPositiveButton( "Close", new Dialogs.OnClickAction() )
                .SetOnCancelListener( new Dialogs.OnCancelAction() );

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Shows the update required dialog.
        /// </summary>
        protected void ShowDataErrorDialog( Action retry, Action cancel = null )
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Error loading data" )
                   .SetMessage( "An error occurred trying to load the content. Please try again later." )
                   .SetOnCancelListener( new Dialogs.OnCancelAction( () => { cancel?.Invoke(); } ) );

            if ( retry != null )
            {
                builder.SetPositiveButton( "Retry", new Dialogs.OnClickAction( retry ) );
            }

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        #endregion
    }
}
