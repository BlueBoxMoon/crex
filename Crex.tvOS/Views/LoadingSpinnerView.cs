using System;
using System.Collections.Generic;
using System.Timers;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Crex.tvOS.Views
{
    public class LoadingSpinnerView : UIView
    {
        #region Properties

        protected Timer AutoShowTimer { get; private set; }
        protected UIImageView ImageView { get; private set; }
        private List<Action> StopActions { get; set; } = new List<Action>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Crex.tvOS.Views.LoadingSpinnerView"/> is running.
        /// </summary>
        /// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.Views.LoadingSpinnerView"/> class.
        /// </summary>
        /// <param name="frame">Initial frame of the spinner.</param>
        public LoadingSpinnerView( CGRect frame )
            : base( frame )
        {
            var stream = Utility.GetStreamForNamedResource( Crex.Application.Current.Config.LoadingSpinner );
            ImageView = new UIImageView( Bounds )
            {
                Image = UIImage.LoadFromData( NSData.FromStream( stream ) ),
                Hidden = true
            };
            AddSubview( ImageView );
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the view needs to update any subview layouts.
        /// </summary>
        public override void LayoutSubviews()
        {
            ImageView.Frame = Bounds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts teh animation on this spinner.
        /// </summary>
        public void Start()
        {
            lock ( this )
            {
                if ( IsRunning )
                {
                    return;
                }

                ImageView.Hidden = true;
                IsRunning = true;

                if ( AutoShowTimer != null )
                {
                    AutoShowTimer.Enabled = false;
                    AutoShowTimer.Dispose();
                }

                AutoShowTimer = new Timer( Crex.Application.Current.Config.LoadingSpinnerDelay.Value );
                AutoShowTimer.Elapsed += AutoShowTimer_Elapsed;
                AutoShowTimer.AutoReset = false;
                AutoShowTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Stops the animation on this spinner.
        /// </summary>
        /// <param name="finished">An action to be performed when the spinner has hidden.</param>
        public void Stop( Action finished = null )
        {
            lock ( this )
            {
                if ( finished != null )
                {
                    StopActions.Add( finished );
                }

                //
                // If we stopped before showing the spinner then just cancel the timer.
                //
                if ( AutoShowTimer != null )
                {
                    AutoShowTimer.Enabled = false;
                    AutoShowTimer.Dispose();
                    AutoShowTimer = null;

                    Stopped();
                }
                else if ( ImageView.Layer.AnimationForKey( "rotation" ) != null )
                {
                    //
                    // Otherwise do a nice fadeout animation.
                    //
                    UIView.Animate( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
                    {
                        ImageView.Alpha = 0.0f;
                    }, () =>
                    {
                        InvokeOnMainThread( () =>
                        {
                            lock ( this )
                            {
                                if ( IsRunning )
                                {
                                    Stopped();
                                }
                            }
                        } );
                    } );
                }
            }
        }

        /// <summary>
        /// We have entered the stopped state. Run actions and update flags.
        /// </summary>
        private void Stopped()
        {
            StopSpinning();

            StopActions.ForEach( a => a() );
            StopActions.Clear();
            ImageView.Hidden = true;

            IsRunning = false;
        }

        /// <summary>
        /// Start the image view spinning.
        /// </summary>
        private void StartSpinning()
        {
            if ( ImageView.Layer.AnimationForKey( "rotation" ) == null )
            {
                var animation = new CABasicAnimation
                {
                    KeyPath = "transform.rotation",
                    Duration = 1.5f,
                    RepeatCount = float.PositiveInfinity,
                    From = NSNumber.FromFloat( 0.0f ),
                    To = NSNumber.FromFloat( ( float ) Math.PI * 2.0f )
                };

                ImageView.Layer.AddAnimation( animation, "rotation" );
            }
        }

        /// <summary>
        /// Stop the ImageView from spinning.
        /// </summary>
        private void StopSpinning()
        {
            if ( ImageView.Layer.AnimationForKey( "rotation" ) != null )
            {
                ImageView.Layer.RemoveAnimation( "rotation" );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Elapsed event of the AutoShowTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The ElapsedEventArgs instance containing the event data.</param>
        void AutoShowTimer_Elapsed( object sender, ElapsedEventArgs e )
        {
            InvokeOnMainThread( () =>
            {
                lock ( this )
                {
                    if ( AutoShowTimer != null )
                    {
                        AutoShowTimer = null;

                        if ( !IsRunning )
                        {
                            return;
                        }

                        StartSpinning();
                        ImageView.Hidden = false;
                    }
                }
            } );
        }

        #endregion
    }
}
