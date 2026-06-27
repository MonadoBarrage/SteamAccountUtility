using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SteamAccountUtility;


/**
 * Note: Functions for fetching credentials are NOT secured or encrypted.
 * This is for testing purposes only and will not be in the final app
 *
 */

public class AppDirectory()
{
    string current_directory = Directory.GetCurrentDirectory();

    private string dataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "steamaccountutility", "userdata");
    
    private string credentialsFile = "credentials.json";
    



    public void SetUpNewDirectory()
    {
        var newCredFile = Path.Combine(dataDirectory, credentialsFile);
        Directory.CreateDirectory(dataDirectory);
        if (!File.Exists(newCredFile))
        {
            File.Create(newCredFile).Close();
        }
    }

    public void SaveCredentials(LoginDetails jsonDetails)
    {
        var storedCredFile = Path.Combine(dataDirectory, credentialsFile);
        
        string newJsonDetails = JsonSerializer.Serialize(jsonDetails);
        File.WriteAllText(storedCredFile, newJsonDetails);
    }

    public LoginDetails GetCredentials()
    {
        var storedCredFile = Path.Combine(dataDirectory, credentialsFile);
        LoginDetails details = new LoginDetails
        {
            Username = "",
            Password = "",
            SteamKey = "",
            GuardData = "",
            AccessToken = ""
        };
        
        try
        {
            string jsonDetails = File.ReadAllText(storedCredFile);
            if (!string.IsNullOrEmpty(jsonDetails))
                details = JsonSerializer.Deserialize<LoginDetails>(jsonDetails);
        }
        catch (Exception e)
        {
            details = new LoginDetails
            {
                Username = "",
                Password = "",
                SteamKey = "",
                GuardData = "",
                AccessToken = ""
            };
        }

        return details;
    }
    
    
}