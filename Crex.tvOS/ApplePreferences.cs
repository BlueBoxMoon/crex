using System;
using Foundation;

namespace Crex.tvOS
{
    public class ApplePreferences : PreferenceManager
    {
        private NSUserDefaults Preferences { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Crex.tvOS.ApplePreferences"/> class.
        /// </summary>
        public ApplePreferences()
        {
            Preferences = NSUserDefaults.StandardUserDefaults;
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public override string GetStringValue( string key, string defaultValue = null )
        {
            return Preferences.StringForKey( key ) ?? defaultValue;
        }

        /// <summary>
        /// Removes the value.
        /// </summary>
        /// <param name="key">The key.</param>
        public override void RemoveValue( string key )
        {
            Preferences.RemoveObject( key );
        }

        /// <summary>
        /// Sets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public override void SetStringValue( string key, string value )
        {
            Preferences.SetString( value, key );
        }
    }
}
