using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Crex.tvOS
{
    public static class Utility
    {
        /// <summary>
        /// Loads the image from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>An awaitable task that will return the image or an error.</returns>
        public static async Task<UIImage> LoadImageFromUrlAsync( string url )
        {
            var client = new System.Net.Http.HttpClient();
            var imageTask = client.GetAsync( url );

            var stream = await ( await imageTask ).Content.ReadAsStreamAsync();

            return UIImage.LoadFromData( NSData.FromStream( stream ) );
        }

        /// <summary>
        /// Gets the stream for named resource.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>A stream that contains the resource data or null if the resource was not found.</returns>
        /// <remarks>
        /// A name must be in one of the following formats:
        /// resource:Assembly.Name.Dll:Assembly.Name.Some.Resource.Jpg
        /// resource:Assembly.Name.Some.Resource.jpg (only looks in executing assembly)
        /// asset:some-asset.jpg (only looks in the Bundle)
        /// </remarks>
        /// <exception cref="ArgumentException">Resource name not recognized - name</exception>
        public static Stream GetStreamForNamedResource( string name )
        {
            var segments = name.Split( ':' );

            if ( segments[0] == "resource" && segments.Length == 3 )
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault( a => a.GetName().Name == segments[1] );

                return assembly?.GetManifestResourceStream( segments[2] );
            }
            else if ( segments[0] == "resource" && segments.Length == 2 )
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                return assembly?.GetManifestResourceStream( segments[1] );
            }
            else if ( segments[0] == "asset" && segments.Length == 2 )
            {
                string path = NSBundle.MainBundle.PathForResource( segments[0].Split( '.' ).First(), segments[0].Split( '.' ).Last() );

                return File.OpenRead( path );
            }
            else
            {
                throw new ArgumentException( "Resource name not recognized", nameof( name ) );
            }
        }

    }
}
