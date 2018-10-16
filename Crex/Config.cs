using System.ComponentModel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Crex
{
    public class Config
    {
        #region Properties

        /// <summary>
        /// Gets or sets the animation time. This is the time in milliseconds used by standard
        /// animations, such as fading an image ine.
        /// </summary>
        /// <value>
        /// The animation time.
        /// </value>
        [DefaultValue(250)]
        public int? AnimationTime { get; set; }

        /// <summary>
        /// Gets or sets the application root template.
        /// </summary>
        /// <value>
        /// The application root template.
        /// </value>
        [DefaultValue( "Menu" )]
        public string ApplicationRootTemplate { get; set; }

        /// <summary>
        /// Gets or sets the application root URL.
        /// </summary>
        /// <value>
        /// The application root URL.
        /// </value>
        public string ApplicationRootUrl { get; set; }

        /// <summary>
        /// Gets or sets the content cache time in seconds. This is how long, for example,
        /// the menu content is cached before attempting a reload.
        /// </summary>
        /// <value>
        /// The content cache time in seconds.
        /// </value>
        [DefaultValue( 600 )]
        public int? ContentCacheTime { get; set; }

        /// <summary>
        /// Gets or sets the image to use for the loading spinner.
        /// </summary>
        /// <value>
        /// The image to use for the loading spinner.
        /// </value>
#if __ANDROID__
        [DefaultValue("resource:Crex.Android:Crex.Android.Resources.crex-default-spinner.png")]
#endif
        public string LoadingSpinner { get; set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to delay before the spinner
        /// shows up. On fast connections this means the spinner may not show
        /// up at all.
        /// </summary>
        /// <value>
        /// The time in milliseconds to delay before the spinner shows up.
        /// </value>
        [DefaultValue(500)]
        public int? LoadingSpinnerDelay { get; set; }

        /// <summary>
        /// Gets or sets the menu bar settings.
        /// </summary>
        /// <value>
        /// The menu bar settings.
        /// </value>
        public MenuBarConfig MenuBar { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the configuration options from the JSON stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        static public Config Initialize( Stream stream )
        {
            Config config;

            try
            {
                using ( var reader = new StreamReader( stream ) )
                {
                    string json = reader.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>( json );
                }
            }
            catch
            {
                config = null;
            }

            //
            // Make sure we have a valid configuration object.
            //
            if ( config == null )
            {
                config = new Config();
            }

            //
            // Make sure we have a valid menu bar object.
            //
            if ( config.MenuBar == null )
            {
                config.MenuBar = new MenuBarConfig();
            }

            //
            // Set some sane defaults.
            //
            InitializeDefaultValues( config );
            InitializeDefaultValues( config.MenuBar );

            return config;
        }

        /// <summary>
        /// Initializes the default values of the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void InitializeDefaultValues( object obj )
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach ( PropertyInfo prop in props )
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if ( d != null && prop.GetValue( obj ) == null )
                {
                    prop.SetValue( obj, d.Value );
                }
            }
        }

        #endregion
    }

    public class Resolution
    {
        public int Width { get; }

        public int Height { get; }

        public Resolution( int width, int height )
        {
            Width = width;
            Height = height;
        }
    }

    public class MenuBarConfig
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        [DefaultValue( "#b2121212" )]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the focused text.
        /// </summary>
        /// <value>
        /// The color of the focused text.
        /// </value>
        [DefaultValue( "#dddddd" )]
        public string FocusedTextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the unfocused text.
        /// </summary>
        /// <value>
        /// The color of the unfocused text.
        /// </value>
        [DefaultValue( "#808080" )]
        public string UnfocusedTextColor { get; set; }
    }
}