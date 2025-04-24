using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMGextractor.Handlers;
using GMGextractor.Utils;

namespace GMGextractor
{
    public partial class MainForm : Form
    {
        private readonly ArchiveHandler _archiveHandler;
        private readonly Logger _logger;
        private bool _isDarkMode = false;

        public MainForm()
        {
            InitializeComponent();
            
            // Initialize components
            _logger = new Logger();
            _archiveHandler = new ArchiveHandler(_logger);
            
            // Set up event handlers
            this.Load += MainForm_Load;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            
            // Enable drag and drop
            this.AllowDrop = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Show ASCII art logo
            DisplayLogo();
            
            // Initialize UI
            InitializeUI();
            
            // Log application start
            _logger.LogInfo("Application started");
        }
        
        private void DisplayLogo()
        {
            string asciiLogo = @"
   ____  __  ____  __       _                  _             
  / ___|/  \/  \ \/ /_____  | |_ _ __ __ _  ___| |_ ___  _ __ 
 | |  _| |\/| |\  /|_____| | __| '__/ _` |/ __| __/ _ \| '__|
 | |_| | |  | |/  \        | |_| | | (_| | (__| || (_) | |   
  \____|_|  |_/_/\_\        \__|_|  \__,_|\___|\__\___/|_|   
                                                            ";
            
            lblLogo.Text = asciiLogo;
            lblLogo.Font = new Font("Consolas", 8);
        }
        
        private void InitializeUI()
        {
            // Set up initial UI state
            UpdateTheme(_isDarkMode);
        }
        
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ProcessFiles(files);
        }
        
        private void ProcessFiles(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                if (_archiveHandler.IsArchiveFile(filePath))
                {
                    ExtractArchive(filePath);
                }
                else if (Directory.Exists(filePath))
                {
                    ProcessFolder(filePath);
                }
                else
                {
                    _logger.LogWarning($"Unsupported file: {filePath}");
                    UpdateStatus($"Unsupported file: {Path.GetFileName(filePath)}");
                }
            }
        }
        
        private void ProcessFolder(string folderPath)
        {
            UpdateStatus($"Scanning folder: {folderPath}");
            _logger.LogInfo($"Scanning folder: {folderPath}");
            
            string[] archiveFiles = _archiveHandler.FindArchivesInFolder(folderPath);
            UpdateStatus($"Found {archiveFiles.Length} archives in folder");
            
            foreach (string archiveFile in archiveFiles)
            {
                ExtractArchive(archiveFile);
            }
        }
        
