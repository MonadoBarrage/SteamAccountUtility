using Avalonia.Media.Imaging;

namespace SteamAccountUtility.Messages;

public class UpdateQrCode(Bitmap? qrCode)
{
    public Bitmap? _steamQrCode = qrCode;
}
