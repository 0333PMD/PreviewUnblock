\# PreviewUnblock - Version 1.1 Release Notes



\## Overview

Version 1.1 represents a significant reliability and security update based on comprehensive code review. This release improves handling of edge cases, enhances performance for large folders, and provides better visibility into processing activity. Also, basic Maintenance & Environment Modernization improvements were made.



---



\## Security Improvements



\### Path Validation

\- \*\*Added:\*\* `IsValidPath()` method to validate folder selections

\- \*\*Protection against:\*\* Path traversal attacks and invalid directory paths

\- \*\*Implementation:\*\* Validates paths are fully qualified, don't contain "..", and exist before processing



\### Enhanced Exception Handling

\- \*\*Changed:\*\* Specific exception catching instead of generic catch-all

\- \*\*Added:\*\* Separate handling for `FileNotFoundException`, `UnauthorizedAccessException`

\- \*\*Benefit:\*\* Better security logging and prevents masking of permission issues



---



\## Reliability Enhancements



\### FileSystemWatcher Hardening

\- \*\*Increased:\*\* Internal buffer size from 8KB (default) to 64KB

\- \*\*Added:\*\* `OnWatcherError` event handler to detect buffer overflow

\- \*\*Added:\*\* Explicit `IncludeSubdirectories = false` setting

\- \*\*Benefit:\*\* Prevents event loss when many files are created rapidly



\### Thread Safety

\- \*\*Changed:\*\* `isMonitoring` flag now uses `volatile` keyword

\- \*\*Changed:\*\* Renamed `logLock` to `syncLock` and actually use it for synchronization

\- \*\*Added:\*\* Thread-safe counters using `Interlocked.Increment()`

\- \*\*Benefit:\*\* Eliminates race conditions in multi-threaded scenarios



\### Duplicate Processing Prevention

\- \*\*Added:\*\* `HashSet<string>` deduplication cache with auto-cleanup

\- \*\*Implementation:\*\* Tracks recently processed files for 5 seconds

\- \*\*Benefit:\*\* Prevents processing same file multiple times when both Created and Changed events fire



\### Smart File Access Retry

\- \*\*Replaced:\*\* Simple 500ms sleep with intelligent `WaitForFileReady()` method

\- \*\*Added:\*\* Retry logic (3 attempts) that checks if file is actually ready

\- \*\*Added:\*\* Proper file locking detection using `FileShare.None`

\- \*\*Benefit:\*\* More reliable processing of files still being written by other processes


---



\## Performance Optimizations



\### Parallel Initial Scan

\- \*\*Changed:\*\* Sequential scan replaced with parallel processing

\- \*\*Configuration:\*\* Maximum 4 concurrent threads

\- \*\*Benefit:\*\* Dramatically faster processing of folders with many existing PDFs



\### Log Performance

\- \*\*Added:\*\* Maximum log line limit (1,000 lines)

\- \*\*Implementation:\*\* Auto-truncates oldest entries when limit exceeded

\- \*\*Benefit:\*\* Prevents UI slowdown during long monitoring sessions with high activity



\### Reduced Redundancy

\- \*\*Optimized:\*\* Removed redundant file existence checks in hot paths

\- \*\*Optimized:\*\* Single-pass log line truncation using LINQ



---



\## User Experience Improvements



\### Real-Time Statistics

\- \*\*Added:\*\* Live counter display in status bar

\- \*\*Shows:\*\* "X unblocked, Y failed" during and after monitoring

\- \*\*Updates:\*\* In real-time as files are processed



\### Enhanced Warning Message

\- \*\*Added:\*\* ⚠️ emoji visual indicator

\- \*\*Improved:\*\* More direct language emphasizing personal trust requirement

\- \*\*Text:\*\* "Only use on folders where you personally downloaded and trust EVERY file"



\### Better Log Messages

\- \*\*Added:\*\* Visual indicators for different outcomes:

&nbsp; - ✓ Successfully unblocked

&nbsp; - → Already unblocked (no ADS present)

&nbsp; - ✗ Failed with reason

&nbsp; - ⚠️ Watcher errors

\- \*\*Changed:\*\* Timestamp format to ISO-style `yyyy-MM-dd HH:mm:ss`

\- \*\*Added:\*\* "Initial scan complete" confirmation message

\- \*\*Added:\*\* File count logging before scan begins



\### Improved Log Readability

\- \*\*Changed:\*\* Font from default to `Consolas 9pt` monospace

\- \*\*Benefit:\*\* Better alignment of timestamps and consistent visual appearance



---



\## Code Quality Improvements



\### Constants

