import React from 'react';

function ExtractProgress({ progress }) {
  return (
    <div className="progress-container">
      <h4>Extracting Files...</h4>
      <div className="progress">
        <div 
          className="progress-bar progress-bar-striped progress-bar-animated" 
          role="progressbar" 
          style={{ width: `${progress}%` }} 
          aria-valuenow={progress} 
          aria-valuemin="0" 
          aria-valuemax="100"
        >
          {progress}%
        </div>
      </div>
    </div>
  );
}

export default ExtractProgress;
