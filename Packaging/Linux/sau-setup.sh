if [[ "$EUID" != 0 ]]; then
  echo "Must be root to run this script"  
  exit 1
fi

#echo "Creating required directories . . ."
#sudo mkdir -p /usr/local/lib/steamaccountutility
#echo "Created all needed directories"


#echo "Moving required libraries to /usr/lib/steamacccountutility . . ."
#sudo cp *.so  /usr/lib/steamaccountutility
#sudo ldconfig
#echo "Updated library links"

echo "Moving executable and libraries to /usr/local/bin/SteamAccountUtility . . ."
sudo mkdir -p /usr/local/bin/SteamAccountUtility
sudo cp *.so SteamAccountUtility /usr/local/bin/SteamAccountUtility
echo "Moved executable and libraries to /usr/local/bin/SteamAccountUtility"

echo "Adding desktop file to /usr/local/share/applications . . ."
sudo mkdir -p /usr/local/share/applications
chmod +x SteamAccountUtility.desktop
sudo cp SteamAccountUtility.desktop /usr/local/share/applications
echo "Copied desktop file"

echo "Adding icon file to /usr/local/share/icons . . ."
sudo mkdir -p /usr/local/share/icons
sudo cp sau-icon.png /usr/local/share/icons
echo "Added icon file"

