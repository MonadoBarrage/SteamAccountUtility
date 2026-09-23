using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2.Authentication;

namespace SteamAccountUtility.Services;

using System;
using System.Threading.Tasks;

public class SteamKit2Authenticator : IAuthenticator
{
    private string? _code;

    public SteamKit2Authenticator()
    {
        WeakReferenceMessenger.Default.Register<SteamKit2Authenticator, SendLoginCodeMessage>(
            this,
            (authenticator, sentMessage) =>
            {
                authenticator._code = sentMessage.loginCode;
            }
        );
    }

    public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
    {
        if (previousCodeWasIncorrect)
        {
            Console.Error.WriteLine(
                "The previous 2-factor auth code you have provided is incorrect."
            );
        }
        WeakReferenceMessenger.Default.Send(new GoToSteamGuardCode());
        while (string.IsNullOrEmpty(_code))
            ;

        return Task.FromResult(_code!);
    }

    public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
    {
        if (previousCodeWasIncorrect)
        {
            Console.Error.WriteLine(
                "The previous 2-factor auth code you have provided is incorrect."
            );
        }
        WeakReferenceMessenger.Default.Send(new GoToSteamGuardCode());
        while (string.IsNullOrEmpty(_code))
            ;

        return Task.FromResult(_code!);
    }

    public Task<bool> AcceptDeviceConfirmationAsync()
    {
        Console.Error.WriteLine("STEAM GUARD! Use the Steam Mobile App to confirm your sign in...");

        WeakReferenceMessenger.Default.Send(
            new CurrentlyLoggingInMessage("Use the Steam Mobile App to confirm your sign in.")
        );
        return Task.FromResult(true);
    }
}
