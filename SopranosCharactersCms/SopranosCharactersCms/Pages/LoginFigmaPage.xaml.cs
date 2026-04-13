using SopranosCharactersCms.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SopranosCharactersCms.Pages
{
    public partial class LoginFigmaPage : Page
    {
        private readonly MainWindow _mainWindow;
        private bool _isPasswordVisible;

        public LoginFigmaPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            UpdatePlaceholders();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            ClearValidationMessages();

            string username = UsernameTextBox.Text.Trim();
            string password = GetCurrentPassword().Trim();

            bool isValid = true;
            bool missingUsername = string.IsNullOrWhiteSpace(username);
            bool missingPassword = string.IsNullOrWhiteSpace(password);

            if (missingUsername && missingPassword)
            {
                ErrorMessageTextBlock.Text = "Username and password are required.";
                isValid = false;
            }
            else if (missingUsername)
            {
                ErrorMessageTextBlock.Text = "Username is required.";
                isValid = false;
            }
            else if (missingPassword)
            {
                ErrorMessageTextBlock.Text = "Password is required.";
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            User user = _mainWindow.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ErrorMessageTextBlock.Text = "Invalid credentials. Use admin/admin or user/user.";
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
            ErrorMessageTextBlock.Text = string.Empty;
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
            ClearValidationMessages();
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernamePlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                return;
            }

            UpdatePlaceholders();
            ClearValidationMessages();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                return;
            }

            UpdatePlaceholders();
            ClearValidationMessages();
        }

        private void PasswordVisibleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PasswordVisibleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void TogglePasswordVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordBox.Password = PasswordVisibleTextBox.Text;
                PasswordVisibleTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                EyeIconImage.Tag = "Images/view.png";
                _isPasswordVisible = false;
            }
            else
            {
                PasswordVisibleTextBox.Text = PasswordBox.Password;
                PasswordVisibleTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                EyeIconImage.Tag = "Images/hide.png";
                _isPasswordVisible = true;
            }

            UpdatePlaceholders();
        }

        private string GetCurrentPassword()
        {
            return _isPasswordVisible ? PasswordVisibleTextBox.Text : PasswordBox.Password;
        }

        private void UpdatePlaceholders()
        {
            UsernamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(UsernameTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            PasswordPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (_isPasswordVisible)
            {
                PasswordPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(PasswordVisibleTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
