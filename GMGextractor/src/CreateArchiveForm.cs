using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GMGextractor.Handlers;
using GMGextractor.Utils;

namespace GMGextractor
{
    public partial class CreateArchiveForm : Form
    {
        private readonly ArchiveHandler _archiveHandler;
        private readonly Logger _logger;
        private readonly List<string> _selectedFiles = new List<string>();
        private bool _isDarkMode;
        
        public CreateArchiveForm(ArchiveHandler archiveHandler, Logger logger, bool isDarkMode)
        {
            InitializeComponent();
            
            _archiveHandler = archiveHandler;
            _logger = logger;
            _isDarkMode = isDarkMode;
            
            // Update form theme
            UpdateTheme(isDarkMode);
            
            // Set up event handlers
            this.Load += CreateArchiveForm_Load;
            this.DragEnter += CreateArchiveForm_DragEnter;
            this.DragDrop += CreateArchiveForm_DragDrop;
            
            // Enable drag and drop
            this.AllowDrop = true;
        }
        
        private void CreateArchiveForm_Load(object sender, EventArgs e)
        {
            // Set up archive format dropdown
            cmbFormat.Items.Add(".zip");
            cmbFormat.Items.Add(".7z");
            cmbFormat.Items.Add(".rar");
            cmbFormat.SelectedIndex = 0;
            
            // Initialize UI
            UpdateFilesList();
            
            _logger.LogInfo("Create Archive dialog opened");
        }
        
        private void CreateArchiveForm_DragEnter(object sender, DragEventArgs e)
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
        
        private void CreateArchiveForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddFilesToList(files);
        }
        
