namespace SteamAccountUtility.Models;

public class ValidateDataFetch
{
    private bool isUserProfileFetched = false;
    private bool isGameLibraryFetched = false;
    private bool isFriendListFetched = false;
    private bool isBadgesAndLevelsFetched = false;
    private bool isRecentGamesFetched = false;
    
    public bool CheckUserProfile()
    {
        return true;
    }

    public bool CheckGameLibrary()
    {
        return true;
    }
    
    public bool CheckFriendList()
    {
        return true;
    }
    
    public bool CheckBadgesAndLevels()
    {
        
        return true;
    }

    public bool CheckRecentGames()
    {
        return true;
    }

    public bool IsAllDataFetched()
    {
        return isUserProfileFetched 
               && isGameLibraryFetched 
               && isFriendListFetched 
               && isBadgesAndLevelsFetched 
               && isRecentGamesFetched;
    }
    
    
}