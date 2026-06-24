using System.Net.Http;
using Duende.IdentityModel.Client;
using SteamKit2;

namespace SteamAccountUtility.Models;

public class OpenIdVerifier
{
    public async void Login()
    {
        

        var client = new HttpClient();

        var response = await client.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = "https://demo.duendesoftware.com/connect/token",
                ClientId = "client",
                ClientSecret = "secret"
            });
        
        
    }
}