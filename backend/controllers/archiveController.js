const path = require('path');
const fs = require('fs');
const archiveHandler = require('../utils/archiveHandler');
const logger = require('../utils/logger');
const { v4: uuidv4 } = require('uuid');
const archiver = require('archiver');

// Track active extractions
const extractions = new Map();

/**
 * Extract archive file
 */
exports.extractArchive = async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({ 
        success: false, 
        message: 'No archive file provided' 
      });
    }

    const archiveFile = req.file;
    const extractionId = uuidv4();
    const extractionDir = path.join(__dirname, '..', 'extracted', extractionId);
    
    // Create extraction directory
    if (!fs.existsSync(extractionDir)) {
      fs.mkdirSync(extractionDir, { recursive: true });
    }
    
    // Store extraction metadata
    extractions.set(extractionId, {
      archiveFile: archiveFile.path,
      extractionDir,
      status: 'processing',
      files: [],
      startTime: new Date()
    });
    
    // Start extraction process
    logger.info(`Starting extraction of ${archiveFile.originalname} (ID: ${extractionId})`);
    
    // Send initial response
    res.status(200).json({ 
      success: true, 
      message: 'Extraction started',
      id: extractionId
    });
    
    // Extract file in background
    try {
      await archiveHandler.extractArchive(
        archiveFile.path, 
        extractionDir, 
        (progress) => {
          // Emit progress updates via WebSocket
          req.io.emit('extractionProgress', {
            id: extractionId,
            progress: Math.round(progress * 100)
          });
        }
      );
      
      // Get list of extracted files
      const files = await getExtractedFiles(extractionDir);
      
      // Update extraction metadata
      extractions.set(extractionId, {
        ...extractions.get(extractionId),
        status: 'completed',
        files,
        endTime: new Date()
      });
      
      // Send completion event
      req.io.emit('extractionComplete', {
        id: extractionId,
        files: files.map(file => ({
          name: file.name,
          path: `${extractionId}/${file.relativePath}`
        }))
      });
      
      logger.info(`Extraction completed for ${archiveFile.originalname} (ID: ${extractionId})`);
    } catch (error) {
      extractions.set(extractionId, {
        ...extractions.get(extractionId),
        status: 'failed',
        error: error.message,
        endTime: new Date()
      });
      
      req.io.emit('extractionError', {
        id: extractionId,
        error: error.message
      });
      
      logger.error(`Extraction failed for ${archiveFile.originalname}: ${error.message}`);
    }
  } catch (error) {
    logger.error(`Error in extractArchive: ${error.message}`);
    return res.status(500).json({ 
      success: false, 
      message: 'Server error during extraction'
    });
  }
};

/**
 * Download a single extracted file
 */
exports.downloadFile = (req, res) => {
  try {
    const { filename } = req.params;
    const [extractionId, ...pathParts] = filename.split('/');
    const filePath = path.join(pathParts.join('/'));
    
    // Validate extraction exists
    if (!extractions.has(extractionId)) {
      return res.status(404).json({
        success: false,
        message: 'Extraction not found'
      });
    }
    
    const extraction = extractions.get(extractionId);
    const fullPath = path.join(extraction.extractionDir, filePath);
    
    // Check if file exists
    if (!fs.existsSync(fullPath)) {
      return res.status(404).json({
        success: false,
        message: 'File not found'
      });
    }
    
    // Log download
    logger.info(`Downloading file: ${fullPath}`);
    
    // Send file
    res.download(fullPath);
  } catch (error) {
    logger.error(`Error in downloadFile: ${error.message}`);
    return res.status(500).json({
      success: false,
      message: 'Server error during file download'
    });
  }
};

/**
 * Download all extracted files as a ZIP
 */
exports.downloadAllFiles = (req, res) => {
  try {
    const { id } = req.params;
    
    // Validate extraction exists
    if (!extractions.has(id)) {
      return res.status(404).json({
        success: false,
        message: 'Extraction not found'
      });
    }
    
    const extraction = extractions.get(id);
    
    // Set headers for ZIP download
    res.writeHead(200, {
      'Content-Type': 'application/zip',
      'Content-Disposition': `attachment; filename="extracted_files.zip"`
    });
    
    // Create ZIP stream
    const archive = archiver('zip', {
      zlib: { level: 5 }
    });
    
    // Pipe archive data to response
    archive.pipe(res);
    
    // Add directory to archive
    archive.directory(extraction.extractionDir, false);
    
    // Finalize archive
    archive.finalize();
    
    logger.info(`Downloading all files from extraction: ${id}`);
  } catch (error) {
    logger.error(`Error in downloadAllFiles: ${error.message}`);
    return res.status(500).json({
      success: false,
      message: 'Server error during files download'
    });
  }
};

/**
 * Get list of files in an extraction directory
 */
async function getExtractedFiles(directory) {
  const files = [];
  
  async function scanDirectory(dir, baseDir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    
    for (const entry of entries) {
      const fullPath = path.join(dir, entry.name);
      const relativePath = path.relative(baseDir, fullPath);
      
      if (entry.isDirectory()) {
        await scanDirectory(fullPath, baseDir);
      } else {
        files.push({
          name: entry.name,
          fullPath,
          relativePath
        });
      }
    }
  }
  
  await scanDirectory(directory, directory);
  return files;
}
