using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SopranosCharactersCms.Dialogs
{
    public partial class ConfirmationDialogWindow : Window
    {
        public ConfirmationDialogWindow(Window owner, string title, string message, string confirmText, bool isDanger, bool showCancel)
        {
            Owner = owner;
            Width = owner.ActualWidth;
            Height = owner.ActualHeight;
            Left = owner.Left;
            Top = owner.Top;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Background = Brushes.Transparent;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Brush overlayBrush = (Brush)new BrushConverter().ConvertFromString("#66000000");
            Brush cardBrush = (Brush)new BrushConverter().ConvertFromString("#1F1F1F");
            Brush cardBorderBrush = (Brush)new BrushConverter().ConvertFromString("#7A1E1E");
            Brush titleBrush = (Brush)new BrushConverter().ConvertFromString("#F5E6C8");
            Brush messageBrush = (Brush)new BrushConverter().ConvertFromString("#B8B5AE");
            Brush goldBrush = (Brush)new BrushConverter().ConvertFromString("#B08D57");
            Brush redBrush = (Brush)new BrushConverter().ConvertFromString("#D65C5C");
            Brush primaryBrush = (Brush)new BrushConverter().ConvertFromString(isDanger ? "#D65C5C" : "#7A1E1E");

            Grid root = new Grid
            {
                Background = overlayBrush
            };

            Border card = new Border
            {
                Width = 500,
                Height = 280,
                CornerRadius = new CornerRadius(12),
                Background = cardBrush,
                BorderBrush = cardBorderBrush,
                BorderThickness = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas canvas = new Canvas();

            TextBlock titleText = new TextBlock
            {
                Text = title,
                Width = 260,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize = 28,
                Foreground = titleBrush,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(titleText, 120);
            Canvas.SetTop(titleText, 20);

            Button closeButton = BuildFlatButton("✕", redBrush, 20, Brushes.Transparent, (Brush)new BrushConverter().ConvertFromString("#F2C94C"));
            closeButton.Width = 24;
            closeButton.Height = 24;
            closeButton.Click += (_, __) => { DialogResult = false; Close(); };
            Canvas.SetLeft(closeButton, 460);
            Canvas.SetTop(closeButton, 8);

            TextBlock messageText = new TextBlock
            {
                Text = message,
                Width = 460,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Regular,
                FontSize = 14,
                Foreground = messageBrush,
                TextAlignment = showCancel ? TextAlignment.Left : TextAlignment.Center
            };
            Canvas.SetLeft(messageText, 20);
            Canvas.SetTop(messageText, 90);

            Border cancelBorder = new Border
            {
                Width = 180,
                Height = 44,
                CornerRadius = new CornerRadius(6),
                Background = Brushes.Transparent,
                BorderBrush = goldBrush,
                BorderThickness = new Thickness(2),
                Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed
            };
            Button cancelButton = BuildFlatButton("Cancel", goldBrush, 14, Brushes.Transparent, (Brush)new BrushConverter().ConvertFromString("#F2C94C"));
            cancelButton.FontWeight = FontWeights.Bold;
            cancelButton.Click += (_, __) => { DialogResult = false; Close(); };
            cancelBorder.Child = cancelButton;
            Canvas.SetLeft(cancelBorder, 50);
            Canvas.SetTop(cancelBorder, 180);

            Border confirmBorder = new Border
            {
                Width = 180,
                Height = 44,
                CornerRadius = new CornerRadius(6),
                Background = primaryBrush,
                BorderBrush = primaryBrush,
                BorderThickness = new Thickness(1)
            };
            Brush confirmHover = (Brush)new BrushConverter().ConvertFromString(isDanger ? "#C94A4A" : "#922626");
            Button confirmButton = BuildFlatButton(confirmText, titleBrush, 14, Brushes.Transparent, confirmHover);
            confirmButton.FontWeight = FontWeights.Bold;
            confirmButton.Click += (_, __) => { DialogResult = true; Close(); };
            confirmBorder.Child = confirmButton;
            Canvas.SetLeft(confirmBorder, showCancel ? 270 : 160);
            Canvas.SetTop(confirmBorder, 180);

            canvas.Children.Add(titleText);
            canvas.Children.Add(closeButton);
            canvas.Children.Add(messageText);
            canvas.Children.Add(cancelBorder);
            canvas.Children.Add(confirmBorder);

            card.Child = canvas;
            root.Children.Add(card);
            Content = root;
        }

        private static Button BuildFlatButton(string content, Brush foreground, double fontSize, Brush normalBackground, Brush hoverBackground)
        {
            Button button = new Button
            {
                Content = content,
                Background = normalBackground,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = foreground,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                Cursor = System.Windows.Input.Cursors.Hand,
                FocusVisualStyle = null,
                OverridesDefaultStyle = true
            };

            button.MouseEnter += (_, __) => button.Background = hoverBackground;
            button.MouseLeave += (_, __) => button.Background = normalBackground;

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);

            template.VisualTree = border;
            button.Template = template;
            return button;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
