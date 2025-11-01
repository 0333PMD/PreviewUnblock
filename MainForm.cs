using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const int FILE_WRITE_DELAY_MS = 500;
        private const int MAX_LOG_LINES = 1000;
        private const int WATCHER_BUFFER_SIZE = 65536; // 64KB

        private FileSystemWatcher? watcher;
        private volatile bool isMonitoring;
        private readonly object syncLock = new();
        private readonly HashSet<string> recentlyProcessed = new();
        private int filesProcessed = 0;
        private int filesFailed = 0;

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
            UpdateStatus();
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
        /// is already running, restart with the new folder.
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
                if (!IsValidPath(dialog.SelectedPath))
                {
                    MessageBox.Show("Invalid folder path selected.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                textBoxFolder.Text = dialog.SelectedPath;
                if (isMonitoring)
                {
                    StopMonitoring();
                    StartMonitoring();
                }
            }
        }

        /// <summary>
        /// Validate that a path is safe and accessible.
        /// </summary>
        private bool IsValidPath(string path)
        {
            try
            {
                return Path.IsPathFullyQualified(path) && 
                       !path.Contains("..") &&
                       Directory.Exists(path);
            }
            catch
            {
                return false;
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
                MessageBox.Show($"Folder does not exist: {folderPath}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isMonitoring = true;
            filesProcessed = 0;
            filesFailed = 0;
            buttonStartStop.Text = "Stop";
            UpdateStatus();
            LogMessage("Started monitoring.");

            // Perform an initial scan of existing PDF files.
            ScanExistingPdfFiles(folderPath);

            // Set up a FileSystemWatcher for *.pdf files in the selected directory.
            watcher = new FileSystemWatcher(folderPath, "*.pdf")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                InternalBufferSize = WATCHER_BUFFER_SIZE,
                IncludeSubdirectories = false
            };

            watcher.Created += OnFileChanged;
            watcher.Changed += OnFileChanged;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnWatcherError;
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop monitoring by disposing of the watcher and updating the UI.
        /// </summary>
        private void StopMonitoring()
        {
            isMonitoring = false;
            buttonStartStop.Text = "Start";
            UpdateStatus();
            LogMessage("Stopped monitoring.");

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileChanged;
                watcher.Changed -= OnFileChanged;
                watcher.Renamed -= OnFileRenamed;
                watcher.Error -= OnWatcherError;
                watcher.Dispose();
                watcher = null;
            }

            lock (syncLock)
            {
                recentlyProcessed.Clear();
            }
        }

        /// <summary>
        /// Scan the current folder for existing PDF files and process each one in parallel.
        /// </summary>
        private void ScanExistingPdfFiles(string folderPath)
        {
            try
            {
                var files = Directory.EnumerateFiles(folderPath, "*.pdf").ToList();
                if (files.Count == 0)
                {
                    LogMessage("No existing PDF files found.");
                    return;
                }

                LogMessage($"Scanning {files.Count} existing PDF files...");

                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, 
                    file => ProcessFile(file));

                LogMessage("Initial scan complete.");
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
            Task.Run(() =>
            {
                // Wait briefly to ensure the file is fully written to disk.
                if (WaitForFileReady(e.FullPath))
                {
                    ProcessFile(e.FullPath);
                }
            });
        }

        /// <summary>
        /// Handle file rename events by processing the new file name.
        /// </summary>
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (!isMonitoring) return;
            OnFileChanged(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, 
                Path.GetDirectoryName(e.FullPath) ?? string.Empty, 
                Path.GetFileName(e.FullPath)));
        }

        /// <summary>
        /// Handle FileSystemWatcher errors (e.g., buffer overflow).
        /// </summary>
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            LogMessage($"⚠️ Watcher error: {e.GetException()?.Message ?? "Unknown error"}");
            LogMessage("Some file events may have been missed. Consider monitoring a smaller folder.");
        }

        /// <summary>
        /// Wait for a file to be ready for reading (not locked by another process).
        /// </summary>
        private bool WaitForFileReady(string path, int maxAttempts = 3)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                    return true;
                }
                catch (IOException)
                {
                    if (i < maxAttempts - 1) 
                        Thread.Sleep(FILE_WRITE_DELAY_MS);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempt to remove the Zone.Identifier alternate data stream from the
        /// specified PDF file. Logs success or failure with a timestamp.
        /// Includes deduplication to avoid processing the same file multiple times.
        /// </summary>
        private void ProcessFile(string filePath)
        {
            try
            {
                if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    return;

                // Deduplication check
                lock (syncLock)
                {
                    if (!recentlyProcessed.Add(filePath))
                        return; // Already processed recently

                    // Schedule removal from cache after 5 seconds
                    System.Threading.Timer cleanupTimer = new System.Threading.Timer(_ =>
			{
				lock (syncLock)
    				{
				        recentlyProcessed.Remove(filePath);
				}
			}, null, 5000, Timeout.Infinite);
                }

                if (!File.Exists(filePath))
                    return;

                string adsPath = filePath + ":Zone.Identifier";
                
                try
                {
                    File.Delete(adsPath);
                    Interlocked.Increment(ref filesProcessed);
                    LogMessage($"✓ Unblocked: {Path.GetFileName(filePath)}");
                    UpdateStatus();
                }
                catch (FileNotFoundException)
                {
                    // ADS doesn't exist - file was never blocked, this is normal
                    LogMessage($"→ Already unblocked: {Path.GetFileName(filePath)}");
                }
                catch (UnauthorizedAccessException)
                {
                    Interlocked.Increment(ref filesFailed);
                    LogMessage($"✗ Access denied: {Path.GetFileName(filePath)}");
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref filesFailed);
                    LogMessage($"✗ Failed: {Path.GetFileName(filePath)} ({ex.Message})");
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Error processing: {Path.GetFileName(filePath)} ({ex.Message})");
            }
        }

        /// <summary>
        /// Update the status label with current statistics.
        /// </summary>
        private void UpdateStatus()
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.Invoke(new Action(UpdateStatus));
                return;
            }

            if (isMonitoring)
            {
                labelStatus.Text = $"Monitoring active — {filesProcessed} unblocked, {filesFailed} failed";
            }
            else
            {
                if (filesProcessed > 0 || filesFailed > 0)
                {
                    labelStatus.Text = $"Monitoring stopped — {filesProcessed} unblocked, {filesFailed} failed";
                }
                else
                {
                    labelStatus.Text = "Ready to start monitoring";
                }
            }
        }

        /// <summary>
        /// Append a timestamped message to the activity log on the UI thread.
        /// Maintains a maximum number of log lines to prevent performance degradation.
        /// </summary>
        private void LogMessage(string message)
        {
            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            richTextBoxLog.AppendText($"[{time}] {message}{Environment.NewLine}");

            // Limit log size to prevent performance issues
            if (richTextBoxLog.Lines.Length > MAX_LOG_LINES)
            {
                var lines = richTextBoxLog.Lines;
                var trimmedLines = lines.Skip(lines.Length - MAX_LOG_LINES).ToArray();
                richTextBoxLog.Lines = trimmedLines;
            }

            richTextBoxLog.ScrollToCaret();
        }

        /// <summary>
        /// Prompt the user when closing the form if monitoring is still active.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isMonitoring)
            {
                var result = MessageBox.Show(
                    "Monitoring is running. Are you sure you want to exit?", 
                    "Confirm Exit",
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
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