        private void AddFilesToList(string[] files)
        {
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    if (!_selectedFiles.Contains(file))
                    {
                        _selectedFiles.Add(file);
                    }
                }
                else if (Directory.Exists(file))
                {
                    // Add all files in the directory
                    AddFilesFromDirectory(file);
                }
            }
            
            UpdateFilesList();
        }
        
        private void AddFilesFromDirectory(string directory)
        {
            try
            {
                string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (!_selectedFiles.Contains(file))
                    {
                        _selectedFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding files from directory {directory}: {ex.Message}");
            }
        }
        
        private void UpdateFilesList()
        {
            // Clear and update the listbox
            lstFiles.Items.Clear();
            
            foreach (string file in _selectedFiles)
            {
                lstFiles.Items.Add(Path.GetFileName(file));
            }
            
            // Update status text
            lblStatus.Text = $"{_selectedFiles.Count} files selected";
        }
        
        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select Files to Archive";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AddFilesToList(openFileDialog.FileNames);
                }
            }
        }
        
        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to add all its files";
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    AddFilesFromDirectory(folderDialog.SelectedPath);
                    UpdateFilesList();
                }
            }
        }
        
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                int index = lstFiles.SelectedIndex;
                _selectedFiles.RemoveAt(index);
                UpdateFilesList();
            }
        }
        
        private void btnClear_Click(object sender, EventArgs e)
        {
            _selectedFiles.Clear();
            UpdateFilesList();
        }
        
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (_selectedFiles.Count == 0)
            {
                MessageBox.Show(
                    "Please add at least one file to create an archive.",
                    "No Files Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                string selectedFormat = cmbFormat.SelectedItem.ToString();
                
                saveDialog.Filter = $"Archive Files (*{selectedFormat})|*{selectedFormat}";
                saveDialog.Title = "Save Archive";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string password = null;
                    if (chkPassword.Checked && !string.IsNullOrEmpty(txtPassword.Text))
                    {
                        password = txtPassword.Text;
                    }
                    
                    try
                    {
                        _archiveHandler.CreateArchive(
                            _selectedFiles.ToArray(),
                            saveDialog.FileName,
                            password
                        );
                        
                        MessageBox.Show(
                            $"Archive created successfully at:\\n{saveDialog.FileName}",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        
                        // Close the form after successful creation
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error creating archive: {ex.Message}");
                        
                        MessageBox.Show(
                            $"Error creating archive:\\n{ex.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }
        
        private void chkPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.Enabled = chkPassword.Checked;
            lblPassword.Enabled = chkPassword.Checked;
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
                    else if (control is Label)
                    {
                        control.ForeColor = Color.White;
                    }
                    else if (control is TextBox || control is ListBox || control is ComboBox)
                    {
                        control.BackColor = Color.FromArgb(30, 30, 35);
                        control.ForeColor = Color.White;
                    }
                }
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
                    else if (control is Label)
                    {
                        control.ForeColor = SystemColors.ControlText;
                    }
                    else if (control is TextBox || control is ListBox || control is ComboBox)
                    {
                        control.BackColor = SystemColors.Window;
                        control.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }
        
        #region Designer Generated Code
        
        private void InitializeComponent()
        {
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblFormat = new System.Windows.Forms.Label();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.chkPassword = new System.Windows.Forms.CheckBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // lstFiles
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.ItemHeight = 16;
            this.lstFiles.Location = new System.Drawing.Point(12, 12);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(357, 180);
            this.lstFiles.TabIndex = 0;
            
            // btnAddFiles
            this.btnAddFiles.Location = new System.Drawing.Point(375, 12);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(110, 32);
            this.btnAddFiles.TabIndex = 1;
            this.btnAddFiles.Text = "Add Files";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            
            // btnAddFolder
            this.btnAddFolder.Location = new System.Drawing.Point(375, 50);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(110, 32);
            this.btnAddFolder.TabIndex = 2;
            this.btnAddFolder.Text = "Add Folder";
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            
            // btnRemove
            this.btnRemove.Location = new System.Drawing.Point(375, 88);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(110, 32);
            this.btnRemove.TabIndex = 3;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            
            // btnClear
            this.btnClear.Location = new System.Drawing.Point(375, 126);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(110, 32);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear All";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            
            // lblFormat
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(12, 208);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(98, 17);
            this.lblFormat.TabIndex = 5;
            this.lblFormat.Text = "Archive Format:";
            
            // cmbFormat
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(116, 205);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(121, 24);
            this.cmbFormat.TabIndex = 6;
            
            // chkPassword
            this.chkPassword.AutoSize = true;
            this.chkPassword.Location = new System.Drawing.Point(12, 240);
            this.chkPassword.Name = "chkPassword";
            this.chkPassword.Size = new System.Drawing.Size(147, 21);
            this.chkPassword.TabIndex = 7;
            this.chkPassword.Text = "Password Protect";
            this.chkPassword.UseVisualStyleBackColor = true;
            this.chkPassword.CheckedChanged += new System.EventHandler(this.chkPassword_CheckedChanged);
            
            // lblPassword
            this.lblPassword.AutoSize = true;
            this.lblPassword.Enabled = false;
            this.lblPassword.Location = new System.Drawing.Point(12, 272);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(69, 17);
            this.lblPassword.TabIndex = 8;
            this.lblPassword.Text = "Password:";
            
            // txtPassword
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(87, 269);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(150, 22);
            this.txtPassword.TabIndex = 9;
            
            // btnCreate
            this.btnCreate.Location = new System.Drawing.Point(285, 310);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(100, 35);
            this.btnCreate.TabIndex = 10;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            
            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(391, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            
            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 319);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(110, 17);
            this.lblStatus.TabIndex = 12;
            this.lblStatus.Text = "0 files selected";
            
            // CreateArchiveForm
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(503, 357);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.chkPassword);
            this.Controls.Add(this.cmbFormat);
            this.Controls.Add(this.lblFormat);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAddFolder);
            this.Controls.Add(this.btnAddFiles);
            this.Controls.Add(this.lstFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateArchiveForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Archive";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        #endregion
        
        private System.Windows.Forms.ListBox lstFiles;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnAddFolder;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.CheckBox chkPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
    }
}