using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace WizardInstaller.Template.Dialogs
{
    /// <summary>
    /// Interaction logic for GetClassNameDialog.xaml
    /// </summary>
    public partial class GetClassNameDialog : DialogWindow
    {
        public string ClassName { get; set; }
        public GetClassNameDialog()
        {
            InitializeComponent();
        }

        public GetClassNameDialog(string title, string hint)
        {
            InitializeComponent();
            Label_Generator.Content = title;
            Textbox_ClassName.Text = hint;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ClassName = Textbox_ClassName.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
