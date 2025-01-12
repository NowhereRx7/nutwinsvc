using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace NutWinSvc.Nut
{

    internal class NutClient : IDisposable
    {
        private readonly NutOptions options;
        private readonly TcpClient client = new() { SendTimeout = 10000, ReceiveTimeout = 10000 };
        private Stream stream = Stream.Null;

        public bool Connected => client.Connected;

        public bool UsingTls => (stream is SslStream);

        public NutClient(IOptions<NutOptions> nutOptions)
        {
            ArgumentNullException.ThrowIfNull(nutOptions);
            this.options = nutOptions.Value;
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.UpsName);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Username);
        }

        public async Task Open()
        {
            await ConnectAsync();
            await TryStartTlsAsync();
            await AuthenticateAsync();
        }

        private async Task ConnectAsync()
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);
                try
                {
                    using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
                    await client.ConnectAsync(options.Host, options.Port, cts.Token);
                }
                catch (OperationCanceledException)
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

        public async Task<CommandResult> ExecuteCommand(Command command, params string[] args)
        {
            StringBuilder cmd = new();
            cmd.Append(command.ToString());
            foreach (string arg in args)
                cmd.Append(" " + arg);
            await WriteStreamAsync(cmd.ToString());
            string[]? response = await GetResponse(command);
            if (response == null || response.Length == 0)
                throw new NutException("Response to command was null.");
            //TODO: ExecuteCommand - Process response

            return new CommandResult(command, response);
        }

        private async Task TryStartTlsAsync()
        {
            if (!options.UseTls) return;
            CommandResult result = await ExecuteCommand(Command.STARTTLS);
            if (result.Error != Error.None)
                throw new NutException(result);
            SslStream sslStream = new(client.GetStream(), true);
            try
            {
                await sslStream.AuthenticateAsClientAsync(options.Host ?? String.Empty);
                stream = sslStream;
            }
            catch { }
        }

        private async Task AuthenticateAsync()
        {
            CommandResult result = await ExecuteCommand(Command.LOGIN, options.UpsName ?? throw new ArgumentNullException(options.UpsName));
            if (result.Error != Error.None)
                throw new NutException("Could not log in.");
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

        private async Task<string[]?> GetResponse(Command command)
        {
            if (!client.Connected)
                throw new NutException("Client not connected!");
            List<string> response = [];
            using StreamReader sr = new(stream, Encoding.ASCII);
            try
            {
                string? line = await sr.ReadLineAsync();
                if (line == null)
                    throw new TimeoutException();
                else if (line.StartsWith("OK") || line.StartsWith("ERR"))
                    response.Add(line);
                else if (line.StartsWith("BEGIN " + command.ToString()))
                {
                    do
                    {
                        line = await sr.ReadLineAsync();
                        if (line == null)
                            throw new TimeoutException();
                        else
                            response.Add(line);
                    } while (!line.StartsWith("END " + command.ToString()));
                }
            }
            catch (Exception ex)
            {
                throw new NutException("Error reading from client.", ex);
            }
            return response.Count == 0 ? null : response.ToArray();
        }

        

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
    }
}
