using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if __IOS__
using UIKit;
#endif
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Util;
#endif

namespace Crex
{
    public abstract class Application
    {
        public static Application Current { get; private set; }

        #region Properties

        public Config Config { get; private set; }

        public Resolution Resolution { get; private set; }

        /// <summary>
        /// Gets the crex version.
        /// </summary>
        /// <value>
        /// The crex version.
        /// </value>
        public int CrexVersion => 1;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="configStream">The stream that contains the configuration JSON data.</param>
        /// <param name="resolution">The resolution of the screen.</param>
        /// <exception cref="Exception">Application has already been initialized</exception>
        public Application( Stream configStream, Resolution resolution )
        {
            if ( Current != null )
            {
                throw new Exception( "Application has already been initialized" );
            }

            Config = Config.Initialize( configStream );
            Resolution = resolution;

            Current = this;
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public abstract void Run( object sender );

        /// <summary>
        /// Shows the action specified.
        /// </summary>
        /// <param name="sender">The sender that is starting the action.</param>
        /// <param name="action">The action that should be loaded.</param>
        public abstract void StartAction( object sender, Rest.CrexAction action );
    }
}
