using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Crex.Android
{
	public class CrexBaseFragment : Fragment
    {
        #region Properties

        /// <summary>
        /// Gets or sets the data that the template will use to display its UI.
        /// </summary>
        /// <value>
        /// The data that the template will use to display its UI.
        /// </value>
        public string Data { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Called to have the fragment instantiate its user interface view. Initializes a
        /// FrameLayout with a dark background color.
        /// </summary>
        public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
        {
            if ( container == null )
            {
                return null;
            }

            var layout = new FrameLayout( Activity );
            layout.SetBackgroundColor( new global::Android.Graphics.Color( 50, 50, 50 ) );

            return layout;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public virtual async Task LoadContentAsync()
        {
            //
            // Silence compiler warnings.
            //
            await Task.Delay( 0 );
        }

        /// <summary>
        /// Converts a DIP/DP value to a pixel value.
        /// </summary>
        /// <param name="dip">The dip value.</param>
        /// <returns>The same value in pixels.</returns>
        protected int DipToPixel( int dip )
        {
            return ( int ) ( dip * Activity.Resources.DisplayMetrics.Density );
        }

        #endregion
    }
}
