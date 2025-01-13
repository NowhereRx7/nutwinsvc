using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace NutClient;

/// <summary>
/// A client for communicating with a NUT server.
/// </summary>
public class NutClient : IDisposable
{
    private readonly string host;
    private readonly string upsName;
    private readonly int port;
    private readonly string username;
    private readonly string password;
    private readonly bool useTls;
    private readonly TcpClient client = new() { SendTimeout = 10000, ReceiveTimeout = 10000 };
    private Stream stream = Stream.Null;

    public string Host => host;

    public int Port => port;

    public string UpsName => upsName;

    public bool Connected => client.Connected;

    public bool UsingTls => stream is SslStream;

    //public string ServerVersion { get; private set; } = string.Empty;

    //public string ProtocolVersion { get; private set; } = string.Empty;

    public event EventHandler? ClientDisconnected;
    protected virtual void OnClientDisconnected()
    {
        ClientDisconnected?.Invoke(this, EventArgs.Empty);
    }

    public NutClient(string host, string upsName, string username, string password, bool useTls = false, int port = 3493)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(upsName);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        this.host = host;
        this.upsName = upsName;
        this.port = port;
        this.useTls = useTls;
        this.username = username;
        this.password = password;
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ConnectAsync(cancellationToken);
        await TryStartTlsAsync(cancellationToken);
        await AuthenticateAsync(cancellationToken);
    }

    public async Task<CommandResult> ExecuteCommandAsync(Command command) => await ExecuteCommandAsync(command, [], CancellationToken.None);
    public async Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken) => await ExecuteCommandAsync(command, [], cancellationToken);
    public async Task<CommandResult> ExecuteCommandAsync(Command command, string[] args, CancellationToken cancellationToken)
    {
        if (!Connected)
            throw new NutException("Client is not connected!");
        StringBuilder cmd = new();
        cmd.Append(command.ToString());
        if (command == Command.GET || command == Command.LIST)
            if (args.Length < 1) throw new ArgumentException($"{command} requires arguments!");
            else if (command == Command.SET)
                if (args.Length < 2) throw new ArgumentException($"{command} requires arguments!");
        foreach (string arg in args)
            cmd.Append(" " + arg);
        await WriteStreamAsync(cmd.ToString());
        string[]? response = await GetResponseAsync(command, cancellationToken);
        if (response == null || response.Length == 0)
            throw new NutException("Response to command was null.");
        if (response[0].StartsWith("ERR"))
        {
            string[] a = response[0].Split(" ", 3);
            if (a.Length < 2)
                return new CommandResult(command, Error.UNKNOWN, response[0]);
            return new CommandResult(command, a[1].ToError(), response[2] ?? null);
        }
        switch (command)
        {
            case Command.LOGOUT:
                if (response[0].StartsWith("OK") || response[0].StartsWith("Goodbye"))
                    return new CommandResult(command);
                else
                    return new CommandResult(command, Error.UNKNOWN, response);
            case Command.FSD:
            case Command.INSTCMD:
            case Command.STARTTLS:
            case Command.LOGIN:
            case Command.PASSWORD:
            case Command.PRIMARY:
            case Command.MASTER:
            case Command.SET:
                if (response[0].StartsWith("OK"))
                {
                    string extra = response[0].Substring(2).Trim();
                    return new CommandResult(command, String.IsNullOrWhiteSpace(extra) ? null : extra);
                }
                else
                    return new CommandResult(command, Error.UNKNOWN, response);
            case Command.GET:
                throw new NotImplementedException();
            case Command.LIST:
                throw new NotImplementedException();
            default:
                break;
        }
        return new CommandResult(command, response.Length == 1 ? response[0] : response);
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Host);
            try
            {
                using CancellationTokenSource tcts = new(TimeSpan.FromSeconds(10));
                CancellationToken timeoutToken = tcts.Token;
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);
                await client.ConnectAsync(Host, Port, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
            if (!client.Connected)
                throw new TimeoutException();
            stream = client.GetStream();
        }
        catch (Exception ex)
        {
            throw new NutException("Could not connect!", ex);
        }
    }

    private async Task TryStartTlsAsync(CancellationToken cancellationToken)
    {
        if (!useTls) return;
        CommandResult result = await ExecuteCommandAsync(Command.STARTTLS, cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        SslStream sslStream = new(client.GetStream(), true);
        try
        {
            SslClientAuthenticationOptions authOptions = new() { TargetHost = Host };
            await sslStream.AuthenticateAsClientAsync(authOptions, cancellationToken);
            stream = sslStream;
        }
        catch { }
    }

    private async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        CommandResult result = await ExecuteCommandAsync(Command.LOGIN, [UpsName], cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        result = await ExecuteCommandAsync(Command.USERNAME, [username], cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        result = await ExecuteCommandAsync(Command.PASSWORD, [password], cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        //result = await ExecuteCommand(Command.VER, cancellationToken);
        //if (result.Error == Error.None)
        //    ServerVersion = result.Data as string ?? string.Empty;
    }

    private async Task WriteStreamAsync(string command)
    {
        if (!client.Connected)
            throw new NutException("Client not connected!");
        using StreamWriter sw = new(stream, Encoding.ASCII) { AutoFlush = true };
        try
        {
            await sw.WriteLineAsync(command);
        }
        catch (Exception ex)
        {
            throw new NutException("Error writing to client.", ex);
        }
    }

    private async Task<string[]?> GetResponseAsync(Command command, CancellationToken cancellationToken)
    {
        if (!client.Connected)
            throw new NutException("Client not connected!");
        List<string> response = [];
        using StreamReader sr = new(stream, Encoding.ASCII);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? line = await sr.ReadLineAsync(cancellationToken);
            if (line == null)
                throw new TimeoutException();
            else if (line.StartsWith("OK") || line.StartsWith("ERR"))
                response.Add(line);
            else if (line.StartsWith("BEGIN " + command.ToString()))
            {
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    line = await sr.ReadLineAsync(cancellationToken);
                    if (line == null)
                        throw new TimeoutException();
                    else
                        response.Add(line);
                } while (!line.StartsWith("END " + command.ToString()));
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new NutException("Error reading from client.", ex);
        }
        return response.Count == 0 ? null : [.. response];
    }

    #region "IDisposable"

    private bool disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            if (stream != null && stream is SslStream)
                stream.Dispose();
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }
        }
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NutClient()
    {
        Dispose(false);
    }

    #endregion

}
