using System;
using System.Collections.Generic;
using System.Timers;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.Animations;
using Android.Util;
using Android.Widget;

namespace Crex.Android.Widgets
{
    public class LoadingSpinner : LinearLayout
    {
        #region Fields

        /// <summary>
        /// The image view that contains the spinning image.
        /// </summary>
        ImageView imageView;

        /// <summary>
        /// Timer used to show the spinner after a period of time.
        /// </summary>
        Timer autoShowTimer;

        /// <summary>
        /// The stop actions to be performed.
        /// </summary>
        readonly List<Action> _stopActions = new List<Action>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingSpinner"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public LoadingSpinner( Context context, IAttributeSet attrs )
            : base( context, attrs )
        {
            Focusable = true;

            imageView = new ImageView( Context )
            {
                Visibility = ViewStates.Invisible
            };
            AddView( imageView );
            imageView.SetScaleType( ImageView.ScaleType.FitCenter );
            imageView.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
            imageView.LayoutParameters.Height = ViewGroup.LayoutParams.MatchParent;

            var imageStream = Utility.GetStreamForNamedResource( Crex.Application.Current.Config.LoadingSpinner );
            imageView.SetImageBitmap( BitmapFactory.DecodeStream( imageStream ) );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the animation on this spinner.
        /// </summary>
        public void Start()
        {
            //
            // Setup a timer that will show the spinner after the delay.
            //
            lock ( this )
            {
                if ( IsRunning )
                {
                    return;
                }

                imageView.Visibility = ViewStates.Invisible;

                IsRunning = true;

                if ( autoShowTimer != null )
                {
                    autoShowTimer.Enabled = false;
                    autoShowTimer.Dispose();
                }

                autoShowTimer = new Timer( Crex.Application.Current.Config.LoadingSpinnerDelay.Value );
                autoShowTimer.Elapsed += autoShowTimer_Elapsed;
                autoShowTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Stops the animation on this spinner.
        /// </summary>
        public void Stop( Action finished = null )
        {
            lock ( this )
            {
                if ( finished != null )
                {
                    _stopActions.Add( finished );
                }

                //
                // If we stopped before showing the spinner then just cancel the timer.
                //
                if ( autoShowTimer != null )
                {
                    autoShowTimer.Enabled = false;
                    autoShowTimer = null;

                    Stopped();
                }
                else if ( imageView.Animation != null )
                {
                    //
                    // Otherwise, do a nice fadeout animation.
                    //
                    var fadeAnimation = new AlphaAnimation( 1.0f, 0.0f )
                    {
                        Duration = Crex.Application.Current.Config.AnimationTime.Value,
                        Interpolator = new LinearInterpolator()
                    };
                    fadeAnimation.AnimationEnd += fadeAnimation_AnimationEnd;

                    var animationSet = ( AnimationSet ) imageView.Animation;
                    animationSet.AddAnimation( fadeAnimation );
                }
                else
                {
                    Stopped();
                }
            }
        }

        /// <summary>
        /// We have entered the stopped state. Run actions and update flags.
        /// </summary>
        private void Stopped()
        {
            imageView.ClearAnimation();

            foreach ( Action a in _stopActions )
            {
                a();
            }

            _stopActions.Clear();

            IsRunning = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AnimationEnd event of the fadeAnimation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Animation.AnimationEndEventArgs"/> instance containing the event data.</param>
        private void fadeAnimation_AnimationEnd( object sender, Animation.AnimationEndEventArgs e )
        {
            lock ( this )
            {
                if ( !IsRunning )
                {
                    return;
                }

                Stopped();
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the autoShowTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void autoShowTimer_Elapsed( object sender, ElapsedEventArgs e )
        {
            lock ( this )
            {
                if ( autoShowTimer != null )
                {
                    autoShowTimer.Dispose();
                    autoShowTimer = null;

                    imageView.Post( () =>
                    {
                        lock ( this )
                        {
                            if ( IsRunning && imageView.Animation == null )
                            {
                                //
                                // Start an animation for rotating the spinner forever.
                                //
                                var anim = new RotateAnimation( 0, 360f, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f )
                                {
                                    Duration = 1500,
                                    RepeatCount = Animation.Infinite,
                                    Interpolator = new LinearInterpolator()
                                };
                                var animationSet = new AnimationSet( false );
                                animationSet.AddAnimation( anim );

                                imageView.StartAnimation( animationSet );
                                imageView.Visibility = ViewStates.Visible;
                            }
                        }
                    } );
                }
            }
        }

        #endregion
    }
}
