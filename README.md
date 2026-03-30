# 🖥️ POS Updater Client - C# Desktop Application

> A lightweight, secure, and autonomous Windows application designed to keep Point of Sale (POS) systems updated seamlessly without disrupting cashier operations.

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Security](https://img.shields.io/badge/Security-DPAPI-red?style=for-the-badge)

## 📋 About the Project

This desktop client is the front-line component of the POS Updater ecosystem. Deployed directly on the cash register machines, it communicates with the [POS Updater NestJS API](link_do_repositorio_da_api_aqui) to fetch, verify, and install the latest versions of the main POS software. 

It was built with a strong focus on **Hardware-Bound Security** to ensure that the software can only run on explicitly authorized machines.

### ✨ Key Features

- **🛡️ Hardware-Bound Encryption (Windows DPAPI):** JWT Refresh Tokens and configuration files are encrypted at rest using `DataProtectionScope.LocalMachine`. If a malicious user copies the application folder to an unauthorized PC, the decryption fails automatically.
- **🔒 TLS 1.2 Enforced Security:** All network traffic to the NestJS backend is heavily encrypted in transit.
- **🔄 Autonomous Update Cycle:** Runs silently in the background. It authenticates, checks for version discrepancies, downloads the new executable, and registers the download with the central server.
- **🚀 Zero Cashier Friction:** Automates the replacement of the `.exe` file without requiring the cashier to click through installers or input daily credentials.

## 🏗️ Architecture & Workflow

1. **Hardware Fingerprinting:** Upon execution, the app reads unique machine identifiers to generate a `deviceId`.
2. **Authentication:** Uses the `deviceId` to log in via the NestJS Server.
3. **Secure Storage:** The server returns Access and Refresh Tokens, which the C# client encrypts using Windows DPAPI and stores locally as a `.dat` file.
4. **Polling / Checking:** The client securely requests the latest version info from the server.
5. **Download & Execution:** If outdated, it downloads the new `PdvFX.exe` via an octet-stream, replaces the old binary, and notifies the backend to update the company's Google Sheets dashboard.

## 🛠️ Tech Stack

- **Language:** C#
- **Framework:** .NET (Windows Forms / Console / Worker Service)
- **Security:** `System.Security.Cryptography.ProtectedData` (DPAPI), TLS 1.2 Protocol
- **Networking:** `HttpClient` for RESTful communication with the Node.js backend

## 🚀 Installation & Setup

### Requirements
- Windows OS (Windows 10 / 11 or Windows Server)
- .NET Runtime installed on the target machine

### Configuration
1. Clone the repository and open the solution in Visual Studio.
2. Update the API Base URL in the `App.config` or `appsettings.json` file to point to your deployed NestJS Server:
   ```xml
   <appSettings>
     <add key="ApiBaseUrl" value="[https://your-nestjs-api-domain.com/api](https://your-nestjs-api-domain.com/api)" />
   </appSettings>
   Build the solution in Release mode.

Deploy the generated .exe and associated .dll files to the target POS machine.

⚠️ Security Notice
Do not distribute the .dat token files across different machines. 
The DPAPI implementation strictly binds the payload to the physical hardware that generated it.