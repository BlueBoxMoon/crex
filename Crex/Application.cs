﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// Gets the current Crex application object.
        /// </summary>
        /// <value>
        /// The current Crex application object.
        /// </value>
        public static Application Current
        {
            get
            {
                if ( _Current == null )
                {
#if __ANDROID__
                    _Current = new Crex.Android.Application();
#elif __TVOS__
                    _Current = new Crex.tvOS.Application();
#endif
                }

                return _Current;
            }
        }
        private static Application _Current;

        #region Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public Config Config { get; private set; }

        /// <summary>
        /// Gets the resolution.
        /// </summary>
        /// <value>
        /// The resolution.
        /// </value>
        public Resolution Resolution { get; private set; }

        /// <summary>
        /// Gets or sets the preference manager.
        /// </summary>
        /// <value>
        /// The preference manager.
        /// </value>
        public PreferenceManager Preferences { get; protected set; }

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
        protected Application( Stream configStream, Resolution resolution )
        {
            if ( _Current != null )
            {
                throw new Exception( "Application has already been initialized" );
            }

            Config = Config.Initialize( configStream );
            Resolution = resolution;
        }

        /// <summary>
        /// Gets the absolute URL for a given URL.
        /// </summary>
        /// <returns>The absolute URL.</returns>
        /// <param name="url">The url to be processed.</param>
        public string GetAbsoluteUrl( string url )
        {
            if ( url == null || url.Contains( "://" ) )
            {
                return url;
            }

            var uri = new Uri( url, UriKind.RelativeOrAbsolute );

            return new Uri( new Uri( Config.ApplicationRootUrl ), uri ).AbsoluteUri;
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
        /// <param name="url">The action url that should be loaded.</param>
        public abstract Task StartAction( object sender, string url );

        /// <summary>
        /// Shows the action specified.
        /// </summary>
        /// <param name="sender">The sender that is starting the action.</param>
        /// <param name="action">The action that should be loaded.</param>
        public abstract Task StartAction( object sender, Rest.CrexAction action );
    }
}
