using System;

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

            var type = Type.GetType( $"Crex.Android.Activities.{ Config.ApplicationRootTemplate }Activity" );

            if ( type == null )
            {
                Log.Debug( "Crex", $"Unknown root template specified: { Config.ApplicationRootTemplate }" );
                return;
            }

            Intent intent = new Intent( activity, type );
            intent.AddFlags( ActivityFlags.ClearTop );
            intent.PutExtra( "data", Config.ApplicationRootUrl.ToJson() );
            activity.Finish();
            activity.StartActivity( intent );
        }

        /// <summary>
        /// Starts the view template.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action that should be loaded.</param>
        public override void StartAction( object sender, Rest.CrexAction action )
        {
            Activity currentActivity = ( Activity ) sender;
            var type = Type.GetType( $"Crex.Android.Activities.{ action.Template }Activity" );

            if ( type == null )
            {
                Log.Debug( "Crex", $"Unknown template specified: { action.Template }" );
                return;
            }

            Log.Debug( "Crex", $"Navigation to { action.Template }" );

            var intent = new Intent( currentActivity, type );

            intent.PutExtra( "data", action.Data.ToJson() );

            currentActivity.StartActivity( intent, ActivityOptions.MakeSceneTransitionAnimation( currentActivity ).ToBundle() );
        }
    }
}