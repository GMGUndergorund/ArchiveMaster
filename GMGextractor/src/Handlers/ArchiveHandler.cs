using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GMGextractor.Utils;

namespace GMGextractor.Handlers
{
    public class ArchiveHandler
    {
        private readonly Logger _logger;
        
        // Supported archive extensions
        private readonly string[] _supportedExtensions = {
            ".zip", ".rar", ".7z", ".tar", ".gz", ".tar.gz"
        };
        
        public ArchiveHandler(Logger logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Checks if a file is a supported archive type
        /// </summary>
        public bool IsArchiveFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            // Special case for .tar.gz
            if (filePath.ToLower().EndsWith(".tar.gz"))
            {
                return true;
            }
            
            return _supportedExtensions.Contains(extension);
        }
        
        /// <summary>
        /// Finds all archive files in a folder
        /// </summary>
        public string[] FindArchivesInFolder(string folderPath)
        {
            List<string> archiveFiles = new List<string>();
            
            try
            {
                // Get all files in the directory
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                
                // Filter archives
                foreach (var file in files)
                {
                    if (IsArchiveFile(file))
                    {
                        archiveFiles.Add(file);
                    }
                }
                
                _logger.LogInfo($"Found {archiveFiles.Count} archive files in {folderPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scanning folder {folderPath}: {ex.Message}");
                throw;
            }
            
            return archiveFiles.ToArray();
        }
        
        /// <summary>
        /// Detects the archive type based on file extension
        /// </summary>
        private ArchiveType DetectArchiveType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            if (filePath.ToLower().EndsWith(".tar.gz"))
            {
                return ArchiveType.TarGz;
            }
            
            switch (extension)
            {
                case ".zip":
                    return ArchiveType.Zip;
                case ".rar":
                    return ArchiveType.Rar;
                case ".7z":
                    return ArchiveType.SevenZip;
                case ".tar":
                    return ArchiveType.Tar;
                case ".gz":
                    return ArchiveType.Gz;
                default:
                    throw new NotSupportedException($"Unsupported archive type: {extension}");
            }
        }
        
        /// <summary>
        /// Extracts an archive to the specified target directory
        /// </summary>
        public void ExtractArchive(string archivePath, string targetDir, Action<int> progressCallback)
        {
            try
            {
                // Make sure target directory exists
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                
                var archiveType = DetectArchiveType(archivePath);
                _logger.LogInfo($"Detected archive type: {archiveType}");
                
                switch (archiveType)
                {
                    case ArchiveType.Zip:
                        ExtractZip(archivePath, targetDir, progressCallback);
                        break;
                    case ArchiveType.Rar:
                    case ArchiveType.SevenZip:
                    case ArchiveType.Tar:
                    case ArchiveType.Gz:
                    case ArchiveType.TarGz:
                        ExtractWithExternalLibrary(archivePath, targetDir, progressCallback);
                        break;
                    default:
                        throw new NotSupportedException($"Extraction not implemented for {archiveType}");
                }
                
                _logger.LogInfo($"Extraction completed: {archivePath} to {targetDir}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Extraction failed for {archivePath}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Extract ZIP files using built-in .NET ZipFile class
        /// </summary>
        private void ExtractZip(string zipPath, string targetDir, Action<int> progressCallback)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                int entryCount = archive.Entries.Count;
                int processedCount = 0;
                
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Create entry output path
                    string entryPath = Path.Combine(targetDir, entry.FullName);
                    string entryDirectory = Path.GetDirectoryName(entryPath);
                    
                    // Create directory if it doesn't exist
                    if (!Directory.Exists(entryDirectory))
                    {
                        Directory.CreateDirectory(entryDirectory);
                    }
                    
                    // Skip directories
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        // Extract entry
                        entry.ExtractToFile(entryPath, true);
                    }
                    
                    // Update progress
                    processedCount++;
                    int percentage = (int)((double)processedCount / entryCount * 100);
                    progressCallback(percentage);
                }
            }
        }
        
        /// <summary>
        /// Extract non-ZIP formats using the SevenZipExtractor library
        /// Note: In a real implementation, this would use an actual library like SevenZipExtractor
        /// </summary>
        private void ExtractWithExternalLibrary(string archivePath, string targetDir, Action<int> progressCallback)
        {
            // In an actual implementation, this would use the appropriate library
            // Here we simulate progress for demonstration
            for (int i = 0; i <= 100; i += 10)
            {
                // Simulate extraction work
                Task.Delay(100).Wait();
                progressCallback(i);
            }
            
            // Placeholder for real extraction
            _logger.LogInfo("Note: This is a placeholder for actual extraction using SevenZipExtractor or similar library");
            
            /* 
             * Real implementation would be something like this:
             *
             * using (var extractor = new SevenZipExtractor(archivePath))
             * {
             *     extractor.ExtractArchive(targetDir);
             * }
             */
        }
        
        /// <summary>
        /// Creates a new archive
        /// </summary>
        public void CreateArchive(string[] filesToArchive, string archivePath, string password = null)
        {
            try
            {
                string extension = Path.GetExtension(archivePath).ToLower();
                _logger.LogInfo($"Creating archive: {archivePath}");
                
                switch (extension)
                {
                    case ".zip":
                        CreateZipArchive(filesToArchive, archivePath, password);
                        break;
                    case ".rar":
                    case ".7z":
                        CreateWithExternalLibrary(filesToArchive, archivePath, password);
                        break;
                    default:
                        throw new NotSupportedException($"Creating {extension} archives is not supported");
                }
                
                _logger.LogInfo($"Archive created: {archivePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating archive {archivePath}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Creates a ZIP archive using built-in .NET ZipFile class
        /// </summary>
        private void CreateZipArchive(string[] filesToArchive, string archivePath, string password)
        {
            // Ensure the target directory exists
            string archiveDir = Path.GetDirectoryName(archivePath);
            if (!Directory.Exists(archiveDir))
            {
                Directory.CreateDirectory(archiveDir);
            }
            
            // Create the ZIP archive
            using (FileStream zipFileStream = new FileStream(archivePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                foreach (string file in filesToArchive)
                {
                    string fileName = Path.GetFileName(file);
                    ZipArchiveEntry entry = archive.CreateEntryFromFile(file, fileName);
                    
                    // Note: System.IO.Compression doesn't support password protection
                    // In a real app, use a library like DotNetZip that supports this
                    if (!string.IsNullOrEmpty(password))
                    {
                        _logger.LogWarning("Password protection not implemented for ZIP files in this version");
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a non-ZIP archive using an external library
        /// </summary>
        private void CreateWithExternalLibrary(string[] filesToArchive, string archivePath, string password)
        {
            // This is a placeholder for using an external library like SevenZipSharp
            _logger.LogInfo("Note: This is a placeholder for actual archive creation using an external library");
            
            /* 
             * Real implementation would be something like this:
             *
             * using (var compressor = new SevenZipCompressor())
             * {
             *     if (!string.IsNullOrEmpty(password))
             *     {
             *         compressor.CompressionMode = CompressionMode.Create;
             *         compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
             *         compressor.CompressFilesEncrypted(archivePath, password, filesToArchive);
             *     }
             *     else
             *     {
             *         compressor.CompressFiles(archivePath, filesToArchive);
             *     }
             * }
             */
        }
    }
    
    public enum ArchiveType
    {
        Zip,
        Rar,
        SevenZip,
        Tar,
        Gz,
        TarGz
    }
}