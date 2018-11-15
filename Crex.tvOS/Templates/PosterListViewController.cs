using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

using Crex.Extensions;
using Crex.tvOS.Extensions;

namespace Crex.tvOS.Templates
{
    public class PosterListViewController : CrexBaseViewController, IUITableViewDataSource, IUITableViewDelegate
    {
        #region Views

        /// <summary>
        /// Gets the background image view.
        /// </summary>
        /// <value>The background image view.</value>
        protected UIImageView BackgroundImageView { get; private set; }

        /// <summary>
        /// Gets the title view.
        /// </summary>
        /// <value>The title view.</value>
        protected UILabel TitleView { get; private set; }

        /// <summary>
        /// Gets the poster image view.
        /// </summary>
        /// <value>The poster image view.</value>
        protected UIImageView PosterImageView { get; private set; }

        /// <summary>
        /// Gets the detail left view.
        /// </summary>
        /// <value>The detail left view.</value>
        protected UILabel DetailLeftView { get; private set; }

        /// <summary>
        /// Gets the detail right view.
        /// </summary>
        /// <value>The detail right view.</value>
        protected UILabel DetailRightView { get; private set; }

        /// <summary>
        /// Gets the description view.
        /// </summary>
        /// <value>The description view.</value>
        protected Views.TopAlignedLabel DescriptionView { get; private set; }

        /// <summary>
        /// Gets the list view.
        /// </summary>
        /// <value>The list view.</value>
        protected UITableView ListView { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the poster data.
        /// </summary>
        /// <value>The poster data.</value>
        protected Rest.PosterList PosterData { get; private set; }

        /// <summary>
        /// Gets the last loaded date.
        /// </summary>
        /// <value>The last loaded date.</value>
        protected DateTime LastLoadedDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the background image.
        /// </summary>
        /// <value>The background image.</value>
        protected UIImage BackgroundImage { get; private set; }

        /// <summary>
        /// Gets the list view images.
        /// </summary>
        /// <value>The list view images.</value>
        protected UIImage[] ListViewImages { get; private set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// The view has loaded, now we can create any child views.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //
            // Initialize the background image view.
            //
            BackgroundImageView = new UIImageView( new CGRect( 0, 0, 1920, 1080 ) )
            {
                Alpha = 0.25f,
                ContentMode = UIViewContentMode.ScaleAspectFit
            };
            View.AddSubview( BackgroundImageView );

            //
            // Initialize the title view.
            //
            TitleView = new UILabel( new CGRect( 80, 60, 1760, 80 ) )
            {
                TextAlignment = UITextAlignment.Center,
                TextColor = "#ffd0d0d0".AsUIColor(),
                Font = UIFont.BoldSystemFontOfSize( 56 )
            };
            View.AddSubview( TitleView );

            //
            // Initialize the poster image view.
            //
            PosterImageView = new UIImageView( new CGRect( 80, 200, 800, 450 ) )
            {
                ContentMode = UIViewContentMode.ScaleAspectFit
            };
            View.AddSubview( PosterImageView );

            //
            // Initialize the left detail view.
            //
            DetailLeftView = new UILabel( new CGRect( 80, 650, 400, 40 ) )
            {
                Font = UIFont.SystemFontOfSize( 28 ),
                TextColor = "#ffd0d0d0".AsUIColor()
           };
            View.AddSubview( DetailLeftView );

            //
            // Initialize the right detail view.
            //
            DetailRightView = new UILabel( new CGRect( 480, 650, 400, 40 ) )
            {
                Font = UIFont.SystemFontOfSize( 28 ),
                TextColor = "#ffd0d0d0".AsUIColor(),
                TextAlignment = UITextAlignment.Right
            };
            View.AddSubview( DetailRightView );

            //
            // Initialize the description view.
            //
            DescriptionView = new Views.TopAlignedLabel( new CGRect( 80, 710, 800, 280 ) )
            {
                Lines = 8,
                LineBreakMode = UILineBreakMode.TailTruncation,
                Font = UIFont.SystemFontOfSize( 28 ),
                TextColor = "#ffd0d0d0".AsUIColor()
            };
            View.AddSubview( DescriptionView );

            //
            // Initialize the list view.
            //
            ListView = new UITableView( new CGRect( 1040, 200, 800, 680 ), UITableViewStyle.Plain )
            {
                DataSource = this,
                Delegate = this,
                RemembersLastFocusedIndexPath = true
            };
            View.AddSubview( ListView );
        }

        /// <summary>
        /// The view is about to appear. Start loading the content.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            if ( DateTime.Now.Subtract( LastLoadedDate ).TotalSeconds > Crex.Application.Current.Config.ContentCacheTime.Value )
            {
                Task.Run( async () =>
                {
                    try
                    {
                        await LoadContentAsync();
                    }
                    catch
                    {
                        LastLoadedDate = DateTime.Now;
                    }
                } );
            }
        }

        /// <summary>
        /// Loads the content asynchronously.
        /// </summary>
        public override async Task LoadContentAsync()
        {
            var data = Data.FromJson<Rest.PosterList>();

            //
            // If the menu content hasn't actually changed, then ignore.
            //
            if ( data.ToJson().ComputeHash() == PosterData.ToJson().ComputeHash() )
            {
                return;
            }

            PosterData = data;

            //
            // Load the background image.
            //
            BackgroundImage = await Utility.LoadImageFromUrlAsync( PosterData.BackgroundImage.BestMatch );
            BackgroundImage = Utility.ScaleImageToWidth( BackgroundImage, ( int ) ( Crex.Application.Current.Resolution.Width / 2.0f ) );
            BackgroundImage = Utility.CreateBlurredImage( BackgroundImage, 8 );

            ListViewImages = new UIImage[PosterData.Items.Count];
            LastLoadedDate = DateTime.Now;

            InvokeOnMainThread( () =>
            {
                EnsureView();
                TitleView.Text = PosterData.Title;
                DetailLeftView.Text = string.Empty;
                DetailRightView.Text = string.Empty;
                DescriptionView.Text = string.Empty;
                BackgroundImageView.Image = BackgroundImage;

                ListView.ReloadData();

                SetNeedsFocusUpdate();
            } );
        }

