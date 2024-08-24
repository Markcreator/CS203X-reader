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

    static Program()
    {
        RfidCleanupTimer = new Timer(CleanupRfidReadings, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    static async Task Main(string[] args)
    {
        string ipAddress;
        if (args.Length > 0)
        {
            ipAddress = args[0];
        }
        else
        {
            Console.Write("Enter IP address of the reader: ");
            ipAddress = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("IP address cannot be empty.");
            return;
        }

        if (ConnectReader(ipAddress))
        {
            Console.WriteLine("Reader connected successfully. Starting HTTP service...");
            StartReading();
            await StartHttpServiceAsync();
        }
        else
        {
            Console.WriteLine("Failed to connect to the reader.");
        }
    }

    static bool ConnectReader(string ipAddress)
    {
        Result result = ReaderCE.Connect(ipAddress, 3000);
        return result == Result.OK;
    }

    static void StartReading()
    {
        ReaderCE.OnAsyncCallback += ReaderCE_MyInventoryEvent;
        ReaderCE.OnStateChanged += ReaderCE_MyRunningStateEvent;
        ReaderCE.SetDynamicQParms(5, 0, 15, 0, 10, 1);
        ReaderCE.SetOperationMode(RadioOperationMode.CONTINUOUS);
        ReaderCE.Options.TagInventory.flags = SelectFlags.ZERO;
        ReaderCE.StartOperation(Operation.TAG_INVENTORY, false);
    }

    static void ReaderCE_MyRunningStateEvent(object? sender, OnStateChangedEventArgs e)
    {
        Console.WriteLine($"Reader State : {e.state}");
    }

    static void ReaderCE_MyInventoryEvent(object? sender, OnAsyncCallbackEventArgs e)
    {
        RfidReadings.AddOrUpdate(e.info.epc.ToString(), e.info.rssi, (_, _) => e.info.rssi);
        RfidReadTimestamps.AddOrUpdate(e.info.epc.ToString(), DateTimeOffset.UtcNow.ToUnixTimeSeconds(), (_, _) => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    static async Task StartHttpServiceAsync()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("HTTP service started. Listening on http://localhost:8080/");

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