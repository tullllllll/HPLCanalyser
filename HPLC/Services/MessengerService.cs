using System;

namespace HPLC.Services;

public class MessengerService
{
    public event Action<string> FileUploaded;

    public void SendMessage(string message)
    {
        FileUploaded?.Invoke(message);
    }
}