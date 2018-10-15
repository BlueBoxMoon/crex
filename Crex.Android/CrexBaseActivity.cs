using System;

using Android.App;

namespace Crex.Android
{
    /// <summary>
    /// Defines common methods that will be used by all Crex activities.
    /// </summary>
    public class CrexBaseActivity : Activity
    {
        /// <summary>
        /// Shows the update required dialog.
        /// </summary>
        protected void ShowUpdateRequiredDialog()
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Update Required" )
                .SetMessage( "An update is required to view this content." )
                .SetPositiveButton( "Close", new Dialogs.OnClickAction( Finish ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( Finish ) );

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }

        /// <summary>
        /// Shows the update required dialog.
        /// </summary>
        protected void ShowDataErrorDialog( Action retry )
        {
            var builder = new AlertDialog.Builder( this, global::Android.Resource.Style.ThemeDeviceDefaultDialogAlert );

            builder.SetTitle( "Error loading data" )
                .SetMessage( "An error occurred trying to load the content. Please try again later." )
                .SetPositiveButton( "Retry", new Dialogs.OnClickAction( retry ) )
                .SetOnCancelListener( new Dialogs.OnCancelAction( Finish ) );

            RunOnUiThread( () =>
            {
                builder.Show();
            } );
        }
    }
}