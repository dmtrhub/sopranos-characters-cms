using SopranosCharactersCms.Dialogs;
using SopranosCharactersCms.Models;
using SopranosCharactersCms.Pages;
using SopranosCharactersCms.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Media.Effects;

namespace SopranosCharactersCms
{
    public partial class MainWindow : Window
    {
        private bool _isFullscreen;

        public MainWindow()
        {
            InitializeComponent();

            EnterFullscreen();
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            DataService = new AppDataService();
            DataService.EnsureAppData();
            Users = DataService.LoadUsers();
            Characters = DataService.LoadCharacters();

            NavigateToLogin();
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.F11)
            {
                return;
            }

            if (_isFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                EnterFullscreen();
            }

            e.Handled = true;
        }

        private void EnterFullscreen()
        {
            _isFullscreen = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void ExitFullscreen()
        {
            _isFullscreen = false;
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
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
                Width = ActualWidth,
                Height = ActualHeight,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowState = WindowState.Maximized,
                Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0A0A0A"),
                Content = new Frame
                {
                    NavigationUIVisibility = NavigationUIVisibility.Hidden,
                    Content = new CharacterDetailsPage(this, CurrentUser, character, closeOnBack: true)
                }
            };

            var previousEffect = Effect;
            Effect = new BlurEffect { Radius = 8 };
            try
            {
                detailsWindow.ShowDialog();
            }
            finally
            {
                Effect = previousEffect;
            }
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
            ConfirmationDialogWindow dialog = new ConfirmationDialogWindow(this, "Exit Application", "Are you sure you want to close the application?", "Exit", isDanger: false, showCancel: true, centerMessage: true);
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
