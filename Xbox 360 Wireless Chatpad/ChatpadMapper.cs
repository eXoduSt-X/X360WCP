using System.Collections.Generic;
using System.Windows.Forms;

using InputManager;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Traduce las teclas del chatpad de Xbox 360 a teclas de Windows.
    /// Contiene los 3 layouts disponibles y resuelve la tecla final
    /// considerando modificadores (Green, Orange, Shift, Caps).
    ///
    /// NO mantiene estado del mando — solo mapea y envía pulsaciones.
    /// </summary>
    internal sealed class ChatpadMapper
    {
        // Mapeo directo (sin modificador)
        private readonly Dictionary<int, Keys> keyMap = new Dictionary<int, Keys>();

        // Mapeo con Green (símbolos especiales)
        private readonly Dictionary<int, string> greenMap = new Dictionary<int, string>();

        // Mapeo con Orange (letras acentuadas / símbolos alternativos)
        private readonly Dictionary<int, string> orangeMap = new Dictionary<int, string>();

        // =========================================================
        // CONFIGURACIÓN DE LAYOUT
        // =========================================================

        /// <summary>
        /// Carga uno de los 3 layouts disponibles:
        ///   "Q W E R T Y"
        ///   "Q W E R T Z"
        ///   "A Z E R T Y"
        /// </summary>
        public void ConfigureLayout(string keyboardType)
        {
            keyMap.Clear();
            greenMap.Clear();
            orangeMap.Clear();

            switch (keyboardType)
            {
                case "Q W E R T Y":
                    LoadQwerty();
                    break;

                case "Q W E R T Z":
                    LoadQwertz();
                    break;

                case "A Z E R T Y":
                    LoadAzerty();
                    break;

                default:
                    // Layout desconocido → cargar el más común
                    LoadQwerty();
                    break;
            }
        }

        // =========================================================
        // PROCESAMIENTO DE TECLAS
        // =========================================================

        /// <summary>
        /// Devuelve la tecla de Windows para una tecla "cruda" del chatpad
        /// SIN modificadores. Devuelve Keys.None si el código no está mapeado.
        /// </summary>
        public Keys GetKey(int chatpadCode)
        {
            return keyMap.TryGetValue(chatpadCode, out var k) ? k : Keys.None;
        }

        /// <summary>
        /// Devuelve la cadena SendKeys para la tecla con modificador Green.
        /// Devuelve string.Empty si no hay mapeo.
        /// </summary>
        public string GetGreenKey(int chatpadCode)
        {
            return greenMap.TryGetValue(chatpadCode, out var s) ? s : string.Empty;
        }

        /// <summary>
        /// Devuelve la cadena SendKeys para la tecla con modificador Orange.
        /// Devuelve string.Empty si no hay mapeo.
        /// </summary>
        public string GetOrangeKey(int chatpadCode)
        {
            return orangeMap.TryGetValue(chatpadCode, out var s) ? s : string.Empty;
        }

        /// <summary>
        /// Pulsa una tecla (sin modificador) usando Keyboard.KeyDown.
        /// </summary>
        public void PressKey(Keys key)
        {
            if (key == Keys.None) return;
            Keyboard.KeyDown(key);
        }

        /// <summary>
        /// Suelta una tecla previamente pulsada.
        /// </summary>
        public void ReleaseKey(Keys key)
        {
            if (key == Keys.None) return;
            Keyboard.KeyUp(key);
        }

        // =========================================================
        // LAYOUTS
        // =========================================================

        private void LoadQwerty()
        {
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
        }

        private void LoadQwertz()
        {
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

            keyMap[39] = Keys.Q; greenMap[39] = "!"; orangeMap[39] = "@";
            keyMap[38] = Keys.W; greenMap[38] = "\""; orangeMap[38] = "¡";
            keyMap[37] = Keys.E; greenMap[37] = "€"; orangeMap[37] = "é";
            keyMap[36] = Keys.R; greenMap[36] = "$"; orangeMap[36] = "¥";
            keyMap[35] = Keys.T; greenMap[35] = "{%}"; orangeMap[35] = "Þ";
            keyMap[34] = Keys.Z; greenMap[34] = "&"; orangeMap[34] = "{^}";
            keyMap[33] = Keys.U; greenMap[33] = "/"; orangeMap[33] = "ü";
            keyMap[118] = Keys.I; greenMap[118] = "{(}"; orangeMap[118] = "í";
            keyMap[117] = Keys.O; greenMap[117] = "{)}"; orangeMap[117] = "ö";
            keyMap[100] = Keys.P; greenMap[100] = "="; orangeMap[100] = "\\";

            keyMap[55] = Keys.A; greenMap[55] = "å"; orangeMap[55] = "ä";
            keyMap[54] = Keys.S; greenMap[54] = "ß"; orangeMap[54] = "š";
            keyMap[53] = Keys.D; greenMap[53] = "«"; orangeMap[53] = "ð";
            keyMap[52] = Keys.F; greenMap[52] = "»"; orangeMap[52] = "£";
            keyMap[51] = Keys.G; greenMap[51] = "¨"; orangeMap[51] = "☺";
            keyMap[50] = Keys.H; greenMap[50] = "{{}"; orangeMap[50] = "`";
            keyMap[49] = Keys.J; greenMap[49] = "{}}"; orangeMap[49] = "ø";
            keyMap[119] = Keys.K; greenMap[119] = "{[}"; orangeMap[119] = "æ";
            keyMap[114] = Keys.L; greenMap[114] = "{]}"; orangeMap[114] = "œ";
            keyMap[98] = Keys.Oemcomma; greenMap[98] = "':"; orangeMap[98] = "#;";

            keyMap[70] = Keys.Y; greenMap[70] = "<"; orangeMap[70] = "°";
            keyMap[69] = Keys.X; greenMap[69] = ">"; orangeMap[69] = "|";
            keyMap[68] = Keys.C; greenMap[68] = "{~}"; orangeMap[68] = "ç";
            keyMap[67] = Keys.V; greenMap[67] = "-"; orangeMap[67] = "_";
            keyMap[66] = Keys.B; greenMap[66] = "*"; orangeMap[66] = "{+}";
            keyMap[65] = Keys.N; greenMap[65] = ";"; orangeMap[65] = "ñ";
            keyMap[82] = Keys.M; greenMap[82] = ":"; orangeMap[82] = "µ";
            keyMap[83] = Keys.OemPeriod; greenMap[83] = "?"; orangeMap[83] = "¿";
            keyMap[99] = Keys.Enter; greenMap[99] = ""; orangeMap[99] = "";

            keyMap[85] = Keys.Left; greenMap[85] = ""; orangeMap[85] = "";
            keyMap[84] = Keys.Space; greenMap[84] = ""; orangeMap[84] = "";
            keyMap[81] = Keys.Right; greenMap[81] = ""; orangeMap[81] = "";
            keyMap[113] = Keys.Back; greenMap[113] = ""; orangeMap[113] = "";
        }

        private void LoadAzerty()
        {
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

            keyMap[39] = Keys.A; greenMap[39] = "à"; orangeMap[39] = "&";
            keyMap[38] = Keys.Z; greenMap[38] = "æ"; orangeMap[38] = "{~}";
            keyMap[37] = Keys.E; greenMap[37] = "€"; orangeMap[37] = "\"";
            keyMap[36] = Keys.R; greenMap[36] = "é"; orangeMap[36] = "$";
            keyMap[35] = Keys.T; greenMap[35] = "#"; orangeMap[35] = "{(}";
            keyMap[34] = Keys.Y; greenMap[34] = "ý"; orangeMap[34] = "-";
            keyMap[33] = Keys.U; greenMap[33] = "ù"; orangeMap[33] = "`";
            keyMap[118] = Keys.I; greenMap[118] = "ì"; orangeMap[118] = "_";
            keyMap[117] = Keys.O; greenMap[117] = "œ"; orangeMap[117] = "@";
            keyMap[100] = Keys.P; greenMap[100] = "ó"; orangeMap[100] = "{)}";

            keyMap[55] = Keys.Q; greenMap[55] = "å"; orangeMap[55] = "☺";
            keyMap[54] = Keys.S; greenMap[54] = "š"; orangeMap[54] = "«";
            keyMap[53] = Keys.D; greenMap[53] = "ð"; orangeMap[53] = "»";
            keyMap[52] = Keys.F; greenMap[52] = "Þ"; orangeMap[52] = "{{}";
            keyMap[51] = Keys.G; greenMap[51] = "¨"; orangeMap[51] = "¥";
            keyMap[50] = Keys.H; greenMap[50] = "|"; orangeMap[50] = "ø";
            keyMap[49] = Keys.J; greenMap[49] = "µ"; orangeMap[49] = "¨";
            keyMap[119] = Keys.K; greenMap[119] = "/"; orangeMap[119] = "\\";
            keyMap[114] = Keys.L; greenMap[114] = "$"; orangeMap[114] = "£";
            keyMap[98] = Keys.M; greenMap[98] = "*"; orangeMap[98] = "{^}";

            keyMap[70] = Keys.W; greenMap[70] = "¡"; orangeMap[70] = "<";
            keyMap[69] = Keys.X; greenMap[69] = "¿"; orangeMap[69] = ">";
            keyMap[68] = Keys.C; greenMap[68] = "ç"; orangeMap[68] = "{[}";
            keyMap[67] = Keys.V; greenMap[67] = "="; orangeMap[67] = "{]}";
            keyMap[66] = Keys.B; greenMap[66] = "{+}"; orangeMap[66] = "{%}";
            keyMap[65] = Keys.N; greenMap[65] = "?"; orangeMap[65] = "ñ";
            keyMap[82] = Keys.Oemcomma; greenMap[82] = "!"; orangeMap[82] = "'";
            keyMap[83] = Keys.OemPeriod; greenMap[83] = ":"; orangeMap[83] = ";";
            keyMap[99] = Keys.Enter; greenMap[99] = ""; orangeMap[99] = "";

            keyMap[85] = Keys.Left; greenMap[85] = ""; orangeMap[85] = "";
            keyMap[84] = Keys.Space; greenMap[84] = ""; orangeMap[84] = "";
            keyMap[81] = Keys.Right; greenMap[81] = ""; orangeMap[81] = "";
            keyMap[113] = Keys.Back; greenMap[113] = ""; orangeMap[113] = "";
        }
    }
}