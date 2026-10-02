using System;
using System.Threading;

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Envuelve un UsbEndpointWriter serializando todas las escrituras.
    /// UsbEndpointWriter.Write NO es thread-safe, así que este wrapper
    /// garantiza que solo un hilo escribe a la vez.
    /// También permite "matar" el transporte para que futuras escrituras
    /// se ignoren silenciosamente sin lanzar excepciones.
    /// </summary>
    internal sealed class UsbTransport
    {
        private readonly object _writeLock = new object();
        private UsbEndpointWriter _writer;
        private volatile bool _dead;

        public UsbTransport(UsbEndpointWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// Escribe datos al endpoint USB. Thread-safe.
        /// Devuelve true si la escritura fue exitosa.
        /// Si el transporte está muerto o falla, devuelve false (no lanza).
        /// </summary>
        public bool Send(byte[] data, int timeoutMs = 2000)
        {
            if (data == null || data.Length == 0) return false;
            if (_dead) return false;

            lock (_writeLock)
            {
                if (_dead || _writer == null) return false;
                
                try
                {
                    ErrorCode ec = _writer.Write(data, timeoutMs, out int _);
                    return ec == ErrorCode.None;
                }
                catch
                {
                    _dead = true;
                    return false;
                }
            }
        }

        /// <summary>
        /// Marca el transporte como muerto. Las escrituras pendientes
        /// o futuras se ignorarán silenciosamente. NO se bloquea esperando
        /// a que termine la escritura en curso.
        /// </summary>
        public void Kill()
        {
            _dead = true;
        }

        /// <summary>
        /// True si el transporte ya no acepta escrituras.
        /// </summary>
        public bool IsDead => _dead;
    }
}