using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

using Crex.Android.Activities;

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
            Preferences = new AndroidPreferences( global::Android.App.Application.Context );
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
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <param name="sender">The UIViewController that is starting this action.</param>
        /// <param name="url">The url to the action to be started.</param>
        public override async Task StartAction( object sender, string url )
        {
            url = GetAbsoluteUrl( url );

            Console.WriteLine( $"Navigation to { url }" );

            await CrexActivity.MainActivity.StartAction( url );
        }

        /// <summary>
        /// Starts the view template.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action that should be loaded.</param>
        public override async Task StartAction( object sender, Rest.CrexAction action )
        {
            await CrexActivity.MainActivity.StartAction( action );
        }
    }
}