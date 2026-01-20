using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Morse
{
    public partial class CheatSheetWindow : Window
    {
        private static readonly Dictionary<char, string> CharToMorse = new()
        {
            { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." }, { 'E', "." },
            { 'F', "..-." }, { 'G', "--." }, { 'H', "...." }, { 'I', ".." }, { 'J', ".---" },
            { 'K', "-.-" }, { 'L', ".-.." }, { 'M', "--" }, { 'N', "-." }, { 'O', "---" },
            { 'P', ".--." }, { 'Q', "--.-" }, { 'R', ".-." }, { 'S', "..." }, { 'T', "-" },
            { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" }, { 'Y', "-.--" },
            { 'Z', "--.." }, { '0', "-----" }, { '1', ".----" }, { '2', "..---" }, { '3', "...--" },
            { '4', "....-" }, { '5', "....." }, { '6', "-....." }, { '7', "--..." }, { '8', "---.." },
            { '9', "----." }
        };

        public CheatSheetWindow()
        {
            InitializeComponent();
            PopulateCheatSheet();
        }

        private void PopulateCheatSheet()
        {
            // Create a grid layout for the cheat sheet
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0;
            int col = 0;

            foreach (var kvp in CharToMorse)
            {
                char character = kvp.Key;
                string morse = kvp.Value;

                // Create a border for each entry
                var border = new Border
                {
                    Background = col % 2 == 0 ? System.Windows.Media.Brushes.WhiteSmoke : System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10),
                    Margin = new Thickness(2)
                };

                var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

                var charBlock = new TextBlock
                {
                    Text = character.ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var morseBlock = new TextBlock
                {
                    Text = morse,
                    FontSize = 14,
                    FontFamily = new System.Windows.Media.FontFamily("Courier New"),
                    Foreground = System.Windows.Media.Brushes.DodgerBlue,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold
                };

                stackPanel.Children.Add(charBlock);
                stackPanel.Children.Add(morseBlock);
                border.Child = stackPanel;

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(border);

                col++;
                if (col >= 3)
                {
                    col = 0;
                    row++;
                }
            }

            CheatSheetPanel.Children.Add(grid);
        }
    }
}
