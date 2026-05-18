using System;

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Xbox360WirelessChatpad
{
    class Receiver
    {
        // Tracks if the Wireless Receiver is attached
        public bool receiverAttached = false;

        // Parent Window object necessary to communicate with form controls
        private Window_Main parentWindow;

        // Xbox Controllers object necessary to communicate with each controller
        private Controller[] xboxControllers = new Controller[4];

        // USB Wireless Receiver to connect
        private IUsbDevice wirelessReceiver;

        // USB Endpoints to send/receive data from the Wireless Receiver
        private UsbEndpointWriter[] epWriters = new UsbEndpointWriter[4];
        private UsbEndpointReader[] epReaders = new UsbEndpointReader[4];        

        public Receiver(Controller[] controller, Window_Main window)
        {
            // Stores the passed window as parentWindow for furtue use
            parentWindow = window;

            // Stores the passed Xbox controllers for future use
            xboxControllers[0] = controller[0];
            xboxControllers[1] = controller[1];
            xboxControllers[2] = controller[2];
            xboxControllers[3] = controller[3];
        }

        public void connectReceiver()
        {
            // Connect to the Xbox Wireless Receiver and register the endpoint
            // readers/writers as necessary.
            try
            {
                // Open the Xbox Wireless Receiver as a USB device
                // VendorID 0x045e, ProductID 0x0719
                wirelessReceiver = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x045E, 0x0719)) as IUsbDevice;

                // If primary IDs not found attempt secondary IDs
                // VendorID 0x045e, Product ID 0x0291
                if (wirelessReceiver == null)
                    wirelessReceiver = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x045E, 0x0291)) as IUsbDevice;

                // If secondary IDs not found report the error
                if (wirelessReceiver == null)
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                        "ERROR: Wireless Receiver Not Found.");
                else
                {
                    // Set the Configuration, Claim the Interface
                    wirelessReceiver.ClaimInterface(1);
                    wirelessReceiver.SetConfiguration(1);

                    // Log if the Wireless Receiver was connected to successfully
                    if (wirelessReceiver.IsOpen)
                    {
                        receiverAttached = true;
                        parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                            "Xbox 360 Wireless Receiver Connected.");

                        // Connect Bulk Endpoint Readers/Writers and register the receiving event handler
                        // Controller 1
                        epReaders[0] = wirelessReceiver.OpenEndpointReader(ReadEndpointID.Ep01);
                        epWriters[0] = wirelessReceiver.OpenEndpointWriter(WriteEndpointID.Ep01);
                        epReaders[0].DataReceived += new EventHandler<EndpointDataEventArgs>(xboxControllers[0].processDataPacket);
                        epReaders[0].DataReceivedEnabled = true;
                        xboxControllers[0].registerEndpointWriter(epWriters[0]);

                        // Controller 2
                        epReaders[1] = wirelessReceiver.OpenEndpointReader(ReadEndpointID.Ep03);
                        epWriters[1] = wirelessReceiver.OpenEndpointWriter(WriteEndpointID.Ep03);
                        epReaders[1].DataReceived += new EventHandler<EndpointDataEventArgs>(xboxControllers[1].processDataPacket);
                        epReaders[1].DataReceivedEnabled = true;
                        xboxControllers[1].registerEndpointWriter(epWriters[1]);

                        // Controller 3
                        epReaders[2] = wirelessReceiver.OpenEndpointReader(ReadEndpointID.Ep05);
                        epWriters[2] = wirelessReceiver.OpenEndpointWriter(WriteEndpointID.Ep05);
                        epReaders[2].DataReceived += new EventHandler<EndpointDataEventArgs>(xboxControllers[2].processDataPacket);
                        epReaders[2].DataReceivedEnabled = true;
                        xboxControllers[2].registerEndpointWriter(epWriters[2]);

                        // Controller 4
                        epReaders[3] = wirelessReceiver.OpenEndpointReader(ReadEndpointID.Ep07);
                        epWriters[3] = wirelessReceiver.OpenEndpointWriter(WriteEndpointID.Ep07);
                        epReaders[3].DataReceived += new EventHandler<EndpointDataEventArgs>(xboxControllers[3].processDataPacket);
                        epReaders[3].DataReceivedEnabled = true;
                        xboxControllers[3].registerEndpointWriter(epWriters[3]);

                        parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                            "Searching for Controllers...Press the Guide Button Now.");
                    }
                }
            }
            catch (Exception ex)
            {
                parentWindow.Invoke(new logCallback(parentWindow.logMessage), "ERROR: " + ex.Message);
            }
        }

        public void killReceiver()
        {
            // Desconectar endpoints y liberar el dispositivo USB de forma segura
            for (int i = 0; i < 4; i++)
            {
                if (epReaders[i] != null)
                {
                    epReaders[i].DataReceivedEnabled = false;
                    epReaders[i].DataReceived -= xboxControllers[i].processDataPacket;
                }
                if (xboxControllers[i].controllerAttached)
                {
                    xboxControllers[i].killController();
                }
            }

            if (wirelessReceiver != null && wirelessReceiver.IsOpen)
            {
                wirelessReceiver.ReleaseInterface(1);
                wirelessReceiver.Close();
            }
            
            UsbDevice.Exit();
            receiverAttached = false;
        }
    }
}
