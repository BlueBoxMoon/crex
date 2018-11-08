using System;
using System.Threading.Tasks;
using AVFoundation;
using AVKit;
using CoreGraphics;
using Crex.Extensions;
using Foundation;
using UIKit;

namespace Crex.tvOS.Templates
{
    public class VideoViewController : CrexBaseViewController
    {
        #region Views

        protected Views.LoadingSpinnerView LoadingSpinnerView { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the last URL played by a video player.
        /// </summary>
        /// <value>The last URL played by a video player.</value>
        private static string LastUrl { get; set; }

        /// <summary>
        /// Gets or sets the last position of playback for the LastUrl.
        /// </summary>
        /// <value>The last position of playback for the LastUrl.</value>
        private static double? LastPosition { get; set; }

        /// <summary>
        /// Gets or sets the player view controller.
        /// </summary>
        /// <value>The player view controller.</value>
        private AVPlayerViewController PlayerViewController { get; set; }

        /// <summary>
        /// Gets or sets the did play to end time notification handle.
        /// </summary>
        /// <value>The did play to end time notification handle.</value>
        private NSObject DidPlayToEndTimeNotificationHandle { get; set; }

        /// <summary>
        /// Gets or sets the state of the video view.
        /// </summary>
        /// <value>The state of the video view.</value>
        private int State { get; set; }

        /// <summary>
        /// Gets or sets the position to start playback from.
        /// </summary>
        /// <value>The position to start playback from.</value>
        private double? PlaybackAtPosition { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded, now we can create any child views.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoadingSpinnerView = new Views.LoadingSpinnerView( new CGRect( 880, 460, 160, 160 ) );
            View.AddSubview( LoadingSpinnerView );

            State = 0;
        }

        /// <summary>
        /// The view has appeared on screen.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewDidAppear( bool animated )
        {
            base.ViewDidAppear( animated );

            ProcessStateChange();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content for the menu.
        /// </summary>
        private void LoadContentInBackground()
        {
            try
            {
                var urlString = Data.FromJson<string>();

                if ( LastUrl == urlString && LastPosition.HasValue && !PlaybackAtPosition.HasValue )
                {
                    ShowResumeDialog();
                    return;
                }

                //
                // Indicate to the user that we have started loading.
                //
                State = 1;
                LoadingSpinnerView.Start();

                PlayerViewController = new AVPlayerViewController
                {
                    Player = new AVPlayer( new AVPlayerItem( new NSUrl( urlString ) ) )
                };

                //
                // Install a notification handler that playback has finished.
                //
                DidPlayToEndTimeNotificationHandle = AVPlayerItem.Notifications.ObserveDidPlayToEndTime( ( sender, args ) =>
                {
                    if ( args.Notification.Object == PlayerViewController.Player.CurrentItem )
                    {
                        DismissPlayer();
                    }
                } );

                Task.Run( async () =>
                {
                    //
                    // Wait for the status to change. Wait at most 10 seconds.
                    //
                    for ( int i = 0; i < 100; i++ )
                    {
                        if ( PlayerViewController.Player.Status != AVPlayerStatus.Unknown )
                        {
                            Console.WriteLine( $"Player ready on loop { i }" );
                            break;
                        }
                        await Task.Delay( 100 );
                    }

                    if ( PlayerViewController.Player.Status != AVPlayerStatus.ReadyToPlay )
                    {
                        throw new Exception( "Failed to prepare video" );
                    }

                    PlayerViewController.Player.Seek( CoreMedia.CMTime.FromSeconds( PlaybackAtPosition ?? 0, CoreMedia.CMTime.MaxTimeScale ) );
                    await PlayerViewController.Player.PrerollAsync( 1.0f );

                    InvokeOnMainThread( () =>
                    {
                        State = 2;
                        LoadingSpinnerView.Stop( () =>
                        {
                            PlayerViewController.Player.PlayImmediatelyAtRate( 1.0f );
                            PresentViewController( PlayerViewController, false, null );
                        } );
                    } );
                } )
                .ContinueWith( ( t ) =>
                {
                    if ( t.IsFaulted )
                    {
                        ShowDataErrorDialog( LoadContentInBackground );
                    }
                } );
            }
            catch
            {
                ShowDataErrorDialog( LoadContentInBackground );
            }
        }

        /// <summary>
        /// Dismisses the player because the playback ended.
        /// </summary>
        private void DismissPlayer()
        {
            InvokeOnMainThread( () =>
            {
                PlayerViewController.DismissViewController( true, null );
            } );
        }

        /// <summary>
        /// Processes any state change we need to perform.
        /// </summary>
        private void ProcessStateChange()
        {
            if ( State == 0 )
            {
                //
                // Initial state, we need to move to the "loading" state.
                //
                LoadContentInBackground();
            }
            else if ( State == 2 )
            {
                //
                // In the "playback" state, we need to clean up.
                //
                if ( DidPlayToEndTimeNotificationHandle != null )
                {
                    DidPlayToEndTimeNotificationHandle.Dispose();
                }

                if ( PlayerViewController != null )
                {
                    //
                    // Store the url and positio for resume later.
                    //
                    try
                    {
                        LastUrl = Data.FromJson<string>();
                        LastPosition = PlayerViewController.Player.CurrentTime.Seconds;
                        var duration = PlayerViewController.Player.CurrentItem.Duration.Seconds;

                        if ( LastPosition > ( duration - 300 ) || LastPosition < 60 )
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

                    PlayerViewController.Player.Pause();
                    PlayerViewController.Player.ReplaceCurrentItemWithPlayerItem( null );
                    PlayerViewController.Player = null;
                }

                NavigationController.PopViewController( true );

                State = 3;
            }
        }

        /// <summary>
        /// Shows the resume dialog.
        /// </summary>
        protected void ShowResumeDialog()
        {
            InvokeOnMainThread( () =>
            {
                var alertController = UIAlertController.Create( "Resume Playback",
                                                  "Do you wish to resume playback where you left off?",
                                                  UIAlertControllerStyle.Alert );

                if ( NavigationController.ViewControllers[0] != this )
                {
                    var action = UIAlertAction.Create( "Resume", UIAlertActionStyle.Default, ( alert ) =>
                    {
                        PlaybackAtPosition = LastPosition;
                        ProcessStateChange();
                    } );
                    alertController.AddAction( action );

                    action = UIAlertAction.Create( "Start Over", UIAlertActionStyle.Default, ( alert ) =>
                    {
                        PlaybackAtPosition = 0;
                        ProcessStateChange();
                    } );
                    alertController.AddAction( action );
                }

                PresentViewController( alertController, true, null );
            } );
        }

        #endregion
    }
}
