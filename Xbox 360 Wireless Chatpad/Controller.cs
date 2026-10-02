using InputManager;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

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

        // Wrapper thread-safe para las escrituras USB
        private UsbTransport usbTransport;

        // Parent Window object necessary to communicate with form controls
        private Window_Main parentWindow;

        // Keep-Alive Thread, this will execute keep-alive commands periodically
        private System.Threading.Thread threadKeepAlive = null;
        private bool inhibitKeepAlive = false;
        private int inhibitCounter = 0;

        // Button Combo Thread
        private System.Threading.Thread threadButtonCombo = null;

        // MouseMode Thread
        private System.Threading.Thread mouseModeThread = null;

        private CancellationTokenSource _keepAliveCts;
        private CancellationTokenSource _buttonComboCts;
        private CancellationTokenSource _mouseModeCts;
        private CancellationTokenSource _ledBlinkCts;

        // Determines if the chatpad needs initialization/handshake command.
        private bool chatpadInitNeeded = true;

        // Para detectar cambios de stick (edge detection)
        private short _logPrevLeftX;
        private short _logPrevLeftY;
        private short _logPrevRightX;
        private short _logPrevRightY;
        private const int STICK_LOG_THRESHOLD = 20000;   // ~60% del rango
        private bool _leftStickActive;
        private bool _rightStickActive;
        // Mapping for various device commands
        private Dictionary<string, byte[]> controllerCommands = new Dictionary<string, byte[]>()
        {
            { "RefreshConnection", new byte[4] {0x08, 0x00, 0x00, 0x00} },
            { "KeepAlive1",        new byte[4] {0x00, 0x00, 0x0C, 0x1F} },
            { "KeepAlive2",        new byte[4] {0x00, 0x00, 0x0C, 0x1E} },
            { "ChatpadInit",       new byte[4] {0x00, 0x00, 0x0C, 0x1B} },
            { "SetControllerNum1", new byte[4] {0x00, 0x00, 0x08, 0x42} },
            { "SetControllerNum2", new byte[4] {0x00, 0x00, 0x08, 0x43} },
            { "SetControllerNum3", new byte[4] {0x00, 0x00, 0x08, 0x44} },
            { "SetControllerNum4", new byte[4] {0x00, 0x00, 0x08, 0x45} },
            { "DisableController", new byte[4] {0x00, 0x00, 0x08, 0xC0} },

            { "GreenOn",     new byte[4] {0x00, 0x00, 0x0C, 0x09} },
            { "GreenOff",    new byte[4] {0x00, 0x00, 0x0C, 0x01} },
            { "OrangeOn",    new byte[4] {0x00, 0x00, 0x0C, 0x0A} },
            { "OrangeOff",   new byte[4] {0x00, 0x00, 0x0C, 0x02} },
            { "MessengerOn", new byte[4] {0x00, 0x00, 0x0C, 0x0B} },
            { "MessengerOff",new byte[4] {0x00, 0x00, 0x0C, 0x03} },
            { "CapslockOn",  new byte[4] {0x00, 0x00, 0x0C, 0x08} },
            { "CapslockOff", new byte[4] {0x00, 0x00, 0x0C, 0x00} }
        };

        // Traduce las teclas del chatpad a teclas de Windows (3 layouts)
        private readonly ChatpadMapper chatpadMapper = new ChatpadMapper();

        // Gamepad virtual ViGEm encapsulado
        private GamepadEmitter gamepad;
        private MouseModeHandler mouseModeHandler;

        // Tracks which Chatpad Modifiers are active
        private Dictionary<string, bool> chatpadMod = new Dictionary<string, bool>()
        {
            { "Green",     false },
            { "Orange",    false },
            { "Shift",     false },
            { "Capslock",  false },
            { "Messenger", false }
        };

        // Tracks which Chatpad LEDs are illuminated
        private Dictionary<string, bool> chatpadLED = new Dictionary<string, bool>()
        {
            { "Green",     false },
            { "Orange",    false },
            { "Capslock",  false },
            { "Messenger", false }
        };

        // Tracks which keys are currently being held down
        private List<byte> chatpadKeysHeld = new List<byte>();

        // Tracks which keyboard keys are down
        private List<Keys> keyboardKeysDown = new List<Keys>();

        // Identifies if the sent key data should be upper case or lower case
        private bool flagUpperCase = false;

        // Identifies if Alt-Tab cycling has begun
        private bool altTabActive = false;

        // Used to determine if the data has changed since the last packet
        private byte[] dataPacketLast = new byte[3];

        // Deadzone variables for the joysticks on the gamepad
        // Deadzone variables for the joysticks on the gamepad.
        // Son propiedades para que cualquier cambio se propague al MouseModeHandler.
        private int _deadzoneL = 0;
        public int deadzoneL
        {
            get { return _deadzoneL; }
            set
            {
                _deadzoneL = value;
                mouseModeHandler?.SetDeadzones(_deadzoneL, _deadzoneR);
            }
        }

        private int _deadzoneR = 0;
        public int deadzoneR
        {
            get { return _deadzoneR; }
            set
            {
                _deadzoneR = value;
                mouseModeHandler?.SetDeadzones(_deadzoneL, _deadzoneR);
            }
        }

        // Global Mouse Mode Flag for use by data packet processing
        public bool mouseModeFlag = false;

      
        // Special Command booleans
        private bool cmdKillController = false;
        private bool cmdMouseModeToggle = false;

        // Guide button edge-detection
        private bool guideButtonLastState = false;

        // Long-press detection
        private DateTime guidePressStart = DateTime.MinValue;
        private bool guidePowerOffTriggered = false;
        private const int GUIDE_HOLD_MS = 5000;

        // Previous gamepad button bytes for press-edge logging
        private byte _logPrevByte6 = 0;
        private byte _logPrevByte7 = 0;
        private bool _logPrevLt = false;
        private bool _logPrevRt = false;


        // Custom key mappings for Mouse Mode
        private System.Windows.Forms.Keys _startKeyCustom = System.Windows.Forms.Keys.Enter;
        private System.Windows.Forms.Keys _backKeyCustom = System.Windows.Forms.Keys.Delete;
        private System.Windows.Forms.Keys _lbKeyCustom = System.Windows.Forms.Keys.None;

        public System.Windows.Forms.Keys StartKeyCustom
        {
            get { return _startKeyCustom; }
            set { _startKeyCustom = value; }
        }

        public System.Windows.Forms.Keys BackKeyCustom
        {
            get { return _backKeyCustom; }
            set { _backKeyCustom = value; }
        }

        public System.Windows.Forms.Keys LBKeyCustom
        {
            get { return _lbKeyCustom; }
            set { _lbKeyCustom = value; }
        }

        private bool startKeySubscribed = false;
        private bool backKeySubscribed = false;
