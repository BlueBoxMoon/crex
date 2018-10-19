using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;
using Foundation;

namespace Crex.tvOS.Views
{
    public class TopAlignedLabel : UILabel
    {
        public TopAlignedLabel()
            : base() { }

        public TopAlignedLabel(CGRect frame)
            :base(frame) { }

        public override void DrawText( CGRect rect )
        {
            var attributes = new Dictionary<NSString, NSObject>
            {
                { new NSString(""), null }
            };
            var attributes2 = new NSDictionary<NSString, NSObject>( new NSString( UIStringAttributeKey.Font ), Font );
            var attributedText = new NSAttributedString( Text ?? new NSString( "" ), attributes2 );

            CGSize size = rect.Size;
            size.Height = attributedText.GetBoundingRect( size, NSStringDrawingOptions.UsesLineFragmentOrigin, null ).Size.Height;

            if (Lines != 0)
            {
                size.Height = ( nfloat ) Math.Min( size.Height, Lines * Font.LineHeight );
            }

            rect.Size = size;

            base.DrawText( rect );
        }
    }
}
