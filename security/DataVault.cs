using Newtonsoft.Json;
using PdvUpdater.model;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class DataVault
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "PdvUpdater", 
        "data.dat"
    );

    public static void SaveData(VaultData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

        string json = JsonConvert.SerializeObject(data);

        byte[] dataBytes = Encoding.UTF8.GetBytes(json);

        byte[] encryptedBytes = ProtectedData.Protect(dataBytes, null, DataProtectionScope.CurrentUser);

        File.WriteAllBytes(FilePath, encryptedBytes);
    }

    public static VaultData GetData()
    {
        if (!File.Exists(FilePath))
            return null;

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(FilePath);
            
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            string json = Encoding.UTF8.GetString(decryptedBytes);

            return JsonConvert.DeserializeObject<VaultData>(json);
        }
        catch (CryptographicException)
        {
            return null; 
        }
    }

    public static void UpdateRefreshToken(string newToken)
    {

        VaultData vault = GetData();

        if (vault != null)
        {
            vault.RefreshToken = newToken;
            SaveData(vault);
        }
    }
}