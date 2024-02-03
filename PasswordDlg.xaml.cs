using System.Windows;

namespace WpfAppProjekty
{
    /// <summary>
    /// Interaction logic for PasswordDlg.xaml
    /// </summary>
    public partial class PasswordDlg : Window
    {
        public string Password { get; private set; }

        public PasswordDlg()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Password = passwordBox.Password;
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
