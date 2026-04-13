using SopranosCharactersCms.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SopranosCharactersCms.Pages
{
    public partial class LoginPage : Page
    {
        private readonly MainWindow _mainWindow;

        public LoginPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            ClearValidationMessages();

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            bool isValid = true;
            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameErrorTextBlock.Text = "Username is required.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordErrorTextBlock.Text = "Password is required.";
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            User user = _mainWindow.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                PasswordErrorTextBlock.Text = "Invalid credentials. Try admin/admin or user/user.";
                return;
            }

            _mainWindow.NavigateToCharacters(user);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ClearValidationMessages()
        {
            UsernameErrorTextBlock.Text = string.Empty;
            PasswordErrorTextBlock.Text = string.Empty;
        }
    }
}
