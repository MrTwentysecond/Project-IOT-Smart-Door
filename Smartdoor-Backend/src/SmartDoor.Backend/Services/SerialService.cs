using System.IO.Ports;

namespace SmartDoor.Backend.Services;

// Serial Arduino: baca "UID:xxxx", tulis "OPEN"/"DENY".
// Protokol: Arduino kirim "UID:xxxx\n" 9600 baud; app balas "OPEN\n"/"DENY\n".
public sealed class SerialService : IDisposable
{
    private SerialPort? _port;

    public event Action<string>? UidReceived;

    public void Start(string portName, int baudRate = 9600)
    {
        Stop();
        _port = new SerialPort(portName, baudRate)
        {
            NewLine = "\n",
            ReadTimeout = 500,
            WriteTimeout = 500,
        };
        _port.DataReceived += OnData;
        _port.Open();
    }

    public void Stop()
    {
        if (_port is null) return;
        try
        {
            _port.DataReceived -= OnData;
            if (_port.IsOpen) _port.Close();
            _port.Dispose();
        }
        catch { /* abaikan saat shutdown */ }
        _port = null;
    }

    // hasil DB = "diterima"/"ditolak"; ke Arduino dikirim "OPEN"/"DENY".
    public void Reply(string hasil)
    {
        if (_port is { IsOpen: true })
            _port.WriteLine(hasil == "diterima" ? "OPEN" : "DENY");
    }

    private void OnData(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var line = _port?.ReadLine().Trim();
            if (line is null) return;
            var uid = line.StartsWith("UID:", StringComparison.OrdinalIgnoreCase)
                ? line[4..].Trim()
                : line.Trim();
            if (uid.Length > 0) UidReceived?.Invoke(uid);
        }
        catch (TimeoutException) { /* baris belum lengkap, tunggu event berikut */ }
    }

    internal void Raise(string uid) => UidReceived?.Invoke(uid);

    public void Dispose() => Stop();
}
