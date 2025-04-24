const API_BASE_URL = 'http://localhost:8000';

export const extractArchive = async (formData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/extract`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to extract archive');
    }

    return await response.json();
  } catch (error) {
    console.error('Error extracting archive:', error);
    throw error;
  }
};

export const downloadFile = async (filePath) => {
  try {
    const response = await fetch(`${API_BASE_URL}/download/${filePath}`);
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to download file');
    }
    
    return await response.blob();
  } catch (error) {
    console.error('Error downloading file:', error);
    throw error;
  }
};

export const downloadAllFiles = async (extractionId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/download-all/${extractionId}`);
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to download files');
    }
    
    return await response.blob();
  } catch (error) {
    console.error('Error downloading all files:', error);
    throw error;
  }
};
