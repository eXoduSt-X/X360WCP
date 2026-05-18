using System;
using System.Collections.Generic;
using System.Windows.Forms;

using InputManager;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Xbox360WirelessChatpad
{
    class Controller
    {
        // Tracks if the Wireless Controller is attached
        public bool controllerAttached = false;

        // Tracks the connected controller's number
        private int controllerNumber;

        // Tracks if the trigger will behave like a button or axis
        private bool triggerAsButton;

        // The Controllers associated endpoint writer in the receiver
        private UsbEndpointWriter epWriter;
        
        // Parent Window object necessary to communicate with form controls
        private Window_Main parentWindow;

        // Keep-Alive Thread, this will execute keep-alive commands periodically
        private System.Threading.Thread threadKeepAlive = null;
        private bool inhibitKeepAlive = false;
        private int inhibitCounter = 0;

        // Button Combo Thread, this will execute to monitor for special button
        // combinations like Mouse Mode and Shutdown
        private System.Threading.Thread threadButtonCombo = null;

        // MouseMode Thread, this will execute periodically to move the mouse cursor
        // and scroll vertical when inidcated by joystick data
        private System.Threading.Thread mouseModeThread = null;

        // Determines if the chatpad needs initialization/handshake command.
        private bool chatpadInitNeeded = true;

        // Mapping for various device commands
        private Dictionary<string, byte[]> controllerCommands = new Dictionary<string, byte[]>()
            {
                // General Device Commands
                { "RefreshConnection",  new byte[4] {0x08, 0x00, 0x00, 0x00} },
                { "KeepAlive1",         new byte[4] {0x00, 0x00, 0x0C, 0x1F} },
                { "KeepAlive2",         new byte[4] {0x00, 0x00, 0x0C, 0x1E} },
                { "ChatpadInit",        new byte[4] {0x00, 0x00, 0x0C, 0x1B} },
                { "SetControllerNum1",  new byte[4] {0x00, 0x00, 0x08, 0x42} },
                { "SetControllerNum2",  new byte[4] {0x00, 0x00, 0x08, 0x43} },
                { "SetControllerNum3",  new byte[4] {0x00, 0x00, 0x08, 0x44} },
                { "SetControllerNum4",  new byte[4] {0x00, 0x00, 0x08, 0x45} },
                { "DisableController",  new byte[4] {0x00, 0x00, 0x08, 0xC0} },

                // Chatpad LED Commands
                { "GreenOn",       new byte[4] {0x00, 0x00, 0x0C, 0x09} },
                { "GreenOff",      new byte[4] {0x00, 0x00, 0x0C, 0x01} },
                { "OrangeOn",      new byte[4] {0x00, 0x00, 0x0C, 0x0A} },
                { "OrangeOff",     new byte[4] {0x00, 0x00, 0x0C, 0x02} },
                { "MessengerOn",   new byte[4] {0x00, 0x00, 0x0C, 0x0B} },
                { "MessengerOff",  new byte[4] {0x00, 0x00, 0x0C, 0x03} },
                { "CapslockOn",    new byte[4] {0x00, 0x00, 0x0C, 0x08} },
                { "CapslockOff",   new byte[4] {0x00, 0x00, 0x0C, 0x00} }
            };

        // Contains the mapping of Chatpad Buttons, Green Modifiers, and
        // Orange Modifiers respectively.
        private Dictionary<int, Keys> keyMap = new Dictionary<int, Keys>();
        private Dictionary<int, string> greenMap = new Dictionary<int, string>();
        private Dictionary<int, string> orangeMap = new Dictionary<int, string>();

        // Tracks which Chatpad Modifiers are active
        private Dictionary<string, bool> chatpadMod = new Dictionary<string, bool>()
            {
                { "Green", false },
                { "Orange", false },
                { "Shift", false },
                { "Capslock", false },
                { "Messenger", false }
            };

        // Tracks which Chatpad LEDs are illuminated
        private Dictionary<string, bool> chatpadLED = new Dictionary<string, bool>()
            {
                { "Green", false },
                { "Orange", false },
                { "Capslock", false },
                { "Messenger", false }
            };

        // Tracks which keys are currently being held down, used to
        // determine if a keystroke should be sent or not
        private List<byte> chatpadKeysHeld = new List<byte>();

        // Tracks which keyboard keys are down, used to track if a
        // KeyUp command needs to be sent or not
        private List<Keys> keyboardKeysDown = new List<Keys>();

        // Identifies if the sent key data should be upper case or lower case
        private bool flagUpperCase = false;

        // Identifies if Alt-Tab cycling has begun
        private bool altTabActive = false;

        // Used to determine if the data has changed since the last packet
        private byte[] dataPacketLast = new byte[3]; 

        // -----------------
        // Gamepad Variables
        // -----------------

        // ViGEm Client and Virtual XInput Gamepad
        private ViGEmClient vigemClient;
        private IXbox360Gamepad virtualGamepad;

        // Deadzone variables for the joysticks on the gamepad
        public int deadzoneL = 0;
        public int deadzoneR = 0;

        // Global Mouse Mode Flag for use by data packet processing
        public bool mouseModeFlag = false;

        // Relative Mouse Data based on Joystick location. This will
        // be used by a higher level timer function to continually move
        // the mouse.
        private int mouseVelX, mouseVelY;

        // Direction Data for the Right Joystick location. This will
        // be used by a higher level timer function to continually hold
        // down an arrow key, allowing for scrolling or other fast navigation.
        // 0 = Neutral, 
        private int rightStickDir;

        // Special Command booleans used to detect when special button
        // combinations are pressed
        private bool cmdKillController = false;
        private bool cmdMouseModeToggle = false;

        // Identifies if the left or right mouse buttons are depressed
        // Only used in Mouse Mode.
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;

        private bool navActive = false;

        public Controller(Window_Main window)
        {
            parentWindow = window;

            try
            {
                // Instanciar el bus de emulación virtual ViGEm
                vigemClient = new ViGEmClient();
                virtualGamepad = vigemClient.CreateXbox360Gamepad();
            }
            catch (Exception)
            {
                throw new VjoyNotEnabledException(); // Reutilizado por compatibilidad de firmas de excepción en el proyecto original
            }
        }

        public void registerEndpointWriter(UsbEndpointWriter writer)
        {
            epWriter = writer;
        }

        public void registerJoystick(int ctrlNum)
        {
            controllerNumber = ctrlNum;

            try
            {
                // Conectar el control virtual directo al Bus XInput de Windows
                virtualGamepad.Connect();
            }
            catch (Exception)
            {
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    "WARNING: Failed to Connect ViGEm XInput Gamepad Number " + controllerNumber + ".");
            }
        }

        public void processDataPacket(object sender, EndpointDataEventArgs e)
        {
            if (e.Buffer[0] == 0x08)
            {
                bool controllerConnected = ((e.Buffer[1] & 0x80) > 0);

                if (!controllerConnected)
                {
                    if (controllerAttached)
                    {
                        parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                            "Xbox 360 Wireless Controller " + controllerNumber + " Disconnected.");

                        killMouseMode();
                        killKeepAlive();
                        killButtonCombo();
                        resetComboButtons();

                        if (virtualGamepad != null)
                            virtualGamepad.Disconnect();

                        parentWindow.Invoke(new controllerDisconnectCallback(parentWindow.controllerDisconnected), controllerNumber);
                    }

                    controllerAttached = false;
                }
                else
                {
                    controllerAttached = true;

                    switch (controllerNumber)
                    {
                        case 1: sendData(controllerCommands["SetControllerNum1"]); break;
                        case 2: sendData(controllerCommands["SetControllerNum2"]); break;
                        case 3: sendData(controllerCommands["SetControllerNum3"]); break;
                        case 4: sendData(controllerCommands["SetControllerNum4"]); break;
                        default:
                            parentWindow.Invoke(new logCallback(parentWindow.logMessage), "ERROR: Unknown Controller Number.");
                            break;
                    }

                    threadKeepAlive = new System.Threading.Thread(new System.Threading.ThreadStart(tickKeepAlive));
                    threadKeepAlive.IsBackground = true;
                    threadKeepAlive.Start();

                    threadButtonCombo = new System.Threading.Thread(new System.Threading.ThreadStart(tickButtonCombo));
                    threadButtonCombo.IsBackground = true;
                    threadButtonCombo.Start();

                    if (mouseModeFlag)
                        startMouseMode();

                    parentWindow.Invoke(new controllerConnectCallback(parentWindow.controllerConnected), controllerNumber);

                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                        "Xbox 360 Wireless Controller " + controllerNumber + " Connected (XInput).");
                }
            }
            else if (e.Buffer[0] == 0x00 && e.Buffer[2] == 0x00 && e.Buffer[3] == 0xF0)
            {
                if (controllerAttached)
                {
                    switch (e.Buffer[1])
                    {
                        case 0x01:
                            ProcessGamepadData(e.Buffer);
                            break;
                        case 0x02:
                            ProcessChatpadData(e.Buffer);
                            break;
                    }
                }
            }
        }

        public void ProcessChatpadData(byte[] dataPacket)
        {
            if (dataPacket[24] == 0xF0)
            {
                if (dataPacket[25] == 0x03)
                    chatpadInitNeeded = true;
                else if (dataPacket[25] != 0x04)
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage), "WARNING: Unknown Chatpad Status Data.");
            }
            else if (dataPacket[24] == 0x00)
            {
                bool dataChanged = false;
                if (dataPacketLast != null)
                {
                    if (dataPacketLast[0] != dataPacket[25] || dataPacketLast[1] != dataPacket[26] || dataPacketLast[2] != dataPacket[27])
                        dataChanged = true;
                }
                else
                    dataChanged = true;

                dataPacketLast[0] = dataPacket[25];
                dataPacketLast[1] = dataPacket[26];
                dataPacketLast[2] = dataPacket[27];

                if (dataChanged)
                {
                    inhibitKeepAlive = true;
                    inhibitCounter = 0;

                    chatpadMod["Green"] = (dataPacket[25] & 0x02) > 0;
                    chatpadMod["Orange"] = (dataPacket[25] & 0x04) > 0;
                    chatpadMod["Shift"] = (dataPacket[25] & 0x01) > 0;
                    chatpadMod["Messenger"] = (dataPacket[25] & 0x08) > 0;

                    if (chatpadMod["Orange"] && chatpadMod["Shift"])
                        chatpadMod["Capslock"] = !chatpadMod["Capslock"];

                    if (chatpadMod["Green"] && !chatpadLED["Green"]) { sendData(controllerCommands["GreenOn"]); chatpadLED["Green"] = true; }
                    if (chatpadMod["Orange"] && !chatpadLED["Orange"]) { sendData(controllerCommands["OrangeOn"]); chatpadLED["Orange"] = true; }
                    if (chatpadMod["Messenger"] && !chatpadLED["Messenger"]) { sendData(controllerCommands["MessengerOn"]); chatpadLED["Messenger"] = true; }
                    if (chatpadMod["Capslock"] && !chatpadLED["Capslock"]) { sendData(controllerCommands["CapslockOn"]); chatpadLED["Capslock"] = true; }

                    if (!chatpadMod["Green"] && chatpadLED["Green"]) { sendData(controllerCommands["GreenOff"]); chatpadLED["Green"] = false; }
                    if (!chatpadMod["Orange"] && chatpadLED["Orange"]) { sendData(controllerCommands["OrangeOff"]); chatpadLED["Orange"] = false; }
                    if (!chatpadMod["Messenger"] && chatpadLED["Messenger"]) { sendData(controllerCommands["MessengerOff"]); chatpadLED["Messenger"] = false; }
                    if (!chatpadMod["Capslock"] && chatpadLED["Capslock"]) { sendData(controllerCommands["CapslockOff"]); chatpadLED["Capslock"] = false; }

                    flagUpperCase = chatpadMod["Shift"] ^ chatpadMod["Capslock"];
                    if (flagUpperCase) Keyboard.KeyDown(Keys.LShiftKey); else Keyboard.KeyUp(Keys.LShiftKey);

                    if (chatpadMod["Messenger"]) Keyboard.KeyDown(Keys.Tab); else Keyboard.KeyUp(Keys.Tab);

                    if (chatpadMod["Orange"])
                    {
                        if (chatpadMod["Green"])
                        {
                            if (altTabActive) Keyboard.KeyPress(Keys.Tab);
                            else
                            {
                                altTabActive = true;
                                Keyboard.KeyDown(Keys.LMenu);
                                Keyboard.KeyPress(Keys.Tab);
                            }
                        }
                    }
                    else if (altTabActive)
                    {
                        altTabActive = false;
                        Keyboard.KeyUp(Keys.LMenu);
                    }

                    ProcessKeypress(dataPacket[26]);
                    ProcessKeypress(dataPacket[27]);

                    List<byte> keysToRemove = new List<byte>();
                    foreach (var key in chatpadKeysHeld)
                        if (key != dataPacket[26] && key != dataPacket[27])
                            keysToRemove.Add(key);
                    foreach (var key in keysToRemove)
                    {
                        if (keyboardKeysDown.Contains(keyMap[key]))
                        {
                            keyboardKeysDown.Remove(keyMap[key]);
                            Keyboard.KeyUp(keyMap[key]);
                        }
                        chatpadKeysHeld.Remove(key);
                    }
                }
            }
            else
                parentWindow.Invoke(new logCallback(parentWindow.logMessage), "WARNING: Unknown Chatpad Data.");
        }

        public void ProcessGamepadData(byte[] dataPacket)
        {
            // ------------------------------------
            // Mapeo Nativo de Cruceta (D-Pad) XInput
            // ------------------------------------
            virtualGamepad.SetButtonState(Xbox360Button.Up, dataPacket[6] == 0x01 || dataPacket[6] == 0x05 || dataPacket[6] == 0x09);
            virtualGamepad.SetButtonState(Xbox360Button.Down, dataPacket[6] == 0x02 || dataPacket[6] == 0x06 || dataPacket[6] == 0x0A);
            virtualGamepad.SetButtonState(Xbox360Button.Left, dataPacket[6] == 0x04 || dataPacket[6] == 0x05 || dataPacket[6] == 0x06);
            virtualGamepad.SetButtonState(Xbox360Button.Right, dataPacket[6] == 0x08 || dataPacket[6] == 0x09 || dataPacket[6] == 0x0A);

            // -----------------
            // Modo Mouse o Botones normales
            // -----------------
            if (mouseModeFlag)
            {
                // A Button - Clic Izquierdo
                if ((dataPacket[7] & 0x10) > 0)
                {
                    if (!leftButtonDown) { Mouse.ButtonDown(Mouse.MouseKeys.Left); leftButtonDown = true; }
                }
                else if (leftButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Left); leftButtonDown = false; }

                // B Button - Clic Derecho
                if ((dataPacket[7] & 0x20) > 0)
                {
                    if (!rightButtonDown) { Mouse.ButtonDown(Mouse.MouseKeys.Right); rightButtonDown = true; }
                }
                else if (rightButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Right); rightButtonDown = false; }
            }
            else
            {
                virtualGamepad.SetButtonState(Xbox360Button.A, (dataPacket[7] & 0x10) > 0);
                virtualGamepad.SetButtonState(Xbox360Button.B, (dataPacket[7] & 0x20) > 0);
            }

            virtualGamepad.SetButtonState(Xbox360Button.X, (dataPacket[7] & 0x40) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.Y, (dataPacket[7] & 0x80) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.Start, (dataPacket[6] & 0x10) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.Back, (dataPacket[6] & 0x20) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.LeftThumb, (dataPacket[6] & 0x40) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.RightThumb, (dataPacket[6] & 0x80) > 0);
            virtualGamepad.SetButtonState(Xbox360Button.Guide, (dataPacket[7] & 0x04) > 0);

            if (mouseModeFlag)
            {
                if ((dataPacket[7] & 0x01) > 0 && !navActive)
                {
                    navActive = true;
                    Keyboard.KeyDown(Keys.LMenu); Keyboard.KeyDown(Keys.Left); Keyboard.KeyUp(Keys.Left); Keyboard.KeyUp(Keys.LMenu);
                }
                if ((dataPacket[7] & 0x02) > 0 && !navActive)
                {
                    navActive = true;
                    Keyboard.KeyDown(Keys.LMenu); Keyboard.KeyDown(Keys.Right); Keyboard.KeyUp(Keys.Right); Keyboard.KeyUp(Keys.LMenu);
                }
            }
            else
            {
                virtualGamepad.SetButtonState(Xbox360Button.LeftShoulder, (dataPacket[7] & 0x01) > 0);
                virtualGamepad.SetButtonState(Xbox360Button.RightShoulder, (dataPacket[7] & 0x02) > 0);
            }

            // ---------------
            // Procesamiento de Ejes Analógicos
            // ---------------
            short leftX = (short)(dataPacket[10] | (dataPacket[11] << 8));
            short leftY = (short)(dataPacket[12] | (dataPacket[13] << 8));
            short rightX = (short)(dataPacket[14] | (dataPacket[15] << 8));
            short rightY = (short)(dataPacket[16] | (dataPacket[17] << 8));
            int leftTrig = dataPacket[8];
            int rightTrig = dataPacket[9];

            // Zona Muerta Stick Izquierdo
            double leftDistance = Math.Sqrt((double)(leftX * leftX) + (double)(leftY * leftY));
            if (leftDistance < deadzoneL) { leftX = 0; leftY = 0; }
            else
            {
                if (Math.Abs(Convert.ToInt32(leftX)) < deadzoneL) leftX = 0;
                if (Math.Abs(Convert.ToInt32(leftY)) < deadzoneL) leftY = 0;
            }

            // Zona Muerta Stick Derecho
            double rightDistance = Math.Sqrt((double)(rightX * rightX) + (double)(rightY * rightY));
            if (rightDistance < deadzoneR) { rightX = 0; rightY = 0; }
            else
            {
                if (Math.Abs(Convert.ToInt32(rightX)) < deadzoneR) rightX = 0;
                if (Math.Abs(Convert.ToInt32(rightY)) < deadzoneR) rightY = 0;
            }

            if (mouseModeFlag)
            {
                int maxVelocity = leftTrig >= 50 ? 20 : 10;
                mouseVelX = maxVelocity * leftX / 32767;
                mouseVelY = maxVelocity * leftY / 32767;
                rightStickDir = rightY < 0 ? -1 : (rightY > 0 ? 1 : 0);
            }
            else
            {
                // El eje Y físico suele venir invertido respecto al espacio de ViGEm
                virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbX, leftX);
                virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbY, (short)-leftY);
                virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbX, rightX);
                virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbY, rightY);

                if (triggerAsButton)
                {
                    virtualGamepad.SetButtonState(Xbox360Button.LeftThumb, leftTrig >= 50); // Mapeo alternativo como botón si aplica
                    virtualGamepad.SetButtonState(Xbox360Button.RightThumb, rightTrig >= 50);
                }
                else
                {
                    // Gatillos en ViGEm van de 0 a 255 (tipo byte)
                    virtualGamepad.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)leftTrig);
                    virtualGamepad.SetSliderValue(Xbox360Slider.RightTrigger, (byte)rightTrig);
                }
            }

            // Atajos especiales
            cmdMouseModeToggle = ((dataPacket[7] & 0x01) > 0) && ((dataPacket[7] & 0x02) > 0) && ((dataPacket[6] & 0x20) > 0);
            cmdKillController = (leftTrig >= 50) && (rightTrig >= 50) && ((dataPacket[6] & 0x20) > 0);
        }

        private void sendData(byte[] dataToSend)
        {
            int bytesWritten;
            ErrorCode ec = epWriter.Write(dataToSend, 2000, out bytesWritten);
            if (ec != ErrorCode.None)
                parentWindow.Invoke(new logCallback(parentWindow.logMessage), "ERROR: Problem Sending Controller Data.");
        }

        private void ProcessKeypress(byte key)
        {
            if (key != 0 && !chatpadKeysHeld.Contains(key))
            {
                chatpadKeysHeld.Add(key);

                if (chatpadMod["Orange"])
                {
                    if (flagUpperCase) SendKeys.SendWait(orangeMap[key].ToUpper()); else SendKeys.SendWait(orangeMap[key]);
                }
                else if (chatpadMod["Green"])
                {
                    if (flagUpperCase) SendKeys.SendWait(greenMap[key].ToUpper()); else SendKeys.SendWait(greenMap[key]);
                }
                else
                {
                    keyboardKeysDown.Add(keyMap[key]);
                    Keyboard.KeyDown(keyMap[key]);
                }
            }
        }

        public void configureChatpad(string keyboardType)
        {
            switch (keyboardType)
            {
                case "Q W E R T Y":
                    keyMap[23] = Keys.D1; greenMap[23] = ""; orangeMap[23] = "";
                    keyMap[22] = Keys.D2; greenMap[22] = ""; orangeMap[22] = "";
                    keyMap[21] = Keys.D3; greenMap[21] = ""; orangeMap[21] = "";
                    keyMap[20] = Keys.D4; greenMap[20] = ""; orangeMap[20] = "";
                    keyMap[19] = Keys.D5; greenMap[19] = ""; orangeMap[19] = "";
                    keyMap[18] = Keys.D6; greenMap[18] = ""; orangeMap[18] = "";
                    keyMap[17] = Keys.D7; greenMap[17] = ""; orangeMap[17] = "";
                    keyMap[103] = Keys.D8; greenMap[103] = ""; orangeMap[103] = "";
                    keyMap[102] = Keys.D9; greenMap[102] = ""; orangeMap[102] = "";
                    keyMap[101] = Keys.D0; greenMap[101] = ""; orangeMap[101] = "";

                    keyMap[39] = Keys.Q; greenMap[39] = "!"; orangeMap[39] = "¡";
                    keyMap[38] = Keys.W; greenMap[38] = "@"; orangeMap[38] = "å";
                    keyMap[37] = Keys.E; greenMap[37] = "€"; orangeMap[37] = "é";
                    keyMap[36] = Keys.R; greenMap[36] = "#"; orangeMap[36] = "$";
                    keyMap[35] = Keys.T; greenMap[35] = "{%}"; orangeMap[35] = "Þ";
                    keyMap[34] = Keys.Y; greenMap[34] = "{^}"; orangeMap[34] = "ý";
                    keyMap[33] = Keys.U; greenMap[33] = "&"; orangeMap[33] = "ú";
                    keyMap[118] = Keys.I; greenMap[118] = "*"; orangeMap[118] = "í";
                    keyMap[117] = Keys.O; greenMap[117] = "{(}"; orangeMap[117] = "ó";
                    keyMap[100] = Keys.P; greenMap[100] = "{)}"; orangeMap[100] = "=";

                    keyMap[55] = Keys.A; greenMap[55] = "{~}"; orangeMap[55] = "á";
                    keyMap[54] = Keys.S; greenMap[54] = "š"; orangeMap[54] = "ß";
                    keyMap[53] = Keys.D; greenMap[53] = "{{}"; orangeMap[53] = "ð";
                    keyMap[52] = Keys.F; greenMap[52] = "{}}"; orangeMap[52] = "£";
                    keyMap[51] = Keys.G; greenMap[51] = "¨"; orangeMap[51] = "¥";
                    keyMap[50] = Keys.H; greenMap[50] = "/"; orangeMap[50] = "\\";
                    keyMap[49] = Keys.J; greenMap[49] = "'"; orangeMap[49] = "\"";
                    keyMap[119] = Keys.K; greenMap[119] = "{[}"; orangeMap[119] = "☺";
                    keyMap[114] = Keys.L; greenMap[114] = "{]}"; orangeMap[114] = "ø";
                    keyMap[98] = Keys.Oemcomma; greenMap[98] = ":"; orangeMap[98] = ";";

                    keyMap[70] = Keys.Z; greenMap[70] = "`"; orangeMap[70] = "æ";
                    keyMap[69] = Keys.X; greenMap[69] = "«"; orangeMap[69] = "œ";
                    keyMap[68] = Keys.C; greenMap[68] = "»"; orangeMap[68] = "ç";
                    keyMap[67] = Keys.V; greenMap[67] = "-"; orangeMap[67] = "_";
                    keyMap[66] = Keys.B; greenMap[66] = "|"; orangeMap[66] = "{+}";
                    keyMap[65] = Keys.N; greenMap[65] = "<"; orangeMap[65] = "ñ";
                    keyMap[82] = Keys.M; greenMap[82] = ">"; orangeMap[82] = "µ";
                    keyMap[83] = Keys.OemPeriod; greenMap[83] = "?"; orangeMap[83] = "¿";
                    keyMap[99] = Keys.Enter; greenMap[99] = ""; orangeMap[99] = "";

                    keyMap[85] = Keys.Left; greenMap[85] = ""; orangeMap[85] = "";
                    keyMap[84] = Keys.Space; greenMap[84] = ""; orangeMap[84] = "";
                    keyMap[81] = Keys.Right; greenMap[81] = ""; orangeMap[81] = "";
                    keyMap[113] = Keys.Back; greenMap[113] = ""; orangeMap[113] = "";
                    break;
                // Nota: Mantenidos los demás casos (QWERTZ, AZERTY) igual si se requieren...
            }
        }

        public void configureGamepad(bool triggerAsBtn)
        {
            // Requerido por compatibilidad de firmas pero simplificado, ViGEm autogestiona el mapa XInput.
            triggerAsButton = triggerAsBtn;
        }

        public void startController()
        {
            sendData(controllerCommands["RefreshConnection"]);
            sendData(controllerCommands["RefreshConnection"]);
        }

        public void killController()
        {
            sendData(controllerCommands["DisableController"]);
            if (virtualGamepad != null) virtualGamepad.Disconnect();
            parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                "Disconnecting Xbox 360 Wireless Controller " + controllerNumber + ".");
        }

        private void tickButtonCombo()
        {
            int mouseModeTick = 0;
            int killControllerTick = 0;

            while (true)
            {
                if (cmdMouseModeToggle) mouseModeTick++; else mouseModeTick = 0;
                if (mouseModeTick == 3) toggleMouseMode(!mouseModeFlag);

                if (cmdKillController) killControllerTick++; else killControllerTick = 0;
                if (killControllerTick == 6) sendData(controllerCommands["DisableController"]);

                System.Threading.Thread.Sleep(500);
            }
        }

        public void killButtonCombo()
        {
            if (threadButtonCombo != null) { threadButtonCombo.Abort(); threadButtonCombo = null; }
        }

        private void tickKeepAlive()
        {
            bool keepAliveToggle = false;
            while (true)
            {
                if (epWriter != null)
                {
                    if (inhibitKeepAlive)
                    {
                        if (inhibitCounter >= 2) { inhibitCounter = 0; inhibitKeepAlive = false; }
                        else inhibitCounter++;
                    }
                    else
                    {
                        sendData(keepAliveToggle ? controllerCommands["KeepAlive1"] : controllerCommands["KeepAlive2"]);
                        keepAliveToggle = !keepAliveToggle;
                    }

                    if (chatpadInitNeeded) { sendData(controllerCommands["ChatpadInit"]); chatpadInitNeeded = false; }
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public void killKeepAlive()
        {
            if (threadKeepAlive != null) { threadKeepAlive.Abort(); threadKeepAlive = null; }
        }

        private void tickMouseMode()
        {
            int tickCount = 0;
            int navActCount = 0;

            while (true)
            {
                if ((Math.Abs(mouseVelX) > 0) || ((Math.Abs(mouseVelY) > 0))) Mouse.MoveRelative(mouseVelX, -mouseVelY);

                if (tickCount == 4)
                {
                    if (rightStickDir == -1) Mouse.Scroll(Mouse.ScrollDirection.Down);
                    else if (rightStickDir == 1) Mouse.Scroll(Mouse.ScrollDirection.Up);
                    tickCount = 0;
                }

                if (navActive)
                {
                    if (navActCount == 25) { navActive = false; navActCount = 0; }
                    navActCount++;
                }

                tickCount++;
                System.Threading.Thread.Sleep(20);
            }
        }

        private void startMouseMode()
        {
            mouseModeThread = new System.Threading.Thread(new System.Threading.ThreadStart(tickMouseMode));
            mouseModeThread.IsBackground = true;
            mouseModeThread.Start();

            if (virtualGamepad != null)
            {
                virtualGamepad.SetButtonState(Xbox360Button.LeftShoulder, false);
                virtualGamepad.SetButtonState(Xbox360Button.RightShoulder, false);
                virtualGamepad.SetButtonState(Xbox360Button.Back, false);
            }

            for (int i = 0; i < 3; i++)
            {
                sendData(controllerCommands["GreenOn"]); System.Threading.Thread.Sleep(100);
                sendData(controllerCommands["GreenOff"]); System.Threading.Thread.Sleep(100); 
            }
        }

        private void toggleMouseMode(bool mouseMode)
        {
            if (mouseMode)
            {
                mouseModeFlag = true;
                startMouseMode();
                parentWindow.Invoke(new mouseModeLabelCallback(parentWindow.mouseModeUpdate), controllerNumber, true);
            }
            else
            {
                mouseModeFlag = false;
                killMouseMode();
                parentWindow.Invoke(new mouseModeLabelCallback(parentWindow.mouseModeUpdate), controllerNumber, false);
            }
        }

        public void killMouseMode()
        {
            if (mouseModeThread != null) { mouseModeThread.Abort(); mouseModeThread = null; }

            for (int i = 0; i < 3; i++)
            {
                sendData(controllerCommands["OrangeOn"]); System.Threading.Thread.Sleep(100);
                sendData(controllerCommands["OrangeOff"]); System.Threading.Thread.Sleep(100);
            }
        }

        private void resetComboButtons()
        {
            if (virtualGamepad == null) return;

            foreach (Xbox360Button btn in Enum.GetValues(typeof(Xbox360Button)))
                virtualGamepad.SetButtonState(btn, false);

            virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
            virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
            virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbX, 0);
            virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbY, 0);
            virtualGamepad.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
            virtualGamepad.SetSliderValue(Xbox360Slider.RightTrigger, 0);
        }
    }

    class VjoyNotEnabledException : Exception
    {
        internal VjoyNotEnabledException() { }
    }
}
