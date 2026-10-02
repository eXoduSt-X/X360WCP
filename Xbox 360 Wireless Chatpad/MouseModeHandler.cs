using System;
using System.Windows.Forms;

using InputManager;

using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Encapsula toda la lógica del "Mouse Mode" del mando Xbox 360.
    ///
    /// En Mouse Mode, el mando funciona como ratón + teclado en vez de gamepad:
    ///   - Stick izquierdo mueve el cursor
    ///   - X/B = clic izq/der, A/Y = scroll
    ///   - D-Pad = flechas (con Alt+← / Alt+→)
    ///   - LT/RB = Ctrl/Shift sostenido
    ///   - RT = Alt+Tab
    ///   - Stick derecho ↑↓ = volumen, ←→ = media prev/next
    ///   - L3/R3 = Win+M / Alt+F4
    ///
    /// El handler NO decide cuándo se activa Mouse Mode (eso lo hace Controller
    /// con el botón GUIDE). Solo procesa paquetes cuando ya está activo.
    /// </summary>
    internal sealed class MouseModeHandler
    {
        // =========================================================
        // DEPENDENCIAS
        // =========================================================

        private readonly Action<string> _log;
        private readonly GamepadEmitter _gamepad;
        private readonly Control _uiInvoker;   // ← NUEVO

        // =========================================================
        // ESTADO INTERNO
        // =========================================================

        // Botones del ratón
        private bool _leftButtonDown;
        private bool _rightButtonDown;

        // D-Pad con flechas simples
        private bool _dpadUpSubscribed;
        private bool _dpadDownSubscribed;
        private bool _dpadLeftSubscribed;
        private bool _dpadRightSubscribed;

        // D-Pad con Alt+← / Alt+→
        private bool _dpadAltLeftHeld;
        private bool _dpadAltRightHeld;

        // Modificadores sostenidos
        private bool _shiftHeld;      // RB
        private bool _ctrlHeld;       // LT

        // Alt+Tab
        private bool _altTabArmed = true;

        // Volumen (stick derecho Y)
        private const int VOLUME_THRESHOLD = 8000;
        private bool _volumeUpArmed = true;
        private bool _volumeDownArmed = true;

        // Media prev/next (stick derecho X)
        private const int MEDIA_THRESHOLD = 30000;
        private bool _mediaNextArmed = true;
        private bool _mediaPrevArmed = true;

        // L3 / R3 edge detection
        private bool _leftThumbLast;
        private bool _rightThumbLast;

        private int _deadzoneL;
        private int _deadzoneR;

        /// <summary>True si Mouse Mode está reteniendo Shift (RB pulsado).</summary>
        public bool IsShiftHeld => _shiftHeld;

        // =========================================================
        // CURSOR — el hilo de Mouse Mode lo lee desde Controller
        // =========================================================

        /// <summary>Velocidad X del cursor (pixeles cada 20ms). Puede ser positiva o negativa.</summary>
        public int CursorVelocityX { get; private set; }

        /// <summary>Velocidad Y del cursor (pixeles cada 20ms). Puede ser positiva o negativa.</summary>
        public int CursorVelocityY { get; private set; }

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public MouseModeHandler(GamepadEmitter gamepad, Control uiInvoker, Action<string> log)
        {
            _gamepad = gamepad;
            _uiInvoker = uiInvoker;
            _log = log ?? (_ => { });
        }

        // =========================================================
        // PROCESAMIENTO PRINCIPAL
        // =========================================================

        /// <summary>
        /// Procesa un paquete de gamepad en Mouse Mode.
        /// Asume que ya se validó que mouseModeFlag está activo.
        /// </summary>
        public void Process(byte[] dataPacket)
        {
            ProcessMouseButtons(dataPacket);
            ProcessDpad(dataPacket);
            ProcessCursor(dataPacket);
            ProcessRightBumper(dataPacket);   
            ProcessLeftTrigger(dataPacket);
            ProcessRightTrigger(dataPacket);
            ProcessRightStickMedia(dataPacket);
            ProcessThumbClicks(dataPacket);

            // Silenciar el gamepad virtual (no debe emitir nada en Mouse Mode)
            _gamepad?.SilenceAll();
        }

        /// <summary>Actualiza los deadzones aplicados al cursor.</summary>
        public void SetDeadzones(int dzL, int dzR)
        {
            _deadzoneL = dzL;
            _deadzoneR = dzR;
        }
        /// <summary>
        /// Ejecuta la acción en el hilo UI si hace falta. SendInput/keybd_event
        /// no llegan a Windows desde el hilo del driver USB sin esto.
        /// </summary>
        private void OnUI(Action action)
        {
            action();   // SendInput funciona desde cualquier hilo; sin depender de la UI
        }
        /// <summary>
        /// Libera teclas modificadoras y resetea todo el estado arm.
        /// Llamar cuando se desactiva Mouse Mode o se pierde la conexión.
        /// </summary>
        public void Reset()
        {
            // Soltar modificadores por si quedaron presionados
            if (_ctrlHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_CONTROL, true)); _ctrlHeld = false; }
            if (_shiftHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_LSHIFT, true)); _shiftHeld = false; }
            if (_dpadAltLeftHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_LEFT, true)); _dpadAltLeftHeld = false; }
            if (_dpadAltRightHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_RIGHT, true)); _dpadAltRightHeld = false; }

            // Soltar flechas si quedaron suscritas
            if (_dpadUpSubscribed) { Keyboard.KeyUp(Keys.Up); _dpadUpSubscribed = false; }
            if (_dpadDownSubscribed) { Keyboard.KeyUp(Keys.Down); _dpadDownSubscribed = false; }
            if (_dpadLeftSubscribed) { Keyboard.KeyUp(Keys.Left); _dpadLeftSubscribed = false; }
            if (_dpadRightSubscribed) { Keyboard.KeyUp(Keys.Right); _dpadRightSubscribed = false; }

            // Soltar clics si quedaron presionados
            if (_leftButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Left); _leftButtonDown = false; }
            if (_rightButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Right); _rightButtonDown = false; }

            // Resetear arm flags
            _altTabArmed = true;
            _volumeUpArmed = true;
            _volumeDownArmed = true;
            _mediaNextArmed = true;
            _mediaPrevArmed = true;
            _leftThumbLast = false;
            _rightThumbLast = false;

            CursorVelocityX = 0;
            CursorVelocityY = 0;
        }

        // =========================================================
        // SUB-PROCESOS
        // =========================================================

        /// <summary>X = clic izquierdo, B = clic derecho, A/Y = scroll.</summary>
        private void ProcessMouseButtons(byte[] dataPacket)
        {
            // X → clic izquierdo
            if ((dataPacket[7] & 0x40) > 0)
            {
                if (!_leftButtonDown) { Mouse.ButtonDown(Mouse.MouseKeys.Left); _leftButtonDown = true; }
            }
            else if (_leftButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Left); _leftButtonDown = false; }

            // B → clic derecho
            if ((dataPacket[7] & 0x20) > 0)
            {
                if (!_rightButtonDown) { Mouse.ButtonDown(Mouse.MouseKeys.Right); _rightButtonDown = true; }
            }
            else if (_rightButtonDown) { Mouse.ButtonUp(Mouse.MouseKeys.Right); _rightButtonDown = false; }

            // A → scroll abajo
            if ((dataPacket[7] & 0x10) > 0)
                Mouse.Scroll(InputManager.Mouse.ScrollDirection.Down);

            // Y → scroll arriba
            if ((dataPacket[7] & 0x80) > 0)
                Mouse.Scroll(InputManager.Mouse.ScrollDirection.Up);
        }

        /// <summary>
        /// D-Pad:
        ///   Arriba+Izquierda = Alt+←
        ///   Arriba+Derecha = Alt+→
        ///   Sueltas = flechas
        ///   Abajo = Down
        /// </summary>
        private void ProcessDpad(byte[] dataPacket)
        {
            bool up = (dataPacket[6] & 0x01) > 0;
            bool down = (dataPacket[6] & 0x02) > 0;
            bool left = (dataPacket[6] & 0x04) > 0;
            bool right = (dataPacket[6] & 0x08) > 0;

            bool altLeftCombo = up && left;
            bool altRightCombo = up && right;

            // --- Combo Alt+← ---
            if (altLeftCombo)
            {
                if (_dpadUpSubscribed) { Keyboard.KeyUp(Keys.Up); _dpadUpSubscribed = false; }
                if (_dpadLeftSubscribed) { Keyboard.KeyUp(Keys.Left); _dpadLeftSubscribed = false; }
                if (_dpadAltRightHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_RIGHT, true)); _dpadAltRightHeld = false; }
                if (!_dpadAltLeftHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_LEFT, false)); _dpadAltLeftHeld = true; }
            }
            else if (_dpadAltLeftHeld)
            {
                OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_LEFT, true));
                _dpadAltLeftHeld = false;
            }

            // --- Combo Alt+→ ---
            if (altRightCombo)
            {
                if (_dpadUpSubscribed) { Keyboard.KeyUp(Keys.Up); _dpadUpSubscribed = false; }
                if (_dpadRightSubscribed) { Keyboard.KeyUp(Keys.Right); _dpadRightSubscribed = false; }
                if (_dpadAltLeftHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_LEFT, true)); _dpadAltLeftHeld = false; }
                if (!_dpadAltRightHeld) { OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_RIGHT, false)); _dpadAltRightHeld = true; }
            }
            else if (_dpadAltRightHeld)
            {
                OnUI(() => NativeInput.HoldChord(NativeInput.VK_MENU, NativeInput.VK_RIGHT, true));
                _dpadAltRightHeld = false;
            }

            // --- Flechas simples (solo si no hay combos activos) ---
            if (!altLeftCombo && !altRightCombo)
            {
                if (up && !left && !right)
                {
                    if (!_dpadUpSubscribed) { Keyboard.KeyDown(Keys.Up); _dpadUpSubscribed = true; }
                }
                else if (_dpadUpSubscribed) { Keyboard.KeyUp(Keys.Up); _dpadUpSubscribed = false; }

                if (left && !up)
                {
                    if (!_dpadLeftSubscribed) { Keyboard.KeyDown(Keys.Left); _dpadLeftSubscribed = true; }
                }
                else if (_dpadLeftSubscribed) { Keyboard.KeyUp(Keys.Left); _dpadLeftSubscribed = false; }

                if (right && !up)
                {
                    if (!_dpadRightSubscribed) { Keyboard.KeyDown(Keys.Right); _dpadRightSubscribed = true; }
                }
                else if (_dpadRightSubscribed) { Keyboard.KeyUp(Keys.Right); _dpadRightSubscribed = false; }
            }
            else
            {
                if (_dpadUpSubscribed) { Keyboard.KeyUp(Keys.Up); _dpadUpSubscribed = false; }
                if (_dpadLeftSubscribed) { Keyboard.KeyUp(Keys.Left); _dpadLeftSubscribed = false; }
                if (_dpadRightSubscribed) { Keyboard.KeyUp(Keys.Right); _dpadRightSubscribed = false; }
            }

            // Down (independiente)
            if (down)
            {
                if (!_dpadDownSubscribed) { Keyboard.KeyDown(Keys.Down); _dpadDownSubscribed = true; }
            }
            else if (_dpadDownSubscribed) { Keyboard.KeyUp(Keys.Down); _dpadDownSubscribed = false; }
        }

        /// <summary>Stick izquierdo mueve el cursor (LB acelera, RB frena).</summary>
        /// <summary>Stick izquierdo mueve el cursor (LB acelera, RB frena).</summary>
        private void ProcessCursor(byte[] dataPacket)
        {
            short leftX = (short)(dataPacket[10] | (dataPacket[11] << 8));
            short leftY = (short)(dataPacket[12] | (dataPacket[13] << 8));

            // Aplicar el deadzone configurado por los sliders
            double distance = Math.Sqrt((double)(leftX * leftX) + (double)(leftY * leftY));
            if (distance < _deadzoneL)
            {
                leftX = 0;
                leftY = 0;
            }
            else
            {
                if (Math.Abs((int)leftX) < _deadzoneL) leftX = 0;
                if (Math.Abs((int)leftY) < _deadzoneL) leftY = 0;
            }

            // Si el stick está dentro del deadzone, el cursor se detiene ya
            if (leftX == 0 && leftY == 0)
            {
                CursorVelocityX = 0;
                CursorVelocityY = 0;
                return;
            }

            int maxVelocity = 10;
            if ((dataPacket[7] & 0x01) > 0) maxVelocity = 16;   // LB → boost
            else if ((dataPacket[7] & 0x02) > 0) maxVelocity = 5; // RB → slow

            CursorVelocityX = maxVelocity * leftX / 32767;
            CursorVelocityY = maxVelocity * leftY / 32767;
        }
        /// <summary>RB = Shift sostenido mientras se pulsa.</summary>

        private void ProcessRightBumper(byte[] dataPacket)
        {
            bool pressed = (dataPacket[7] & 0x02) > 0;

            if (pressed)
            {
                if (!_shiftHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_LSHIFT, false)); _shiftHeld = true; }
            }
            else if (_shiftHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_LSHIFT, true)); _shiftHeld = false; }
        }

        /// <summary>LT = Control sostenido mientras se pulsa.</summary>
        private void ProcessLeftTrigger(byte[] dataPacket)
        {
            bool pressed = dataPacket[8] >= 50;

            if (pressed)
            {
                if (!_ctrlHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_CONTROL, false)); _ctrlHeld = true; }
            }
            else if (_ctrlHeld) { OnUI(() => NativeInput.HoldKey(NativeInput.VK_CONTROL, true)); _ctrlHeld = false; }
        }

        /// <summary>RT = Alt+Tab (una vez por pulsación).</summary>
        private void ProcessRightTrigger(byte[] dataPacket)
        {
            bool pressed = dataPacket[9] >= 50;
            if (pressed && _altTabArmed)
            {
                OnUI(() => NativeInput.PressChord(NativeInput.VK_MENU, NativeInput.VK_TAB));
                _altTabArmed = false;
            }
            else if (!pressed)
            {
                _altTabArmed = true;
            }
        }

        /// <summary>
        /// Stick derecho:
        ///   Y arriba/abajo = volumen
        ///   X izq/der = media prev/next
        /// </summary>
        private void ProcessRightStickMedia(byte[] dataPacket)
        {
            short rightXRaw = (short)(dataPacket[14] | (dataPacket[15] << 8));
            short rightYRaw = (short)(dataPacket[16] | (dataPacket[17] << 8));

            // Volumen (Y)
            if (rightYRaw > VOLUME_THRESHOLD)
            {
                if (_volumeUpArmed)
                {
                    NativeInput.SendAppCommand(NativeInput.AppCommand.VolumeUp);
                    _volumeUpArmed = false;
                }
                _volumeDownArmed = true;
            }
            else if (rightYRaw < -VOLUME_THRESHOLD)
            {
                if (_volumeDownArmed)
                {
                    NativeInput.SendAppCommand(NativeInput.AppCommand.VolumeDown);
                    _volumeDownArmed = false;
                }
                _volumeUpArmed = true;
            }
            else
            {
                _volumeUpArmed = true;
                _volumeDownArmed = true;
            }

            // Media (X)
            if (rightXRaw > MEDIA_THRESHOLD)
            {
                if (_mediaNextArmed)
                {
                    NativeInput.SendAppCommand(NativeInput.AppCommand.MediaNext);
                    _mediaNextArmed = false;
                }
                _mediaPrevArmed = true;
            }
            else if (rightXRaw < -MEDIA_THRESHOLD)
            {
                if (_mediaPrevArmed)
                {
                    NativeInput.SendAppCommand(NativeInput.AppCommand.MediaPrevious);
                    _mediaPrevArmed = false;
                }
                _mediaNextArmed = true;
            }
            else
            {
                _mediaNextArmed = true;
                _mediaPrevArmed = true;
            }
        }

        /// <summary>L3 = Win+M, R3 = Alt+F4 (una vez por pulsación).</summary>
        private void ProcessThumbClicks(byte[] dataPacket)
        {
            bool leftNow = (dataPacket[6] & 0x40) > 0;
            if (leftNow && !_leftThumbLast)
            {
                _log?.Invoke($"[L3] → Win+M");
                OnUI(() => NativeInput.PressChord(NativeInput.VK_LWIN, NativeInput.VK_M));
            }
            _leftThumbLast = leftNow;

            bool rightNow = (dataPacket[6] & 0x80) > 0;
            if (rightNow && !_rightThumbLast)
            {
                _log?.Invoke($"[R3] → Alt+F4");
                OnUI(() => NativeInput.PressChord(NativeInput.VK_MENU, NativeInput.VK_F4));
            }
            _rightThumbLast = rightNow;
        }
    }
}