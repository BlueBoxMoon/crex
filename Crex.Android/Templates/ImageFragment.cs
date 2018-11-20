using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Crex.Extensions;

namespace Crex.Android.Templates
{
    public class ImageFragment : CrexBaseFragment
    {
        #region Views

        /// <summary>
        /// Gets the background image view.
        /// </summary>
        /// <value>
        /// The background image view.
        /// </value>
        protected ImageView BackgroundImageView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the background image that was loaded on the background thread.
        /// </summary>
        /// <value>
        /// The background image that was loaded on the background thread.
        /// </value>
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
            // Setup the background image view.
            //
            BackgroundImageView = new ImageView( Activity )
            {
                LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent ),
                Focusable = true
            };
            BackgroundImageView.SetScaleType( ImageView.ScaleType.CenterCrop );
            layout.AddView( BackgroundImageView );

            //
            // Set our initial data if we have already loaded.
            //
            UpdateUserInterfaceFromContent();
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            var urlSet = Data.FromJson<Rest.UrlSet>();

            //
            // Load the background image.
            //
            BackgroundImage = await Utility.LoadImageFromUrlAsync( Crex.Application.Current.GetAbsoluteUrl( urlSet.BestMatch ) );

            if ( Activity != null )
            {
                Activity.RunOnUiThread( UpdateUserInterfaceFromContent );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the user interface to match the content.
        /// </summary>
        private void UpdateUserInterfaceFromContent()
        {
            BackgroundImageView.SetImageBitmap( BackgroundImage );
            BackgroundImageView.RequestFocus();
        }

        #endregion
    }
}