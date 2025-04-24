const express = require('express');
const multer = require('multer');
const cors = require('cors');
const path = require('path');
const http = require('http');
const socketIo = require('socket.io');
const archiveController = require('./controllers/archiveController');
const logger = require('./utils/logger');

// Initialize Express app
const app = express();
const server = http.createServer(app);
const io = socketIo(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Serve static frontend files
app.use(express.static(path.join(__dirname, '..', 'frontend', 'public')));

// Set up multer for file uploads
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    cb(null, path.join(__dirname, 'uploads'));
  },
  filename: (req, file, cb) => {
    cb(null, `${Date.now()}-${file.originalname}`);
  }
});

const upload = multer({
  storage,
  limits: { fileSize: 500 * 1024 * 1024 }, // 500MB limit
  fileFilter: (req, file, cb) => {
    const fileExtension = path.extname(file.originalname).toLowerCase();
    const validExtensions = ['.zip', '.rar', '.7z', '.tar', '.gz'];
    
    if (validExtensions.includes(fileExtension)) {
      return cb(null, true);
    }
    
    cb(new Error('Invalid file type. Only archive files are allowed.'));
  }
});

// Socket.io setup
io.on('connection', (socket) => {
  logger.info('Client connected');
  
  socket.on('disconnect', () => {
    logger.info('Client disconnected');
  });
});

// Pass socket.io instance to controller
app.use((req, res, next) => {
  req.io = io;
  next();
});

// Routes
app.post('/extract', upload.single('archive'), (req, res) => {
  archiveController.extractArchive(req, res);
});

app.get('/download/:filename', (req, res) => {
  archiveController.downloadFile(req, res);
});

app.get('/download-all/:id', (req, res) => {
  archiveController.downloadAllFiles(req, res);
});

// Error handling middleware
app.use((err, req, res, next) => {
  logger.error(`Error: ${err.message}`);
  res.status(500).json({ success: false, message: err.message });
});

// Start server
const PORT = process.env.PORT || 8000;
server.listen(PORT, '0.0.0.0', () => {
  logger.info(`Server running on http://0.0.0.0:${PORT}`);
});

// Create necessary directories if they don't exist
const fs = require('fs');
const dirs = ['uploads', 'extracted'];

dirs.forEach(dir => {
  const dirPath = path.join(__dirname, dir);
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
    logger.info(`Created directory: ${dirPath}`);
  }
});

module.exports = app;
