using System;

namespace Crex
{
    public abstract class PreferenceManager
    {
        #region Get Methods

        /// <summary>
        /// Gets the bool value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public virtual bool? GetBoolValue( string key, bool? defaultValue = null )
        {
            if ( !bool.TryParse( GetStringValue( key ), out bool result ) )
            {
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Gets the date time value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public virtual DateTime? GetDateTimeValue( string key, DateTime? defaultValue = null )
        {
            if ( !DateTime.TryParse( GetStringValue( key ), out DateTime result ) )
            {
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Gets the float value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public virtual float? GetFloatValue( string key, float? defaultValue = null )
        {
            if ( !float.TryParse( GetStringValue( key ), out float result ) )
            {
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public virtual int? GetIntValue( string key, int? defaultValue = null )
        {
            if ( !int.TryParse( GetStringValue( key ), out int result ) )
            {
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public abstract string GetStringValue( string key, string defaultValue = null );

        #endregion

        #region Set Methods

        /// <summary>
        /// Sets the bool value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public virtual void SetBoolValue( string key, bool value )
        {
            SetStringValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the date time value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void SetDateTimeValue( string key, DateTime value )
        {
            SetStringValue( key, value.ToString( "s" ) );
        }

        /// <summary>
        /// Sets the int value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void SetIntValue( string key, int value )
        {
            SetStringValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the float value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void SetFloatValue( string key, float value )
        {
            SetStringValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public abstract void SetStringValue( string key, string value );

        /// <summary>
        /// Removes the value.
        /// </summary>
        /// <param name="key">The key.</param>
        public abstract void RemoveValue( string key );

        #endregion
    }
}
