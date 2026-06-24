using System;
using System.Net.Http;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;

namespace SteamAccountUtility.Models;

public class SteamHTTP
{
    private static HttpClient steamClient = new HttpClient();

    public async Task ConnectToSteam()
    {
        var tokenRequest = new ClientCredentialsTokenRequest
        {
            Address = "https://steamcommunity.com/openid",
            ClientId = "client",
            ClientSecret = "secret"
        };
        
        var response = await steamClient.RequestClientCredentialsTokenAsync(tokenRequest);
        Console.WriteLine("Response:");
        Console.WriteLine(response.HttpResponse);
    }
    
}