using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Newtonsoft.Json;

using Crex.Extensions;

namespace Crex.Android.Activities
{
    [Activity( Label = "Menu" )]
    public class MenuActivity : CrexBaseActivity
    {
        #region View Fields

        ImageView ivBackground;
        Widgets.MenuBar mbMainMenu;
        Widgets.LoadingSpinner lsLoading;
        Rest.MainMenu mainMenu;

        #endregion

        #region Fields

        DateTime lastLoadedData = DateTime.MinValue;

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
            ivBackground = FindViewById<ImageView>( Resource.Id.ivBackground );
            mbMainMenu = FindViewById<Widgets.MenuBar>( Resource.Id.mbMainMenu );
            lsLoading = FindViewById<Widgets.LoadingSpinner>( Resource.Id.lsLoading );

            //
            // Set initial states and indicate that we are loading to the user.
            //
            ivBackground.Alpha = 0;
            mbMainMenu.Alpha = 0;
            mbMainMenu.ButtonClicked += menuBar_ButtonClicked;

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
                Log.Debug( "Crex", "Loading Content" );
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
                var menu = JsonConvert.DeserializeObject<Rest.MainMenu>( json.ToString() );

                //
                // If the menu content hasn't actually changed, then ignore.
                //
                if ( menu.ToJson().ComputeHash() == mainMenu.ToJson().ComputeHash() )
                {
                    return;
                }

                mainMenu = menu;

                //
                // Check if an update is required to show this menu.
                //
                if ( mainMenu.RequiredCrexVersion > Crex.Application.Current.CrexVersion )
                {
                    ShowUpdateRequiredDialog();

                    return;
                }

                //
                // Load the background image and prepate the menu buttons.
                //
                var imageTask = Utility.LoadImageFromUrlAsync( mainMenu.BackgroundImage.BestMatch );
                var buttons = mainMenu.Buttons.Select( b => b.Title ).ToList();
                var image = await imageTask;

                RunOnUiThread( () =>
                {
                    //
                    // Update the UI with the image and buttons.
                    //
                    ivBackground.SetImageBitmap( image );
                    mbMainMenu.SetButtons( buttons );
                    mbMainMenu.RequestFocus();

                    //
                    // Animate the transition from black to our UI.
                    //
                    mbMainMenu.Animate()
                        .SetDuration( Crex.Application.Current.Config.AnimationTime.Value )
                        .Alpha( 1 );
                    ivBackground.Animate()
                        .SetDuration( Crex.Application.Current.Config.AnimationTime.Value )
                        .Alpha( 1 );
                    lsLoading.Stop();
                } );

                lastLoadedData = DateTime.Now;
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
            var button = mainMenu.Buttons[e.Position];

            Crex.Application.Current.StartAction( this, button.Action );
        }

        #endregion
    }
}
