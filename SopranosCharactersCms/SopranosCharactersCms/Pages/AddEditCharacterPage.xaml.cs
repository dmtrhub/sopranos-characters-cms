using Microsoft.Win32;
using SopranosCharactersCms.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SopranosCharactersCms.Pages
{
    public partial class AddEditCharacterPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly User _currentUser;
        private readonly CharacterContent _editingCharacter;

        private string _selectedImageSourcePath;
        private string _loadedRtfSourcePath;
        private ObservableCollection<ColorOption> _colorOptions;

        public AddEditCharacterPage(MainWindow mainWindow, User currentUser, CharacterContent editingCharacter)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentUser = currentUser;
            _editingCharacter = editingCharacter;

            InitializeEditorTools();
            InitializeForMode();
            UpdateBasicFieldPlaceholders();
        }

        private bool IsEditMode => _editingCharacter != null;

        private void InitializeEditorTools()
        {
            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontFamilyComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == "Segoe UI");

            FontSizeComboBox.ItemsSource = new List<double> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32 };
            FontSizeComboBox.SelectedItem = 12d;

            _colorOptions = new ObservableCollection<ColorOption>(typeof(Colors)
                .GetProperties()
                .Select(propertyInfo => new ColorOption
                {
                    Name = propertyInfo.Name,
                    Brush = new SolidColorBrush((Color)propertyInfo.GetValue(null, null))
                })
                .OrderBy(option => option.Name));

            FontColorComboBox.ItemsSource = _colorOptions;
            FontColorComboBox.SelectedItem = _colorOptions.FirstOrDefault(c => c.Name == "White");

            DescriptionRichTextBox.Foreground = Brushes.White;
            NormalizeDescriptionDocumentLayout(DescriptionRichTextBox.Document);
        }

        private void InitializeForMode()
        {
            if (!IsEditMode)
            {
                RtfPathTextBox.Text = "Auto-generated on save";
                RtfStatusTextBlock.Text = "No file selected";
                ImagePreview.Source = new Converters.RelativeImagePathToBitmapConverter().Convert(_mainWindow.DataService.PlaceholderImageRelativePath, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource;
                UpdateBasicFieldPlaceholders();
                UpdateDescriptionPlaceholder();
                return;
            }

            PageTitleTextBlock.Text = "Edit Character";
            IdTextBox.Text = _editingCharacter.Id.ToString(CultureInfo.InvariantCulture);
            NameTextBox.Text = _editingCharacter.FullName;
            RoleTextBox.Text = _editingCharacter.Role;
            RtfPathTextBox.Text = _editingCharacter.RtfPath;
            RtfStatusTextBlock.Text = _editingCharacter.RtfPath;
            _selectedImageSourcePath = _mainWindow.DataService.GetAbsolutePathFromRelative(_editingCharacter.ImagePath);

            ImagePathTextBlock.Text = _editingCharacter.ImagePath;
            ImagePreview.Source = new Converters.RelativeImagePathToBitmapConverter().Convert(_editingCharacter.ImagePath, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource;
            DescriptionRichTextBox.Document = _mainWindow.DataService.LoadRtfAsFlowDocument(_editingCharacter.RtfPath);
            NormalizeDescriptionDocumentLayout(DescriptionRichTextBox.Document);
            UpdateWordCount();
            UpdateBasicFieldPlaceholders();
            UpdateDescriptionPlaceholder();
        }

        private void IdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IdPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(IdTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            IdErrorTextBlock.Text = string.Empty;
        }

        private void IdTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            IdPlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void IdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IdPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(IdTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(NameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            NameErrorTextBlock.Text = string.Empty;
        }

        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NamePlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            NamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(NameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RolePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(RoleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            RoleErrorTextBlock.Text = string.Empty;
        }

        private void RoleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            RolePlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void RoleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RolePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(RoleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateBasicFieldPlaceholders()
        {
            IdPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(IdTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            NamePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(NameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            RolePlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(RoleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            _selectedImageSourcePath = openFileDialog.FileName;
            ImagePathTextBlock.Text = openFileDialog.SafeFileName;
            ImagePreview.Source = new Converters.RelativeImagePathToBitmapConverter().Convert(_selectedImageSourcePath, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource;
            ImageErrorTextBlock.Text = string.Empty;
        }

        private void BrowseRtfButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "RTF Files|*.rtf"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            _loadedRtfSourcePath = openFileDialog.FileName;
            RtfPathTextBox.Text = openFileDialog.SafeFileName + " (will be copied to project path)";
            RtfStatusTextBlock.Text = openFileDialog.SafeFileName;

            FlowDocument loadedDocument = _mainWindow.DataService.LoadRtfAsFlowDocument(_loadedRtfSourcePath);
            DescriptionRichTextBox.Document = loadedDocument;
            NormalizeDescriptionDocumentLayout(DescriptionRichTextBox.Document);
            UpdateWordCount();
            UpdateDescriptionPlaceholder();
            RtfErrorTextBlock.Text = string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            int id = int.Parse(IdTextBox.Text.Trim(), CultureInfo.InvariantCulture);
            string fullName = NameTextBox.Text.Trim();
            string role = RoleTextBox.Text.Trim();
            string imagePath = IsEditMode && string.IsNullOrWhiteSpace(_selectedImageSourcePath)
                ? _editingCharacter.ImagePath
                : _mainWindow.DataService.SaveImageToProject(_selectedImageSourcePath, id, fullName);
            string rtfRelativePath = _mainWindow.DataService.BuildDefaultRtfRelativePath(id, fullName);

            if (!string.IsNullOrWhiteSpace(_loadedRtfSourcePath))
            {
                DescriptionRichTextBox.Document = _mainWindow.DataService.LoadRtfAsFlowDocument(_loadedRtfSourcePath);
            }

            _mainWindow.DataService.SaveFlowDocumentAsRtf(DescriptionRichTextBox.Document, rtfRelativePath);

            if (IsEditMode)
            {
                _editingCharacter.Id = id;
                _editingCharacter.FullName = fullName;
                _editingCharacter.Role = role;
                _editingCharacter.ImagePath = imagePath;
                _editingCharacter.RtfPath = rtfRelativePath;
            }
            else
            {
                _mainWindow.Characters.Add(new CharacterContent
                {
                    Id = id,
                    FullName = fullName,
                    Role = role,
                    ImagePath = imagePath,
                    RtfPath = rtfRelativePath,
                    DateAddedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                });
            }

            _mainWindow.SaveCharacters();
            _mainWindow.ShowSuccessDialog(IsEditMode ? "Character updated successfully." : "Character added successfully.");

            _mainWindow.NavigateToCharacters(_currentUser);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToCharacters(_currentUser);
        }

        private bool ValidateForm()
        {
            ClearValidationMessages();
            bool isValid = true;

            if (!int.TryParse(IdTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedId) || parsedId <= 0)
            {
                IdErrorTextBlock.Text = "Character ID must be a positive number.";
                isValid = false;
            }
            else
            {
                bool exists = _mainWindow.Characters.Any(c => c.Id == parsedId && (!IsEditMode || c != _editingCharacter));
                if (exists)
                {
                    IdErrorTextBlock.Text = "Character ID already exists.";
                    isValid = false;
                }
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameErrorTextBlock.Text = "Full name is required.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(RoleTextBox.Text))
            {
                RoleErrorTextBlock.Text = "Role is required.";
                isValid = false;
            }

            bool hasImage = !string.IsNullOrWhiteSpace(_selectedImageSourcePath) || (IsEditMode && !string.IsNullOrWhiteSpace(_editingCharacter.ImagePath));
            if (!hasImage)
            {
                ImageErrorTextBlock.Text = "Image selection is required.";
                isValid = false;
            }

            if (CountWords() == 0)
            {
                DescriptionErrorTextBlock.Text = "Description editor cannot be empty.";
                isValid = false;
            }

            if (!isValid)
            {
                return false;
            }

            string generatedRtfPath = _mainWindow.DataService.BuildDefaultRtfRelativePath(parsedId, NameTextBox.Text.Trim());
            RtfPathTextBox.Text = generatedRtfPath;
            RtfStatusTextBlock.Text = generatedRtfPath;
            return true;
        }

        private void ClearValidationMessages()
        {
            IdErrorTextBlock.Text = string.Empty;
            NameErrorTextBlock.Text = string.Empty;
            RoleErrorTextBlock.Text = string.Empty;
            ImageErrorTextBlock.Text = string.Empty;
            RtfErrorTextBlock.Text = string.Empty;
            DescriptionErrorTextBlock.Text = string.Empty;
        }

        private void DescriptionRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWordCount();
            UpdateDescriptionPlaceholder();
        }

        private void DescriptionRichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DescriptionPlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void DescriptionRichTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateDescriptionPlaceholder();
        }

        private void UpdateWordCount()
        {
            WordCountTextBlock.Text = string.Format(CultureInfo.InvariantCulture, "Words: {0}", CountWords());
        }

        private void UpdateDescriptionPlaceholder()
        {
            DescriptionPlaceholderTextBlock.Visibility = (CountWords() == 0 && !DescriptionRichTextBox.IsKeyboardFocusWithin)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private static void NormalizeDescriptionDocumentLayout(FlowDocument document)
        {
            if (document == null)
            {
                return;
            }

            document.PagePadding = new Thickness(0);
            document.TextAlignment = TextAlignment.Left;

            foreach (Block block in document.Blocks)
            {
                Paragraph paragraph = block as Paragraph;
                if (paragraph != null)
                {
                    paragraph.Margin = new Thickness(0);
                }
            }
        }

        private int CountWords()
        {
            string text = new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd).Text;
            return Regex.Matches(text, @"\b\w+\b", RegexOptions.CultureInvariant).Count;
        }

        private void DescriptionRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object fontWeight = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            BoldToggleButton.IsChecked = fontWeight != DependencyProperty.UnsetValue && fontWeight.Equals(FontWeights.Bold);

            object fontStyle = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            ItalicToggleButton.IsChecked = fontStyle != DependencyProperty.UnsetValue && fontStyle.Equals(FontStyles.Italic);

            object textDecorations = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineToggleButton.IsChecked = textDecorations != DependencyProperty.UnsetValue && textDecorations.Equals(TextDecorations.Underline);

            object fontFamily = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            if (fontFamily != DependencyProperty.UnsetValue)
            {
                FontFamilyComboBox.SelectedItem = fontFamily;
            }

            object fontSize = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
            if (fontSize != DependencyProperty.UnsetValue)
            {
                double parsedFontSize = (double)fontSize;
                FontSizeComboBox.SelectedItem = FontSizeComboBox.Items.Cast<double>().OrderBy(value => Math.Abs(value - parsedFontSize)).FirstOrDefault();
            }
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem == null)
            {
                return;
            }

            DescriptionRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
            DescriptionRichTextBox.Focus();
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(FontSizeComboBox.SelectedItem is double selectedSize))
            {
                return;
            }

            DescriptionRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, selectedSize);
            DescriptionRichTextBox.Focus();
        }

        private void FontColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(FontColorComboBox.SelectedItem is ColorOption selectedColorOption))
            {
                return;
            }

            DescriptionRichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, selectedColorOption.Brush);
            DescriptionRichTextBox.Focus();
        }

    }
}
