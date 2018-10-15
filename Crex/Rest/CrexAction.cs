using Crex.Extensions;

namespace Crex.Rest
{
    public class CrexAction
    {
        /// <summary>
        /// Gets the template name for the action.
        /// </summary>
        /// <value>
        /// The template name for the action.
        /// </value>
        public string Template { get; set; }

        /// <summary>
        /// Gets the object that will provide the needed data for the template.
        /// </summary>
        /// <value>
        /// The object that will provide the needed data for the template.
        /// </value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the required crex version.
        /// </summary>
        /// <value>
        /// The required crex version.
        /// </value>
        public int? RequiredCrexVersion { get; set; }
    }
}
