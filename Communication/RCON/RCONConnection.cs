namespace WibboEmulator.Communication.RCON;
using System.Net.Sockets;
using System.Text;
using WibboEmulator.Core;

public class RCONConnection
{
    private byte[] _buffer = new byte[1024];
    private Socket _socket;

    private readonly Encoding _encoding = Encoding.UTF8;

    public RCONConnection(Socket socket)
    {
        this._socket = socket;
        try
        {
            _ = this._socket.BeginReceive(this._buffer, 0, this._buffer.Length, SocketFlags.None, new AsyncCallback(this.OnCallBack), this._socket);
        }
        catch
        {
            this.Dispose();
        }
    }

    public void OnCallBack(IAsyncResult iAr)
    {
        try
        {
            if (!int.TryParse(this._socket.EndReceive(iAr).ToString(), out var bytes))
            {
                this.Dispose();
                return;
            }

            var data = this._encoding.GetString(this._buffer, 0, bytes);

            if (!WibboEnvironment.GetRCONSocket().GetCommands().Parse(data))
            {
                ExceptionLogger.WriteLine("Failed to execute a MUS command. Raw data: " + data);
            }
        }
        catch (Exception ex)
        {
            ExceptionLogger.LogException("Erreur mus: " + ex);
        }

        this.Dispose();
    }

    public void Dispose()
    {
        try
        {
            if (this._socket != null)
            {
                this._socket.Shutdown(SocketShutdown.Both);
                this._socket.Close();
                this._socket.Dispose();
            }
        }
        catch
        {
        }
        this._socket = null;
        this._buffer = null;
    }
}
