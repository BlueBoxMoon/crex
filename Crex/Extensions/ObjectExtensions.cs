using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Crex.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Serialize the object into a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A JSON encoded string or null if it could not be encoded.</returns>
        public static string ToJson( this object obj )
        {
            try
            {
                return JsonConvert.SerializeObject( obj );
            }
            catch
            {
                return null;
            }
        }

        public static T ToObject<T>( this IDictionary<string, object> source )
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach ( var item in source )
            {
                var property = someObjectType.GetProperty( item.Key );

                if ( property != null )
                {
                    property.SetValue( someObject, item.Value, null );
                }
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary( this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance )
        {
            return source.GetType().GetProperties( bindingAttr ).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue( source, null )
            );

        }
    }
}