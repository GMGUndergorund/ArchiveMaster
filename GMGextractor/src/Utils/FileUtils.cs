using System;
using System.IO;
using System.Text;

namespace GMGextractor.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Gets a human-readable file size
        /// </summary>
        public static string GetReadableFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Gets the MIME type for a file extension
        /// </summary>
        public static string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".zip":
                    return "application/zip";
                case ".rar":
                    return "application/x-rar-compressed";
                case ".7z":
                    return "application/x-7z-compressed";
                default:
                    return "application/octet-stream";
            }
        }
        
        /// <summary>
        /// Safely delete a file, handling exceptions
        /// </summary>
        public static bool SafeDeleteFile(string filePath, Logger logger)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to delete file {filePath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Safely delete a directory and all its contents
        /// </summary>
        public static bool SafeDeleteDirectory(string dirPath, Logger logger)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to delete directory {dirPath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets file count in a directory (including subdirectories)
        /// </summary>
        public static int GetFileCount(string dirPath)
        {
            try
            {
                return Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories).Length;
            }
            catch
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Gets directory size (sum of all file sizes)
        /// </summary>
        public static long GetDirectorySize(string dirPath)
        {
            long size = 0;
            
            try
            {
                // Add file sizes
                string[] files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    size += fileInfo.Length;
                }
            }
            catch
            {
                // Ignore errors
            }
            
            return size;
        }
        
        /// <summary>
        /// Check if a path has valid characters
        /// </summary>
        public static bool IsValidPath(string path)
        {
            try
            {
                // Check for invalid characters
                char[] invalidChars = Path.GetInvalidPathChars();
                
                foreach (char c in path)
                {
                    if (invalidChars.Contains(c))
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Extension method to check if an array contains a value
        /// </summary>
        private static bool Contains<T>(this T[] array, T value)
        {
            foreach (T item in array)
            {
                if (item.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}