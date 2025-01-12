using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NutWinSvc.Nut
{

    internal class NutClient
    {
        private readonly NutOptions options;

        private TcpClient client = new() { SendTimeout = 10000, ReceiveTimeout = 10000 };

        private Stream stream = Stream.Null;

        public bool Connected => client.Connected;

        public bool UsingTls => (stream is SslStream);

        public NutClient(IOptions<NutOptions> nutOptions)
        {
            ArgumentNullException.ThrowIfNull(nutOptions);
            this.options = nutOptions.Value;
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Username);

        }

        public async Task ConnectAsync()
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
                try
                {
                    await client.ConnectAsync(options.Host, options.Port, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
                if (!client.Connected)
                    throw new TimeoutException();
                stream = client.GetStream();
                if (options.UseTls)
                    await TryStartTlsAsync();
            }
            catch (Exception ex)
            {
                throw new NutException("Could not connect!", ex);
            }
        }

        private async Task WriteStreamAsync(string command)
        {
            if (!client.Connected)
                throw new NutException("Client not connected!");
            using StreamWriter sw = new(stream);
            try
            {
                await sw.WriteLineAsync(command);
            }
            catch (Exception ex)
            {
                throw new NutException("Error writing to client.", ex);
            }
        }

        private async Task<bool> ExecuteCommand(Command command, params string[] args)
        {
            StringBuilder cmd = new();
            cmd.Append(command.ToString());
            await WriteStreamAsync(cmd.ToString());

            return true;
        }

        private async Task TryStartTlsAsync()
        {
            await WriteStreamAsync(Command.STARTTLS.ToString());
            //TODO: Get and check response
            SslStream sslStream = new(client.GetStream(), true);
            try
            {
                await sslStream.AuthenticateAsClientAsync(options.Host ?? String.Empty);
                stream = sslStream;
            }
            catch { }
        }

    }
}
