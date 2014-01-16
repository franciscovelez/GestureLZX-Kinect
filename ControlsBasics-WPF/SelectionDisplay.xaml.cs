namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System.Globalization;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic
    /// </summary>
    public partial class SelectionDisplay : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionDisplay"/> class. 
        /// </summary>
        /// <param name="itemId">ID of the item that was selected</param>
        public SelectionDisplay(string itemId, string color)
        {
            this.InitializeComponent();

            //this.messageTextBlock.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SelectedMessage, itemId);
            this.messageTextBlock.Text = itemId;
            var converter = new System.Windows.Media.BrushConverter();
            System.Windows.Media.Brush Fill;
            Fill = (System.Windows.Media.Brush)converter.ConvertFromString(color);
            this.messageTextBlock.Foreground = Fill;
        }

        /// <summary>
        /// Called when the OnLoaded storyboard completes.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnLoadedStoryboardCompleted(object sender, System.EventArgs e)
        {
            var parent = (Panel)this.Parent;
            parent.Children.Remove(this);
        }
    }
}