#pragma warning disable 0414
        private bool lbKeySubscribed = false;
#pragma warning restore 0414

        public Controller(Window_Main window)
        {
            parentWindow = window;
            // El GamepadEmitter se crea en registerJoystick(), cuando ya
            // sabemos el número de mando.
        }

        // Parpadea un LED del chatpad 3 veces sin bloquear el hilo de paquetes
        private void ParpadearLed(string onCmd, string offCmd)
        {
            // Cancela un parpadeo anterior para que no se mezclen
            try { _ledBlinkCts?.Cancel(); } catch { }

            var cts = new CancellationTokenSource();
            _ledBlinkCts = cts;
            var token = cts.Token;

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        sendData(controllerCommands[onCmd]);
                        if (token.WaitHandle.WaitOne(100)) break;
                        sendData(controllerCommands[offCmd]);
                        if (token.WaitHandle.WaitOne(100)) break;
                    }
                }
                catch { }
                finally
                {
                    // Si se canceló a mitad, deja el LED apagado
                    try { sendData(controllerCommands[offCmd]); } catch { }
                }
            });
        }
        public void registerEndpointWriter(UsbEndpointWriter writer)
        {
            epWriter = writer;
            usbTransport = new UsbTransport(writer);
        }

        public void registerJoystick(int ctrlNum)
        {
            controllerNumber = ctrlNum;

            // Si ya existía un emitter, liberarlo
            gamepad?.Dispose();

            try
            {
                gamepad = new GamepadEmitter(ctrlNum);
                gamepad.Connect();
            }
            catch (Exception)
            {
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    "WARN: ViGEm C" + controllerNumber + " falló.");
            }

            // Crear el handler de Mouse Mode con el gamepad ya listo
            mouseModeHandler = new MouseModeHandler(
                gamepad,
                parentWindow,                              // ← Control para el Invoke
                msg => parentWindow.BeginInvoke(new logCallback(parentWindow.logMessage), msg));
            // Aplicar deadzones actuales al handler recién creado
            mouseModeHandler.SetDeadzones(_deadzoneL, _deadzoneR);
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
                               "C" + controllerNumber + " desconectado.");

                        killMouseMode();
                        killKeepAlive();
                        killButtonCombo();
                        resetComboButtons();

                        gamepad?.Disconnect();

                        parentWindow.Invoke(new controllerDisconnectCallback(parentWindow.controllerDisconnected), controllerNumber);
                    }

                    controllerAttached = false;
                }
                else
                {
                    controllerAttached = true;

                    // Reconectar el pad virtual si hace falta (idempotente)
                    gamepad?.Connect();

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
                    _keepAliveCts = new CancellationTokenSource();
                    threadKeepAlive = new System.Threading.Thread(() => tickKeepAlive(_keepAliveCts.Token));
                    threadKeepAlive.IsBackground = true;
                    threadKeepAlive.Name = $"KeepAlive-C{controllerNumber}";
                    threadKeepAlive.Start();

                    _buttonComboCts = new CancellationTokenSource();
                    threadButtonCombo = new System.Threading.Thread(() => tickButtonCombo(_buttonComboCts.Token));
                    threadButtonCombo.IsBackground = true;
                    threadButtonCombo.Name = $"ButtonCombo-C{controllerNumber}";
                    threadButtonCombo.Start();

                    if (mouseModeFlag)
                        startMouseMode();

                    parentWindow.Invoke(new controllerConnectCallback(parentWindow.controllerConnected), controllerNumber);
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                        "C" + controllerNumber + " conectado.");
                }
            }
            else if (e.Buffer[0] == 0x00 && e.Buffer[2] == 0x00 && e.Buffer[3] == 0xF0)
            {
                if (controllerAttached)
                {
                    switch (e.Buffer[1])
                    {
                        case 0x01: ProcessGamepadData(e.Buffer); break;
                        case 0x02: ProcessChatpadData(e.Buffer); break;
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
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage), "WARN: chatpad status desconocido.");
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
                    if (flagUpperCase)
                    {
                        Keyboard.KeyDown(Keys.LShiftKey);
                    }
                    else
                    {
                        // Solo soltar LShift si nadie más lo está reteniendo (p.ej. RB en Mouse Mode)
                        bool mouseModeHoldingShift = mouseModeFlag && mouseModeHandler != null && mouseModeHandler.IsShiftHeld;
                        if (!mouseModeHoldingShift)
                            Keyboard.KeyUp(Keys.LShiftKey);
                    }

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
                        Keys mapped = chatpadMapper.GetKey(key);
                        if (mapped != Keys.None && keyboardKeysDown.Contains(mapped))
                        {
                            keyboardKeysDown.Remove(mapped);
                            chatpadMapper.ReleaseKey(mapped);
                        }
                        chatpadKeysHeld.Remove(key);
                    }
                }
            }
            else
                parentWindow.Invoke(new logCallback(parentWindow.logMessage), "WARN: chatpad data desconocida.");
        }

        // -------------------------------------------------------------------
        // Helpers de hilo UI
        // -------------------------------------------------------------------

        private delegate void VoidDelegate();

        private void EjecutarEnHiloUI(Action action, bool sincrono)
        {
            if (parentWindow.InvokeRequired)
            {
                if (sincrono)
                    parentWindow.Invoke(new VoidDelegate(() => action()));
                else
                    parentWindow.BeginInvoke(new VoidDelegate(() => action()));
            }
            else
                action();
        }

        // -------------------------------------------------------------------
        // PROCESS GAMEPAD DATA
        // -------------------------------------------------------------------
        // Helper que devuelve el nombre del botón según el modo
        string MapLabel(string buttonName)
        {
            if (!mouseModeFlag) return buttonName;

            switch (buttonName)
            {
                case "LT": return "LT (CTRL)";
                case "RT": return "RT (ALT+TAB)";
                case "LB": return "LB (FAST+)";
                case "RB": return "RB (SHIFT+SLOW-)";
                case "X": return "X (CLIC IZQ)";
                case "B": return "B (CLIC DER)";
                case "A": return "A (SCROLL↓)";
                case "Y": return "Y (SCROLL↑)";
                case "L3": return "L3 (WIN+M)";
                case "R3": return "R3 (ALT+F4)";
                case "DPAD ARRIBA": return "DPAD ↑ (ARRIBA)";
                case "DPAD ABAJO": return "DPAD ↓ (ABAJO)";
                case "DPAD IZQUIERDA": return "DPAD ← (IZQ)";
                case "DPAD DERECHA": return "DPAD → (DER)";
                case "START": return "START (ENTER)";
                case "BACK": return "BACK (DELETE)";
                case "GUIDE": return "GUIDE (TOGGLE)";
                default: return buttonName;
            }
        }
        private void RegistrarBotonesEnLog(byte[] dataPacket)
        {
            byte b6 = dataPacket[6];
            byte b7 = dataPacket[7];
            bool ltNow = dataPacket[8] >= 50;
            bool rtNow = dataPacket[9] >= 50;

            void LogPress(string nombre)
            {
                string modo = mouseModeFlag ? " [RATÓN]" : "";
                parentWindow.BeginInvoke(new logCallback(parentWindow.logMessage),
                    string.Format("C{0}{1} ► {2}", controllerNumber, modo, nombre));
            }

            if ((b6 & 0x01) > 0 && (_logPrevByte6 & 0x01) == 0) LogPress(MapLabel("DPAD ARRIBA"));
            if ((b6 & 0x02) > 0 && (_logPrevByte6 & 0x02) == 0) LogPress(MapLabel("DPAD ABAJO"));
            if ((b6 & 0x04) > 0 && (_logPrevByte6 & 0x04) == 0) LogPress(MapLabel("DPAD IZQUIERDA"));
            if ((b6 & 0x08) > 0 && (_logPrevByte6 & 0x08) == 0) LogPress(MapLabel("DPAD DERECHA"));
            if ((b6 & 0x10) > 0 && (_logPrevByte6 & 0x10) == 0) LogPress(MapLabel("START"));
            if ((b6 & 0x20) > 0 && (_logPrevByte6 & 0x20) == 0) LogPress(MapLabel("BACK"));
            if ((b6 & 0x40) > 0 && (_logPrevByte6 & 0x40) == 0) LogPress(MapLabel("L3"));
            if ((b6 & 0x80) > 0 && (_logPrevByte6 & 0x80) == 0) LogPress(MapLabel("R3"));

            if ((b7 & 0x01) > 0 && (_logPrevByte7 & 0x01) == 0) LogPress(MapLabel("LB"));
            if ((b7 & 0x02) > 0 && (_logPrevByte7 & 0x02) == 0) LogPress(MapLabel("RB"));
            if ((b7 & 0x04) > 0 && (_logPrevByte7 & 0x04) == 0) LogPress(MapLabel("GUIDE"));
            if ((b7 & 0x10) > 0 && (_logPrevByte7 & 0x10) == 0) LogPress(MapLabel("A"));
            if ((b7 & 0x20) > 0 && (_logPrevByte7 & 0x20) == 0) LogPress(MapLabel("B"));
            if ((b7 & 0x40) > 0 && (_logPrevByte7 & 0x40) == 0) LogPress(MapLabel("X"));
            if ((b7 & 0x80) > 0 && (_logPrevByte7 & 0x80) == 0) LogPress(MapLabel("Y"));

            if (ltNow && !_logPrevLt) LogPress(MapLabel("LT"));
            if (rtNow && !_logPrevRt) LogPress(MapLabel("RT"));



            bool upNow = (b6 & 0x01) > 0;
            bool leftNow = (b6 & 0x04) > 0;
            bool rightNow = (b6 & 0x08) > 0;
            bool upBefore = (_logPrevByte6 & 0x01) > 0;
            bool leftBefore = (_logPrevByte6 & 0x04) > 0;
            bool rightBefore = (_logPrevByte6 & 0x08) > 0;

            if (upNow && leftNow && !(upBefore && leftBefore))
                LogPress(mouseModeFlag ? "DPAD ARRIBA+IZQUIERDA (Alt+←)" : "DPAD ARRIBA+IZQUIERDA");
            if (upNow && rightNow && !(upBefore && rightBefore))
                LogPress(mouseModeFlag ? "DPAD ARRIBA+DERECHA (Alt+→)" : "DPAD ARRIBA+DERECHA");

            _logPrevByte6 = b6;
            _logPrevByte7 = b7;
            _logPrevLt = ltNow;
            _logPrevRt = rtNow;

            // Sticks analógicos — detectar cuando cruzan el umbral
            short leftX = (short)(dataPacket[10] | (dataPacket[11] << 8));
            short leftY = (short)(dataPacket[12] | (dataPacket[13] << 8));
            short rightX = (short)(dataPacket[14] | (dataPacket[15] << 8));
            short rightY = (short)(dataPacket[16] | (dataPacket[17] << 8));

            bool leftActive = Math.Abs((int)leftX) > STICK_LOG_THRESHOLD || Math.Abs((int)leftY) > STICK_LOG_THRESHOLD;
            bool rightActive = Math.Abs((int)rightX) > STICK_LOG_THRESHOLD || Math.Abs((int)rightY) > STICK_LOG_THRESHOLD;

            // LS: log al entrar/salir del umbral
            if (leftActive && !_leftStickActive)
            {
                string dir = GetStickDirection(leftX, leftY);
                LogPress($"LS {dir}");
                _leftStickActive = true;
            }
            else if (!leftActive && _leftStickActive)
            {
                _leftStickActive = false;
            }

            // RS: igual
            if (rightActive && !_rightStickActive)
            {
                string dir = GetStickDirection(rightX, rightY);
                LogPress($"RS {dir}");
                _rightStickActive = true;
            }
            else if (!rightActive && _rightStickActive)
            {
                _rightStickActive = false;
            }
        }

        private string GetStickDirection(short x, short y)
        {
            if (Math.Abs((int)x) > Math.Abs((int)y))
                return x > 0 ? "→" : "←";
            else
                return y > 0 ? "↑" : "↓";
        }
        public void ProcessGamepadData(byte[] dataPacket)
        {
            RegistrarBotonesEnLog(dataPacket);

            // -----------------
            // Modo Mouse o Botones normales
            // -----------------
            if (mouseModeFlag)
            {
                // Todo el Mouse Mode vive ahora en MouseModeHandler
                mouseModeHandler?.Process(dataPacket);
            }
            else
            {
                // COMPORTAMIENTO NORMAL DEL MANDO
                gamepad?.SetButton(Xbox360Button.A, (dataPacket[7] & 0x10) > 0);
                gamepad?.SetButton(Xbox360Button.B, (dataPacket[7] & 0x20) > 0);
                gamepad?.SetButton(Xbox360Button.X, (dataPacket[7] & 0x40) > 0);
                gamepad?.SetButton(Xbox360Button.Y, (dataPacket[7] & 0x80) > 0);

                gamepad?.SetButton(Xbox360Button.Up, (dataPacket[6] & 0x01) > 0);
                gamepad?.SetButton(Xbox360Button.Down, (dataPacket[6] & 0x02) > 0);
                gamepad?.SetButton(Xbox360Button.Left, (dataPacket[6] & 0x04) > 0);
                gamepad?.SetButton(Xbox360Button.Right, (dataPacket[6] & 0x08) > 0);
            }
            
            if (mouseModeFlag)
            {
                if ((dataPacket[6] & 0x10) > 0)
                {
                    if (!startKeySubscribed)
                    {
                        EjecutarTeclaEspecial(StartKeyCustom);
                        startKeySubscribed = true;
                    }
                }
                else if (startKeySubscribed)
                {
                    EjecutarLiberacionEspecial(StartKeyCustom);
                    startKeySubscribed = false;
                }
                gamepad?.SetButton(Xbox360Button.Start, false);
            }
            else
            {
                gamepad?.SetButton(Xbox360Button.Start, (dataPacket[6] & 0x10) > 0);
            }

            // BOTÓN BACK
            if (mouseModeFlag)
            {
                if ((dataPacket[6] & 0x20) > 0)
                {
                    if (!backKeySubscribed)
                    {
                        EjecutarTeclaEspecial(BackKeyCustom);
                        backKeySubscribed = true;
                    }
                }
                else if (backKeySubscribed)
                {
                    EjecutarLiberacionEspecial(BackKeyCustom);
                    backKeySubscribed = false;
                }
                gamepad?.SetButton(Xbox360Button.Back, false);
            }
            else
            {
                gamepad?.SetButton(Xbox360Button.Back, (dataPacket[6] & 0x20) > 0);
            }

            // BOTÓN GUIDE
            bool guideButtonPressedNow = (dataPacket[7] & 0x04) > 0;

            if (guideButtonPressedNow && !guideButtonLastState)
            {
                toggleMouseMode(!mouseModeFlag);
                guidePressStart = DateTime.UtcNow;
                guidePowerOffTriggered = false;
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    "C" + controllerNumber + " GUIDE pulsado.");
            }

            if (guideButtonPressedNow && !guidePowerOffTriggered && guidePressStart != DateTime.MinValue)
            {
                var heldMs = (DateTime.UtcNow - guidePressStart).TotalMilliseconds;

                if ((int)heldMs % 1000 < 50)
                {
                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                        $"GUIDE hold {heldMs / 1000.0:F1}s C{controllerNumber}.");
                }
                if (heldMs >= GUIDE_HOLD_MS)
                {
                    sendData(controllerCommands["DisableController"]);

                    try { killKeepAlive(); } catch { }
                    try { killMouseMode(); } catch { }
                    try { killButtonCombo(); } catch { }
                    // ← eliminada la línea: try { killController(); } catch { }

                    parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                        $"C{controllerNumber} apagado (GUIDE {GUIDE_HOLD_MS / 1000}s).");

                    guidePowerOffTriggered = true;
                }
            }

            if (!guideButtonPressedNow && guidePressStart != DateTime.MinValue)
            {
                guidePressStart = DateTime.MinValue;
                guidePowerOffTriggered = false;
                parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                    $"GUIDE released C{controllerNumber}.");
            }

            guideButtonLastState = guideButtonPressedNow;

            gamepad?.SetButton(Xbox360Button.Guide, mouseModeFlag ? false : guideButtonPressedNow);

            // BUMPERS
            if (mouseModeFlag)
            {
                gamepad?.SetButton(Xbox360Button.LeftShoulder, false);
                gamepad?.SetButton(Xbox360Button.RightShoulder, false);
            }
            else
            {
                gamepad?.SetButton(Xbox360Button.LeftShoulder, (dataPacket[7] & 0x01) > 0);
                gamepad?.SetButton(Xbox360Button.RightShoulder, (dataPacket[7] & 0x02) > 0);
            }

            // STICKS Y GATILLOS
            short leftX = (short)(dataPacket[10] | (dataPacket[11] << 8));
            short leftY = (short)(dataPacket[12] | (dataPacket[13] << 8));
            short rightX = (short)(dataPacket[14] | (dataPacket[15] << 8));
            short rightY = (short)(dataPacket[16] | (dataPacket[17] << 8));
            int leftTrig = dataPacket[8];
            int rightTrig = dataPacket[9];

            double leftDistance = Math.Sqrt((double)(leftX * leftX) + (double)(leftY * leftY));
            if (leftDistance < deadzoneL) { leftX = 0; leftY = 0; }
            else
            {
                if (Math.Abs(Convert.ToInt32(leftX)) < deadzoneL) leftX = 0;
                if (Math.Abs(Convert.ToInt32(leftY)) < deadzoneL) leftY = 0;
            }

            double rightDistance = Math.Sqrt((double)(rightX * rightX) + (double)(rightY * rightY));
            if (rightDistance < deadzoneR) { rightX = 0; rightY = 0; }
            else
            {
                if (Math.Abs(Convert.ToInt32(rightX)) < deadzoneR) rightX = 0;
                if (Math.Abs(Convert.ToInt32(rightY)) < deadzoneR) rightY = 0;
            }

            if (!mouseModeFlag)
            {
                gamepad?.SetAxis(Xbox360Axis.LeftThumbX, leftX);
                gamepad?.SetAxis(Xbox360Axis.LeftThumbY, leftY);
                gamepad?.SetAxis(Xbox360Axis.RightThumbX, rightX);
                gamepad?.SetAxis(Xbox360Axis.RightThumbY, rightY);

                if (triggerAsButton)
                {
                    gamepad?.SetSlider(Xbox360Slider.LeftTrigger, (byte)(leftTrig >= 50 ? 255 : 0));
                    gamepad?.SetSlider(Xbox360Slider.RightTrigger, (byte)(rightTrig >= 50 ? 255 : 0));
                }
                else
                {
                    gamepad?.SetSlider(Xbox360Slider.LeftTrigger, (byte)leftTrig);
                    gamepad?.SetSlider(Xbox360Slider.RightTrigger, (byte)rightTrig);
                }

                gamepad?.SetButton(Xbox360Button.LeftThumb, (dataPacket[6] & 0x40) > 0);
                gamepad?.SetButton(Xbox360Button.RightThumb, (dataPacket[6] & 0x80) > 0);
            }

            cmdMouseModeToggle = ((dataPacket[7] & 0x01) > 0) && ((dataPacket[7] & 0x02) > 0) && ((dataPacket[6] & 0x20) > 0);
            cmdKillController = (leftTrig >= 50) && (rightTrig >= 50) && ((dataPacket[6] & 0x20) > 0);
        }

        private void sendData(byte[] dataToSend)
        {
            var transport = usbTransport;
            if (transport == null) return;

            if (!transport.Send(dataToSend))
            {
                if (!transport.IsDead)
                {
                    parentWindow.BeginInvoke(new logCallback(parentWindow.logMessage),
                        "ERROR: envío USB falló.");
                }
            }
        }

        private void ProcessKeypress(byte key)
        {
            if (key == 0 || chatpadKeysHeld.Contains(key)) return;

            chatpadKeysHeld.Add(key);

            if (chatpadMod["Orange"])
            {
                string s = chatpadMapper.GetOrangeKey(key);
                if (s.Length > 0)
                    SendKeys.SendWait(flagUpperCase ? s.ToUpper() : s);
            }
            else if (chatpadMod["Green"])
            {
                string s = chatpadMapper.GetGreenKey(key);
                if (s.Length > 0)
                    SendKeys.SendWait(flagUpperCase ? s.ToUpper() : s);
            }
            else
            {
                Keys mapped = chatpadMapper.GetKey(key);
                if (mapped != Keys.None)
                {
                    keyboardKeysDown.Add(mapped);
                    chatpadMapper.PressKey(mapped);
                }
            }
        }

        public void configureChatpad(string keyboardType)
        {
            chatpadMapper.ConfigureLayout(keyboardType);
        }

        public void configureGamepad(bool triggerAsBtn)
        {
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
            usbTransport?.Kill();

            // Liberar teclas/estado del Mouse Mode antes de destruir todo
            mouseModeHandler?.Reset();
            mouseModeHandler = null;

            gamepad?.Dispose();
            gamepad = null;

            parentWindow.Invoke(new logCallback(parentWindow.logMessage),
                "C" + controllerNumber + " cerrando.");
        }

        private void tickButtonCombo(CancellationToken token)
        {
            int mouseModeTick = 0;
            int killControllerTick = 0;

            while (!token.IsCancellationRequested)
            {
                if (cmdMouseModeToggle) mouseModeTick++; else mouseModeTick = 0;
                if (mouseModeTick == 3) toggleMouseMode(!mouseModeFlag);

                if (cmdKillController) killControllerTick++; else killControllerTick = 0;
                if (killControllerTick == 6) sendData(controllerCommands["DisableController"]);

                if (token.WaitHandle.WaitOne(500)) break;
            }
        }

        public void killButtonCombo()
        {
            try { _buttonComboCts?.Cancel(); } catch { }
            try { threadButtonCombo?.Join(500); } catch { }
            _buttonComboCts?.Dispose();
            _buttonComboCts = null;
            threadButtonCombo = null;
        }

        private void tickKeepAlive(CancellationToken token)
        {
            bool keepAliveToggle = false;
            while (!token.IsCancellationRequested)
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
                }

                // Sleep cancelable: despierta inmediatamente si cancelan
                if (token.WaitHandle.WaitOne(1000)) break;
            }
        }

        public void killKeepAlive()
        {
            try { _keepAliveCts?.Cancel(); } catch { }
            try { threadKeepAlive?.Join(500); } catch { }
            _keepAliveCts?.Dispose();
            _keepAliveCts = null;
            threadKeepAlive = null;
        }

        private void tickMouseMode(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var handler = mouseModeHandler;
                if (handler != null)
                {
                    int vx = handler.CursorVelocityX;
                    int vy = handler.CursorVelocityY;

                    if (vx != 0 || vy != 0)
                        Mouse.MoveRelative(vx, -vy);
                }

                if (token.WaitHandle.WaitOne(20)) break;
            }
        }

        private void startMouseMode()
        {
            var cts = new CancellationTokenSource();
            _mouseModeCts = cts;

            mouseModeThread = new System.Threading.Thread(() => tickMouseMode(cts.Token));
            mouseModeThread.IsBackground = true;
            mouseModeThread.Name = $"MouseMode-C{controllerNumber}";
            mouseModeThread.Start();

            ParpadearLed("GreenOn", "GreenOff");
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
                LiberarEstadoMouseMode();   // ← NUEVO
                killMouseMode();
                parentWindow.Invoke(new mouseModeLabelCallback(parentWindow.mouseModeUpdate), controllerNumber, false);
            }
        }

        private void LiberarEstadoMouseMode()
        {
            // Suelta modificadores, flechas, clics y resetea los flags del handler
            mouseModeHandler?.Reset();

            // START/BACK personalizados que pudieran haber quedado pulsados
            if (startKeySubscribed) { EjecutarLiberacionEspecial(StartKeyCustom); startKeySubscribed = false; }
            if (backKeySubscribed) { EjecutarLiberacionEspecial(BackKeyCustom); backKeySubscribed = false; }
        }

        public void killMouseMode()
        {
            try { _mouseModeCts?.Cancel(); } catch { }
            try { mouseModeThread?.Join(500); } catch { }
            _mouseModeCts?.Dispose();
            _mouseModeCts = null;
            mouseModeThread = null;

            ParpadearLed("OrangeOn", "OrangeOff");
        }

        private void resetComboButtons()
        {
            cmdMouseModeToggle = false;
            cmdKillController = false;

            // Delegar el reset del Mouse Mode al handler
            mouseModeHandler?.Reset();

            // Limpiar el gamepad virtual
            if (gamepad != null)
            {
                gamepad.SetSlider(Xbox360Slider.LeftTrigger, 0);
                gamepad.SetSlider(Xbox360Slider.RightTrigger, 0);
                gamepad.SetButton(Xbox360Button.Back, false);
                gamepad.SetButton(Xbox360Button.LeftShoulder, false);
                gamepad.SetButton(Xbox360Button.RightShoulder, false);
            }
        }

        // EjecutarTeclaEspecial / EjecutarLiberacionEspecial — usadas por START/BACK en Mouse Mode
        private void EjecutarTeclaEspecial(Keys tecla)
        {
            if (tecla == Keys.M)
            {
                NativeInput.SendKeys(
                    (NativeInput.VK_LWIN, false),
                    (NativeInput.VK_M, false));
                return;
            }

            if (tecla == Keys.Shift || tecla == Keys.LShiftKey) { NativeInput.SendKey(NativeInput.VK_SHIFT, false); return; }
            if (tecla == Keys.Control || tecla == Keys.LControlKey) { NativeInput.SendKey(NativeInput.VK_CONTROL, false); return; }
            if (tecla == Keys.Alt || tecla == Keys.LMenu) { NativeInput.SendKey(NativeInput.VK_MENU, false); return; }

            Keyboard.KeyDown(tecla);
        }

        private void EjecutarLiberacionEspecial(Keys tecla)
        {
            if (tecla == Keys.M)
            {
                NativeInput.SendKeys(
                    (NativeInput.VK_M, true),
                    (NativeInput.VK_LWIN, true));
                return;
            }

            if (tecla == Keys.Shift || tecla == Keys.LShiftKey) { NativeInput.SendKey(NativeInput.VK_SHIFT, true); return; }
            if (tecla == Keys.Control || tecla == Keys.LControlKey) { NativeInput.SendKey(NativeInput.VK_CONTROL, true); return; }
            if (tecla == Keys.Alt || tecla == Keys.LMenu) { NativeInput.SendKey(NativeInput.VK_MENU, true); return; }

            Keyboard.KeyUp(tecla);
        }
    }
}