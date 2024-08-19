using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Events;

class Program
{
    private static readonly HighLevelInterface ReaderCE = new();

    static void Main(string[] args)
    {
        string ipAddress;
        if (args.Length > 0)
        {
            ipAddress = args[0];
            Console.WriteLine($"Using IP address from command line: {ipAddress}");
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

            // Keep the program running indefinitely
            while (true)
            {
                Thread.Sleep(100); // Small delay to prevent CPU overuse
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