using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class TokenVault
{
    // Onde vamos salvar o token (ex: AppData do Windows)
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "PdvUpdater", 
        "auth.dat"
    );

    public static void SaveRefreshToken(string token)
    {
        // Garante que a pasta existe
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

        // Transforma o token em bytes
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

        // A MÁGICA DO DPAPI: Criptografa atrelando ao usuário do Windows atual
        byte[] encryptedBytes = ProtectedData.Protect(tokenBytes, null, DataProtectionScope.CurrentUser);

        // Salva o arquivo embaralhado no disco
        File.WriteAllBytes(FilePath, encryptedBytes);
    }

    public static string GetRefreshToken()
    {
        if (!File.Exists(FilePath))
            return null; // Retorna nulo para engatilhar a tela de "First Boot"

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(FilePath);
            
            // A MÁGICA REVERSA: Descriptografa (só funciona nesta máquina!)
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException)
        {
            // Se cair aqui, o arquivo foi corrompido ou movido para outro PC.
            return null; 
        }
    }
}