using System;
using Android.Content;

namespace Crex.Android.Dialogs
{
    /// <summary>
    /// Simplifies allowing an activity to respond to the back button being pressed
    /// on a dialog.
    /// </summary>
    /// <seealso cref="Java.Lang.Object" />
    /// <seealso cref="Android.Content.IDialogInterfaceOnClickListener" />
    public class OnClickAction : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        /// <summary>
        /// Gets or sets the action to call on cancel.
        /// </summary>
        /// <value>
        /// The action to call on cancel.
        /// </value>
        public Action<int> Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnClickAction"/> class.
        /// </summary>
        public OnClickAction()
        {
            Action = ( button ) => { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogOnClickAction"/> class.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public OnClickAction( Action<int> action )
        {
            Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogOnClickAction"/> class.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public OnClickAction( Action action )
        {
            Action = ( button ) => { action(); };
        }

        /// <summary>
        /// This method will be invoked when a button in the dialog is clicked.
        /// </summary>
        /// <param name="dialog">The dialog that received the click.</param>
        /// <param name="which">The button that was clicked (e.g.
        /// <c><see cref="F:Android.Content.DialogInterface.Button1" /></c>) or the position
        /// of the item clicked.</param>
        /// <remarks>
        public void OnClick( IDialogInterface dialog, int which )
        {
            Action( which );
        }
    }
}