using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using Tmds.DBus.Protocol;

namespace SteamAccountUtility.Views;

public partial class LoginWindowView : UserControl
{
    
    private Dictionary<string, string> friendList = new Dictionary<string, string>();
    private string profileName = "";
    
    
    public LoginWindowView()
    {
        InitializeComponent();
        
        
        
        WeakReferenceMessenger.Default.Register<LoginWindowView, ReceiveFriendsList>
        (this, static (win, mang) =>
        {
            win.friendList = mang.NewFriendsList;

            #if DEBUG
            Console.WriteLine("Received FriendsList:");
            foreach (var keyPair in win.friendList)
            {   
                Console.WriteLine($"{keyPair.Key}: {keyPair.Value}");
            }
            #endif
            if (win.friendList.Count > 0 && !string.IsNullOrEmpty(win.profileName))
            {
                WeakReferenceMessenger.Default.Send(new GoToHomePage(true));
            }
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowView, ReceiveProfileName>
        (this, static (win, mang) =>
        {
            win.profileName = mang.ProfileName;

            #if DEBUG
                Console.WriteLine("Received ProfileName:");
                Console.WriteLine(mang.ProfileName);
            #endif
            if (win.friendList.Count > 0 && !string.IsNullOrEmpty(win.profileName))
            {
                WeakReferenceMessenger.Default.Send(new GoToHomePage(true));
            }
            
            
        });
        
        // WeakReferenceMessenger.Default.Register<LoginWindowView, ReceiveGameList>
        // (this, static (win, mang) =>
        // {
        //     win.profileName = mang.ProfileName;
        //
        //     Console.WriteLine("Received ProfileName:");
        //     Console.WriteLine(mang.ProfileName);
        //     
        //     if (win.friendList.Count > 0 && !string.IsNullOrEmpty(win.profileName))
        //     {
        //         WeakReferenceMessenger.Default.Send(new GoToHomePage(true));
        //     }
        //     
        //     
        // });
        
    }
    
    
}