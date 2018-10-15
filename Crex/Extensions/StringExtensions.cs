using Newtonsoft.Json;

namespace Crex.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert the JSON encoded string into an object instance.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="s">The string.</param>
        /// <returns>An instance of T or null if it could not be deserialized.</returns>
        public static T FromJson<T>( this string s )
        {
            try
            {
                return JsonConvert.DeserializeObject<T>( s );
            }
            catch
            {
                return default( T );
            }
        }
    }
}