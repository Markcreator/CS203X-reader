using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Events;

class Program
{
    private static readonly HighLevelInterface ReaderCE = new();

    static async Task Main(string[] args)
    {
        string ipAddress;
        int? shutdownSeconds = null;

        if (args.Length > 0)
        {
            ipAddress = args[0];
            Console.WriteLine($"Using IP address from command line: {ipAddress}");

            if (args.Length > 1 && int.TryParse(args[1], out int seconds))
            {
                shutdownSeconds = seconds;
                Console.WriteLine($"Application will shut down after {seconds} seconds");
            }
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
            Console.WriteLine("Reader connected successfully.");
            StartReading();

            if (shutdownSeconds.HasValue)
            {
                await Task.Delay(TimeSpan.FromSeconds(shutdownSeconds.Value));
                Console.WriteLine("Shutdown timer elapsed. Exiting...");
                return;
            }

            // Keep the program running indefinitely if no shutdown timer
            while (true)
            {
                await Task.Delay(100); // Small delay to prevent CPU overuse
            }
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
        Console.WriteLine($"{e.info.epc},{e.info.rssi}");
    }
}