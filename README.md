# Morse Code Decoder

A lightweight WPF desktop application for encoding text input as Morse code and decoding Morse code back to text.

Practice Morse code on Windows while doing other things. Simply write in Morse code and copy the translated text.

## Features

- **Real-time Morse Code Entry**: Type Morse code using your keyboard
  - `.` (period) key for dot
  - `-` (minus) key for dash
  - `,` (comma) key to add word separator
  - Backspace to remove the last character

- **Automatic Decoding**: Automatically converts Morse code to letters after a configurable delay

- **Built-in Cheat Sheet**: Quick reference window displaying all Morse code mappings for letters (A-Z) and numbers (0-9)

- **One-Click Copy**: Press Space to automatically copy the decoded text to your clipboard

- **Status Indicators**: Visual feedback showing what's currently typed and what's been decoded

## Requirements

- Windows (.NET 8.0 or later)
- WPF runtime (included with .NET 8.0)

## Installation

### Option 1: Build from Source
1. Clone the repository
2. Open `Morse.sln` in Visual Studio
3. Build the solution (Release or Debug configuration)
4. Run the application from `bin\Debug\net8.0-windows` or `bin\Release\net8.0-windows`


## Usage

### Basic Input
1. Launch the application
2. Start typing Morse code:
   - Press `.` for dots
   - Press `-` for dashes
   - Letters will automatically decode after ~800ms of inactivity
3. Press `,` to separate words
4. Press Backspace to undo the last dot or dash
5. Press Space to copy the entire decoded text to clipboard

### Examples
- `.-` → A
- `-...` → B
- `-.-` → K
- `...` → S
- `-----` → 0

### View Cheat Sheet
Click the "Cheat Sheet" button to open a reference window showing all available Morse code mappings.

## Project Structure

```
Morse/
├── MainWindow.xaml           # Main UI layout
├── MainWindow.xaml.cs        # Core Morse decoder logic
├── CheatSheetWindow.xaml     # Reference window layout
├── CheatSheetWindow.xaml.cs  # Cheat sheet display logic
├── App.xaml                  # Application resources
├── App.xaml.cs               # Application startup code
├── Morse.csproj              # Project configuration
└── README.md                 # This file
```

## Technical Details

- **Framework**: .NET 8.0 WPF
- **Language**: C#
- **UI**: XAML with WPF
- **Supported Characters**: 
  - Letters: A-Z
  - Numbers: 0-9
  - Invalid Morse sequences display as `?`

## How It Works

1. **Input Capture**: The application captures keyboard events for Morse symbols
2. **Buffering**: Current Morse sequence is stored and displayed in real-time
3. **Decoding**: On idle timeout (or manual word separator), the Morse sequence is looked up in the dictionary
4. **Display Update**: Decoded character is added to the text output and displayed to the user
5. **Clipboard**: Decoded text can be copied with a single keystroke

## License

MIT

## Contributing

Contributions are welcome! Feel free to fork, modify, and submit pull requests.

## Author

Created by Nejc

## Future Enhancements

- Adjustable delay timeout for auto-decoding
- Support for special characters and punctuation
- Audio Morse code playback
- Text-to-Morse encoding feature
- Keybinding customization
- Dark mode support
