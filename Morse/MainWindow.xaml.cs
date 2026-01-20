using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Morse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Morse code dictionary
        private static readonly Dictionary<string, char> MorseDict = new()
        {
            { ".-", 'A' }, { "-...", 'B' }, { "-.-.", 'C' }, { "-..", 'D' }, { ".", 'E' },
            { "..-.", 'F' }, { "--.", 'G' }, { "....", 'H' }, { "..", 'I' }, { ".---", 'J' },
            { "-.-", 'K' }, { ".-..", 'L' }, { "--", 'M' }, { "-.", 'N' }, { "---", 'O' },
            { ".--.", 'P' }, { "--.-", 'Q' }, { ".-.", 'R' }, { "...", 'S' }, { "-", 'T' },
            { "..-", 'U' }, { "...-", 'V' }, { ".--", 'W' }, { "-..-", 'X' }, { "-.--", 'Y' },
            { "--..", 'Z' }, { "-----", '0' }, { ".----", '1' }, { "..---", '2' }, { "...--", '3' },
            { "....-", '4' }, { ".....", '5' }, { "-....", '6' }, { "--...", '7' }, { "---..", '8' },
            { "----.", '9' }
        };

        private string currentMorse = "";
        private string decodedLetters = "";
        private string fullText = "";
        private DispatcherTimer? idleTimer;
        private int delayMs = 800; // Default delay in milliseconds
        private const int DefaultDelayMs = 800;

        public MainWindow()
        {
            InitializeComponent();
            SetupIdleTimer();
            PopulateCheatSheet();
            Focus();
        }

        private void SetupIdleTimer()
        {
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromMilliseconds(delayMs);
            idleTimer.Tick += IdleTimer_Tick;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal) // . key
            {
                AddMorse('.');
                e.Handled = true;
            }
            else if (e.Key == Key.OemMinus || e.Key == Key.Subtract) // - key
            {
                AddMorse('-');
                e.Handled = true;
            }
            else if (e.Key == Key.OemComma) // , key for word separator
            {
                DecodeCurrent();
                fullText += " ";
                currentMorse = "";
                UpdateDisplays();
                idleTimer?.Stop();
                e.Handled = true;
            }
            else if (e.Key == Key.Back) // Backspace to remove last character
            {
                if (currentMorse.Length > 0)
                {
                    currentMorse = currentMorse.Substring(0, currentMorse.Length - 1);
                    UpdateDisplays();
                    RestartIdleTimer();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Space) // Space to copy current text and clear
            {
                if (fullText.Length > 0)
                {
                    CopyToClipboard(fullText.Trim().ToLower());
                    decodedLetters = "";
                    fullText = "";
                    currentMorse = "";
                    UpdateDisplays();
                    idleTimer?.Stop();
                    StatusText.Text = "✓ Copied!";
                    var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
                    clearTimer.Start();
                }
                e.Handled = true;
            }
        }

        private void AddMorse(char symbol)
        {
            currentMorse += symbol;
            UpdateDisplays();
            RestartIdleTimer();
        }

        private void RestartIdleTimer()
        {
            idleTimer?.Stop();
            idleTimer?.Start();
        }

        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            idleTimer?.Stop();
            DecodeCurrent();
            UpdateDisplays();
        }

        private void DecodeCurrent()
        {
            if (currentMorse.Length > 0)
            {
                if (MorseDict.TryGetValue(currentMorse, out char letter))
                {
                    decodedLetters += letter;
                    fullText += letter;
                }
                else
                {
                    // Invalid morse code - just skip or show as ?
                    decodedLetters += "?";
                    fullText += "?";
                }
                currentMorse = "";
            }
        }

        private void UpdateDisplays()
        {
            CurrentMorseDisplay.Text = currentMorse.Length > 0 ? currentMorse : "(waiting)";
            TextDisplay.Text = fullText;
        }

        private void CopyToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                MessageBox.Show("Failed to copy to clipboard.");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            currentMorse = "";
            decodedLetters = "";
            fullText = "";
            idleTimer?.Stop();
            UpdateDisplays();
            StatusText.Text = "Cleared!";
            var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
            clearTimer.Start();
            Focus();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (fullText.Length > 0)
            {
                CopyToClipboard(fullText.Trim().ToLower());
                decodedLetters = "";
                fullText = "";
                currentMorse = "";
                UpdateDisplays();
                idleTimer?.Stop();
                StatusText.Text = "✓ Copied!";
                var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
                clearTimer.Start();
            }
            Focus();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (fullText.Length > 0)
            {
                char removed = fullText[fullText.Length - 1];
                fullText = fullText.Substring(0, fullText.Length - 1);
                UpdateDisplays();
                StatusText.Text = $"Removed: {removed}";
                var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
                clearTimer.Start();
            }
            Focus();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = SettingsPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            DelayInput.Text = (delayMs / 1000.0).ToString("F1");
            Focus();
        }

        private void CheatSheetButton_Click(object sender, RoutedEventArgs e)
        {
            CheatSheetPanel.Visibility = CheatSheetPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            Focus();
        }

        private void PopulateCheatSheet()
        {
            // Create columns for 4-column layout
            CheatSheetGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 4; i++)
            {
                CheatSheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int row = 0;
            int col = 0;

            foreach (var kvp in MorseDict)
            {
                string morse = kvp.Key;
                char character = kvp.Value;

                // Add row definition
                if (col == 0)
                {
                    CheatSheetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Create stack panel for letter and morse
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var letterBlock = new TextBlock
                {
                    Text = character.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                var morseBlock = new TextBlock
                {
                    Text = morse,
                    FontSize = 11,
                    FontFamily = new System.Windows.Media.FontFamily("Courier New"),
                    Foreground = System.Windows.Media.Brushes.DodgerBlue,
                    FontWeight = FontWeights.Bold
                };

                stackPanel.Children.Add(letterBlock);
                stackPanel.Children.Add(morseBlock);

                Grid.SetRow(stackPanel, row);
                Grid.SetColumn(stackPanel, col);
                CheatSheetGrid.Children.Add(stackPanel);

                col++;
                if (col >= 4)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(DelayInput.Text, out double seconds))
            {
                delayMs = (int)(seconds * 1000);
                if (delayMs < 100) delayMs = 100; // Minimum 100ms
                if (delayMs > 10000) delayMs = 10000; // Maximum 10 seconds
                
                // Update the timer interval
                if (idleTimer != null)
                {
                    idleTimer.Interval = TimeSpan.FromMilliseconds(delayMs);
                }
                
                StatusText.Text = $"✓ Delay set to {delayMs}ms";
                DelayInput.Text = (delayMs / 1000.0).ToString("F1");
                var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
                clearTimer.Start();
                SettingsPanel.Visibility = Visibility.Collapsed;
                Focus();
            }
            else
            {
                StatusText.Text = "✗ Invalid input";
                var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                clearTimer.Tick += (s, e) => { StatusText.Text = ""; clearTimer.Stop(); };
                clearTimer.Start();
            }
        }
    }
}
