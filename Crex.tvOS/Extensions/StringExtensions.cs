using System;
using CoreGraphics;
using UIKit;

namespace Crex.tvOS.Extensions
{
    public static class StringExtensions
    {
        public static UIColor AsUIColor( this string s )
        {
            string colorString = s.Replace( "#", string.Empty );
            float alpha;
            float red;
            float green;
            float blue;

            switch (colorString.Length)
            {
                case 3: // RGB
                    alpha = 1.0f;
                    red = Convert.ToInt32( new String( colorString[0], 2 ), 16 ) / 255.0f;
                    green = Convert.ToInt32( new String( colorString[1], 2 ), 16 ) / 255.0f;
                    blue = Convert.ToInt32( new String( colorString[2], 2 ), 16 ) / 255.0f;
                    break;

                case 4: // ARGB
                    alpha = Convert.ToInt32( new String( colorString[0], 2 ), 16 ) / 255.0f;
                    red = Convert.ToInt32( new String( colorString[1], 2 ), 16 ) / 255.0f;
                    green = Convert.ToInt32( new String( colorString[2], 2 ), 16 ) / 255.0f;
                    blue = Convert.ToInt32( new String( colorString[3], 2 ), 16 ) / 255.0f;
                    break;

                case 6: // RRGGBB
                    alpha = 1.0f;
                    red = Convert.ToInt32( colorString.Substring( 0, 2 ), 16 ) / 255.0f;
                    green = Convert.ToInt32( colorString.Substring( 2, 2 ), 16 ) / 255.0f;
                    blue = Convert.ToInt32( colorString.Substring( 4, 2 ), 16 ) / 255.0f;
                    break;

                case 8: // AARRGGBB
                    alpha = Convert.ToInt32( colorString.Substring( 0, 2 ), 16 ) / 255.0f;
                    red = Convert.ToInt32( colorString.Substring( 2, 2 ), 16 ) / 255.0f;
                    green = Convert.ToInt32( colorString.Substring( 4, 2 ), 16 ) / 255.0f;
                    blue = Convert.ToInt32( colorString.Substring( 6, 2 ), 16 ) / 255.0f;
                    break;

                default:
                    throw new Exception( $"Unknown color format '{ s }'" );
            }

            return new UIColor( red, green, blue, alpha );
        }

        private static float ColorComponentFromHex( string hex )
        {
            return Convert.ToInt32( hex, 16 ) / 255.0f;
        }
    }
}
