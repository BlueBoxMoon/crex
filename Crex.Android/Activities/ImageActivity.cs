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
        #region Views

        protected ImageView BackgroundImageView { get; private set; }

        protected Widgets.LoadingSpinner LoadingSpinnerView { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            SetContentView( Resource.Layout.ImageView );

            BackgroundImageView = FindViewById<ImageView>( Resource.Id.ivBackground );
            LoadingSpinnerView = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            LoadingSpinnerView.Start();

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
                    BackgroundImageView.SetImageBitmap( image );

                    BackgroundImageView.Animate()
                        .SetDuration( Crex.Application.Current.Config.AnimationTime.Value )
                        .Alpha( 1 );

                    LoadingSpinnerView.Stop();
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
