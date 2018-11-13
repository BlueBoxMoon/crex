using Android.Content;

namespace Crex.Android
{
    public class AndroidPreferences : PreferenceManager
    {
        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        /// <value>
        /// The preferences.
        /// </value>
        private ISharedPreferences Preferences { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidPreferences"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public AndroidPreferences( Context context )
        {
            Preferences = global::Android.Preferences.PreferenceManager.GetDefaultSharedPreferences( context );
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public override string GetStringValue( string key, string defaultValue = null )
        {
            return Preferences.GetString( key, defaultValue );
        }

        /// <summary>
        /// Sets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public override void SetStringValue( string key, string value )
        {
            var editor = Preferences.Edit();

            editor.PutString( key, value );
            editor.Apply();
        }

        /// <summary>
        /// Removes the value.
        /// </summary>
        /// <param name="key">The key.</param>
        public override void RemoveValue( string key )
        {
            var editor = Preferences.Edit();

            editor.Remove( key );
            editor.Apply();
        }
    }
}
