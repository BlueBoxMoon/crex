using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;

using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "Image" )]
    public class ImageActivity : CrexBaseActivity
    {
        #region Widgets

        ImageView ivBackground;
        Widgets.LoadingSpinner lsLoading;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            SetContentView( Resource.Layout.ImageView );

            ivBackground = FindViewById<ImageView>( Resource.Id.ivBackground );
            lsLoading = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            lsLoading.Start();

            LoadContentInBackground();
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
                var data = Intent.GetStringExtra( "data" ).FromJson<Rest.UrlSet>();
                var client = new System.Net.Http.HttpClient();
                var imageTask = client.GetAsync( data.BestMatch );
                var imageStream = await ( await imageTask ).Content.ReadAsStreamAsync();
                var image = BitmapFactory.DecodeStream( imageStream );

                if (image == null)
                {
                    throw new Exception( "Could not load image" );
                }

                RunOnUiThread( () =>
                {
                    ivBackground.SetImageBitmap( image );

                    lsLoading.Stop();
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
    }
}
