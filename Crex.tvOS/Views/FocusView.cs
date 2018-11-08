using UIKit;

namespace Crex.tvOS.Views
{
    /// <summary>
    /// Simple view that allows it to receive focus directly.
    /// </summary>
    public class FocusView : UIView
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Crex.tvOS.Views.FocusView"/> has focus enabled.
        /// </summary>
        /// <value><c>true</c> if focus enabled; otherwise, <c>false</c>.</value>
        public bool FocusEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Crex.tvOS.Views.FocusView"/> can become focused.
        /// </summary>
        /// <value><c>true</c> if can become focused; otherwise, <c>false</c>.</value>
        public override bool CanBecomeFocused => FocusEnabled;
    }
}
