# GMGextractor

A Windows desktop application for extracting and creating archive files.

## Features

- Support for multiple archive formats:
  - ZIP (.zip)
  - RAR (.rar)
  - 7-Zip (.7z)
  - TAR (.tar)
  - GZip (.gz, .tar.gz)
  
- Automatic archive type detection
- Drag and drop functionality for files and folders
- Batch extraction capabilities
- Dark mode support
- Archive creation with optional password protection
- Progress tracking during extraction
- Detailed logging system

## Requirements

- Windows operating system
- .NET Framework 4.7.2 or higher
- Visual Studio 2019 or higher (for compilation)

## Dependencies

The application relies on the following libraries:

- **Built-in .NET libraries:**
  - System.IO.Compression (for ZIP extraction)
  - Windows Forms for GUI

- **External libraries (to be added manually):**
  - SevenZipExtractor (for RAR, 7Z, TAR, GZ format support)
    - This can be installed via NuGet: `Install-Package SevenZipExtractor`

## Building the Application

1. Clone or download this repository
2. Open Visual Studio
3. Create a new Windows Forms Application project named "GMGextractor"
4. Copy the contents of the `src` folder into your project
5. Ensure the folder structure matches the original repository
6. Install required NuGet packages
7. Build the solution

## Usage

### Extracting Archives

1. Launch the application
2. Use one of the following methods to select archives:
   - Drag and drop archive files or folders onto the application window
   - Click "Select Files" to choose individual archive files
   - Click "Select Folder" to process all archives in a folder
3. The application will automatically detect the archive type and extract its contents
4. Progress will be displayed during extraction
5. Extracted files will be saved to a folder with the same name as the archive in the same location

### Creating Archives

1. Click "Create Archive" in the main window
2. Add files or folders to include in the archive
3. Select the archive format (.zip, .7z, etc.)
4. Optionally enable password protection
5. Choose a save location
6. Click "Create" to generate the archive

## Project Structure

- `src/` - Source code
  - `Program.cs` - Application entry point
  - `MainForm.cs` - Main application window
  - `CreateArchiveForm.cs` - Archive creation dialog
  - `Handlers/` - Core functionality
    - `ArchiveHandler.cs` - Archive extraction and creation logic
  - `Utils/` - Utility classes
    - `Logger.cs` - Logging functionality
    - `FileUtils.cs` - File operation utilities

## License

[MIT License](LICENSE)

## Author

GMGextractor was created by [Your Name]