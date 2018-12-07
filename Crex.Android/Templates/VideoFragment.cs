using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Crex.Extensions;
using Crex.Android.Activities;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Util;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Source;

namespace Crex.Android.Templates
{
    public class VideoFragment : CrexBaseFragment, IPlayerEventListener
    {
        #region Views

        /// <summary>
        /// Gets the video view.
        /// </summary>
        /// <value>
        /// The video view.
        /// </value>
        protected PlayerView PlayerView { get; private set; }

        /// <summary>
        /// Gets the loading spinner view.
        /// </summary>
        /// <value>
        /// The loading spinner view.
        /// </value>
        protected Widgets.LoadingSpinner LoadingSpinnerView { get; private set; }

        /// <summary>
        /// Gets the player for the current video.
        /// </summary>
        /// <value>
        /// The player for the current video.
        /// </value>
        protected SimpleExoPlayer VideoPlayer { get; private set; }

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
        private static long? LastPosition { get; set; }

        /// <summary>
        /// Gets or sets the playback position when the stream is ready.
        /// </summary>
        /// <value>
        /// The playback position when the stream is ready.
        /// </value>
        private long? PlaybackAtPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic play].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic play]; otherwise, <c>false</c>.
        /// </value>
        private bool AutoPlay { get; set; } = true;

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
            PlayerView = new PlayerView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent ),
                UseController = true,
                ControllerAutoShow = false
            };
            layout.AddView( PlayerView );

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
        /// Dispatches the key event.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns>true if the event was handled.</returns>
        public override bool DispatchKeyEvent( KeyEvent e )
        {
            //
            // If they pressed one of the media control keys and the player
            // controller is not visible then show the controller and pass on
            // the key press.
            //
            if ( !PlayerView.IsControllerVisible && e.Action == KeyEventActions.Down )
            {
                if ( e.KeyCode == Keycode.MediaFastForward || e.KeyCode == Keycode.MediaRewind || e.KeyCode == Keycode.MediaPlayPause )
                {
                    PlayerView.ShowController();
                    return PlayerView.DispatchKeyEvent( e );
                }
            }

            //
            // If they pressed a different key and it wasn't back, then show the controller.
            //
            if ( !PlayerView.IsControllerVisible && e.Action == KeyEventActions.Up )
            {
                if ( e.KeyCode != Keycode.Back )
                {
                    PlayerView.ShowController();
                    return true;
                }
            }

            return base.DispatchKeyEvent( e );
        }

        /// <summary>
        /// Called as part of the activity lifecycle when an activity is going into
        /// the background, but has not (yet) been killed.
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();

            VideoPlayer.PlayWhenReady = false;
            PlayerView.HideController();

            try
            {
                LastUrl = Data.FromJson<string>();
                LastPosition = VideoPlayer.CurrentPosition;
                var duration = VideoPlayer.Duration;

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
        private void PlayVideo( string url, long? position )
        {
            if ( position.HasValue )
            {
                PlaybackAtPosition = position;
            }

            LoadingSpinnerView.Start();

            //
            // Prepare the standard HTTP information for the source.
            //
            var mediaUri = global::Android.Net.Uri.Parse( url );
            var userAgent = Util.GetUserAgent( Activity, "Crex" );
            var defaultHttpDataSourceFactory = new DefaultHttpDataSourceFactory( userAgent );
            var defaultDataSourceFactory = new DefaultDataSourceFactory( Activity, null, defaultHttpDataSourceFactory );
            IMediaSource source;

            //
            // Determine if this is an HLS or MP4 stream.
            //
            if ( mediaUri.Path.EndsWith( ".m3u8" ) || mediaUri.Path.EndsWith( "/hsl" ) )
            {
                source = new HlsMediaSource.Factory( defaultDataSourceFactory )
                    .CreateMediaSource( mediaUri );
            }
            else
            {
                source = new ExtractorMediaSource.Factory( defaultDataSourceFactory )
                    .CreateMediaSource( mediaUri );
            }

            //
            // Create the player and get it ready for playback.
            //
            VideoPlayer = ExoPlayerFactory.NewSimpleInstance( Activity, new DefaultTrackSelector() );
            VideoPlayer.AddListener( this );
            PlayerView.Player = VideoPlayer;
            VideoPlayer.Prepare( source );
        }

        #endregion

        #region IPlayerEventListener

        /// <summary>
        /// Called when the loading status for the player has changed.
        /// </summary>
        /// <param name="isLoading">if set to <c>true</c> [is loading].</param>
        void IPlayerEventListener.OnLoadingChanged( bool isLoading )
        {
            Console.WriteLine( $"OnLoadingChanged( { isLoading } )" );
            if ( isLoading && PlaybackAtPosition.HasValue )
            {
                VideoPlayer.SeekTo( PlaybackAtPosition.Value );
            }
        }

        void IPlayerEventListener.OnPlaybackParametersChanged( PlaybackParameters playbackParameters )
        {
        }

        /// <summary>
        /// Called when an error happens during playback.
        /// </summary>
        /// <param name="error">The error.</param>
        void IPlayerEventListener.OnPlayerError( ExoPlaybackException error )
        {
            VideoPlayer.PlayWhenReady = false;

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
        /// Called when the player's state has changed.
        /// </summary>
        /// <param name="playWhenReady">if set to <c>true</c> [play when ready].</param>
        /// <param name="playbackState">State of the playback.</param>
        void IPlayerEventListener.OnPlayerStateChanged( bool playWhenReady, int playbackState )
        {
            //
            // If the video is ready, hide the spinner and start the video.
            //
            if ( playbackState == Player.StateReady && AutoPlay )
            {
                AutoPlay = false;

                LoadingSpinnerView.Stop( () =>
                {
                    LoadingSpinnerView.Visibility = ViewStates.Invisible;

                    VideoPlayer.PlayWhenReady = true;
                    PlayerView.ControllerAutoShow = true;
                } );
            }

            //
            // Playback has ended naturally, close this video player.
            //
            if ( playbackState == Player.StateEnded )
            {
                CrexActivity.MainActivity.PopTopFragment();
            }
        }

        void IPlayerEventListener.OnPositionDiscontinuity( int reason )
        {
        }

        void IPlayerEventListener.OnRepeatModeChanged( int repeatMode )
        {
        }

        void IPlayerEventListener.OnSeekProcessed()
        {
        }

        void IPlayerEventListener.OnShuffleModeEnabledChanged( bool shuffleModeEnabled )
        {
        }

        void IPlayerEventListener.OnTimelineChanged( Timeline timeline, Java.Lang.Object manifest, int reason )
        {
        }

        void IPlayerEventListener.OnTracksChanged( TrackGroupArray trackGroups, TrackSelectionArray trackSelections )
        {
        }

        #endregion
    }
}