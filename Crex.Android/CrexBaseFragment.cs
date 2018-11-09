using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Crex.Android
{
	public class CrexBaseFragment : Fragment
    {
        public string Data { get; set; }

        public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
        {
            if ( container == null )
            {
                return null;
            }

            var layout = new FrameLayout( Activity );
            layout.SetBackgroundColor( new global::Android.Graphics.Color( 178, 178, 178 ) );

            return layout;
        }

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
    }
}
