namespace Crex.Rest
{
    public class MenuButton
    {
        /// <summary>
        /// Gets or sets the title of the button.
        /// </summary>
        /// <value>
        /// The title of the button.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public CrexAction Action { get; set; }

        /// <summary>
        /// Gets the required crex version needed for this action to show up.
        /// </summary>
        /// <value>
        /// The required crex version needed for this action to show up.
        /// </value>
        public int? RequiredCrexVersion { get; set; }
    }
}
