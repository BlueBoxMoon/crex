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
        #region Widgets

        ImageView ivBackground;
        TextView tvTitle;
        ImageView ivPosterImage;
        TextView tvDetailLeft;
        TextView tvDetailRight;
        TextView tvDescription;
        Widgets.LoadingSpinner lsLoading;

        #endregion

        #region Fields

        Rest.PosterList posterListData;
        ListView lvPosterItems;
        ArrayAdapter listAdapter;
        Bitmap[] listViewImages;
        const float BackgroundAlpha = 0.25f;
        DateTime lastLoadedData = DateTime.MinValue;

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
            ivBackground = FindViewById<ImageView>( Resource.Id.ivBackground );
            tvTitle = FindViewById<TextView>( Resource.Id.tvTitle );
            ivPosterImage = FindViewById<ImageView>( Resource.Id.ivPosterImage );
            tvDetailLeft = FindViewById<TextView>( Resource.Id.tvDetailLeft );
            tvDetailRight = FindViewById<TextView>( Resource.Id.tvDetailRight );
            tvDescription = FindViewById<TextView>( Resource.Id.tvDescription );
            lvPosterItems = FindViewById<ListView>( Resource.Id.lvPosterItems );
            lsLoading = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            //
            // Set initial states and indicate that we are loading to the user.
            //
            tvTitle.Text = string.Empty;
            tvDetailLeft.Text = string.Empty;
            tvDetailRight.Text = string.Empty;
            tvDescription.Text = string.Empty;
            ivBackground.Alpha = 0;

            //
            // Initialize the data and settings for the list view.
            //
            listAdapter = new ArrayAdapter<string>( this, Resource.Layout.PosterListViewItem );
            lvPosterItems.Adapter = listAdapter;
            lvPosterItems.ItemClick += listView_ItemClick;
            lvPosterItems.ItemSelected += listView_ItemSelected;

            lsLoading.Start();
        }

        /// <summary>
        /// Called after <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c>, <c><see cref="M:Android.App.Activity.OnRestart" /></c>, or
        /// <c><see cref="M:Android.App.Activity.OnPause" /></c>, for your activity to start interacting with the user.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            if ( DateTime.Now.Subtract( lastLoadedData ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
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
                if ( data.ToJson().ComputeHash() == posterListData.ToJson().ComputeHash() )
                {
                    return;
                }

                posterListData = data;

                //
                // Check if an update is required to show this menu.
                //
                if ( posterListData.RequiredCrexVersion > Crex.Application.Current.CrexVersion )
                {
                    ShowUpdateRequiredDialog();

                    return;
                }

                //
                // Load the background image in the background.
                //
                var imageTask = client.GetAsync( posterListData.BackgroundImage.BestMatch );

                RunOnUiThread( () =>
                {
                    listViewImages = new Bitmap[posterListData.Items.Count];

                    tvTitle.Text = posterListData.Title;

                    //
                    // Initialize the adapter list.
                    //
                    listAdapter.Clear();
                    foreach ( var item in posterListData.Items )
                    {
                        listAdapter.Add( item.Title );
                    }

                    //
                    // Notify the list view that the content has changed and give it
                    // input focus.
                    //
                    listAdapter.NotifyDataSetChanged();
                    lvPosterItems.RequestFocus();

                    lsLoading.Stop();
                } );

                //
                // Wait for the background image to load and then display it.
                //
                var imageStream = await ( await imageTask ).Content.ReadAsStreamAsync();
                var image = BitmapFactory.DecodeStream( imageStream );
                image = Utility.CreateBlurredImage( image, 16 );

                RunOnUiThread( () =>
                {
                    ivBackground.SetImageBitmap( image );
                    ivBackground.Animate().Alpha( BackgroundAlpha ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                } );
            } ).ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
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
            var item = posterListData.Items[e.Position];

            Crex.Application.Current.StartAction( this, item.Action );
        }

        /// <summary>
        /// Handles the ItemSelected event of the listView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AdapterView.ItemSelectedEventArgs"/> instance containing the event data.</param>
        private void listView_ItemSelected( object sender, AdapterView.ItemSelectedEventArgs e )
        {
            if ( listViewImages == null || listViewImages[e.Position] == null )
            {
                //
                // Create and set a temporary 1 pixel by 1 pixel transparent bitmap image.
                //
                ivPosterImage.Visibility = ViewStates.Invisible;
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
                    var imageTask = client.GetAsync( posterListData.Items[e.Position].Image.BestMatch );
                    var imageStream = await ( await imageTask ).Content.ReadAsStreamAsync();
                    var image = BitmapFactory.DecodeStream( imageStream );

                    //
                    // Store the image in our cache.
                    //
                    listViewImages[e.Position] = image;

                    //
                    // Update the UI.
                    //
                    RunOnUiThread( () =>
                    {
                        if ( lvPosterItems.SelectedItemPosition == e.Position )
                        {
                            if (ivPosterImage.Visibility == ViewStates.Invisible)
                            {
                                ivPosterImage.Alpha = 0;
                                ivPosterImage.Visibility = ViewStates.Visible;
                                ivPosterImage.Animate().Alpha( 1 ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                            }

                            ivPosterImage.SetImageBitmap( image );
                        }
                    } );
                } );
            }
            else
            {
                if ( ivPosterImage.Visibility == ViewStates.Invisible )
                {
                    ivPosterImage.Alpha = 0;
                    ivPosterImage.Visibility = ViewStates.Visible;
                    ivPosterImage.Animate().Alpha( 1 ).SetDuration( Crex.Application.Current.Config.AnimationTime.Value );
                }

                ivPosterImage.SetImageBitmap( listViewImages[e.Position] );
            }

            //
            // Update the text content about the item.
            //
            tvDetailLeft.Text = posterListData.Items[e.Position].DetailLeft;
            tvDetailRight.Text = posterListData.Items[e.Position].DetailRight;
            tvDescription.Text = posterListData.Items[e.Position].Description;
        }

        #endregion
    }
}
