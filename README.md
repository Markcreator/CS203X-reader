# CS203X-reader

This is a simple console application that connects to an RFID reader and continuously prints RFID tag detections to the console.

## Features

- Connects to an RFID reader using an IP address
- Continuously reads RFID tags
- Prints EPC (Electronic Product Code) and RSSI (Received Signal Strength Indicator) for each detected tag
- Displays reader state changes

## Prerequisites

- .NET 6.0 or later
- CSLibrary DLLs (make sure these are referenced in your project)

## Usage

1. Compile the application.

2. Run the application using one of the following methods:

   a. With IP address as a command-line argument:
      ```
      dotnet run <IP_ADDRESS>
      ```
      Example: `dotnet run 192.168.1.100`

   b. Without command-line argument:
      ```
      dotnet run
      ```
      The application will prompt you to enter the IP address.

3. The application will connect to the RFID reader and start printing tag detections to the console.

4. To stop the application, press Ctrl+C.

## Output Format

The application outputs two types of information:

1. RFID Tag Detections:
`<EPC>,<RSSI>`
2. Reader State Changes:
`Reader State : <STATE>`

## Notes

- The application runs indefinitely until manually terminated.
- Ensure proper error handling and connection management for production use.
- Make sure your firewall allows the application to connect to the RFID reader.

## Troubleshooting

- If the application fails to connect, verify that the IP address is correct and that the RFID reader is powered on and accessible on the network.
- Check that all necessary CSLibrary DLLs are properly referenced in your project.