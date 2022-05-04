using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows;
using System.Windows.Media;

namespace REST.Template.Dialogs
{
	/// <summary>
	/// Interaction logic for COFRSNewProjectDialog.xaml
	/// </summary>
	public partial class NewProjectDialog : DialogWindow, IDisposable
    {
        private bool disposedValue;

        public string Framework { get; set; }
		public string SecurityModel { get; set; }
		public string DatabaseTechnology { get; set; }
		public string TeamName { get; set; }
		public string TeamEmail { get; set; }
		public string TeamUrl { get; set; }
		public string VendorTag { get; set; }

		public NewProjectDialog()
        {
            InitializeComponent();

			// Subscribe to theme changes events so we can refresh the colors
			VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			//	This may seem rundant, given that we have the ComboboxItems in xaml, but
			//	if you don't include these lines, the dialog will throw an exception
			//	as soon as you try to change a value in a combobox

			frameworkCombobox.Items.Clear();
			frameworkCombobox.Items.Add(".NET 6.0");
			frameworkCombobox.SelectedIndex = 0;

			DatabaseTechnologyCombobox.Items.Clear();
			DatabaseTechnologyCombobox.Items.Add("None");
			DatabaseTechnologyCombobox.Items.Add("SQL Server");
			DatabaseTechnologyCombobox.Items.Add("Postgresql");
			DatabaseTechnologyCombobox.Items.Add("MySQL");
			DatabaseTechnologyCombobox.SelectedIndex = 1;

			TeamNameTextBox.Text = TeamName;
			TeamEmailTextBox.Text = TeamEmail;
			TeamUrlTextBox.Text = TeamUrl;
			VendorTagTextBox.Text = VendorTag; 
		}

		private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
		{
			RefreshColors();
		}

		private void RefreshColors()
        {
			MainGrid.Background = new SolidColorBrush(ConvertColor(VSColorTheme.GetThemedColor(EnvironmentColors.BrandedUIBackgroundColorKey)));
		}

		private Color ConvertColor(System.Drawing.Color clr)
        {
			return Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }

		private void OnOK(object sender, RoutedEventArgs e)
        {
			if (string.IsNullOrWhiteSpace(TeamNameTextBox.Text))
			{
				if (MessageBox.Show("Are you sure you want to leave your name blank? If you include your name, your name will be displayed on the Swagger page, so user's of your service know how to contact you. Select \"Yes\" to leave the name blank.", "Visual Studio", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
					return;
			}

			if (string.IsNullOrWhiteSpace(TeamEmailTextBox.Text))
			{
				if (MessageBox.Show("Are you sure you want to leave your email address blank? If you include an email address, your eamil address will be displayed on the Swagger page, so other teams know how to contact you. Select \"Yes\" to leave the email blank.", "Visual Studio", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
					return;
			}

			if (string.IsNullOrWhiteSpace(TeamUrlTextBox.Text))
			{
				if (MessageBox.Show("Are you sure you want to leave your team Url blank? You should include the project website, if you have one, or a perhaps your github page dedicated to this service. If you do, your Url address will be displayed on the Swagger page, so other teams can navigate to it. Select \"Yes\" to leave the Url blank.", "Visual Studio", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
					return;
			}

			switch (frameworkCombobox.SelectedIndex)
			{
				case 0:
					Framework = "net6.0";
					break;

				default:
					Framework = "net6.0";
					break;
			}

			switch (DatabaseTechnologyCombobox.SelectedIndex)
			{
				case 0:
					DatabaseTechnology = "None";
					break;

				case 1:
					DatabaseTechnology = "SQLServer";
					break;

				case 2:
					DatabaseTechnology = "Postgresql";
					break;

				case 3:
					DatabaseTechnology = "MySQL";
					break;
			}

			TeamName = TeamNameTextBox.Text;
			TeamEmail = TeamEmailTextBox.Text;
			TeamUrl = TeamUrlTextBox.Text;
			VendorTag = VendorTagTextBox.Text;

			DialogResult = true;
			Close();
		}

		private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
					VSColorTheme.ThemeChanged -= this.VSColorTheme_ThemeChanged;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~COFRSNewProjectDialog()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
	}
}
