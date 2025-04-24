import React, { useState, useEffect } from 'react';
import FileUpload from './components/FileUpload';
import ExtractProgress from './components/ExtractProgress';
import FileList from './components/FileList';
import { extractArchive } from './services/api';
import io from 'socket.io-client';

function App() {
  const [files, setFiles] = useState([]);
  const [extracting, setExtracting] = useState(false);
  const [extractionProgress, setExtractionProgress] = useState(0);
  const [extractedFiles, setExtractedFiles] = useState([]);
  const [error, setError] = useState(null);
  const [extractionId, setExtractionId] = useState(null);

  useEffect(() => {
    // Setup WebSocket connection for real-time progress updates
    const socket = io('http://localhost:8000');
    
    socket.on('connect', () => {
      console.log('Connected to WebSocket server');
    });
    
    socket.on('extractionProgress', (data) => {
      if (data.id === extractionId) {
        setExtractionProgress(data.progress);
      }
    });
    
    socket.on('extractionComplete', (data) => {
      if (data.id === extractionId) {
        setExtracting(false);
        setExtractedFiles(data.files);
        setExtractionProgress(100);
      }
    });
    
    socket.on('extractionError', (data) => {
      if (data.id === extractionId) {
        setExtracting(false);
        setError(data.error);
      }
    });
    
    return () => {
      socket.disconnect();
    };
  }, [extractionId]);

  const handleFileUpload = (uploadedFiles) => {
    setFiles(uploadedFiles);
    setError(null);
  };

  const handleExtract = async () => {
    if (files.length === 0) {
      setError('Please upload an archive file first.');
      return;
    }

    setExtracting(true);
    setExtractedFiles([]);
    setExtractionProgress(0);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('archive', files[0]);
      
      const response = await extractArchive(formData);
      setExtractionId(response.id);
    } catch (err) {
      setExtracting(false);
      setError(err.message || 'An error occurred during extraction.');
    }
  };

  const getLogo = () => {
    return (
      <pre className="app-logo">
{`   ____  __  ____  __       _                  _             
  / ___|/  \\/  \\ \\/ /_____  | |_ _ __ __ _  ___| |_ ___  _ __ 
 | |  _| |\\/| |\\  /|_____| | __| '__/ _\` |/ __| __/ _ \\| '__|
 | |_| | |  | |/  \\        | |_| | | (_| | (__| || (_) | |   
  \\____|_|  |_/_/\\_\\        \\__|_|  \\__,_|\\___|\\__\\___/|_|   
                                                            `}
      </pre>
    );
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Archive Extractor</h1>
        {getLogo()}
        <p>A web-based tool to extract your archive files</p>
      </header>
      
      {error && (
        <div className="error-message">
          <strong>Error:</strong> {error}
        </div>
      )}
      
      <div className="upload-container">
        <FileUpload 
          onFileUpload={handleFileUpload} 
          disabled={extracting}
        />
        
        {files.length > 0 && (
          <div className="mt-3">
            <FileList files={files} />
            <button 
              className="btn btn-primary mt-3" 
              onClick={handleExtract}
              disabled={extracting}
            >
              {extracting ? 'Extracting...' : 'Extract Files'}
            </button>
          </div>
        )}
        
        {extracting && (
          <ExtractProgress progress={extractionProgress} />
        )}
      </div>
      
      {extractedFiles.length > 0 && (
        <div className="extraction-result">
          <h3>Extracted Files</h3>
          <ul className="extracted-files">
            {extractedFiles.map((file, index) => (
              <li key={index} className="extracted-file-item">
                <span>{file.name}</span>
                <a 
                  href={`http://localhost:8000/download/${file.path}`} 
                  className="btn btn-sm btn-outline-primary btn-download"
                  download
                >
                  Download
                </a>
              </li>
            ))}
          </ul>
          <a 
            href={`http://localhost:8000/download-all/${extractionId}`}
            className="btn btn-success mt-3"
            download
          >
            Download All Files
          </a>
        </div>
      )}
    </div>
  );
}

export default App;
