using System;
using System.Collections.Generic;
using System.Linq;

namespace Crex.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Get the next notification from a list of notifications.
        /// </summary>
        /// <param name="notifications">The notifications.</param>
        /// <returns>The next notification or null if there is none remaining.</returns>
        public static Rest.Notification GetNextNotification( this List<Rest.Notification> notifications )
        {
            if ( notifications == null )
            {
                return null;
            }

            //
            // Get the current time and the last notification date we saw.
            //
            var now = DateTime.Now;
            var lastSeenNotification = Application.Current.Preferences.GetDateTimeValue( "Crex.LastSeenNotification", DateTime.MinValue );

            //
            // Find the next notification.
            //
            return notifications
                .Where( n => n.StartDateTime.ToLocalTime() > lastSeenNotification && n.StartDateTime.ToLocalTime() <= now )
                .OrderBy( n => n.StartDateTime.ToLocalTime() )
                .FirstOrDefault();
        }
    }
}
