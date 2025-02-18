using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;

namespace NutClient;

/// <summary>
/// A client for communicating with a NUT server.
/// </summary>
public partial class NutClient : IDisposable
{
    //TODO: I should move some of this up into an inheriting class; make this a Base class with very basic access methods

    private readonly string host;
    private readonly int port;
    private readonly string upsName;
    private readonly bool useTls;
    private readonly TcpClient client = new() { SendTimeout = 10000, ReceiveTimeout = 10000 };
    private Stream stream = Stream.Null;
    private ILogger? logger = default;

    public string Host => host;

    public int Port => port;

    public string UpsName => upsName;

    public bool Connected => client.Connected;

    public bool UsingTls => stream is SslStream;

    public bool IsLoggedIn { get; protected set; } = false;

    public Version? ServerVersion { get; protected set; } = default;

    public Version? ProtocolVersion { get; protected set; } = default;


    public NutClient(string host, string upsName, bool useTls = false, int port = 3493, ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(upsName);
        this.host = host;
        this.upsName = upsName;
        this.port = port;
        this.useTls = useTls;
        this.logger = logger;
    }


    public virtual async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (Connected) return;
        //throw new NutException("Client is already connected.", new InvalidOperationException("The socket is already connected."));
        await ConnectAsync(cancellationToken);
        await GetVersionsAsync(cancellationToken);
        await TryStartTlsAsync(cancellationToken);
    }


    /// <summary>
    /// Executes a NUT command asynchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="NutException">Thrown when a communications or command error occurs.</exception>
    /// <exception cref="ArgumentException">Thrown when an argument is invalid.</exception>
    /// <exception cref="NotImplementedException">Thrown when a command is not yet supported.</exception>
    public virtual async Task<CommandResult> ExecuteCommandAsync(Command command) => await ExecuteCommandAsync(command, [], CancellationToken.None);
    /// <inheritdoc cref="ExecuteCommandAsync(Command)"/>
    /// <param name="cancellationToken">A token for requesting cancellation of the task.</param>
    public virtual async Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken) => await ExecuteCommandAsync(command, [], cancellationToken);
    /// <inheritdoc cref="ExecuteCommandAsync(Command, CancellationToken)"/>
    /// <param name="args">A string array of arguments for the command.</param>
    public virtual async Task<CommandResult> ExecuteCommandAsync(Command command, string[] args, CancellationToken cancellationToken)
    {
        if (!Connected)
            throw new NutException("Client is not connected!");
        StringBuilder cmd = new();
        cmd.Append(command.ToString());
        if ((command == Command.GET || command == Command.LIST) && args.Length < 1)
            throw new ArgumentException($"{command} requires arguments!");
        else if (command == Command.SET && args.Length < 2)
            throw new ArgumentException($"{command} requires arguments!");
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
            else
                return new CommandResult(command, a[1].ToError(), a.Length > 2 ? a[2] : null);
        }
        switch (command)
        {
            case Command.LOGOUT:
                if (response[0].StartsWith("OK") || response[0].StartsWith("Goodbye"))
                {
                    IsLoggedIn = false;
                    return new CommandResult(command);
                }
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
            case Command.VER:
            case Command.NETVER:
            case Command.PROTVER:
                return new CommandResult(command, response[0]);
            default:
                break;
        }
        return new CommandResult(command, response.Length == 1 ? response[0] : response);
    }


    protected virtual async Task ConnectAsync(CancellationToken cancellationToken)
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
            if (!Connected)
                throw new TimeoutException();
            stream = client.GetStream();
        }
        catch (Exception ex)
        {
            throw new NutException("Could not connect!", ex);
        }
    }


    protected virtual async Task TryStartTlsAsync(CancellationToken cancellationToken)
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


    protected virtual async Task GetVersionsAsync(CancellationToken cancellationToken)
    {
        Regex rex = new(@"\d?\.\d?(|\.\d?)");
        try
        {
            CommandResult result = await ExecuteCommandAsync(Command.VER, cancellationToken);
            if (result.Error == Error.None)
            {
                string s = result.Data as string ?? string.Empty;
                if (rex.IsMatch(s) && Version.TryParse(rex.Match(s).Value, out var version))
                    ServerVersion = version;
            }
        }
        catch (Exception) { }
        try
        {
            CommandResult result = await ExecuteCommandAsync(Command.NETVER, cancellationToken);
            if (result.Error == Error.None)
            {
                string s = result.Data as string ?? string.Empty;
                if (rex.IsMatch(s) && Version.TryParse(rex.Match(s).Value, out var version))
                    ProtocolVersion = version;
            }
        }
        catch (Exception) { }
    }


    /// <summary>
    /// Authenticates a client as a consumer of the UPS (e.g. upsmon) asynchronously.
    /// This requires "upsmon secondary" or "upsmon primary" in upsd.users.<br/>
    /// Use this to log the fact that a system is drawing power from this UPS.<br/>
    /// The upsmon primary will wait until the count of attached systems reaches 1 — itself.This allows the secondaries to shut down first.
    /// </summary>
    /// <param name="username">The username for the UPS.</param>
    /// <param name="password">The password for the UPS.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> for cancelling the task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="NutException">
    /// Thrown when an error occurs (communication or command).
    /// </exception>
    public virtual async Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        CommandResult result = await ExecuteCommandAsync(Command.USERNAME, [username], cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        result = await ExecuteCommandAsync(Command.PASSWORD, [password], cancellationToken);
        if (result.Error != Error.None)
            throw new NutException(result);
        //TODO: LOGIN is really only for upsmon clients.  Which works for this app, but should be broken out.
        result = await ExecuteCommandAsync(Command.LOGIN, [UpsName], cancellationToken);
        if (result.Error == Error.None)
            IsLoggedIn = true;
        else
            throw new NutException(result);
    }


    protected virtual async Task WriteStreamAsync(string command)
    {
        if (!Connected)
            throw new NutException("Client not connected!");
        using StreamWriter sw = new(stream, encoding: Encoding.ASCII, leaveOpen: true) { AutoFlush = true, NewLine = "\n" };
        try
        {
            await sw.WriteLineAsync(command);
        }
        catch (Exception ex)
        {
            throw new NutException("Error writing to client.", ex);
        }
    }


    protected virtual async Task<string[]?> GetResponseAsync(Command command, CancellationToken cancellationToken)
    {
        if (!Connected)
            throw new NutException("Client not connected!");
        List<string> response = [];
        using StreamReader sr = new(stream, encoding: Encoding.ASCII, leaveOpen: true);
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
            else if (!String.IsNullOrEmpty(line.Trim()))
            {
                response.Add(line.Trim());
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

    private int _disposed;
    protected virtual async void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) > 0) return;
        if (disposing)
        {
            if (IsLoggedIn)
                try
                {
                    await ExecuteCommandAsync(Command.LOGOUT);
                }
                catch { }
            if (stream != null && stream is SslStream)
                stream.Dispose();
            if (client != null)
            {
                try
                {
                    client.Close();
                }
                catch { }
                client.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NutClient() => Dispose(false);

    #endregion

}
