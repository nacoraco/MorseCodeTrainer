using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Morse
{
    public partial class MainWindow : Window
    {
        private enum InputMode { Morse, Chord }

        // Morse
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

        // Chord: bitmask per key  A=7=bit0(1) B=8=bit1(2) C=4=bit2(4) D=5=bit3(8) E=1=bit4(16) F=2=bit5(32)
        private static readonly Dictionary<int, char> ChordDict = new()
        {
            { 1, 'A' }, { 2, 'B' }, { 4, 'C' }, { 8, 'D' }, { 16, 'E' }, { 32, 'F' },
            { 3, 'G' }, { 5, 'H' }, { 9, 'I' }, { 17, 'J' }, { 33, 'K' },
            { 6, 'L' }, { 10, 'M' }, { 18, 'N' }, { 34, 'O' },
            { 12, 'P' }, { 20, 'R' }, { 36, 'S' },
            { 24, 'T' }, { 40, 'U' },
            { 48, 'V' },
            { 7, 'X' }, { 11, 'Y' }, { 19, 'Z' }, { 35, 'W' },
            { 13, 'Q' }
        };

        private static readonly int[] ChordBits = { 1, 2, 4, 8, 16, 32 };
        private static readonly string[] ChordKeyLabels = { "7", "8", "4", "5", "1", "2" };
        private static readonly string[] ChordDotLabels = { "A", "B", "C", "D", "E", "F" };

        // Chord key set: NumPad and regular digits
        private static readonly HashSet<Key> ChordKeys = new()
        {
            Key.NumPad7, Key.D7, Key.NumPad8, Key.D8,
            Key.NumPad4, Key.D4, Key.NumPad5, Key.D5,
            Key.NumPad1, Key.D1, Key.NumPad2, Key.D2
        };

        // State
        private InputMode mode = InputMode.Morse;
        private string currentMorse = "";
        private int chordMask = 0;
        private string decodedLetters = "";
        private string fullText = "";
        private DispatcherTimer? idleTimer;
        private int delayMs = 800;
        private const int DefaultDelayMs = 800;

        private const double BaseWindowHeight = 330;
        private const double SettingsHeight = 80;
        private const double CheatSheetHeight = 350;

        public MainWindow()
        {
            InitializeComponent();
            SetupIdleTimer();
            PopulateCheatSheet();
            UpdateModeUI();
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
            if (mode == InputMode.Morse)
            {
                HandleMorseKeyDown(e);
            }
            else
            {
                HandleChordKeyDown(e);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (mode == InputMode.Chord && ChordKeys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void HandleMorseKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                AddMorse('.');
                e.Handled = true;
            }
            else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
            {
                AddMorse('-');
                e.Handled = true;
            }
            else if (e.Key == Key.OemComma)
            {
                DecodeCurrent();
                fullText += " ";
                currentMorse = "";
                UpdateDisplays();
                idleTimer?.Stop();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                if (currentMorse.Length > 0)
                {
                    currentMorse = currentMorse.Substring(0, currentMorse.Length - 1);
                    UpdateDisplays();
                    RestartIdleTimer();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Space)
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

        private void HandleChordKeyDown(KeyEventArgs e)
        {
            if (ChordKeys.Contains(e.Key))
            {
                int bit = KeyToBit(e.Key);
                chordMask ^= bit;
                UpdateDisplays();
                RestartIdleTimer();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                chordMask = 0;
                UpdateDisplays();
                idleTimer?.Stop();
                e.Handled = true;
            }
            else if (e.Key == Key.OemComma)
            {
                DecodeCurrent();
                fullText += " ";
                chordMask = 0;
                UpdateDisplays();
                idleTimer?.Stop();
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                if (fullText.Length > 0)
                {
                    CopyToClipboard(fullText.Trim().ToLower());
                    decodedLetters = "";
                    fullText = "";
                    chordMask = 0;
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

        private static int KeyToBit(Key key)
        {
            return key switch
            {
                Key.NumPad7 or Key.D7 => 1,   // A
                Key.NumPad8 or Key.D8 => 2,   // B
                Key.NumPad4 or Key.D4 => 4,   // C
                Key.NumPad5 or Key.D5 => 8,   // D
                Key.NumPad1 or Key.D1 => 16,  // E
                Key.NumPad2 or Key.D2 => 32,  // F
                _ => 0
            };
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
            if (mode == InputMode.Morse)
            {
                if (currentMorse.Length > 0)
                {
                    if (MorseDict.TryGetValue(currentMorse, out char letter))
                    {
                        decodedLetters += letter;
                        fullText += letter;
                    }
                    currentMorse = "";
                }
            }
            else
            {
                if (chordMask != 0)
                {
                    if (ChordDict.TryGetValue(chordMask, out char letter))
                    {
                        decodedLetters += letter;
                        fullText += letter;
                    }
                    chordMask = 0;
                }
            }
        }

        private void UpdateDisplays()
        {
            if (mode == InputMode.Morse)
            {
                CurrentMorseDisplay.Text = currentMorse.Length > 0 ? currentMorse : "(waiting)";
            }
            else
            {
                CurrentMorseDisplay.Text = chordMask != 0 ? FormatChordDisplay() : "(waiting)";
            }
            TextDisplay.Text = fullText;
        }

        private string FormatChordDisplay()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ChordBits.Length; i++)
            {
                sb.Append((chordMask & ChordBits[i]) != 0 ? ChordKeyLabels[i] : "·");
                if (i == 1 || i == 3) sb.Append('\n');
                else if (i < 5) sb.Append(' ');
            }
            if (ChordDict.TryGetValue(chordMask, out char letter))
            {
                sb.Append("  → ").Append(letter);
            }
            return sb.ToString();
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
            chordMask = 0;
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
                chordMask = 0;
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
            bool isExpanding = SettingsPanel.Visibility == Visibility.Collapsed;
            SettingsPanel.Visibility = isExpanding ? Visibility.Visible : Visibility.Collapsed;
            DelayInput.Text = (delayMs / 1000.0).ToString("F1");

            double targetHeight = BaseWindowHeight;
            if (isExpanding)
            {
                targetHeight += SettingsHeight;
            }
            if (CheatSheetPanel.Visibility == Visibility.Visible)
            {
                targetHeight += CheatSheetHeight;
            }

            AnimateWindowHeight(targetHeight);
            Focus();
        }

        private void CheatSheetButton_Click(object sender, RoutedEventArgs e)
        {
            bool isExpanding = CheatSheetPanel.Visibility == Visibility.Collapsed;
            CheatSheetPanel.Visibility = isExpanding ? Visibility.Visible : Visibility.Collapsed;

            double targetHeight = BaseWindowHeight;
            if (SettingsPanel.Visibility == Visibility.Visible)
            {
                targetHeight += SettingsHeight;
            }
            if (isExpanding)
            {
                targetHeight += CheatSheetHeight;
            }

            AnimateWindowHeight(targetHeight);
            Focus();
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(DelayInput.Text, out double seconds))
            {
                delayMs = (int)(seconds * 1000);
                if (delayMs < 100) delayMs = 100;
                if (delayMs > 10000) delayMs = 10000;

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

                double targetHeight = BaseWindowHeight;
                if (CheatSheetPanel.Visibility == Visibility.Visible)
                {
                    targetHeight += CheatSheetHeight;
                }
                AnimateWindowHeight(targetHeight);

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

        private void AnimateWindowHeight(double targetHeight)
        {
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = this.ActualHeight,
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut }
            };
            this.BeginAnimation(Window.HeightProperty, animation);
        }

        // ─── Mode switching ────────────────────────────────────────

        private void MorseModeButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(InputMode.Morse);
            Focus();
        }

        private void ChordModeButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(InputMode.Chord);
            Focus();
        }

        private void SetMode(InputMode newMode)
        {
            if (mode == newMode) return;
            ResetAll();
            mode = newMode;
            UpdateModeUI();
            PopulateCheatSheet();
        }

        private void ResetAll()
        {
            idleTimer?.Stop();
            currentMorse = "";
            chordMask = 0;
            decodedLetters = "";
            fullText = "";
            UpdateDisplays();
        }

        private void UpdateModeUI()
        {
            bool isMorse = mode == InputMode.Morse;
            MorseModeButton.Background = isMorse
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC));
            MorseModeButton.Foreground = isMorse
                ? System.Windows.Media.Brushes.White
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
            ChordModeButton.Background = !isMorse
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC));
            ChordModeButton.Foreground = !isMorse
                ? System.Windows.Media.Brushes.White
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
            InputLabel.Text = isMorse ? "Morse:" : "Chord:";
            InstructionText.Text = isMorse
                ? ". = dot  |  - = dash  |  , = space  |  Backspace = delete  |  Space = copy"
                : "7=A 8=B 4=C 5=D 1=E 2=F  |  Backspace = clear  |  , = space  |  Space = copy";
            CurrentMorseDisplay.Foreground = isMorse
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x66, 0xCC))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00));
            CurrentMorseDisplay.TextAlignment = isMorse ? TextAlignment.Center : TextAlignment.Left;
            CurrentMorseDisplay.FontSize = isMorse ? 18 : 14;
            UpdateDisplays();
        }

        // ─── Cheat sheet ───────────────────────────────────────────

        private void PopulateCheatSheet()
        {
            CheatSheetGrid.Children.Clear();
            CheatSheetGrid.ColumnDefinitions.Clear();
            CheatSheetGrid.RowDefinitions.Clear();

            if (mode == InputMode.Morse)
            {
                PopulateMorseCheatSheet();
            }
            else
            {
                PopulateChordCheatSheet();
            }
        }

        private void PopulateMorseCheatSheet()
        {
            for (int i = 0; i < 4; i++)
            {
                CheatSheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int row = 0;
            int col = 0;

            foreach (var kvp in MorseDict)
            {
                if (col == 0)
                {
                    CheatSheetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = kvp.Value.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Black
                });

                stackPanel.Children.Add(new TextBlock
                {
                    Text = kvp.Key,
                    FontSize = 11,
                    FontFamily = new System.Windows.Media.FontFamily("Courier New"),
                    Foreground = System.Windows.Media.Brushes.DodgerBlue,
                    FontWeight = FontWeights.Bold
                });

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

        private void PopulateChordCheatSheet()
        {
            CheatSheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CheatSheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            int row = 0;
            var entries = new List<(int mask, char letter)>();
            foreach (var kvp in ChordDict)
            {
                entries.Add((kvp.Key, kvp.Value));
            }
            entries.Sort((a, b) => a.letter.CompareTo(b.letter));

            foreach (var (mask, letter) in entries)
            {
                CheatSheetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var letterBlock = new TextBlock
                {
                    Text = letter.ToString(),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(letterBlock, row);
                Grid.SetColumn(letterBlock, 0);
                CheatSheetGrid.Children.Add(letterBlock);

                var dotsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(8, 2, 0, 2)
                };

                for (int i = 0; i < ChordBits.Length; i++)
                {
                    bool active = (mask & ChordBits[i]) != 0;
                    var dotText = new TextBlock
                    {
                        Text = active ? "●" : "○",
                        FontSize = 11,
                        Foreground = active
                            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00))
                            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)),
                        FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                        Margin = new Thickness(1),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    dotsPanel.Children.Add(dotText);

                    if (i == 1 || i == 3)
                    {
                        dotsPanel.Children.Add(new TextBlock
                        {
                            Text = " ",
                            FontSize = 1,
                            Width = 12
                        });
                    }
                }

                var keysText = new TextBlock
                {
                    Text = GetChordKeyLabels(mask),
                    FontSize = 10,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88)),
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                dotsPanel.Children.Add(keysText);

                Grid.SetRow(dotsPanel, row);
                Grid.SetColumn(dotsPanel, 1);
                CheatSheetGrid.Children.Add(dotsPanel);

                row++;
            }
        }

        private static string GetChordKeyLabels(int mask)
        {
            var keys = new List<string>();
            for (int i = 0; i < ChordBits.Length; i++)
            {
                if ((mask & ChordBits[i]) != 0)
                {
                    keys.Add(ChordDotLabels[i]);
                }
            }
            return string.Join("+", keys);
        }
    }
}