        #endregion

        #region Methods

        #endregion

        #region UITableView Methods

        /// <summary>
        /// User has selected (clicked) the specified row in the table.
        /// </summary>
        /// <param name="tableView">Table view.</param>
        /// <param name="indexPath">Index path.</param>
        [Export( "tableView:didSelectRowAtIndexPath:" )]
        public void RowSelected( UITableView tableView, NSIndexPath indexPath )
        {
            tableView.SelectRow( NSIndexPath.FromRowSection( 0, -1 ), false, UITableViewScrollPosition.None );

            if ( PosterData.Items[indexPath.Row].Action != null )
            {
                Crex.Application.Current.StartAction( this, PosterData.Items[indexPath.Row].Action );
            }
            else
            {
                Crex.Application.Current.StartAction( this, PosterData.Items[indexPath.Row].ActionUrl );
            }
        }

        /// <summary>
        /// The focus has updated for the table view. We need to make sure the
        /// on screen UI reflects the new selection.
        /// </summary>
        /// <param name="tableView">Table view.</param>
        /// <param name="focusUpdateContext">Focus update context.</param>
        /// <param name="animationCoordinator">Animation coordinator.</param>
        [Export( "tableView:didUpdateFocusInContext:withAnimationCoordinator:" )]
        void DidUpdateFocus( UITableView tableView, UITableViewFocusUpdateContext focusUpdateContext, UIFocusAnimationCoordinator animationCoordinator )
        {
            NSIndexPath indexPath = focusUpdateContext.NextFocusedIndexPath;

            if ( indexPath == null || PosterData?.Items?.Count == 0 )
            {
                return;
            }

            if ( ListViewImages == null || ListViewImages[indexPath.Row] == null )
            {
                //
                // Create and set a temporary 1 pixel by 1 pixel transparent bitmap image.
                //
                PosterImageView.Hidden = true;

                //
                // Start a background task to load the image.
                //
                Task.Run( async () =>
                {
                    //
                    // Load the image.
                    //
                    var client = new System.Net.Http.HttpClient();
                    var image = await Utility.LoadImageFromUrlAsync( PosterData.Items[indexPath.Row].Image.BestMatch );

                    //
                    // Store the image in our cache.
                    //
                    ListViewImages[indexPath.Row] = image;

                    //
                    // Update the UI.
                    //
                    InvokeOnMainThread( () =>
                    {
                        NSIndexPath highlightedIndexPath = NSIndexPath.FromRowSection( -1, 0 );

                        for ( int i = 0; i < PosterData.Items.Count; i++ )
                        {
                            var cell = ListView.CellAt( NSIndexPath.FromRowSection( i, 0 ) );
                            if ( cell.Focused )
                            {
                                highlightedIndexPath = NSIndexPath.FromRowSection( i, 0 );
                                break;
                            }
                        }

                        if ( highlightedIndexPath.Row == indexPath.Row )
                        {
                            if ( PosterImageView.Hidden == true )
                            {
                                PosterImageView.Alpha = 0;
                                PosterImageView.Hidden = false;
                                UIView.Animate( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
                                {
                                    PosterImageView.Alpha = 1;
                                } );
                            }

                            PosterImageView.Image = image;
                        }
                    } );
                } );
            }
            else
            {
                if ( PosterImageView.Hidden == true )
                {
                    PosterImageView.Alpha = 0;
                    PosterImageView.Hidden = false;
                    UIView.Animate( Crex.Application.Current.Config.AnimationTime.Value / 1000.0f, () =>
                    {
                        PosterImageView.Alpha = 1;
                    } );
                }

                PosterImageView.Image = ListViewImages[indexPath.Row];
            }

            //
            // Update the text content about the item.
            //
            DetailLeftView.Text = PosterData.Items[indexPath.Row].DetailLeft;
            DetailRightView.Text = PosterData.Items[indexPath.Row].DetailRight;
            DescriptionView.Text = PosterData.Items[indexPath.Row].Description;
        }

        /// <summary>
        /// Retrieves the cell to use to display the given item.
        /// </summary>
        /// <returns>The cell.</returns>
        /// <param name="tableView">Table view.</param>
        /// <param name="indexPath">Index path of the item to be displayed.</param>
        public UITableViewCell GetCell( UITableView tableView, NSIndexPath indexPath )
        {
            UITableViewCell cell = tableView.DequeueReusableCell( "MainCell" );

            //
            // If we couldn't find an available cell to reuse, create one.
            //
            if ( cell == null )
            {
                cell = new Views.PosterViewCell( UITableViewCellStyle.Default, "MainCell" );
                cell.TextLabel.Font = UIFont.SystemFontOfSize( 32 );
                cell.TextLabel.TextColor = UIColor.White;
            }

            cell.TextLabel.Text = PosterData.Items[indexPath.Row].Title;

            return cell;
        }

        /// <summary>
        /// Retrieves the number of rows to be displayed in the table.
        /// </summary>
        /// <returns>The in section.</returns>
        /// <param name="tableView">Table view.</param>
        /// <param name="section">Section.</param>
        public nint RowsInSection( UITableView tableView, nint section )
        {
            return PosterData?.Items?.Count ?? 0;
        }


        #endregion
    }
}