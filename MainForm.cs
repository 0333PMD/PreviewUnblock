using System;
using System.IO;
using System.Windows.Forms;

namespace PreviewUnblock
{
    /// <summary>
    /// Code-behind for the main form. Handles folder selection, start/stop
    /// monitoring and the core logic for unblocking PDF files by removing
    /// their Zone.Identifier alternate data streams.
    /// </summary>
    public partial class MainForm : Form
    {
        private FileSystemWatcher? watcher;
        private bool isMonitoring;
        private readonly object logLock = new();

        /// <summary>
        /// Initialize the form and set up the default folder.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            // Default to the user's Downloads folder. Fall back to user profile
            // if the Downloads folder cannot be determined.
            try
            {
                string downloads = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");
                textBoxFolder.Text = downloads;
            }
            catch
            {
                textBoxFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            // Show an initial status.
            labelStatus.Text = string.Empty;
        }

        /// <summary>
        /// Enable or disable the Start button based on the user's agreement.
        /// </summary>
        private void checkBoxAgree_CheckedChanged(object sender, EventArgs e)
        {
            buttonStartStop.Enabled = checkBoxAgree.Checked;
        }

        /// <summary>
        /// Allow the user to choose a different folder to monitor. If monitoring
        /// is already running, restarting with the new folder.
        /// </summary>
        private void buttonChangeFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select folder to monitor for PDF files.",
                UseDescriptionForTitle = true,
                SelectedPath = textBoxFolder.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFolder.Text = dialog.SelectedPath;
                if (isMonitoring)
                {
                    StopMonitoring();
                    StartMonitoring();
                }
            }
        }

        /// <summary>
        /// Toggle monitoring based on the current state.
        /// </summary>
        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            if (!isMonitoring)
            {
                StartMonitoring();
            }
            else
            {
                StopMonitoring();
            }
        }

        /// <summary>
        /// Start monitoring the selected folder: perform an initial scan and
        /// subscribe to file system events for future changes.
        /// </summary>
        private void StartMonitoring()
        {
            string folderPath = textBoxFolder.Text;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show($"Folder does not exist: {folderPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isMonitoring = true;
            buttonStartStop.Text = "Stop";
            labelStatus.Text = "Monitoring for new PDFs…";
            LogMessage("Started monitoring.");

            // Perform an initial scan of existing PDF files.
            ScanExistingPdfFiles(folderPath);

            // Set up a FileSystemWatcher for *.pdf files in the selected directory.
            watcher = new FileSystemWatcher(folderPath, "*.pdf")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += OnFileChanged;
            watcher.Changed += OnFileChanged;
            watcher.Renamed += OnFileRenamed;
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop monitoring by disposing of the watcher and updating the UI.
        /// </summary>
        private void StopMonitoring()
        {
            isMonitoring = false;
            buttonStartStop.Text = "Start";
            labelStatus.Text = "Monitoring stopped.";
            LogMessage("Stopped monitoring.");

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileChanged;
                watcher.Changed -= OnFileChanged;
                watcher.Renamed -= OnFileRenamed;
                watcher.Dispose();
                watcher = null;
            }
        }

        /// <summary>
        /// Scan the current folder for existing PDF files and process each one.
        /// </summary>
        private void ScanExistingPdfFiles(string folderPath)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folderPath, "*.pdf"))
                {
                    ProcessFile(file);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error scanning folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle file created/changed events by processing the file after a short delay.
        /// </summary>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!isMonitoring) return;
            // Process on a background task to avoid blocking the UI thread.
            System.Threading.Tasks.Task.Run(() =>
            {
                // Wait briefly to ensure the file is fully written to disk.
                System.Threading.Thread.Sleep(500);
                ProcessFile(e.FullPath);
            });
        }

        /// <summary>
        /// Handle file rename events by processing the new file name.
        /// </summary>
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (!isMonitoring) return;
            // Treat rename as creation of a new file.
            OnFileChanged(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath) ?? string.Empty, Path.GetFileName(e.FullPath)));
        }

        /// <summary>
        /// Attempt to remove the Zone.Identifier alternate data stream from the
        /// specified PDF file. Logs success or failure with a timestamp.
        /// </summary>
        private void ProcessFile(string filePath)
        {
            try
            {
                if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    return;

                // Only proceed if the file itself exists.
                if (!File.Exists(filePath))
                    return;

                string adsPath = filePath + ":Zone.Identifier";
                try
                {
                    // Attempt to delete the ADS. This will throw if the stream
                    // does not exist or cannot be accessed.
                    File.Delete(adsPath);
                    LogMessage($"Unblocked file: {Path.GetFileName(filePath)}");
                }
                catch (Exception ex2)
                {
                    LogMessage($"Skipped file: {Path.GetFileName(filePath)} ({ex2.Message})");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Skipped file: {Path.GetFileName(filePath)} ({ex.Message})");
            }
        }

        /// <summary>
        /// Append a timestamped message to the activity log on the UI thread.
        /// </summary>
        private void LogMessage(string message)
        {
            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }
            string time = DateTime.Now.ToString("hh:mm tt");
            richTextBoxLog.AppendText($"[{time}] {message}{Environment.NewLine}");
            richTextBoxLog.ScrollToCaret();
        }

        /// <summary>
        /// Prompt the user when closing the form if monitoring is still active.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isMonitoring)
            {
                var result = MessageBox.Show("Monitoring is running. Are you sure you want to exit?", "Confirm Exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }
    }
}