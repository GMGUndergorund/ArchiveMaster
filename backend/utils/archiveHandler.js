const path = require('path');
const fs = require('fs');
const AdmZip = require('adm-zip');
const seven = require('node-7z');
const { exec } = require('child_process');
const logger = require('./logger');

/**
 * Detect archive type based on file extension
 * @param {string} filePath - Path to the archive file
 * @returns {string} - Archive type
 */
function detectArchiveType(filePath) {
  const extension = path.extname(filePath).toLowerCase();
  
  switch (extension) {
    case '.zip':
      return 'zip';
    case '.rar':
      return 'rar';
    case '.7z':
      return '7z';
    case '.tar':
      return 'tar';
    case '.gz':
      if (filePath.endsWith('.tar.gz')) {
        return 'tar.gz';
      }
      return 'gz';
    default:
      throw new Error(`Unsupported archive type: ${extension}`);
  }
}

/**
 * Extract archive file
 * @param {string} archivePath - Path to the archive file
 * @param {string} extractionDir - Directory to extract files to
 * @param {function} progressCallback - Callback for progress updates
 * @returns {Promise<void>}
 */
exports.extractArchive = async (archivePath, extractionDir, progressCallback) => {
  try {
    const archiveType = detectArchiveType(archivePath);
    logger.info(`Detected archive type: ${archiveType}`);
    
    switch (archiveType) {
      case 'zip':
        await extractZip(archivePath, extractionDir, progressCallback);
        break;
      case 'rar':
      case '7z':
      case 'tar':
      case 'tar.gz':
      case 'gz':
        await extract7zCompatible(archivePath, extractionDir, progressCallback);
        break;
      default:
        throw new Error(`Unsupported archive type: ${archiveType}`);
    }
  } catch (error) {
    logger.error(`Error extracting archive: ${error.message}`);
    throw error;
  }
};

/**
 * Extract ZIP archive
 * @param {string} zipPath - Path to the ZIP file
 * @param {string} extractionDir - Directory to extract files to
 * @param {function} progressCallback - Callback for progress updates
 * @returns {Promise<void>}
 */
async function extractZip(zipPath, extractionDir, progressCallback) {
  try {
    const zip = new AdmZip(zipPath);
    const zipEntries = zip.getEntries();
    const totalEntries = zipEntries.length;
    
    logger.info(`Extracting ZIP with ${totalEntries} entries`);
    
    // Extract each entry with progress updates
    zipEntries.forEach((entry, index) => {
      zip.extractEntryTo(entry, extractionDir, false, true);
      
      const progress = (index + 1) / totalEntries;
      progressCallback(progress);
      
      if ((index + 1) % 50 === 0 || index === totalEntries - 1) {
        logger.info(`Extracted ${index + 1}/${totalEntries} files (${Math.round(progress * 100)}%)`);
      }
    });
  } catch (error) {
    logger.error(`Error extracting ZIP: ${error.message}`);
    throw error;
  }
}

/**
 * Extract 7z-compatible archives (7z, RAR, TAR, etc.)
 * @param {string} archivePath - Path to the archive file
 * @param {string} extractionDir - Directory to extract files to
 * @param {function} progressCallback - Callback for progress updates
 * @returns {Promise<void>}
 */
async function extract7zCompatible(archivePath, extractionDir, progressCallback) {
  return new Promise((resolve, reject) => {
    let totalFiles = 0;
    let processedFiles = 0;
    let isListingComplete = false;
    
    // First list the contents to get file count
    const listStream = seven.list(archivePath);
    
    listStream.on('data', () => {
      totalFiles++;
    });
    
    listStream.on('end', () => {
      isListingComplete = true;
      logger.info(`Archive contains ${totalFiles} files`);
      
      // Now extract the files
      const extractStream = seven.extract(archivePath, extractionDir, {
        $progress: true
      });
      
      extractStream.on('data', (data) => {
        if (data.file) {
          processedFiles++;
          
          if (isListingComplete && totalFiles > 0) {
            const progress = processedFiles / totalFiles;
            progressCallback(progress);
            
            if (processedFiles % 20 === 0 || processedFiles === totalFiles) {
              logger.info(`Extracted ${processedFiles}/${totalFiles} files (${Math.round(progress * 100)}%)`);
            }
          }
        }
      });
      
      extractStream.on('end', () => {
        logger.info('7z extraction completed');
        resolve();
      });
      
      extractStream.on('error', (error) => {
        logger.error(`7z extraction error: ${error.message}`);
        reject(error);
      });
    });
    
    listStream.on('error', (error) => {
      // If 7z fails, try unzip as fallback for ZIP files
      if (path.extname(archivePath).toLowerCase() === '.zip') {
        logger.info('Falling back to unzip command');
        
        exec(`unzip "${archivePath}" -d "${extractionDir}"`, (err, stdout, stderr) => {
          if (err) {
            logger.error(`unzip error: ${err.message}`);
            reject(err);
            return;
          }
          
          logger.info('unzip extraction completed');
          progressCallback(1);
          resolve();
        });
      } else {
        logger.error(`7z listing error: ${error.message}`);
        reject(error);
      }
    });
  });
}
