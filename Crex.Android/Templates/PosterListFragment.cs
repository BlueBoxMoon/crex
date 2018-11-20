using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Crex.Extensions;

namespace Crex.Android.Templates
{
    public class PosterListFragment : CrexBaseFragment
    {
        #region Views

        protected ImageView BackgroundImageView { get; private set; }

        protected TextView TitleView { get; private set; }

        protected ImageView PosterImageView { get; private set; }

        protected TextView DetailLeftView { get; private set; }

        protected TextView DetailRightView { get; private set; }

        protected TextView DescriptionView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the poster data.
        /// </summary>
        /// <value>
        /// The poster data.
        /// </value>
        protected Rest.PosterList PosterData { get; private set; }

        /// <summary>
        /// Gets the date we last loaded our content.
        /// </summary>
        /// <value>
        /// The date we last loaded our content.
        /// </value>
        protected DateTime LastLoadedDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the poster items.
        /// </summary>
        /// <value>
        /// The poster items.
        /// </value>
        protected ListView PosterItems { get; private set; }

        /// <summary>
        /// Gets the poster items adapter.
        /// </summary>
        /// <value>
        /// The poster items adapter.
        /// </value>
        protected ArrayAdapter PosterItemsAdapter { get; private set; }

        /// <summary>
        /// Gets the poster images.
        /// </summary>
        /// <value>
        /// The poster images.
        /// </value>
        protected Bitmap[] PosterImages { get; private set; }

        protected Bitmap BackgroundImage { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called immediately after <c><see cref="M:Android.App.Fragment.OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)" /></c>
        /// has returned, but before any saved state has been restored in to the view.
        /// </summary>
        /// <param name="view">The View returned by <c><see cref="M:Android.App.Fragment.OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)" /></c>.</param>
        /// <param name="savedInstanceState">If non-null, this fragment is being re-constructed
        /// from a previous saved state as given here.</param>
        public override void OnViewCreated( View view, Bundle savedInstanceState )
        {
            base.OnViewCreated( view, savedInstanceState );

            var layout = ( FrameLayout ) view;

            //
            // Initialize the background image view.
            //
            BackgroundImageView = new ImageView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent ),
                Alpha = 0.25f
            };
            BackgroundImageView.SetScaleType( ImageView.ScaleType.FitCenter );
            layout.AddView( BackgroundImageView );

