using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2;
using Tmds.DBus.Protocol;

namespace SteamAccountUtility.Views;

public partial class LoginWindowView : UserControl
{
    
    private Dictionary<SteamID, FriendData> friendList;
    private UserData userData;
    private List<Game> gamesList;
    
    
    public LoginWindowView()
    {
        InitializeComponent();
        
        friendList = new Dictionary<SteamID, FriendData>();
        userData = new UserData();
        gamesList = new List<Game>();
        
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
            
            win.CheckIfAllDataFetched();
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowView, ReceiveUserData>
        (this, static (win, mang) =>
        {
            win.userData = mang.User;

            #if DEBUG
                Console.WriteLine("Received ProfileName:");
                Console.WriteLine(mang.User.ProfileName);
            #endif
            win.CheckIfAllDataFetched();
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowView, ReceiveGameList>
        (this, static (win, mang) =>
        {
            win.gamesList = mang.NewGameList;
            
            #if DEBUG
                Console.WriteLine("Received GameList:");
                Console.WriteLine("Received FriendsList:");
                foreach (var g in win.gamesList)
                {   
                    Console.WriteLine(g.Name);
                }
            #endif
            win.CheckIfAllDataFetched();
            
        });
        
    }


    public void CheckIfAllDataFetched()
    {
        if (friendList.Count > 0 && gamesList.Count > 0 && userData.validateData())
        {
            WeakReferenceMessenger.Default.Send(new GoToHomePage(true, userData, gamesList, friendList));
        }
    }
    
}