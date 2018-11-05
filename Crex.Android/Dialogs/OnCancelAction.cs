using System;
using Android.Content;

namespace Crex.Android.Dialogs
{
    /// <summary>
    /// Simplifies allowing an activity to respond to the back button being pressed
    /// on a dialog.
    /// </summary>
    /// <seealso cref="Java.Lang.Object" />
    /// <seealso cref="Android.Content.IDialogInterfaceOnCancelListener" />
    public class OnCancelAction : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        /// <summary>
        /// Gets or sets the action to call on cancel.
        /// </summary>
        /// <value>
        /// The action to call on cancel.
        /// </value>
        public Action Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnCancelAction"/> class.
        /// </summary>
        public OnCancelAction()
        {
            Action = () => { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnCancelAction"/> class.
        /// </summary>
        /// <param name="action">The activity.</param>
        public OnCancelAction( Action action )
        {
            Action = action;
        }

        /// <summary>
        /// This method will be invoked when the dialog is canceled.
        /// </summary>
        /// <param name="dialog">The dialog that was canceled will be passed into the
        /// method.</param>
        public void OnCancel( IDialogInterface dialog )
        {
            Action();
        }
    }
}