        private async void ExtractArchive(string archivePath)
        {
            UpdateStatus($"Extracting: {Path.GetFileName(archivePath)}");
            _logger.LogInfo($"Starting extraction of: {archivePath}");
            
            string targetDir = Path.Combine(
                Path.GetDirectoryName(archivePath),
                Path.GetFileNameWithoutExtension(archivePath)
            );
            
            progressExtract.Value = 0;
            progressExtract.Visible = true;
            
            try
            {
                await Task.Run(() => {
                    _archiveHandler.ExtractArchive(
                        archivePath, 
                        targetDir, 
                        progress => {
                            UpdateProgressBar(progress);
                        }
                    );
                });
                
                UpdateStatus($"Extraction completed: {Path.GetFileName(archivePath)}");
                _logger.LogInfo($"Extraction completed: {archivePath}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Extraction failed: {ex.Message}");
                _logger.LogError($"Extraction failed: {ex.Message}");
                MessageBox.Show(
                    $"Error extracting {Path.GetFileName(archivePath)}: {ex.Message}",
                    "Extraction Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                progressExtract.Visible = false;
            }
        }
        
        private void UpdateProgressBar(int percentage)
        {
            if (progressExtract.InvokeRequired)
            {
                progressExtract.Invoke(new Action(() => progressExtract.Value = percentage));
            }
            else
            {
                progressExtract.Value = percentage;
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusStrip.InvokeRequired)
            {
                statusStrip.Invoke(new Action(() => toolStripStatusLabel.Text = message));
            }
            else
            {
                toolStripStatusLabel.Text = message;
            }
        }
        
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archive files|*.zip;*.rar;*.7z;*.tar;*.gz|All files|*.*";
                openFileDialog.Title = "Select Archive Files";
                openFileDialog.Multiselect = true;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFiles(openFileDialog.FileNames);
                }
            }
        }
        
        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder containing archive files";
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFolder(folderDialog.SelectedPath);
                }
            }
        }
        
        private void btnCreateArchive_Click(object sender, EventArgs e)
        {
            // Open the create archive dialog
            ShowCreateArchiveDialog();
        }
        
        private void ShowCreateArchiveDialog()
        {
            // Implementation for create archive dialog
            // This would be implemented in a separate form
        }
        
        private void toggleDarkMode_Click(object sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            UpdateTheme(_isDarkMode);
        }
        
        private void UpdateTheme(bool darkMode)
        {
            if (darkMode)
            {
                // Dark mode colors
                this.BackColor = Color.FromArgb(45, 45, 48);
                this.ForeColor = Color.White;
                
                // Update controls colors
                foreach (Control control in this.Controls)
                {
                    if (control is Button)
                    {
                        control.BackColor = Color.FromArgb(60, 60, 65);
                        control.ForeColor = Color.White;
                    }
                    else if (control is Label || control is TextBox || control is ListBox)
                    {
                        control.BackColor = Color.FromArgb(30, 30, 35);
                        control.ForeColor = Color.White;
                    }
                }
                
                // Update logo and status strip
                lblLogo.ForeColor = Color.LightGreen;
                statusStrip.BackColor = Color.FromArgb(30, 30, 35);
                toolStripStatusLabel.ForeColor = Color.White;
                
                toggleDarkMode.Text = "Light Mode";
            }
            else
            {
                // Light mode colors
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                
                // Reset controls to default colors
                foreach (Control control in this.Controls)
                {
                    if (control is Button)
                    {
                        control.BackColor = SystemColors.Control;
                        control.ForeColor = SystemColors.ControlText;
                    }
                    else if (control is Label || control is TextBox || control is ListBox)
                    {
                        control.BackColor = SystemColors.Window;
                        control.ForeColor = SystemColors.WindowText;
                    }
                }
                
                // Update logo and status strip
                lblLogo.ForeColor = Color.DarkGreen;
                statusStrip.BackColor = SystemColors.Control;
                toolStripStatusLabel.ForeColor = SystemColors.ControlText;
                
                toggleDarkMode.Text = "Dark Mode";
            }
        }
        
        #region Designer Generated Code
        
        private void InitializeComponent()
        {
            this.lblLogo = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.btnCreateArchive = new System.Windows.Forms.Button();
            this.progressExtract = new System.Windows.Forms.ProgressBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDropHint = new System.Windows.Forms.Label();
            this.toggleDarkMode = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            
            // lblLogo
            this.lblLogo.AutoSize = true;
            this.lblLogo.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogo.Location = new System.Drawing.Point(12, 9);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(105, 17);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "GMGextractor";
            
            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(12, 140);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(120, 35);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Select Files";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            
            // btnBrowseFolder
            this.btnBrowseFolder.Location = new System.Drawing.Point(138, 140);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(120, 35);
            this.btnBrowseFolder.TabIndex = 2;
            this.btnBrowseFolder.Text = "Select Folder";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            
            // btnCreateArchive
            this.btnCreateArchive.Location = new System.Drawing.Point(264, 140);
            this.btnCreateArchive.Name = "btnCreateArchive";
            this.btnCreateArchive.Size = new System.Drawing.Size(120, 35);
            this.btnCreateArchive.TabIndex = 3;
            this.btnCreateArchive.Text = "Create Archive";
            this.btnCreateArchive.UseVisualStyleBackColor = true;
            this.btnCreateArchive.Click += new System.EventHandler(this.btnCreateArchive_Click);
            
            // progressExtract
            this.progressExtract.Location = new System.Drawing.Point(12, 190);
            this.progressExtract.Name = "progressExtract";
            this.progressExtract.Size = new System.Drawing.Size(460, 23);
            this.progressExtract.TabIndex = 4;
            this.progressExtract.Visible = false;
            
            // statusStrip
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 250);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(484, 26);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip";
            
            // toolStripStatusLabel
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(50, 20);
            this.toolStripStatusLabel.Text = "Ready";
            
            // lblDropHint
            this.lblDropHint.AutoSize = true;
            this.lblDropHint.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropHint.Location = new System.Drawing.Point(110, 100);
            this.lblDropHint.Name = "lblDropHint";
            this.lblDropHint.Size = new System.Drawing.Size(263, 23);
            this.lblDropHint.TabIndex = 6;
            this.lblDropHint.Text = "Drag and drop files or folders here";
            
            // toggleDarkMode
            this.toggleDarkMode.Location = new System.Drawing.Point(390, 140);
            this.toggleDarkMode.Name = "toggleDarkMode";
            this.toggleDarkMode.Size = new System.Drawing.Size(82, 35);
            this.toggleDarkMode.TabIndex = 7;
            this.toggleDarkMode.Text = "Dark Mode";
            this.toggleDarkMode.UseVisualStyleBackColor = true;
            this.toggleDarkMode.Click += new System.EventHandler(this.toggleDarkMode_Click);
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 276);
            this.Controls.Add(this.toggleDarkMode);
            this.Controls.Add(this.lblDropHint);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressExtract);
            this.Controls.Add(this.btnCreateArchive);
            this.Controls.Add(this.btnBrowseFolder);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblLogo);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GMGextractor";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        #endregion
        
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Button btnCreateArchive;
        private System.Windows.Forms.ProgressBar progressExtract;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label lblDropHint;
        private System.Windows.Forms.Button toggleDarkMode;
    }
}