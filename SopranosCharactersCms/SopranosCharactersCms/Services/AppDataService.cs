using SopranosCharactersCms.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SopranosCharactersCms.Services
{
    public class AppDataService
    {
        private readonly XmlDataService _xmlDataService = new XmlDataService();

        public string DataDirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        public string UsersFilePath => Path.Combine(DataDirectoryPath, "users.xml");

        public string CharactersFilePath => Path.Combine(DataDirectoryPath, "characters.xml");

        public string RtfDirectoryPath => Path.Combine(DataDirectoryPath, "Rtf");

        public string ImagesDirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images");

        public string PlaceholderImageRelativePath => NormalizeRelativePath(Path.Combine("Resources", "Images", "placeholder.png"));

        public void EnsureAppData()
        {
            Directory.CreateDirectory(DataDirectoryPath);
            Directory.CreateDirectory(RtfDirectoryPath);
            Directory.CreateDirectory(ImagesDirectoryPath);

            EnsurePlaceholderImage();
            EnsureUsersSeed();
            EnsureCharactersSeed();
        }

        public ObservableCollection<User> LoadUsers()
        {
            return _xmlDataService.Deserialize<ObservableCollection<User>>(UsersFilePath) ?? new ObservableCollection<User>();
        }

        public void SaveUsers(ObservableCollection<User> users)
        {
            _xmlDataService.Serialize(users, UsersFilePath);
        }

        public ObservableCollection<CharacterContent> LoadCharacters()
        {
            return _xmlDataService.Deserialize<ObservableCollection<CharacterContent>>(CharactersFilePath) ?? new ObservableCollection<CharacterContent>();
        }

        public void SaveCharacters(ObservableCollection<CharacterContent> characters)
        {
            _xmlDataService.Serialize(characters, CharactersFilePath);
        }

        public string SaveImageToProject(string sourcePath, int characterId, string fullName)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return PlaceholderImageRelativePath;
            }

            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", characterId, BuildSafeFileName(fullName), extension);
            string destinationPath = Path.Combine(ImagesDirectoryPath, fileName);

            string sourceFullPath = Path.GetFullPath(sourcePath);
            string destinationFullPath = Path.GetFullPath(destinationPath);

            if (!string.Equals(sourceFullPath, destinationFullPath, StringComparison.OrdinalIgnoreCase))
            {
                string tempDestinationPath = destinationFullPath + ".tmp";

                using (FileStream sourceStream = new FileStream(sourceFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream targetStream = new FileStream(tempDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    sourceStream.CopyTo(targetStream);
                }

                if (File.Exists(destinationFullPath))
                {
                    File.Delete(destinationFullPath);
                }

                File.Move(tempDestinationPath, destinationFullPath);
            }

            return NormalizeRelativePath(Path.Combine("Resources", "Images", fileName));
        }

        public string BuildDefaultRtfRelativePath(int characterId, string fullName)
        {
            string fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}.rtf", characterId, BuildSafeFileName(fullName));
            return NormalizeRelativePath(Path.Combine("Data", "Rtf", fileName));
        }

        public void SaveFlowDocumentAsRtf(FlowDocument document, string relativePath)
        {
            string absolutePath = GetAbsolutePathFromRelative(relativePath);
            string directoryPath = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
            using (FileStream stream = new FileStream(absolutePath, FileMode.Create))
            {
                range.Save(stream, DataFormats.Rtf);
            }
        }

        public FlowDocument LoadRtfAsFlowDocument(string relativePath)
        {
            FlowDocument document = new FlowDocument();
            string absolutePath = GetAbsolutePathFromRelative(relativePath);
            if (!File.Exists(absolutePath))
            {
                return document;
            }

            TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
            using (FileStream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
            {
                range.Load(stream, DataFormats.Rtf);
            }

            return document;
        }

        public string LoadRtfAsPlainText(string relativePath)
        {
            FlowDocument document = LoadRtfAsFlowDocument(relativePath);
            TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
            return range.Text.Trim();
        }

        public void DeleteCharacterArtifacts(CharacterContent character)
        {
            if (character == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(character.RtfPath))
            {
                string rtfAbsolutePath = GetAbsolutePathFromRelative(character.RtfPath);
                if (File.Exists(rtfAbsolutePath))
                {
                    File.Delete(rtfAbsolutePath);
                }
            }
        }

        public string GetAbsolutePathFromRelative(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            return Path.IsPathRooted(relativePath)
                ? relativePath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        public string NormalizeRelativePath(string relativePath)
        {
            return relativePath?.Replace('\\', '/');
        }

        private void EnsureUsersSeed()
        {
            ObservableCollection<User> users = LoadUsers();
            if (users.Count > 0)
            {
                return;
            }

            users.Add(new User
            {
                Username = "admin",
                Password = "admin",
                Role = UserRole.Admin
            });

            users.Add(new User
            {
                Username = "user",
                Password = "user",
                Role = UserRole.Visitor
            });

            SaveUsers(users);
        }

        private void EnsureCharactersSeed()
        {
            ObservableCollection<CharacterContent> characters = LoadCharacters();
            if (characters.Count > 0)
            {
                return;
            }

            List<CharacterContent> seededCharacters = new List<CharacterContent>
            {
                CreateSeedCharacter(1, "Tony Soprano", "Boss", "Main character of the series. Tony balances crime leadership and family life while fighting panic attacks and navigating pressure."),
                CreateSeedCharacter(2, "Christopher Moltisanti", "Soldier", "Ambitious but unstable. Christopher wants a bigger mafia role while dreaming of a film career, and struggles with addiction."),
                CreateSeedCharacter(3, "Silvio Dante", "Consigliere", "Tony's loyal advisor. Calm, tactical, and reliable, Silvio manages conflict and operations with discipline."),
                CreateSeedCharacter(4, "Paulie \"Walnuts\" Gualtieri", "Capo", "Eccentric and superstitious, Paulie is loyal but often reactive. He stands out through strong personality and comic moments."),
                CreateSeedCharacter(5, "Carmela Soprano", "Family", "Tony's wife, balancing morality, family priorities, and life inside a world built on violence and privilege.")
            };

            foreach (CharacterContent character in seededCharacters)
            {
                characters.Add(character);
            }

            SaveCharacters(characters);
        }

        private CharacterContent CreateSeedCharacter(int id, string fullName, string role, string description)
        {
            string rtfRelativePath = BuildDefaultRtfRelativePath(id, fullName);
            SaveTextAsRtf(description, rtfRelativePath);

            return new CharacterContent
            {
                Id = id,
                FullName = fullName,
                Role = role,
                ImagePath = PlaceholderImageRelativePath,
                RtfPath = rtfRelativePath,
                DateAddedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            };
        }

        private string BuildSafeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "content";
            }

            string sanitized = Regex.Replace(input.ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            return string.IsNullOrWhiteSpace(sanitized) ? "content" : sanitized;
        }

        private void SaveTextAsRtf(string text, string relativePath)
        {
            FlowDocument document = new FlowDocument(new Paragraph(new Run(text ?? string.Empty)));
            SaveFlowDocumentAsRtf(document, relativePath);
        }

        private void EnsurePlaceholderImage()
        {
            string placeholderPath = Path.Combine(ImagesDirectoryPath, "placeholder.png");
            if (File.Exists(placeholderPath))
            {
                return;
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A")), null, new Rect(0, 0, 320, 320));
                drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B08D57")), 2), new Rect(4, 4, 312, 312));

                FormattedText formattedText = new FormattedText(
                    "No Image",
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    32,
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5E6C8")),
                    1.0);

                drawingContext.DrawText(formattedText, new Point((320 - formattedText.Width) / 2, (320 - formattedText.Height) / 2));
            }

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(320, 320, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (FileStream stream = new FileStream(placeholderPath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}
