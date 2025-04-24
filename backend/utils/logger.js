const fs = require('fs');
const path = require('path');

// Create logs directory if it doesn't exist
const logsDir = path.join(__dirname, '..', 'logs');
if (!fs.existsSync(logsDir)) {
  fs.mkdirSync(logsDir, { recursive: true });
}

// Log file paths
const logFile = path.join(logsDir, 'app.log');
const errorLogFile = path.join(logsDir, 'error.log');

/**
 * Logger utility for application
 */
class Logger {
  /**
   * Format a log message with timestamp
   * @param {string} level - Log level
   * @param {string} message - Log message
   * @returns {string} - Formatted log message
   */
  static formatLogMessage(level, message) {
    const timestamp = new Date().toISOString();
    return `[${timestamp}] [${level}] ${message}\n`;
  }
  
  /**
   * Write log message to console and file
   * @param {string} level - Log level
   * @param {string} message - Log message
   * @param {boolean} isError - Whether this is an error log
   */
  static log(level, message, isError = false) {
    const formattedMessage = this.formatLogMessage(level, message);
    
    // Log to console
    console.log(formattedMessage);
    
    // Log to file
    fs.appendFileSync(logFile, formattedMessage);
    
    // Log errors to separate error log file
    if (isError) {
      fs.appendFileSync(errorLogFile, formattedMessage);
    }
  }
  
  /**
   * Log info message
   * @param {string} message - Info message
   */
  static info(message) {
    this.log('INFO', message);
  }
  
  /**
   * Log warning message
   * @param {string} message - Warning message
   */
  static warn(message) {
    this.log('WARN', message);
  }
  
  /**
   * Log error message
   * @param {string} message - Error message
   */
  static error(message) {
    this.log('ERROR', message, true);
  }
  
  /**
   * Log debug message
   * @param {string} message - Debug message
   */
  static debug(message) {
    if (process.env.NODE_ENV !== 'production') {
      this.log('DEBUG', message);
    }
  }
}

module.exports = Logger;
