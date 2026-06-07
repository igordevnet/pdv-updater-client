# POS Updater Client

A Windows desktop updater for POS/PDV executables. It registers a device with the POS Updater API, stores refresh credentials locally with Windows DPAPI, checks for new `PdvFX` or `DotMart` versions, downloads updates safely, keeps backups, and restarts the POS when it is safe to do so.

## 🏗️ Architecture

This client is the workstation-side piece of the POS updater system. The server owns authentication, version metadata, executable downloads, and update audit logging; the client handles local setup, token refresh, version comparison, file replacement, rollback, and process restart.

* **Runtime:** .NET Framework 4.8 Windows executable
* **Networking:** `HttpClient` against the NestJS POS Updater API
* **Configuration:** `appsettings.json` loaded from the application directory
* **Security:** Windows DPAPI using `DataProtectionScope.CurrentUser`
* **Storage:** `%LOCALAPPDATA%\PdvUpdater\data.dat` for encrypted device/session data
* **Updates:** File version comparison through Windows executable metadata
* **Recovery:** Local executable backups and forced rollback support
* **Logging:** Rotating text logs in the application directory

## 🔄 Main Flow

1. The updater starts with a global mutex so only one instance runs.
2. On first run, it opens a console setup flow and asks the operator to choose `PdvFX` or `DotMart`.
3. The operator enters company name, password, and device name.
4. The client generates a device ID, signs in with the API, and stores the refresh token/device metadata in encrypted `data.dat`.
5. On later runs, the client refreshes the access token silently.
6. It calls `/updates/check` and compares the server version with the local executable version.
7. If an update is needed, it downloads the new executable to a temporary file.
8. It validates the downloaded file has version metadata, backs up the current executable, replaces it, and calls `/updates/save`.
9. It launches the selected executable if it is not already running.
10. It keeps polling every 10 minutes for future updates.

## 🧩 Supported Executables

Valid executable types are:

* `PdvFX`
* `DotMart`

The updater expects the selected executable to live beside the updater binaries:

```text
ApplicationFolder/
  pdv_updater_client.exe
  appsettings.json
  PdvFX.exe
  DotMart.exe
```

The executable files are deployment artifacts and must not be committed to Git.

## 🚀 Getting Started

### Prerequisites

* Windows 10/11 or Windows Server
* .NET Framework 4.8 runtime on target machines
* Visual Studio or MSBuild/.NET tooling for development builds
* Access to the POS Updater API

### Configuration

Create `appsettings.json` from the example file:

```json
{
  "ApiConfig": {
    "BaseUrl": "http://localhost:3000"
  },
  "CNPJ": "store-cnpj"
}
```

| Key | Description |
| :--- | :--- |
| `ApiConfig:BaseUrl` | Base URL of the POS Updater API. |
| `CNPJ` | Store/company CNPJ used to validate forced updates. |

`appsettings.json` is ignored by Git because it can contain environment-specific values.

### Build

```powershell
dotnet restore
dotnet build -c Release
```

For .NET Framework 4.8 builds, Visual Studio or the .NET Framework targeting pack may be required.

### First Run Setup

On the first run, no encrypted vault exists yet. The updater will:

1. Show a console selector for `PdvFX` or `DotMart`.
2. Ask for company name, password, and device name.
3. Send a login request to the API.
4. Save the refresh token, generated device ID, device name, and selected executable type into `%LOCALAPPDATA%\PdvUpdater\data.dat`.

After setup, the updater can run silently.

## 📡 API Contract

The client talks to these server endpoints:

| Method | Endpoint | Purpose |
| :--- | :--- | :--- |
| `POST` | `/auth/local/signin` | First-run authentication and refresh-token creation. |
| `POST` | `/auth/refresh` | Silent access-token refresh. |
| `GET` | `/updates/check` | Returns latest version and optional force-update metadata. |
| `GET` | `/updates/download` | Streams the selected executable. |
| `POST` | `/updates/save` | Notifies the server after install or rollback. |

The client sends `exeType` as either `PdvFX` or `DotMart`.

## 🧠 Update Behavior

### Normal Update

* Reads the local executable version with `FileVersionInfo`.
* Compares it with the version returned by `/updates/check`.
* Downloads the update to `PdvFX.temp` or `DotMart.temp`.
* Validates that the downloaded file exists, is non-empty, and exposes version metadata.
* Backs up the current executable before replacing it.
* Notifies the server with the installed version.

### Forced Update / Rollback

When the server returns `force = true` and the payload CNPJ matches local config:

* If server version is newer, the client updates normally.
* If server version is older, the client attempts rollback from local backups.
* Rollback looks for a backup whose filename contains the last version segment.
* After rollback, the client notifies the server with the target version.

### Restart Logic

For `PdvFX`, after a non-boot update, the client checks the latest `NFCe*.ini` file and reads `Estado`.

* `Estado = 2`: PDV is considered free and can be restarted.
* Any other status: restart is postponed.

`DotMart` does not use the PDV restart flow.

## 💾 Backup And Recovery

Before replacing an executable, the client moves the current file to a versioned backup name:

```text
PdvFX52.exe
DotMart52.exe
```

The backup manager keeps the latest 5 backups for the selected executable type.

If the main executable is missing at startup, the client tries to restore the newest backup automatically.

## 🔐 Security

* TLS 1.2 is enforced for API communication.
* Refresh tokens are encrypted locally with Windows DPAPI `CurrentUser` scope.
* The vault file is stored at `%LOCALAPPDATA%\PdvUpdater\data.dat`.
* Access tokens are kept in memory and refreshed silently.
* `appsettings.json`, executables, DLLs, PDBs, icons, logs, and build outputs should not be committed.

## 🔭 Observability

Logs are written beside the updater executable:

| File | Purpose |
| :--- | :--- |
| `updater_logs.txt` | Current updater log file. |
| `updater_logs_old.txt` | Rotated previous log file. |

Logs rotate when the current file reaches 5 MB.

## 🧪 Testing

There is no dedicated automated test project in this repository yet. For now, validate changes with:

```powershell
dotnet build -c Release
```

Recommended manual checks:

* First-run setup creates encrypted `data.dat`.
* Refresh-token flow works after restarting the updater.
* `/updates/check` detects newer versions.
* Downloaded executables replace the old file only after validation.
* Backup restoration works when the main executable is missing.
* Forced rollback works for matching CNPJ/version data.

## ⚙️ Git Safety

Do not commit deployment artifacts or customer-specific runtime files:

* `PdvFX.exe`
* `DotMart.exe`
* `*.dll`
* `*.pdb`
* `*.ico`
* `appsettings.json`
* `data.dat`
* build folders such as `bin/` and `obj/`
* generated cache files such as `*.lscache`