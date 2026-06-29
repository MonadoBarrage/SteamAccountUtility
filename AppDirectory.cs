using System;
using System.IO;
using System.Text.Json;

namespace SteamAccountUtility;


/**
 * Note: Functions for fetching credentials are NOT secured or encrypted.
 * This is for testing purposes only and will not be in the final app
 *
 */

public class AppDirectory
{
    // string current_directory = Directory.GetCurrentDirectory();
    
    private const string CredentialsFile = "credentials.json";
    private readonly string _dataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "steamaccountutility", "userdata");
    
    
    
    public void SetUpNewDirectory()
    {
        var newCredFile = Path.Combine(_dataDirectory, CredentialsFile);
        Directory.CreateDirectory(_dataDirectory);
        if (!File.Exists(newCredFile))
        {
            File.Create(newCredFile).Close();
        }
    }

    public void SaveCredentials(LoginDetails jsonDetails)
    {
        var storedCredFile = Path.Combine(_dataDirectory, CredentialsFile);
        
        string newJsonDetails = JsonSerializer.Serialize(jsonDetails);
        File.WriteAllText(storedCredFile, newJsonDetails);
    }

    public LoginDetails GetCredentials()
    {
        var storedCredFile = Path.Combine(_dataDirectory, CredentialsFile);
        LoginDetails? details = null;
        
        try
        {
            var jsonDetails = File.ReadAllText(storedCredFile);
            if (!string.IsNullOrEmpty(jsonDetails))
                details = JsonSerializer.Deserialize<LoginDetails>(jsonDetails);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new LoginDetails();
        }

        
        return details ?? new LoginDetails();

    }
    
    
}