using System.Collections.Concurrent;
using System.Net;
using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Events;

class Program
{
    private static readonly HighLevelInterface ReaderCE = new();
    private static readonly ConcurrentDictionary<string, float> RfidReadings = new();
    private static readonly ConcurrentDictionary<string, long> RfidReadTimestamps = new();
    private static readonly Timer RfidCleanupTimer;
    private static int maxAgeSeconds = 60; // Default to 60 seconds
    private static string ipAddress = string.Empty;
    private static DateTime lastRfStateReceived = DateTime.UtcNow;
    private static readonly Timer ReconnectTimer;
    private static bool isReaderConnected = false;

    static Program()
    {
        RfidCleanupTimer = new Timer(CleanupRfidReadings, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        ReconnectTimer = new Timer(CheckAndReconnectReader, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    static async Task Main(string[] args)
    {
        ipAddress = Environment.GetEnvironmentVariable("READER_IP_ADDRESS") ?? string.Empty;

        if (!string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine($"Using IP address from environment variable: {ipAddress}");
        }
        else if (args.Length > 0)
        {
            ipAddress = args[0];
            Console.WriteLine($"Using IP address from command line argument: {ipAddress}");
        }
        else
        {
            Console.Write("Enter IP address of the reader: ");
            ipAddress = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            throw new Exception("IP address cannot be empty.");
        }
        
        string portStr = Environment.GetEnvironmentVariable("HTTP_PORT") ?? string.Empty;
        int port;
        if (!string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out int parsedEnvPort))
        {
            port = parsedEnvPort;
            Console.WriteLine($"Using port number from environment variable: {port}");
        }
        else if (args.Length > 1 && int.TryParse(args[1], out int parsedPort))
        {
            port = parsedPort;
            Console.WriteLine($"Using port number from command line argument: {port}");
        }
        else
        {
            Console.Write("Enter port number for the HTTP service: ");
            port = int.Parse(Console.ReadLine() ?? string.Empty);
        }

        if (port <= 0 || port > 65535)
        {
            Console.WriteLine("Invalid port number.");
            return;
        }

        await ConnectReader();
        Console.WriteLine("Reader connected successfully. Starting HTTP service...");
        await StartHttpServiceAsync(port);
    }

    static async Task ConnectReader()
    {
        while (true)
        {
            StopReading();
            Result result = ReaderCE.Connect(ipAddress, 3000);
            if (result == Result.OK)
            {
                StartReading();
                lastRfStateReceived = DateTime.UtcNow;
                isReaderConnected = true;
                return;
            }
            else
            {
                Console.WriteLine($"Failed to connect to reader. Retrying in 10 seconds...");
                await Task.Delay(10000);
            }
        }
    }

    static void StopReading()
    {
        isReaderConnected = false;
        ReaderCE.Disconnect();
        ReaderCE.OnAsyncCallback -= ReaderCE_MyInventoryEvent;
        ReaderCE.OnStateChanged -= ReaderCE_MyRunningStateEvent;
    }

    static void StartReading()
    {
        ReaderCE.OnAsyncCallback += ReaderCE_MyInventoryEvent;
        ReaderCE.OnStateChanged += ReaderCE_MyRunningStateEvent;

        ReaderCE.SetDynamicQParms(5, 0, 15, 0, 10, 1);
        ReaderCE.SetOperationMode(RadioOperationMode.NONCONTINUOUS);
        ReaderCE.Options.TagInventory.flags = SelectFlags.ZERO;

        ReaderCE.StartOperation(Operation.TAG_INVENTORY, false);
    }

    static void ReaderCE_MyRunningStateEvent(object? sender, OnStateChangedEventArgs e)
    {
        Console.WriteLine($"Reader State: {e.state}");
        lastRfStateReceived = DateTime.UtcNow;

        if (e.state == RFState.IDLE)
        {
            ReaderCE.StartOperation(Operation.TAG_INVENTORY, false);
        }
    }

    static void ReaderCE_MyInventoryEvent(object? sender, OnAsyncCallbackEventArgs e)
    {
        Console.WriteLine($"EPC: {e.info.epc}, RSSI: {e.info.rssi}");

        RfidReadings.AddOrUpdate(
            e.info.epc.ToString(),
            e.info.rssi,
            (_, _) => e.info.rssi
        );
        RfidReadTimestamps.AddOrUpdate(
            e.info.epc.ToString(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            (_, _) => DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }

    static async void CheckAndReconnectReader(object? state)
    {
        if (isReaderConnected && (DateTime.UtcNow - lastRfStateReceived).TotalSeconds > 10)
        {
            Console.WriteLine("No RFState received in the last 10 seconds. Attempting to reconnect...");
            await ConnectReader();
        }
    }

    static async Task StartHttpServiceAsync(int port)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/");
        listener.Start();
        Console.WriteLine($"HTTP service started. Listening on http://*:{port}/");

        while (true)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }
    }

    static async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            if (request.Url.AbsolutePath == "/rfids")
            {
                int? maxAge = GetMaxAgeSeconds(request);
                var rfidReadings = GetRfidReadings(maxAge);
                var responseString = string.Join(Environment.NewLine, rfidReadings.Select(r => $"{r.Item1},{r.Item2}"));
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    static int? GetMaxAgeSeconds(HttpListenerRequest request)
    {
        if (request.QueryString["maxAge"] != null && int.TryParse(request.QueryString["maxAge"], out int maxAge))
        {
            return maxAge;
        }
        return null;
    }

    static List<(string, float)> GetRfidReadings(int? maxAgeSeconds)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var readings = RfidReadTimestamps.Where(r => maxAgeSeconds == null || (now - r.Value) <= maxAgeSeconds.Value)
                                         .Select(r => (r.Key, RfidReadings[r.Key]))
                                         .ToList();
        return readings;
    }

    static void CleanupRfidReadings(object? state)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        RfidReadTimestamps.Where(r => (now - r.Value) > maxAgeSeconds)
                          .ToList()
                          .ForEach(r =>
                          {
                              RfidReadings.TryRemove(r.Key, out _);
                              RfidReadTimestamps.TryRemove(r.Key, out _);
                          });
    }
}