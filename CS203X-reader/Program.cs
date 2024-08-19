using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Events;

class Program
{
    private static readonly HighLevelInterface ReaderCE = new();
    private static string RFIDdata = string.Empty;
    private static string readerResponse = string.Empty;

    static void Main(string[] args)
    {
        Console.WriteLine("RFID Reader Console Application");
        Console.Write("Enter IP address of the reader: ");
        string? ipAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("IP address cannot be empty.");
            return;
        }

        if (ConnectReader(ipAddress))
        {
            Console.WriteLine("Reader connected successfully.");
            StartReading();
            Console.WriteLine("Press any key to stop reading...");
            Console.ReadKey();
            StopReading();
        }
        else
        {
            Console.WriteLine("Failed to connect to the reader.");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
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

    static void StopReading()
    {
        if (ReaderCE.State != RFState.BUSY)
            return;

        ReaderCE.StopOperation(true);
        while (ReaderCE.State != RFState.IDLE)
            Thread.Sleep(1000);

        ReaderCE.OnAsyncCallback -= ReaderCE_MyInventoryEvent;
        ReaderCE.OnStateChanged -= ReaderCE_MyRunningStateEvent;
    }

    static void ReaderCE_MyRunningStateEvent(object? sender, OnStateChangedEventArgs e)
    {
        readerResponse = $"Reader State : {e.state}";
        Console.WriteLine(readerResponse);
    }

    static void ReaderCE_MyInventoryEvent(object? sender, OnAsyncCallbackEventArgs e)
    {
        RFIDdata = $"{e.info.epc},{e.info.rssi}";
        Console.WriteLine(RFIDdata);
    }
}