using System;
using System.IO;

namespace GMGextractor.Utils
{
    public class Logger
    {
        private readonly string _logFilePath;
        private readonly string _errorLogFilePath;
        
        public Logger()
        {
            // Create logs directory in the application path
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string logDir = Path.Combine(appPath, "logs");
            
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            // Set up log file paths
            _logFilePath = Path.Combine(logDir, "app.log");
            _errorLogFilePath = Path.Combine(logDir, "error.log");
        }
        
        /// <summary>
        /// Format a log message with timestamp
        /// </summary>
        private string FormatLogMessage(string level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        }
        
        /// <summary>
        /// Write log message to console and file
        /// </summary>
        private void Log(string level, string message, bool isError = false)
        {
            string formattedMessage = FormatLogMessage(level, message);
            
            // Log to console
            Console.WriteLine(formattedMessage);
            
            // Log to file
            try
            {
                File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                
                // Also log errors to a separate file
                if (isError)
                {
                    File.AppendAllText(_errorLogFilePath, formattedMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // If logging to file fails, just print to console
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Log an informational message
        /// </summary>
        public void LogInfo(string message)
        {
            Log("INFO", message);
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        public void LogError(string message)
        {
            Log("ERROR", message, true);
        }
        
        /// <summary>
        /// Log a debug message (only in debug mode)
        /// </summary>
        public void LogDebug(string message)
        {
            #if DEBUG
            Log("DEBUG", message);
            #endif
        }
    }
}