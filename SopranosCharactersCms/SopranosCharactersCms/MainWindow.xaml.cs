using SopranosCharactersCms.Dialogs;
using SopranosCharactersCms.Models;
using SopranosCharactersCms.Pages;
using SopranosCharactersCms.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace SopranosCharactersCms
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataService = new AppDataService();
            DataService.EnsureAppData();
            Users = DataService.LoadUsers();
            Characters = DataService.LoadCharacters();

            NavigateToLogin();
        }

        public AppDataService DataService { get; }

        public ObservableCollection<User> Users { get; }

        public ObservableCollection<CharacterContent> Characters { get; }

        public User CurrentUser { get; private set; }

        public void NavigateToLogin()
        {
            CurrentUser = null;
            MainFrame.Navigate(new LoginFigmaPage(this));
        }

        public void NavigateToCharacters(User user)
        {
            CurrentUser = user;
            MainFrame.Navigate(new CharacterListPage(this, user));
        }

        public void NavigateToAddEdit(CharacterContent character)
        {
            if (CurrentUser == null)
            {
                NavigateToLogin();
                return;
            }

            MainFrame.Navigate(new AddEditCharacterPage(this, CurrentUser, character));
        }

        public void NavigateToDetails(CharacterContent character)
        {
            if (CurrentUser == null)
            {
                NavigateToLogin();
                return;
            }

            MainFrame.Navigate(new CharacterDetailsPage(this, CurrentUser, character));
        }

        public void ShowVisitorDetailsWindow(CharacterContent character)
        {
            if (CurrentUser == null)
            {
                NavigateToLogin();
                return;
            }

            Window detailsWindow = new Window
            {
                Owner = this,
                Title = "Character Details",
                Width = 1920,
                Height = 1080,
                MinWidth = 1920,
                MinHeight = 1080,
                MaxWidth = 1920,
                MaxHeight = 1080,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0A0A0A"),
                Content = new Frame
                {
                    NavigationUIVisibility = NavigationUIVisibility.Hidden,
                    Content = new CharacterDetailsPage(this, CurrentUser, character, closeOnBack: true)
                }
            };

            detailsWindow.ShowDialog();
        }

        public void SaveCharacters()
        {
            DataService.SaveCharacters(Characters);
        }

        public void SaveUsers()
        {
            DataService.SaveUsers(Users);
        }

        public bool ShowDeleteConfirmation(string message)
        {
            ConfirmationDialogWindow dialog = new ConfirmationDialogWindow(this, "Delete Character", message, "Delete", isDanger: true, showCancel: true);
            return dialog.ShowDialog() == true;
        }

        public bool ShowExitConfirmation()
        {
            ConfirmationDialogWindow dialog = new ConfirmationDialogWindow(this, "Exit Application", "Are you sure you want to close the application?", "Exit", isDanger: false, showCancel: true);
            return dialog.ShowDialog() == true;
        }

        public void ShowSuccessDialog(string message)
        {
            ConfirmationDialogWindow dialog = new ConfirmationDialogWindow(this, "Success", message, "OK", isDanger: false, showCancel: false);
            dialog.ShowDialog();
        }

        public void ShowInfoDialog(string message)
        {
            ConfirmationDialogWindow dialog = new ConfirmationDialogWindow(this, "Delete Character", message, "OK", isDanger: false, showCancel: false);
            dialog.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!ShowExitConfirmation())
            {
                e.Cancel = true;
                return;
            }

            SaveUsers();
            SaveCharacters();
        }
    }
}
