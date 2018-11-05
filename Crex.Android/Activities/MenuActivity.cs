using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "Menu" )]
    public class MenuActivity : CrexBaseActivity
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

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );
            SetContentView( Resource.Layout.MenuView );

            //
            // Get the controls in the view.
            //
            BackgroundImageView = FindViewById<ImageView>( Resource.Id.ivBackground );
            MenuBarView = FindViewById<Widgets.MenuBar>( Resource.Id.mbMainMenu );
            LoadingSpinnerView = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            //
            // Set initial states and indicate that we are loading to the user.
            //
            BackgroundImageView.Alpha = 0;
            MenuBarView.Alpha = 0;
            MenuBarView.ButtonClicked += menuBar_ButtonClicked;

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
        /// Loads the content for the main menu.
        /// </summary>
        /// <returns></returns>
        private void LoadContentInBackground()
        {
            Task.Run( async () =>
            {
                //
                // Load the menu data.
                //
                string url = Intent.GetStringExtra( "data" ).FromJson<string>();
                var json = await new System.Net.Http.HttpClient().GetStringAsync( url );
                var menu = JsonConvert.DeserializeObject<Rest.Menu>( json.ToString() );

                //
                // If the menu content hasn't actually changed, then ignore.
                //
                if ( menu.ToJson().ComputeHash() == MenuData.ToJson().ComputeHash() )
                {
                    return;
                }

                MenuData = menu;

                //
                // Check if an update is required to show this menu.
                //
                if ( MenuData.RequiredCrexVersion > Crex.Application.Current.CrexVersion )
                {
                    ShowUpdateRequiredDialog();

                    return;
                }

                //
                // Load the background image and prepate the menu buttons.
                //
                var imageTask = Utility.LoadImageFromUrlAsync( MenuData.BackgroundImage.BestMatch );
                var buttons = MenuData.Buttons.Select( b => b.Title ).ToList();
                var image = await imageTask;

                RunOnUiThread( () =>
                {
                    //
                    // Update the UI with the image and buttons.
                    //
                    BackgroundImageView.SetImageBitmap( image );
                    MenuBarView.SetButtons( buttons );
                    MenuBarView.RequestFocus();

                    //
                    // Animate the transition from black to our UI.
                    //
                    MenuBarView.Animate()
                        .SetDuration( Crex.Application.Current.Config.AnimationTime.Value )
                        .Alpha( 1 );
                    BackgroundImageView.Animate()
                        .SetDuration( Crex.Application.Current.Config.AnimationTime.Value )
                        .Alpha( 1 );
                    LoadingSpinnerView.Stop();
                } );

                LastLoadedDate = DateTime.Now;
            } )
            .ContinueWith( ( t ) =>
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
        /// Handles the ButtonClicked event of the menuBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Widgets.ButtonClickEventArgs"/> instance containing the event data.</param>
        private void menuBar_ButtonClicked( object sender, Widgets.ButtonClickEventArgs e )
        {
            var button = MenuData.Buttons[e.Position];

            Crex.Application.Current.StartAction( this, button.Action );
        }

        #endregion
    }
}
