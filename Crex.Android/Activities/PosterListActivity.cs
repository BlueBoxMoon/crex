using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "PosterList" )]
    public class PosterListActivity : CrexBaseActivity
    {
        #region Views

        protected ImageView BackgroundImageView { get; private set; }

        protected TextView TitleView { get; private set; }

        protected ImageView PosterImageView { get; private set; }

        protected TextView DetailLeftView { get; private set; }

        protected TextView DetailRightView { get; private set; }

        protected TextView DescriptionView { get; private set; }

        protected Widgets.LoadingSpinner LoadingSpinnerView { get; private set; }

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

        /// <summary>
        /// Gets the target alpha for the background image.
        /// </summary>
        /// <value>
        /// The target alpha for the background image.
        /// </value>
        protected float BackgroundAlpha => 0.25f;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            SetContentView( Resource.Layout.PosterListView );

            //
            // Get all our child views.
            //
            BackgroundImageView = FindViewById<ImageView>( Resource.Id.ivBackground );
            TitleView = FindViewById<TextView>( Resource.Id.tvTitle );
            PosterImageView = FindViewById<ImageView>( Resource.Id.ivPosterImage );
            DetailLeftView = FindViewById<TextView>( Resource.Id.tvDetailLeft );
            DetailRightView = FindViewById<TextView>( Resource.Id.tvDetailRight );
            DescriptionView = FindViewById<TextView>( Resource.Id.tvDescription );
            PosterItems = FindViewById<ListView>( Resource.Id.lvPosterItems );
            LoadingSpinnerView = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            //
            // Set initial states and indicate that we are loading to the user.
            //
            TitleView.Text = string.Empty;
            DetailLeftView.Text = string.Empty;
            DetailRightView.Text = string.Empty;
            DescriptionView.Text = string.Empty;
            BackgroundImageView.Alpha = 0;

            //
            // Initialize the data and settings for the list view.
            //
            PosterItemsAdapter = new ArrayAdapter<string>( this, Resource.Layout.PosterListViewItem );
            PosterItems.Adapter = PosterItemsAdapter;
            PosterItems.ItemClick += listView_ItemClick;
            PosterItems.ItemSelected += listView_ItemSelected;

            LoadingSpinnerView.Start();
        }

        /// <summary>
        /// Called after <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c>, <c><see cref="M:Android.App.Activity.OnRestart" /></c>, or
        /// <c><see cref="M:Android.App.Activity.OnPause" /></c>, for your activity to start interacting with the user.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            if ( DateTime.Now.Subtract( LastLoadedDate ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
            {
                LoadContentInBackground();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content for the image view.
        /// </summary>
        /// <returns></returns>
        private void LoadContentInBackground()
        {
            Task.Run( async () =>
            {
                //
                // Load the content.
                //
                var url = Intent.GetStringExtra( "data" ).FromJson<string>();
                var client = new System.Net.Http.HttpClient();
                var json = await client.GetStringAsync( url );
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Rest.PosterList>( json );

                //
                // If the content hasn't actually changed, then ignore.
                //
                if ( data.ToJson().ComputeHash() == PosterData.ToJson().ComputeHash() )
                {
                    return;
                }

                PosterData = data;

                //
                // Check if an update is required to show this menu.
                //
                if ( PosterData.RequiredCrexVersion > Crex.Application.Current.CrexVersion )
                {
                    ShowUpdateRequiredDialog();

                    return;
                }

                //
                // Load the background image in the background.
                //
                var imageTask = client.GetAsync( PosterData.BackgroundImage.BestMatch );

                //
                // Set the UI elements related to the initial selections.
                //
                RunOnUiThread( () =>
                {
                    PosterImages = new Bitmap[PosterData.Items.Count];

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

                    LoadingSpinnerView.Stop();
                } );

                //
                // Wait for the background image to load and then display it.
                //
                var imageStream = await ( await imageTask ).Content.ReadAsStreamAsync();
                var image = BitmapFactory.DecodeStream( imageStream );
                image = Utility.ScaleImageToWidth( image, ( int ) ( BackgroundImageView.Width / 2.0f ) );
                image = Utility.CreateBlurredImage( image, 4 );

                RunOnUiThread( () =>
                {
                    BackgroundImageView.SetImageBitmap( image );
                    BackgroundImageView.Animate().Alpha( BackgroundAlpha ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                } );

                LastLoadedDate = DateTime.Now;
            } ).ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    Console.WriteLine( $"Data Load Error: { t.Exception.InnerException.Message }\n{ t.Exception.InnerException.StackTrace }" );
                    ShowDataErrorDialog( LoadContentInBackground );
                }
            } );
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

            Crex.Application.Current.StartAction( this, item.Action );
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
                //listViewImages[e.Position] = Bitmap.CreateBitmap( 1, 1, Bitmap.Config.Argb8888 );
                //ivPosterImage.SetImageBitmap( listViewImages[e.Position] );

                //
                // Start a background task to load the image.
                //
                Task.Run( async () =>
                {
                    //
                    // Load the image.
                    //
                    var client = new System.Net.Http.HttpClient();
                    var imageTask = client.GetAsync( PosterData.Items[e.Position].Image.BestMatch );
                    var imageStream = await ( await imageTask ).Content.ReadAsStreamAsync();
                    var image = BitmapFactory.DecodeStream( imageStream );

                    //
                    // Store the image in our cache.
                    //
                    PosterImages[e.Position] = image;

                    //
                    // Update the UI.
                    //
                    RunOnUiThread( () =>
                    {
                        if ( PosterItems.SelectedItemPosition == e.Position )
                        {
                            if (PosterImageView.Visibility == ViewStates.Invisible)
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
