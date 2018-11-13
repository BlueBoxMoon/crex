using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using CoreImage;
using CoreGraphics;
using Accelerate;

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

        /// <summary>
        /// Creates a blurred image of an existing image at the given radius.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public static UIImage CreateBlurredImage( UIImage originalImage, int radius )
        {
            UIImage newImage = null;

            if (originalImage == null)
            {
                return null;
            }

            originalImage.InvokeOnMainThread( () => newImage = InternalCreateBlurredImage( originalImage, radius ) );

            return newImage;
        }

        /// <summary>
        /// Scales the image to th specified width, preserving aspect ratio.
        /// </summary>
        /// <returns>A new image at the requested width.</returns>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The target width.</param>
        public static UIImage ScaleImageToWidth( UIImage image, int width )
        {
            UIImage scaledImage = null;

            image.InvokeOnMainThread( () =>
            {
                var size = new CGSize( width, width / image.Size.Width * image.Size.Height );

                UIGraphics.BeginImageContextWithOptions( size, false, UIScreen.MainScreen.Scale );
                var context = UIGraphics.GetCurrentContext();
                context.TranslateCTM( 0, size.Height );
                context.ScaleCTM( 1.0f, -1.0f );

                context.DrawImage( new CGRect( CGPoint.Empty, size ), image.CGImage );

                scaledImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
            } );

            return scaledImage;
        }

        /// <summary>
        /// Internal method to create a blurred image since this has to run on the main thread.
        /// </summary>
        /// <returns>The blurred image.</returns>
        /// <param name="image">Image to be blurred.</param>
        /// <param name="blurRadius">Blur radius.</param>
        /// <remarks>
        /// Originally from: https://github.com/xamarin/ios-samples/blob/master/UIImageEffects/UIImageEffects.cs
        /// </remarks>
        private static UIImage InternalCreateBlurredImage( UIImage image, float blurRadius )
        {
            var imageRect = new CGRect( CGPoint.Empty, image.Size );
            var effectImage = image;

            UIGraphics.BeginImageContextWithOptions( image.Size, false, UIScreen.MainScreen.Scale );
            var contextIn = UIGraphics.GetCurrentContext();
            contextIn.ScaleCTM( 1.0f, -1.0f );
            contextIn.TranslateCTM( 0, -image.Size.Height );
            contextIn.DrawImage( imageRect, image.CGImage );
            var effectInContext = contextIn.AsBitmapContext() as CGBitmapContext;

            var effectInBuffer = new vImageBuffer()
            {
                Data = effectInContext.Data,
                Width = ( int ) effectInContext.Width,
                Height = ( int ) effectInContext.Height,
                BytesPerRow = ( int ) effectInContext.BytesPerRow
            };

            UIGraphics.BeginImageContextWithOptions( image.Size, false, UIScreen.MainScreen.Scale );
            var effectOutContext = UIGraphics.GetCurrentContext().AsBitmapContext() as CGBitmapContext;
            var effectOutBuffer = new vImageBuffer()
            {
                Data = effectOutContext.Data,
                Width = ( int ) effectOutContext.Width,
                Height = ( int ) effectOutContext.Height,
                BytesPerRow = ( int ) effectOutContext.BytesPerRow
            };

            var inputRadius = blurRadius * UIScreen.MainScreen.Scale / 2.0f;
            uint radius = ( uint ) ( Math.Floor( inputRadius * 3 * Math.Sqrt( 2 * Math.PI ) / 4 + 0.5 ) );
            if ( ( radius % 2 ) != 1 )
                radius += 1;
            vImage.BoxConvolveARGB8888( ref effectInBuffer, ref effectOutBuffer, IntPtr.Zero, 0, 0, radius, radius, Pixel8888.Zero, vImageFlags.EdgeExtend );
            vImage.BoxConvolveARGB8888( ref effectOutBuffer, ref effectInBuffer, IntPtr.Zero, 0, 0, radius, radius, Pixel8888.Zero, vImageFlags.EdgeExtend );
            vImage.BoxConvolveARGB8888( ref effectInBuffer, ref effectOutBuffer, IntPtr.Zero, 0, 0, radius, radius, Pixel8888.Zero, vImageFlags.EdgeExtend );

            effectImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            UIGraphics.EndImageContext();

            // Setup up output context
            UIGraphics.BeginImageContextWithOptions( image.Size, false, UIScreen.MainScreen.Scale );
            var outputContext = UIGraphics.GetCurrentContext();
            outputContext.ScaleCTM( 1, -1 );
            outputContext.TranslateCTM( 0, -image.Size.Height );

            // Draw base image
            outputContext.SaveState();
            outputContext.DrawImage( imageRect, effectImage.CGImage );
            outputContext.RestoreState();
            var outputImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return outputImage;
        }
    }
}
