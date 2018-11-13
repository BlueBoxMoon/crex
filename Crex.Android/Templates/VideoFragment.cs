using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Crex.Extensions;
using Crex.Android.Activities;

namespace Crex.Android.Templates
{
    public class VideoFragment : CrexBaseFragment
    {
        #region Views

        /// <summary>
        /// Gets the video view.
        /// </summary>
        /// <value>
        /// The video view.
        /// </value>
        protected VideoView VideoView { get; private set; }

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
        public override void OnViewCreated( View view, Bundle savedInstanceState )
        {
            base.OnViewCreated( view, savedInstanceState );

            var layout = ( FrameLayout ) view;

            //
            // Initialize the video view.
            //
            VideoView = new VideoView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent )
            };
            VideoView.Completion += video_Completion;
            VideoView.Error += video_Error;
            VideoView.Prepared += video_Prepared;
            layout.AddView( VideoView );

            //
            // Initialize the loading spinner.
            //
            LoadingSpinnerView = new Widgets.LoadingSpinner( Activity, null )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ( int ) ( 80 * Resources.DisplayMetrics.Density ), ( int ) ( 80 * Resources.DisplayMetrics.Density ) )
                {
                    Gravity = GravityFlags.Center
                }
            };
            layout.AddView( LoadingSpinnerView );

            var mediaController = new MediaController( Activity );
            mediaController.SetAnchorView( VideoView );
            VideoView.SetMediaController( mediaController );
        }

        /// <summary>
        /// Called when the fragment is visible to the user and actively running.
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();

            var url = Data.FromJson<string>();

            if ( LastUrl == url && LastPosition.HasValue )
            {
                ShowResumeDialog();
            }
            else
            {
                PlayVideo( url, null );
            }
        }

        /// <summary>
        /// Called as part of the activity lifecycle when an activity is going into
        /// the background, but has not (yet) been killed.
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();

            VideoView.Pause();

            try
            {
                LastUrl = Data.FromJson<string>();
                LastPosition = VideoView.CurrentPosition;
                var duration = VideoView.Duration;

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
            var builder = new AlertDialog.Builder( Activity, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Resume Playback" )
                .SetMessage( "Do you wish to resume playback where you left off?" )
                .SetPositiveButton( "Resume", new Dialogs.OnClickAction( ResumePlayback ) )
                .SetNegativeButton( "Start Over", new Dialogs.OnClickAction( StartPlaybackOver ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( CrexActivity.MainActivity.PopTopFragment ) );

            Activity.RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Resumes playback at the last position.
        /// </summary>
        private void ResumePlayback()
        {
            var url = Data.FromJson<string>();

            PlayVideo( url, LastPosition.Value );
        }

        /// <summary>
        /// Starts the playback over at the beginning.
        /// </summary>
        private void StartPlaybackOver()
        {
            var url = Data.FromJson<string>();

            PlayVideo( url, 0 );
        }

        /// <summary>
        /// Plays the video contained in the URL at the given starting position.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="position">The position.</param>
        private void PlayVideo( string url, int? position )
        {
            if ( position.HasValue )
            {
                PlaybackAtPosition = position;
            }

            LoadingSpinnerView.Start();
            VideoView.SetVideoURI( global::Android.Net.Uri.Parse( url ) );
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
            CrexActivity.MainActivity.PopTopFragment();
        }

        /// <summary>
        /// Handles the Error event of the videoView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Android.Media.MediaPlayer.ErrorEventArgs"/> instance containing the event data.</param>
        private void video_Error( object sender, MediaPlayer.ErrorEventArgs e )
        {
            VideoView.StopPlayback();

            var builder = new AlertDialog.Builder( Activity, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Video Playback Error" )
                .SetMessage( "An error occurred trying to play the video." )
                .SetPositiveButton( "Close", new Dialogs.OnClickAction( CrexActivity.MainActivity.PopTopFragment ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( CrexActivity.MainActivity.PopTopFragment ) );

            Activity.RunOnUiThread( () =>
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
            LoadingSpinnerView.Stop( () =>
            {
                LoadingSpinnerView.Visibility = ViewStates.Invisible;
                VideoView.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                VideoView.LayoutParameters.Height = ViewGroup.LayoutParams.MatchParent;

                if ( PlaybackAtPosition.HasValue )
                {
                    VideoView.SeekTo( PlaybackAtPosition.Value );
                }

                VideoView.Start();
                VideoView.RequestFocus();
            } );
        }

        #endregion
    }
}