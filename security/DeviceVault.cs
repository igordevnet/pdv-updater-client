using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class DeviceVault
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "PdvUpdater", 
        "device.dat"
    );

    public static void SaveDeviceName(string deviceName)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

        byte[] deviceBytes = Encoding.UTF8.GetBytes(deviceName);

        byte[] encryptedBytes = ProtectedData.Protect(deviceBytes, null, DataProtectionScope.CurrentUser);

        File.WriteAllBytes(FilePath, encryptedBytes);
    }

    public static string GetDeviceName()
    {
        if (!File.Exists(FilePath))
            return null;

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(FilePath);
            
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException)
        {
            return null; 
        }
    }
}