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

        #region Properties

        /// <summary>
        /// Gets or sets the last URL played by a video player.
        /// </summary>
        /// <value>
        /// The last URL played by a video player.
        /// </value>
        private static string LastUrl { get; set; }

        /// <summary>
        /// Gets or sets the last position of playback for the LastUrl.
        /// </summary>
        /// <value>
        /// The last position of playback for the LastUrl.
        /// </value>
        private static int? LastPosition { get; set; }

        /// <summary>
        /// Gets or sets the playback position when the stream is ready.
        /// </summary>
        /// <value>
        /// The playback position when the stream is ready.
        /// </value>
        private int? PlaybackAtPosition { get; set; }

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
        }

        /// <summary>
        /// Called after <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c>, <c><see cref="M:Android.App.Activity.OnRestart" /></c>, or
        /// <c><see cref="M:Android.App.Activity.OnPause" /></c>, for your activity to start interacting with the user.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            var url = Intent.GetStringExtra( "data" ).FromJson<string>();

            if ( LastUrl == url && LastPosition.HasValue )
            {
                ShowResumeDialog();
            }
            else
            {
                PlayVideo( url, 0 );
            }
        }

        /// <summary>
        /// Called as part of the activity lifecycle when an activity is going into
        /// the background, but has not (yet) been killed.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();

            vvVideo.Pause();

            try
            {
                LastUrl = Intent.GetStringExtra( "data" ).FromJson<string>();
                LastPosition = vvVideo.CurrentPosition;
                var duration = vvVideo.Duration;

                //
                // Times our in milliseconds.
                //
                if ( LastPosition > ( duration - 300000 ) || LastPosition < 60000 )
                {
                    LastUrl = null;
                    LastPosition = null;
                }
            }
            catch
            {
                LastUrl = null;
                LastPosition = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the resume playback dialog.
        /// </summary>
        private void ShowResumeDialog()
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Resume Playback" )
                .SetMessage( "Do you wish to resume playback where you left off?" )
                .SetPositiveButton( "Resume", new Dialogs.OnClickAction( ResumePlayback ) )
                .SetNegativeButton( "Start Over", new Dialogs.OnClickAction( StartPlaybackOver ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( Finish ) );

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Resumes playback at the last position.
        /// </summary>
        private void ResumePlayback()
        {
            var url = Intent.GetStringExtra( "data" ).FromJson<string>();

            PlayVideo( url, LastPosition.Value );
        }

        /// <summary>
        /// Starts the playback over at the beginning.
        /// </summary>
        private void StartPlaybackOver()
        {
            var url = Intent.GetStringExtra( "data" ).FromJson<string>();

            PlayVideo( url, 0 );
        }

        /// <summary>
        /// Plays the video contained in the URL at the given starting position.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="position">The position.</param>
        private void PlayVideo( string url, int position )
        {
            lsLoading.Start();

            PlaybackAtPosition = position;
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

                if ( PlaybackAtPosition.HasValue )
                {
                    vvVideo.SeekTo( PlaybackAtPosition.Value );
                }

                vvVideo.Start();
            } );
        }

        #endregion
    }
}
