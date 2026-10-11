namespace SmartDoor.Backend.Services;

// ponytail: ceiling = stub tanpa System.IO.Ports; upgrade path = SerialPort baca "UID:xxxx", tulis "OPEN"/"DENY".
public sealed class SerialService
{
    public event Action<string>? UidReceived;

    public void Start(string portName, int baudRate = 9600) => throw new NotImplementedException(); // TODO
    public void Stop() { } // TODO
    public void Reply(string hasil) { } // TODO: tulis OPEN/DENY + "\n"
    internal void Raise(string uid) => UidReceived?.Invoke(uid);
}