            //
            // Initialize the title view.
            //
            TitleView = new TextView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 880 ), DipToPixel( 40 ) )
                {
                    TopMargin = DipToPixel( 30 ),
                    Gravity = GravityFlags.CenterHorizontal
                },
                TextAlignment = TextAlignment.Center,
                TextSize = 28,
                Typeface = Typeface.DefaultBold
            };
            layout.AddView( TitleView );

            //
            // Initialize the poster image view.
            //
            PosterImageView = new ImageView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 400 ), DipToPixel( 225 ) )
                {
                    TopMargin = DipToPixel( 100 ),
                    LeftMargin = DipToPixel( 40 )
                }
            };
            PosterImageView.SetScaleType( ImageView.ScaleType.FitCenter );
            layout.AddView( PosterImageView );

            //
            // Initialize the left detail text view.
            //
            DetailLeftView = new TextView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 200 ), DipToPixel( 20 ) )
                {
                    TopMargin = DipToPixel( 325 ),
                    LeftMargin = DipToPixel( 40 )
                },
                TextAlignment = TextAlignment.ViewStart,
                TextSize = 14
            };
            layout.AddView( DetailLeftView );

            //
            // Initialize the right detail text view.
            //
            DetailRightView = new TextView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 200 ), DipToPixel( 20 ) )
                {
                    TopMargin = DipToPixel( 325 ),
                    LeftMargin = DipToPixel( 240 )
                },
                TextAlignment = TextAlignment.ViewEnd,
                TextSize = 14
            };
            layout.AddView( DetailRightView );

            //
            // Initialize the description text view.
            //
            DescriptionView = new TextView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 400 ), DipToPixel( 140 ) )
                {
                    TopMargin = DipToPixel( 355 ),
                    LeftMargin = DipToPixel( 40 )
                },
                TextSize = 14,
                Ellipsize = global::Android.Text.TextUtils.TruncateAt.End
            };
            DescriptionView.SetMaxLines( 8 );
            DescriptionView.SetSingleLine( false );
            layout.AddView( DescriptionView );

            //
            // Initialize the poster items view.
            //
            PosterItems = new ListView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( DipToPixel( 400 ), DipToPixel( 340 ) )
                {
                    TopMargin = DipToPixel( 100 ),
                    RightMargin = DipToPixel( 40 ),
                    Gravity = GravityFlags.Right
                }
            };
            layout.AddView( PosterItems );

            //
            // Initialize the data and settings for the list view.
            //
            PosterItemsAdapter = new ArrayAdapter<string>( Activity, Resource.Layout.PosterListViewItem );
            PosterItems.Adapter = PosterItemsAdapter;
            PosterItems.ItemClick += listView_ItemClick;
            PosterItems.ItemSelected += listView_ItemSelected;

            if ( PosterData != null )
            {
                UpdateUserInterfaceFromContent();
            }
        }

        /// <summary>
        /// Called after <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c>, <c><see cref="M:Android.App.Activity.OnRestart" /></c>, or
        /// <c><see cref="M:Android.App.Activity.OnPause" /></c>, for your activity to start interacting with the user.
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();

            if ( DateTime.Now.Subtract( LastLoadedDate ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
            {
                Task.Run( LoadContentAsync );
            }
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            var data = Data.FromJson<Rest.PosterList>();

            //
            // If the menu content hasn't actually changed, then ignore.
            //
            if ( data.ToJson().ComputeHash() == PosterData.ToJson().ComputeHash() )
            {
                return;
            }

            PosterData = data;
            PosterImages = new Bitmap[PosterData.Items.Count];

            //
            // Load the background image and prepate the menu buttons.
            //
            BackgroundImage = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( PosterData.BackgroundImage.BestMatch ) );
            BackgroundImage = Utility.ScaleImageToWidth( BackgroundImage, ( int ) ( Crex.Application.Current.Resolution.Width / 2.0f ) );
            BackgroundImage = Utility.CreateBlurredImage( BackgroundImage, 8 );

            if ( Activity != null )
            {
                Activity.RunOnUiThread( UpdateUserInterfaceFromContent );
            }

            LastLoadedDate = DateTime.Now;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the user interface to match our content.
        /// </summary>
        private void UpdateUserInterfaceFromContent()
        {
            BackgroundImageView.SetImageBitmap( BackgroundImage );
            TitleView.Text = PosterData.Title;

            //
            // Initialize the adapter list.
            //
            PosterItemsAdapter.Clear();
            foreach ( var item in PosterData.Items )
            {
                PosterItemsAdapter.Add( item.Title );
            }

            //
            // Notify the list view that the content has changed and give it
            // input focus.
            //
            PosterItemsAdapter.NotifyDataSetChanged();
            PosterItems.RequestFocus();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemClick event of the listView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AdapterView.ItemClickEventArgs"/> instance containing the event data.</param>
        private void listView_ItemClick( object sender, AdapterView.ItemClickEventArgs e )
        {
            var item = PosterData.Items[e.Position];

            if ( item.Action != null )
            {
                Crex.Application.Current.StartAction( this, item.Action );
            }
            else
            {
                Crex.Application.Current.StartAction( this, item.ActionUrl );
            }
        }

        /// <summary>
        /// Handles the ItemSelected event of the listView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AdapterView.ItemSelectedEventArgs"/> instance containing the event data.</param>
        private void listView_ItemSelected( object sender, AdapterView.ItemSelectedEventArgs e )
        {
            if ( PosterImages == null || PosterImages[e.Position] == null )
            {
                //
                // Create and set a temporary 1 pixel by 1 pixel transparent bitmap image.
                //
                PosterImageView.Visibility = ViewStates.Invisible;

                //
                // Start a background task to load the image.
                //
                Task.Run( async () =>
                {
                    //
                    // Load the image.
                    //
                    var image = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( PosterData.Items[e.Position].Image.BestMatch ) );

                    //
                    // Store the image in our cache.
                    //
                    PosterImages[e.Position] = image;

                    //
                    // Update the UI.
                    //
                    Activity.RunOnUiThread( () =>
                    {
                        if ( PosterItems.SelectedItemPosition == e.Position )
                        {
                            if ( PosterImageView.Visibility == ViewStates.Invisible )
                            {
                                PosterImageView.Alpha = 0;
                                PosterImageView.Visibility = ViewStates.Visible;
                                PosterImageView.Animate().Alpha( 1 ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                            }

                            PosterImageView.SetImageBitmap( image );
                        }
                    } );
                } );
            }
            else
            {
                if ( PosterImageView.Visibility == ViewStates.Invisible )
                {
                    PosterImageView.Alpha = 0;
                    PosterImageView.Visibility = ViewStates.Visible;
                    PosterImageView.Animate().Alpha( 1 ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                }

                PosterImageView.SetImageBitmap( PosterImages[e.Position] );
            }

            //
            // Update the text content about the item.
            //
            DetailLeftView.Text = PosterData.Items[e.Position].DetailLeft;
            DetailRightView.Text = PosterData.Items[e.Position].DetailRight;
            DescriptionView.Text = PosterData.Items[e.Position].Description;
        }

        #endregion
    }
}