using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Renderscripts;

namespace Crex.Android
{
    internal static class Utility
    {
        /// <summary>
        /// Loads the image from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>An awaitable task that will return the Bitmap image or an error.</returns>
        public static async Task<Bitmap> LoadImageFromUrlAsync( string url )
        {
            var client = new System.Net.Http.HttpClient();
            var imageTask = client.GetAsync( url );

            var stream = await( await imageTask ).Content.ReadAsStreamAsync();

            return BitmapFactory.DecodeStream( stream );
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
        /// asset:some-asset.jpg (only looks in the Android asset list)
        /// </remarks>
        /// <exception cref="ArgumentException">Resource name not recognized - name</exception>
        public static Stream GetStreamForNamedResource( string name )
        {
            var segments = name.Split( ':' );

            if ( segments[0] == "resource" && segments.Length == 3 )
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault( a => a.GetName().Name == segments[1] );

                if ( assembly == null )
                {
                    return null;
                }

                return assembly.GetManifestResourceStream( segments[2] );
            }
            else if ( segments[0] == "resource" && segments.Length == 2 )
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if ( assembly == null )
                {
                    return null;
                }

                return assembly.GetManifestResourceStream( segments[1] );
            }
            else if ( segments[0] == "asset" && segments.Length == 2 )
            {
                return global::Android.App.Application.Context.Assets.Open( segments[1] );
            }
            else
            {
                throw new ArgumentException( "Resource name not recognized", "name" );
            }
        }

        /// <summary>
        /// Creates a blurred image of an existing image at the given radius.
        /// </summary>
        /// <param name="originalBitmap">The original bitmap.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public static Bitmap CreateBlurredImage( Bitmap originalBitmap, int radius )
        {
            // Create another bitmap that will hold the results of the filter.
            Bitmap blurredBitmap = Bitmap.CreateBitmap( originalBitmap );

            // Create the Renderscript instance that will do the work.
            RenderScript rs = RenderScript.Create( global::Android.App.Application.Context );

            // Allocate memory for Renderscript to work with
            Allocation input = Allocation.CreateFromBitmap( rs, originalBitmap, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script );
            Allocation output = Allocation.CreateTyped( rs, input.Type );

            // Load up an instance of the specific script that we want to use.
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create( rs, Element.U8_4( rs ) );
            script.SetInput( input );

            // Set the blur radius
            script.SetRadius( radius );

            // Start the ScriptIntrinisicBlur
            script.ForEach( output );

            // Copy the output to the blurred bitmap
            output.CopyTo( blurredBitmap );

            return blurredBitmap;
        }

        /// <summary>
        /// Scales the image to the specified width, preserving aspect ratio.
        /// </summary>
        /// <param name="originalBitmap">The original bitmap.</param>
        /// <param name="width">The new width.</param>
        /// <returns>An image of the given width in pixels.</returns>
        public static Bitmap ScaleImageToWidth( Bitmap originalBitmap, int width )
        {
            float ratio = originalBitmap.Width / ( float ) width;

            return Bitmap.CreateScaledBitmap( originalBitmap, width, ( int ) ( originalBitmap.Height / ratio ), true );
        }
    }
}