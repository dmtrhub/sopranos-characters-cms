using SopranosCharactersCms.Converters;
using SopranosCharactersCms.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SopranosCharactersCms.Pages
{
    public partial class CharacterDetailsPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly User _currentUser;
        private readonly CharacterContent _character;
        private readonly bool _closeOnBack;

        public CharacterDetailsPage(MainWindow mainWindow, User currentUser, CharacterContent character, bool closeOnBack = false)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentUser = currentUser;
            _character = character;
            _closeOnBack = closeOnBack;
            FillViewData();
        }

        private void FillViewData()
        {
            CharacterNameTextBlock.Text = _character.FullName;
            MetadataTextBlock.Text = string.Format(CultureInfo.InvariantCulture, "Character ID: {0} | Added: {1}", _character.Id, GetAddedDateOnly());
            RoleStatusTextBlock.Text = string.Format(CultureInfo.InvariantCulture, "{0} \\ New Jersey", string.IsNullOrWhiteSpace(_character.Role) ? "Active Member" : _character.Role);
            DescriptionTextBlock.Text = _mainWindow.DataService.LoadRtfAsPlainText(_character.RtfPath);
            CharacterImage.Source = new RelativeImagePathToBitmapConverter().Convert(_character.ImagePath, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource;
            Visibility editVisibility = _currentUser.Role == UserRole.Admin && !_closeOnBack ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = editVisibility;
            EditButtonBorder.Visibility = editVisibility;
        }

        private string GetAddedDateOnly()
        {
            if (DateTime.TryParse(_character.DateAddedUtc, null, DateTimeStyles.RoundtripKind, out DateTime parsedDate))
            {
                return parsedDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return _character.DateAddedDisplay;
        }

        private void FullProfileTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string absolutePath = _mainWindow.DataService.GetAbsolutePathFromRelative(_character.RtfPath);
            if (string.IsNullOrWhiteSpace(absolutePath) || !System.IO.File.Exists(absolutePath))
            {
                MessageBox.Show("RTF profile file was not found.", "Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = absolutePath,
                UseShellExecute = true
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_closeOnBack)
            {
                Window.GetWindow(this)?.Close();
                return;
            }

            _mainWindow.NavigateToCharacters(_currentUser);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToAddEdit(_character);
        }
    }
}
