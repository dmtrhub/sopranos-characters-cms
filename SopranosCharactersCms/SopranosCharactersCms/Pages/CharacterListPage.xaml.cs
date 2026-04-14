using SopranosCharactersCms.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SopranosCharactersCms.Pages
{
    public partial class CharacterListPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly User _currentUser;

        public CharacterListPage(MainWindow mainWindow, User currentUser)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentUser = currentUser;
            DataContext = this;
        }

        public ObservableCollection<CharacterContent> Characters => _mainWindow.Characters;

        public bool IsAdmin => _currentUser.Role == UserRole.Admin;

        public bool IsSelectionVisible => _currentUser.Role == UserRole.Admin;

        public double HeaderTop => IsAdmin ? 240 : 160;

        public double HeaderTextTop => IsAdmin ? 254 : 174;

        public double HeaderIconTop => HeaderTextTop + 3;

        public double HeaderSelectTop => IsAdmin ? 252 : 172;

        public double RowsTop => IsAdmin ? 300 : 220;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToAddEdit(null);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCharacters = Characters.Where(c => c.IsSelected).ToList();
            if (!selectedCharacters.Any())
            {
                _mainWindow.ShowInfoDialog("Select at least one character for deletion.");
                return;
            }

            if (!_mainWindow.ShowDeleteConfirmation("Are you sure you want to delete selected characters? This action cannot be undone."))
            {
                return;
            }

            foreach (CharacterContent character in selectedCharacters)
            {
                _mainWindow.DataService.DeleteCharacterArtifacts(character);
                Characters.Remove(character);
            }

            _mainWindow.SaveCharacters();
            _mainWindow.ShowSuccessDialog("Selected characters were deleted successfully.");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToLogin();
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CharacterContent character in Characters)
            {
                character.IsSelected = true;
            }
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CharacterContent character in Characters)
            {
                character.IsSelected = false;
            }
        }

        private void CharacterNameTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            CharacterContent selectedCharacter = element.DataContext as CharacterContent;
            if (selectedCharacter == null)
            {
                return;
            }

            if (_currentUser.Role == UserRole.Admin)
            {
                _mainWindow.NavigateToAddEdit(selectedCharacter);
                return;
            }

            _mainWindow.ShowVisitorDetailsWindow(selectedCharacter);
        }

        private void ViewTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            CharacterContent selectedCharacter = element.DataContext as CharacterContent;
            if (selectedCharacter == null)
            {
                return;
            }

            if (_currentUser.Role == UserRole.Visitor)
            {
                _mainWindow.ShowVisitorDetailsWindow(selectedCharacter);
                return;
            }

            _mainWindow.NavigateToDetails(selectedCharacter);
        }
    }
}
