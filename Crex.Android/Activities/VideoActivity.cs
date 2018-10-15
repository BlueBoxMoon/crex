using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;

using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "Video" )]
    public class VideoActivity : Activity
    {
        #region Widgets

        VideoView vvVideo;
        Widgets.LoadingSpinner lsLoading;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            SetContentView( Resource.Layout.VideoView );

            vvVideo = FindViewById<VideoView>( Resource.Id.vvVideo );
            lsLoading = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            vvVideo.Completion += video_Completion;
            vvVideo.Error += video_Error;
            vvVideo.Prepared += video_Prepared;

            var mediaController = new MediaController( this );
            mediaController.SetAnchorView( vvVideo );
            vvVideo.SetMediaController( mediaController );

            lsLoading.Start();

            var url = Intent.GetStringExtra( "data" ).FromJson<string>();
            vvVideo.SetVideoURI( global::Android.Net.Uri.Parse( url ) );
            vvVideo.RequestFocus();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Completion event of the videoView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void video_Completion( object sender, EventArgs e )
        {
            Finish();
        }

        /// <summary>
        /// Handles the Error event of the videoView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Android.Media.MediaPlayer.ErrorEventArgs"/> instance containing the event data.</param>
        private void video_Error( object sender, MediaPlayer.ErrorEventArgs e )
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Video Playback Error" )
                .SetMessage( "An error occurred trying to play the video." )
                .SetPositiveButton( "Close", new Dialogs.OnClickAction( Finish ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( Finish ) );

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Handles the Prepared event of the video control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void video_Prepared( object sender, EventArgs e )
        {
            lsLoading.Stop( () =>
            {
                lsLoading.Visibility = ViewStates.Invisible;
                vvVideo.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                vvVideo.LayoutParameters.Height = ViewGroup.LayoutParams.MatchParent;
                vvVideo.Start();
            } );
        }

        #endregion
    }
}
