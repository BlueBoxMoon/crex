using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Crex.Extensions;

namespace Crex.Android.Activities
{
    public class MenuFragment : CrexBaseFragment
    {
        #region Views

        protected ImageView BackgroundImageView { get; private set; }

        protected Widgets.MenuBar MenuBarView { get; private set; }

        protected Widgets.LoadingSpinner LoadingSpinnerView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the menu data.
        /// </summary>
        /// <value>
        /// The menu data.
        /// </value>
        protected Rest.Menu MenuData { get; private set; }

        /// <summary>
        /// Gets the date we last loaded our content.
        /// </summary>
        /// <value>
        /// The date we last loaded our content.
        /// </value>
        protected DateTime LastLoadedDate { get; private set; } = DateTime.MinValue;

        #endregion

        #region Base Method Overrides

        public override void OnViewCreated( View view, Bundle savedInstanceState )
        {
            base.OnViewCreated( view, savedInstanceState );

            var layout = ( ViewGroup ) view;

            //
            // Setup the background image view.
            //
            BackgroundImageView = new ImageView( Activity );
            BackgroundImageView.LayoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
            BackgroundImageView.SetScaleType( ImageView.ScaleType.CenterCrop );
            layout.AddView( BackgroundImageView );

            //
            // Setup the Menu Bar
            //
            MenuBarView = new Widgets.MenuBar( Activity, null );
            int height = ( int ) TypedValue.ApplyDimension( ComplexUnitType.Dip, 60, Resources.DisplayMetrics );
            var layoutParameters = new FrameLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, height );
            layoutParameters.Gravity = GravityFlags.Bottom;
            MenuBarView.LayoutParameters = layoutParameters;
            MenuBarView.ButtonClicked += MenuBar_ButtonClicked;
            layout.AddView( MenuBarView );
        }

        public override async Task LoadContentAsync()
        {
            var menu = Data.FromJson<Rest.Menu>();

            //
            // If the menu content hasn't actually changed, then ignore.
            //
            if ( menu.ToJson().ComputeHash() == MenuData.ToJson().ComputeHash() )
            {
                return;
            }

            MenuData = menu;

            //
            // Load the background image and prepate the menu buttons.
            //
            var imageTask = Utility.LoadImageFromUrlAsync( MenuData.BackgroundImage.BestMatch );
            var buttons = MenuData.Buttons.Select( b => b.Title ).ToList();
            var image = await imageTask;

            Activities.CrexActivity.MainActivity.RunOnUiThread( () =>
            {
                //
                // Update the UI with the image and buttons.
                //
//                BackgroundImageView.SetImageBitmap( image );
//                MenuBarView.SetButtons( buttons );
//                MenuBarView.RequestFocus();
            } );

            LastLoadedDate = DateTime.Now;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ButtonClicked event of the menuBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Widgets.ButtonClickEventArgs"/> instance containing the event data.</param>
        private void MenuBar_ButtonClicked( object sender, Widgets.ButtonClickEventArgs e )
        {
            var button = MenuData.Buttons[e.Position];

            Crex.Application.Current.StartAction( this, button.Action );
        }

        #endregion
    }
}
