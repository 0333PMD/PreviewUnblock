# PreviewUnblock

**PreviewUnblock** is a lightweight Windows desktop utility that automatically removes the `Zone.Identifier` alternate data stream from PDF files.  When files are downloaded from the Internet, Windows tags them with this stream to indicate they came from an untrusted source.  Some PDF viewers refuse to preview such files.  PreviewUnblock monitors a folder (by default your **Downloads** directory) and unblocks new PDF files as soon as they arrive.

## Features

* **Automatic unblocking** – the app scans the selected folder and removes the `Zone.Identifier` stream from all `.pdf` files.
* **Real‑time monitoring** – using `FileSystemWatcher`, it watches for new or modified PDFs and processes them immediately.
* **Safe by default** – runs under the current user context with no administrative permissions required.
* **Activity log** – a scrolling log window shows a timestamped record of each file processed or skipped.
* **Change folder** – optionally choose any folder to monitor instead of the default Downloads folder.

## Usage

1. Launch the application.  The default folder (e.g. `C:\Users\&lt;user&gt;\Downloads`) will be displayed.
2. Optionally click **Change…** to select a different folder.
3. Read the warning banner and tick the checkbox to acknowledge that you understand the risks.
4. Click **Start** to begin monitoring.  The button will change to **Stop**, and the status bar will indicate that monitoring is active.  Existing PDFs in the folder will be processed immediately, and new PDFs will be handled as they appear.
5. Click **Stop** to halt monitoring.  If you attempt to exit while monitoring is active, the app will ask for confirmation.

Removing the `Zone.Identifier` stream means Windows will no longer warn you before opening the file.  Only enable this on folders where you trust the source of the files.

## Building

This project targets **.NET 6** and uses **Windows Forms**.  To build it yourself, install the .NET 6 SDK or later and run:

```bash
dotnet restore
dotnet build
```

### Publishing a stand‑alone executable

To create a single‑file, trimmed release suitable for distribution on 64‑bit Windows, use the following `dotnet publish` command:

```bash
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true
```

The output executable will be located in `bin/Release/net6.0-windows/win-x64/publish/PreviewUnblock.exe`.

## License

This project is provided as‑is without warranty.  Removing security flags from downloaded files can increase your exposure to malicious content.  Use at your own risk.