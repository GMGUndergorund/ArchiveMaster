import React, { useRef, useState } from 'react';

function FileUpload({ onFileUpload, disabled }) {
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef(null);

  const handleDragOver = (e) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const files = Array.from(e.dataTransfer.files);
      const archiveFiles = files.filter(file => {
        const extension = file.name.split('.').pop().toLowerCase();
        return ['zip', 'rar', '7z', 'tar', 'gz'].includes(extension);
      });
      
      if (archiveFiles.length === 0) {
        alert('Please upload valid archive files (ZIP, RAR, 7Z, TAR, GZ)');
        return;
      }
      
      onFileUpload(archiveFiles);
    }
  };

  const handleFileSelect = (e) => {
    if (e.target.files && e.target.files.length > 0) {
      const files = Array.from(e.target.files);
      const archiveFiles = files.filter(file => {
        const extension = file.name.split('.').pop().toLowerCase();
        return ['zip', 'rar', '7z', 'tar', 'gz'].includes(extension);
      });
      
      if (archiveFiles.length === 0) {
        alert('Please upload valid archive files (ZIP, RAR, 7Z, TAR, GZ)');
        return;
      }
      
      onFileUpload(archiveFiles);
    }
  };

  const openFileSelector = () => {
    fileInputRef.current.click();
  };

  return (
    <div>
      <div 
        className={`drop-zone ${isDragging ? 'active' : ''}`}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={openFileSelector}
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="feather feather-upload mb-3">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
          <polyline points="17 8 12 3 7 8"></polyline>
          <line x1="12" y1="3" x2="12" y2="15"></line>
        </svg>
        <h3>Drag & Drop Archives Here</h3>
        <p>or</p>
        <button 
          className="btn btn-outline-primary"
          type="button"
          onClick={openFileSelector}
          disabled={disabled}
        >
          Select Files
        </button>
        <p className="mt-2 text-muted">Supported formats: ZIP, RAR, 7Z, TAR, GZ</p>
      </div>
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileSelect}
        style={{ display: 'none' }}
        accept=".zip,.rar,.7z,.tar,.gz"
        disabled={disabled}
      />
    </div>
  );
}

export default FileUpload;