\- \*\*Added:\*\* Named constants replacing magic numbers:

&nbsp; ```csharp

&nbsp; FILE\_WRITE\_DELAY\_MS = 500

&nbsp; MAX\_LOG\_LINES = 1000

&nbsp; WATCHER\_BUFFER\_SIZE = 65536

&nbsp; ```



\### Simplified Exception Handling

\- \*\*Removed:\*\* Nested try-catch blocks in `ProcessFile()`

\- \*\*Simplified:\*\* Single exception handling with specific catch blocks

\- \*\*Improved:\*\* More descriptive error messages



\### New Methods

\- \*\*Added:\*\* `IsValidPath(string path)` - Path validation

\- \*\*Added:\*\* `WaitForFileReady(string path, int maxAttempts)` - Smart file access retry

\- \*\*Added:\*\* `UpdateStatus()` - Centralized status bar updates

\- \*\*Added:\*\* `OnWatcherError(object sender, ErrorEventArgs e)` - Error handler



\### Better State Management

\- \*\*Added:\*\* Tracking fields:

&nbsp; ```csharp

&nbsp; private int filesProcessed = 0;

&nbsp; private int filesFailed = 0;

&nbsp; private readonly HashSet<string> recentlyProcessed = new();

&nbsp; ```

\- \*\*Added:\*\* State reset when starting monitoring

\- \*\*Added:\*\* Deduplication cache clearing when stopping


## Key Maintenance & Environment Modernization Changes
- **Framework Upgrade:** Migrated from `.NET 6` → `.NET 8 (Windows 10 SDK 22621)`
- **Dependency Simplification:** Removed deprecated `Microsoft.Windows.SDK.NET.Ref` package  
- **Code Fix:** Resolved ambiguous reference between `System.Windows.Forms.Timer` and `System.Threading.Timer`  
- **Build Configuration:** Disabled trimming (`/p:PublishTrimmed=false`) to ensure WinForms compatibility  
- **Packaging:** Now builds as a **single self-contained `.exe`** for direct deployment  
- **Compatibility:** Verified on Windows 10 and newer 


---



\## Migration Notes



\### Breaking Changes

\*\*None\*\* - Version 1.1 is fully backward compatible with 1.0



\### Configuration Changes

\*\*None\*\* - All improvements are internal



\### Data Format Changes

\*\*None\*\* - No changes to how files are processed or what data is stored



---



\## Technical Details



\### Modified Files

\- `MainForm.cs` - Core logic improvements

\- `MainForm.Designer.cs` - UI text and font updates



\### Unmodified Files

\- `Program.cs` - No changes

\- `PreviewUnblock.csproj` - No changes

\- `app.manifest` - No changes

\- `README.md` - Should be updated with new features



\### Lines Changed

\- \*\*MainForm.cs:\*\* ~180 lines modified/added

\- \*\*MainForm.Designer.cs:\*\* ~15 lines modified



---



\## Bugs Fixed



1\. \*\*FileSystemWatcher event loss\*\* - Buffer overflow in high-volume scenarios

2\. \*\*Duplicate processing\*\* - Same file processed multiple times

3\. \*\*Race condition\*\* - File accessed before write complete

4\. \*\*UI performance\*\* - Log window slowdown with thousands of entries

5\. \*\*Thread safety\*\* - Potential data races on shared boolean flag

6\. \*\*Poor error visibility\*\* - Generic error messages didn't distinguish between error types



---



\## Future Considerations



While not implemented in v1.1, the following were considered for future versions:



\- Digital code signing (deferred - internal use only)

\- Log export functionality

\- Subdirectory monitoring option

\- Configurable file type filters beyond PDF

\- Pause/resume functionality

\- System tray minimization



---



\## Testing Recommendations



Before deploying v1.1 internally, test these scenarios:



1\. \*\*Large folder scan\*\* - Folder with 100+ PDFs

2\. \*\*Rapid file creation\*\* - Download multiple PDFs simultaneously

3\. \*\*File in use\*\* - Process PDF while it's open in a reader

4\. \*\*Permission denied\*\* - Monitor folder with mixed permissions

5\. \*\*Long running\*\* - Leave monitoring active for extended period

6\. \*\*Restart during monitoring\*\* - Change folder while actively monitoring



---



\## Version History



\- \*\*v1.0\*\* (Initial Release) - Basic monitoring and unblocking functionality

\- \*\*v1.1\*\* (Current) - Security, reliability, and performance improvements



---



\## Credits



\- Initial release: btec, OpenAi agent mode (GPT5)

\- v1.1 improvements: Based on comprehensive code audit and security review by btec, Claude Sonnet 4.5

