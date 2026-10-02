using System;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Envuelve el gamepad virtual ViGEm. Encapsula el cliente ViGEm y
    /// el IXbox360Controller, gestionando correctamente su ciclo de vida.
    ///
    /// GARANTÍAS:
    ///  - Connect() es idempotente: múltiples llamadas no duplican la conexión.
    ///  - Disconnect() libera el gamepad virtual sin destruir el cliente.
    ///  - El cliente ViGEm vive durante toda la sesión y se libera con Dispose().
    ///
    /// NO toma decisiones sobre qué botones pulsar — eso es responsabilidad
    /// del Controller que lo usa.
    /// </summary>
    internal sealed class GamepadEmitter : IDisposable
    {
        private ViGEmClient _client;
        private IXbox360Controller _gamepad;
        private bool _connected;
        private bool _disposed;

        /// <summary>Número de mando asignado (1-4), solo para logs.</summary>
        public int ControllerNumber { get; }

        /// <summary>True si el gamepad virtual está conectado a ViGEm.</summary>
        public bool IsConnected => _connected && !_disposed;

        public GamepadEmitter(int controllerNumber)
        {
            ControllerNumber = controllerNumber;

            try
            {
                _client = new ViGEmClient();
                _gamepad = _client.CreateXbox360Controller();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al instanciar el bus de emulación virtual ViGEm " +
                    "(¿está instalado ViGEmBus?).", ex);
            }
        }

        // =========================================================
        // CICLO DE VIDA
        // =========================================================

        /// <summary>
        /// Conecta el gamepad virtual. Idempotente: si ya estaba conectado,
        /// no hace nada. Devuelve true si la conexión es efectiva al terminar.
        /// </summary>
        public bool Connect()
        {
            if (_disposed) return false;
            if (_connected) return true;

            try
            {
                _gamepad.Connect();
                _connected = true;
                return true;
            }
            catch
            {
                _connected = false;
                return false;
            }
        }

        /// <summary>
        /// Desconecta el gamepad virtual (pero no destruye el cliente ViGEm,
        /// para poder reconectar después). Idempotente.
        /// </summary>
        public void Disconnect()
        {
            if (_disposed) return;
            if (!_connected) return;

            try
            {
                _gamepad.Disconnect();
            }
            catch
            {
                // Ignorar — el driver puede devolver error si ya está desconectado
            }
            finally
            {
                _connected = false;
            }
        }

        /// <summary>
        /// Libera todos los recursos ViGEm. Después de llamar a esto,
        /// la instancia no es reutilizable.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { if (_connected) _gamepad?.Disconnect(); } catch { }
            // IXbox360Controller NO implementa IDisposable → no llamamos Dispose()
            try { _client?.Dispose(); } catch { }

            _gamepad = null;
            _client = null;
            _connected = false;
        }

        // =========================================================
        // BOTONES / EJES / SLIDERS
        // =========================================================

        /// <summary>
        /// Cambia el estado de un botón del gamepad virtual.
        /// Silencioso si el gamepad no está conectado.
        /// </summary>
        public void SetButton(Xbox360Button button, bool pressed)
        {
            if (!IsConnected) return;
            try { _gamepad.SetButtonState(button, pressed); }
            catch { /* Ignorar si ViGEm falla transitoriamente */ }
        }

        /// <summary>
        /// Cambia el estado de un eje del gamepad virtual.
        /// Silencioso si el gamepad no está conectado.
        /// </summary>
        public void SetAxis(Xbox360Axis axis, short value)
        {
            if (!IsConnected) return;
            try { _gamepad.SetAxisValue(axis, value); }
            catch { }
        }

        /// <summary>
        /// Cambia el estado de un slider (trigger) del gamepad virtual.
        /// Silencioso si el gamepad no está conectado.
        /// </summary>
        public void SetSlider(Xbox360Slider slider, byte value)
        {
            if (!IsConnected) return;
            try { _gamepad.SetSliderValue(slider, value); }
            catch { }
        }

        // =========================================================
        // HELPERS DE CONVENIENCIA
        // =========================================================

        /// <summary>
        /// Silencia todos los botones y ejes del gamepad virtual.
        /// Útil en Mouse Mode, donde el gamepad no debe emitir nada
        /// mientras el teclado/ratón lo hace.
        /// </summary>
        public void SilenceAll()
        {
            if (!IsConnected) return;

            SetButton(Xbox360Button.A, false);
            SetButton(Xbox360Button.B, false);
            SetButton(Xbox360Button.X, false);
            SetButton(Xbox360Button.Y, false);

            SetButton(Xbox360Button.Up, false);
            SetButton(Xbox360Button.Down, false);
            SetButton(Xbox360Button.Left, false);
            SetButton(Xbox360Button.Right, false);

            SetButton(Xbox360Button.Start, false);
            SetButton(Xbox360Button.Back, false);
            SetButton(Xbox360Button.Guide, false);

            SetButton(Xbox360Button.LeftShoulder, false);
            SetButton(Xbox360Button.RightShoulder, false);
            SetButton(Xbox360Button.LeftThumb, false);
            SetButton(Xbox360Button.RightThumb, false);

            SetSlider(Xbox360Slider.LeftTrigger, 0);
            SetSlider(Xbox360Slider.RightTrigger, 0);

            SetAxis(Xbox360Axis.LeftThumbX, 0);
            SetAxis(Xbox360Axis.LeftThumbY, 0);
            SetAxis(Xbox360Axis.RightThumbX, 0);
            SetAxis(Xbox360Axis.RightThumbY, 0);
        }
    }
}