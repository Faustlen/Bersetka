using System.Windows;

namespace Bersetka.windows.main.addUser
{
    public partial class AddUserWindow : Window
    {
        public string UserName { get; private set; }

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                UserName = txtUserName.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Имя пользователя не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
