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
            parentWindow = window;

            xboxControllers[0] = controller[0];
            xboxControllers[1] = controller[1];
            xboxControllers[2] = controller[2];
            xboxControllers[3] = controller[3];
        }

        public void connectReceiver()
        {
            try
            {
                // Open the Xbox Wireless Receiver as a USB device
                // VendorID 0x045E, ProductID 0x0719
                wirelessReceiver = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x045E, 0x0719)) as IUsbDevice;

                // If primary IDs not found attempt secondary IDs
                // VendorID 0x045E, ProductID 0x0291
                if (wirelessReceiver == null)
                    wirelessReceiver = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x045E, 0x0291)) as IUsbDevice;

                if (wirelessReceiver == null)
                {
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                       "ERROR: receptor no encontrado.");
                    return;
                }

                // Set the Configuration, Claim the Interface
                wirelessReceiver.ClaimInterface(1);
                wirelessReceiver.SetConfiguration(1);

                if (!wirelessReceiver.IsOpen)
                    return;

                receiverAttached = true;
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    "Receptor conectado.");

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
                    "Buscando mandos... pulsa GUIDE.");
            }
            catch (Exception ex)
            {
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    "ERROR: receptor: " + ex.Message);

                try
                {
                    using (var id = System.Security.Principal.WindowsIdentity.GetCurrent())
                    {
                        var p = new System.Security.Principal.WindowsPrincipal(id);
                        bool isAdmin = p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                        if (!isAdmin)
                        {
                            var res = System.Windows.Forms.MessageBox.Show(
                                "Se requieren permisos de administrador para acceder al receptor USB. ¿Deseas reiniciar la aplicación con privilegios?",
                                "Permisos necesarios",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Question);

                            if (res == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (MainApplication.RequestRunElevated())
                                {
                                    System.Windows.Forms.Application.Exit();
                                    return;
                                }
                                else
                                {
                                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                                        "ERROR: Elevación cancelada por el usuario.");
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignorar errores de comprobación de rol; ya hemos registrado el error original.
                }
            }
        }

        public void killReceiver()
        {
            // FIX: Correct cleanup order — stop data flow FIRST, then kill controllers,
            // then release USB. The old ACTUALReceiver skipped Abort/Dispose on
            // epReaders/epWriters which could leave USB threads running after close.
            for (int i = 0; i < 4; i++)
            {
                // 1. Stop incoming data immediately to avoid processing after shutdown
                if (epReaders[i] != null)
                {
                    epReaders[i].DataReceivedEnabled = false;
                    epReaders[i].DataReceived -= xboxControllers[i].processDataPacket;
                }

                // 2. Kill controller threads and ViGEm virtual gamepad
                if (receiverAttached && xboxControllers[i] != null && xboxControllers[i].controllerAttached)
                {
                    xboxControllers[i].killKeepAlive();
                    xboxControllers[i].killMouseMode();
                    xboxControllers[i].killButtonCombo();
                    xboxControllers[i].killController();
                }

                // 3. Abort and dispose USB endpoints
                if (epWriters[i] != null)
                {
                    epWriters[i].Abort();
                    epWriters[i].Dispose();
                    epWriters[i] = null;
                }

                if (epReaders[i] != null)
                {
                    epReaders[i].Abort();
                    epReaders[i].Dispose();
                    epReaders[i] = null;
                }
            }

            // 4. Release and close the USB device
            if (wirelessReceiver != null)
            {
                if (wirelessReceiver.IsOpen)
                {
                    wirelessReceiver.ReleaseInterface(1);
                    wirelessReceiver.Close();
                }
                wirelessReceiver = null;
            }

            // 5. Clean up LibUsbDotNet global state
            UsbDevice.Exit();

            receiverAttached = false;
        }
    }
}
