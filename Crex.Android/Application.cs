using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;

using Crex.Extensions;

namespace Crex.Android
{
    public class Application : Crex.Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
            : base( global::Android.App.Application.Context.Assets.Open( "config.json" ), new Resolution( global::Android.App.Application.Context.Resources.DisplayMetrics.WidthPixels, global::Android.App.Application.Context.Resources.DisplayMetrics.HeightPixels ) )
        {
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public override void Run( object sender )
        {
            Activity activity = ( Activity ) sender;

            var intent = new Intent( activity, typeof( Activities.CrexActivity ) );

            intent.AddFlags( ActivityFlags.ClearTop );
            activity.Finish();

            activity.StartActivity( intent );

            Console.WriteLine( Activities.CrexActivity.MainActivity );
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="url">The url to the action to be started.</param>
        public override async Task StartAction( object sender, string url )
        {
            Console.WriteLine( $"Navigation to { url }" );

            if ( sender is CrexBaseActivity )
            {
                //            navigationController.ShowLoading();
            }

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
                if ( sender is CrexBaseActivity )
                {
                    //                navigationController.HideLoading();
                }
                ShowDataErrorDialog( ( Activity ) sender, null );

                return;
            }

            await StartAction( sender, action );
        }

        /// <summary>
        /// Starts the view template.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action that should be loaded.</param>
        public override async Task StartAction( object sender, Rest.CrexAction action )
        {
            Activity currentActivity = ( Activity ) sender;

            //
            // Check if we can display this action.
            //
            if ( action.RequiredCrexVersion.HasValue && action.RequiredCrexVersion.Value > CrexVersion )
            {
                ShowUpdateRequiredDialog( currentActivity );

                return;
            }

            var fragment = GetFragmentForTemplate( action.Template );
            fragment.Data = action.Data.ToJson();

            try
            {
                await fragment.LoadContentAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine( e.Message );
//                navigationController.HideLoading();
//                ShowDataErrorDialog( navigationController, null );

                return;
            }

            Activities.CrexActivity.MainActivity.PushFragment( fragment );

            if ( sender is CrexBaseActivity )
            {
                // Hide Loading
            }

            await Task.Delay( 0 );
        }

        /// <summary>
        /// Gets the activity type for template.
        /// </summary>
        /// <returns>The type for template.</returns>
        /// <param name="template">Template.</param>
        protected CrexBaseFragment GetFragmentForTemplate( string template )
        {
            var type = Type.GetType( $"Crex.Android.Activities.{ template }Fragment" );

            if ( type == null )
            {
                Log.Debug( "Crex", $"Unknown template specified: { template }" );
                return null;
            }

            return ( CrexBaseFragment ) Activator.CreateInstance( type );
        }


        /// <summary>
        /// Gets the activity type for template.
        /// </summary>
        /// <returns>The type for template.</returns>
        /// <param name="template">Template.</param>
        protected Type GetTypeForTemplate( string template )
        {
            var type = Type.GetType( $"Crex.Android.Activities.{ template }Activity" );

            if ( type == null )
            {
                Log.Debug( "Crex", $"Unknown template specified: { template }" );
                return null;
            }

            return type;
        }

        /// <summary>
        /// Shows the update required dialog, does not pop the current activity.
        /// </summary>
        protected void ShowUpdateRequiredDialog( Activity activity )
        {
            var builder = new AlertDialog.Builder( activity, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Update Required" )
                .SetMessage( "An update is required to view this content." )
                .SetPositiveButton( "Close", new Dialogs.OnClickAction() )
                .SetOnCancelListener( new Dialogs.OnCancelAction() );

            activity.RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Shows the update required dialog.
        /// </summary>
        protected void ShowDataErrorDialog( Activity activity, Action retry )
        {
            var builder = new AlertDialog.Builder( activity, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Error loading data" )
                   .SetMessage( "An error occurred trying to load the content. Please try again later." )
                   .SetOnCancelListener( new Dialogs.OnCancelAction( () => { } ) );

            if (retry != null)
            {
                builder.SetPositiveButton( "Retry", new Dialogs.OnClickAction( retry ) );
            }

            activity.RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }
    }
}