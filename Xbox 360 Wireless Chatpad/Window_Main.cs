using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    delegate void controllerDisconnectCallback(int ctrl);
    delegate void controllerConnectCallback(int ctrl);
    delegate void mouseModeLabelCallback(int ctrl, bool modeStatus);
    delegate void logCallback(string message);

    public partial class Window_Main : Form
    {
        // =========================================================
        // CAMPOS
        // =========================================================
        private string _toastText = "";
        private Receiver xboxReceiver;
        private Controller[] xboxControllers = new Controller[4];

        private DeadzoneBar barDeadzoneL;
        private DeadzoneBar barDeadzoneR;

        private Image _cachedXbg;
        private Image _cachedJoy;
        private Bitmap _scaledXbg;
        private Bitmap _scaledJoy;

#pragma warning disable 0414
        private bool _allowExit = false;
#pragma warning restore 0414

        // Tema
        private Color _themeBgPrincipal = Color.FromArgb(56, 56, 56);
        private Color _themeBgContenedor = Color.FromArgb(30, 31, 32);
        private Color _themeTextPrincipal = Color.FromArgb(227, 227, 227);
        private Color _themeTextSecundario = Color.FromArgb(148, 150, 154);
        private Color _themeColorMarco = Color.FromArgb(60, 64, 67);

        // Estado de toggles
        private bool _colorsVisible = false;
        private bool _logVisible = false;
        private int _activeControl = 1;
        private bool _suppressDeadzoneEvents = false;

        // Centrado
        private Size tamañoDiseñoOriginal;
        private Dictionary<Control, Point> posicionesOriginales;
        private Point offsetCentrado = Point.Empty;

        // Modos
        private Dictionary<Control, (string gamepad, string mouse)> textosPorModo;

        // Toast (texto flotante al pulsar botones)
        private Timer _toastTimer;
        private int _toastRemaining;

        // Imágenes configurables
        private const string ConfigImagenesPath = "imagenes.txt";

        // Paleta de colores
        private static readonly Color[] paletaColores = new Color[]
        {
            Color.Black,
            Color.FromArgb(0, 128, 0),
            Color.FromArgb(20, 20, 20),
            Color.FromArgb(0, 120, 255),
            Color.FromArgb(120, 20, 20),
            Color.FromArgb(90, 30, 120),
            Color.FromArgb(180, 100, 0),
            Color.FromArgb(0, 100, 100),
        };

        // Nombres de líneas conectoras (para colorear)
        private static readonly string[] nombresLineasConectoras = new string[]
        {
            "label1","label2","label3","label4","label5","label6","label7","label8",
            "label9","label10","label13","label14","label15","label16",
            "label17","label18","label19","label20","label21","label22","label23","label24",
            "LINE","label28","label29"
        };

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public Window_Main()
        {
            InitializeComponent();

            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.MinimumSize = new Size(1380, 805);
            this.MaximumSize = new Size(1380, 805);

            try { CargarImagenesFondo(); }
            catch
            {
                _cachedXbg = null;
                _cachedJoy = null;
                _scaledXbg = null;
                _scaledJoy = null;
            }

            WP.Click -= WP_Click;
            WP.Click += WP_Click;
            JOY.Click -= JOY_Click;
            JOY.Click += JOY_Click;

            InicializarTextosModo();
            InicializarSelectorColores();
            InicializarIconoBandeja();
            ApplyDarkTheme();

            try
            {
                xboxControllers[0] = new Controller(this);
                xboxControllers[1] = new Controller(this);
                xboxControllers[2] = new Controller(this);
                xboxControllers[3] = new Controller(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Xbox 360 Wireless Chatpad could not be loaded.\n\n" +
                    "The ViGEmBus driver is not installed or enabled on this system. " +
                    "Please make sure you have the ViGEmBus driver installed (DS4Windows standard driver) " +
                    "for this application to emulate native XInput controllers.\n\nDetails: " + ex.Message,
                    "Xbox 360 Wireless Chatpad Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        // =========================================================
        // TEXTOS SEGÚN MODO
        // =========================================================
     
        private void InicializarTextosModo()
        {
            textosPorModo = new Dictionary<Control, (string, string)>
            {
                { LT,       ("LT",     "CTRL") },
                { RT,       ("RT",     "ALT + TAB") },
                { LB,       ("LB",     "CURSOR RÁPIDO") },
                { RB,       ("RB",     "SHIFT / LENTO") },
                { B,        ("B",      "RIGHT CLICK") },
                { Y,        ("Y",      "SCROLL UP") },
                { X,        ("X",      "LEFT CLICK") },
                { A,        ("A",      "SCROLL DOWN") },
                { label26,  ("R3",     "ALT + F4") },
                { LS,       ("LS",     "MOUSE MOVE") },
                { L3,       ("L3",     "MINIMIZAR") },
                { DPAD,     ("DPAD",   "← + ALT + →") },
                { BACK,     ("BACK",   "DELETE") },
                { START,    ("Gamepad Mode", "Mouse Mode") },
                { GUIDE,    ("GUIDE",  "GAMEPAD/MOUSE") },
                { label25,  ("RS Y",   "VOL + / -") },
                { label27,  ("RS X",   "MEDIA PREV - NEXT") },
            };
        }

        // =========================================================
        // ICONO DE BANDEJA
        // =========================================================

        private void InicializarIconoBandeja()
        {
            Icon icono = null;
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(iconPath))
                    icono = new Icon(iconPath);
            }
            catch { }

            if (icono == null)
            {
                try { icono = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
                catch { }
            }

            if (icono != null)
            {
                this.Icon = icono;
                trayIcon.Icon = (Icon)icono.Clone();
            }

            trayIcon.Text = "Xbox 360 Wireless Chatpad (XCONTROL)";
            trayIcon.Visible = true;

            var showItem = new ToolStripMenuItem("Show");
            showItem.Click += (s, e) => RestaurarVentana();
            trayIconMenu.Items.Insert(0, showItem);
        }

        // =========================================================
        // IMÁGENES DE FONDO
        // =========================================================

        private Bitmap EscalarAltaCalidad(Image origen, Size destino)
        {
            var bmp = new Bitmap(destino.Width, destino.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(origen, new Rectangle(Point.Empty, destino));
            }
            return bmp;
        }

        private void CargarImagenesFondo()
        {
            string rutaXbg = null, rutaJoy = null;
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigImagenesPath);

            if (File.Exists(configPath))
            {
                foreach (var linea in File.ReadAllLines(configPath))
                {
                    if (!linea.Contains("=")) continue;
                    var partes = linea.Split(new[] { '=' }, 2);
                    if (partes[0].Trim().Equals("xbg", StringComparison.OrdinalIgnoreCase) && File.Exists(partes[1]))
                        rutaXbg = partes[1];
                    else if (partes[0].Trim().Equals("joy", StringComparison.OrdinalIgnoreCase) && File.Exists(partes[1]))
                        rutaJoy = partes[1];
                }
            }

            _scaledXbg?.Dispose();
            _scaledJoy?.Dispose();

            using (Image xbgSrc = rutaXbg != null ? Image.FromFile(rutaXbg) : Properties.Resources.XBG)
                _scaledXbg = EscalarAltaCalidad(xbgSrc, new Size(1366, 768));

            using (Image joySrc = rutaJoy != null ? Image.FromFile(rutaJoy) : Properties.Resources.JOY)
                _scaledJoy = EscalarAltaCalidad(joySrc, new Size(710, 520));

            _cachedXbg = _scaledXbg;
            _cachedJoy = _scaledJoy;
        }

        private void CambiarImagenFondo(bool esXbg)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = esXbg ? "Elegir nueva imagen de fondo" : "Elegir nueva imagen de joystick";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigImagenesPath);
                var lineas = File.Exists(configPath)
                    ? new List<string>(File.ReadAllLines(configPath))
                    : new List<string>();

                string clave = esXbg ? "xbg" : "joy";
                bool encontrada = false;
                for (int i = 0; i < lineas.Count; i++)
                {
                    if (lineas[i].StartsWith(clave + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        lineas[i] = clave + "=" + ofd.FileName;
                        encontrada = true;
                        break;
                    }
                }
                if (!encontrada) lineas.Add(clave + "=" + ofd.FileName);
                File.WriteAllLines(configPath, lineas);

                CargarImagenesFondo();
                this.Invalidate();
                logMessage("Imagen actualizada.");
            }
        }

        // =========================================================
        // TEMA OSCURO
        // =========================================================

        private void ApplyDarkTheme()
        {
            Color bgPrincipal = Color.Black;
            Color bgContenedor = Color.Black;
            Color textPrincipal = Color.FromArgb(227, 227, 227);
            Color textSecundario = Color.FromArgb(148, 150, 154);
            Color colorMarco = Color.FromArgb(60, 64, 67);
            Color colorEtiqueta = Color.FromArgb(0, 128, 0);

            string themeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt");

            if (!File.Exists(themeFilePath))
            {
                var defaults = new[]
                {
                    "fondoprincipal=#000000",
                    "fondocontenedor=#000000",
                    "textoprincipal=#E3E3E3",
                    "textosecundario=#94969A",
                    "colormarco=#3C4043",
                    "coloretiqueta=#008000"
                };
                try { File.WriteAllLines(themeFilePath, defaults); } catch { }
            }

            if (File.Exists(themeFilePath))
            {
                try
                {
                    var lineas = File.ReadAllLines(themeFilePath);
                    foreach (var linea in lineas)
                    {
                        if (string.IsNullOrWhiteSpace(linea) || !linea.Contains("=")) continue;
                        var partes = linea.Split('=');
                        if (partes.Length < 2) continue;
                        string clave = partes[0].Trim().ToLower();
                        string valorHex = partes[1].Trim();

                        if (clave == "fondoprincipal") bgPrincipal = ColorTranslator.FromHtml(valorHex);
                        else if (clave == "fondocontenedor") bgContenedor = ColorTranslator.FromHtml(valorHex);
                        else if (clave == "textoprincipal") textPrincipal = ColorTranslator.FromHtml(valorHex);
                        else if (clave == "textosecundario") textSecundario = ColorTranslator.FromHtml(valorHex);
                        else if (clave == "colormarco") colorMarco = ColorTranslator.FromHtml(valorHex);
                        else if (clave == "coloretiqueta") colorEtiqueta = ColorTranslator.FromHtml(valorHex);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al cargar theme.txt: " + ex.Message);
                }
            }

            _themeBgPrincipal = bgPrincipal;
            _themeBgContenedor = bgContenedor;
            _themeTextPrincipal = textPrincipal;
            _themeTextSecundario = textSecundario;
            _themeColorMarco = colorMarco;

            this.BackColor = bgPrincipal;
            this.ForeColor = textPrincipal;

            void FormatControl(Control ctrl)
            {
                if (ctrl is GroupBox grp)
                {
                    grp.BackColor = bgPrincipal;
                    grp.ForeColor = textPrincipal;
                    grp.Tag = colorMarco;
                    grp.Paint -= GroupBox_Paint;
                    grp.Paint += GroupBox_Paint;
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = bgContenedor;
                    txt.ForeColor = textPrincipal;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is CheckBox chk)
                {
                    chk.BackColor = Color.Transparent;
                    chk.ForeColor = textPrincipal;
                    chk.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is Label lbl)
                {
                    if (lbl.Tag?.ToString() == "ColorPrincipal")
                        lbl.ForeColor = textPrincipal;
                    else
                        lbl.ForeColor = lbl.Name.Contains("Percent") || lbl.Name.Contains("label")
                                        ? textSecundario
                                        : textPrincipal;
                }
                else if (ctrl is PictureBox pic)
                {
                    if (pic == pbToastIcon)
                        pic.BackColor = Color.Transparent;   // el toast conserva la transparencia del PNG
                    else
                        pic.BackColor = bgPrincipal;
                }
                else if (ctrl is ContextMenuStrip menu)
                {
                    menu.BackColor = bgContenedor;
                    menu.ForeColor = textPrincipal;
                    menu.RenderMode = ToolStripRenderMode.System;
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.BackColor = bgContenedor;
                        item.ForeColor = textPrincipal;
                    }
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = bgContenedor;
                    btn.ForeColor = textPrincipal;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = colorMarco;
                }

                foreach (Control child in ctrl.Controls)
                    FormatControl(child);
            }

            foreach (Control c in this.Controls)
                FormatControl(c);

            FormatControl(trayIconMenu);

            if (textosPorModo != null)
            {
                foreach (var kvp in textosPorModo)
                {
                    kvp.Key.BackColor = colorEtiqueta;
                    kvp.Key.ForeColor = Color.White;
                }
            }

            foreach (string nombre in nombresLineasConectoras)
            {
                Control[] encontrados = this.Controls.Find(nombre, true);
                if (encontrados.Length > 0)
                    encontrados[0].BackColor = colorEtiqueta;
            }

            foreach (var toggle in new Control[] { lblToggleLog, lblToggleColors, lblToggleDeadzone, lblglobalKB })
            {
                if (toggle == null) continue;
                toggle.BackColor = Color.Black;
                toggle.ForeColor = textPrincipal;
            }
        }

        // =========================================================
        // SELECTOR DE COLORES (dentro de themePanel)
        // =========================================================

        private void InicializarSelectorColores()
        {
            const int squareSize = 10;
            const int spacing = 14;
            const int labelHeight = 12;
            const int verticalGap = 6;

            int squaresStartX = 30;
            int rowFondoY = 30;
            int rowEtiqY = 70;

            int areaWidth = (paletaColores.Length - 1) * spacing + squareSize;

            float tinyFontSize = Math.Max(6f, this.Font.Size / 2f);

            Label lblFondo = new Label
            {
                Name = "themeFondoLabel",
                Text = "TEMA FONDO",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(areaWidth, labelHeight),
                Location = new Point(squaresStartX, rowFondoY - labelHeight - verticalGap),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font.FontFamily, tinyFontSize, FontStyle.Bold)
            };
            themePanel.Controls.Add(lblFondo);

            int x = squaresStartX;
            for (int i = 0; i < paletaColores.Length; i++)
            {
                Color color = paletaColores[i];
                Panel cuadro = CrearCirculoColor(color, new Point(x, rowFondoY), CirculoFondo_Click);
                cuadro.Name = $"themeFondoSq{i}";
                cuadro.Size = new Size(squareSize, squareSize);
                themePanel.Controls.Add(cuadro);
                x += spacing;
            }

            var thirds = themePanel.Controls.Find("themeFondoSq2", true);
            if (thirds.Length > 0 && thirds[0] is Panel thirdCuadro)
            {
                Color blancoTenue = Color.FromArgb(200, 200, 200);
                thirdCuadro.BackColor = blancoTenue;
                thirdCuadro.Tag = blancoTenue;
            }

            Label lblEtiquetas = new Label
            {
                Name = "themeEtiquetasLabel",
                Text = "TEMA ETIQUETAS",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(areaWidth, labelHeight),
                Location = new Point(squaresStartX, rowEtiqY - labelHeight - verticalGap),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font.FontFamily, tinyFontSize, FontStyle.Bold)
            };
            themePanel.Controls.Add(lblEtiquetas);

            x = squaresStartX;
            for (int i = 0; i < paletaColores.Length; i++)
            {
                Color color = paletaColores[i];
                Panel cuadro = CrearCirculoColor(color, new Point(x, rowEtiqY), CirculoEtiqueta_Click);
                cuadro.Name = $"themeEtiqSq{i}";
                cuadro.Size = new Size(squareSize, squareSize);
                themePanel.Controls.Add(cuadro);
                x += spacing;
            }
        }

        private Panel CrearCirculoColor(Color color, Point location, EventHandler onClick)
        {
            Panel cuadro = new Panel
            {
                Size = new Size(10, 10),
                Location = location,
                BackColor = color,
                Cursor = Cursors.Hand,
                Tag = color,
                BorderStyle = BorderStyle.FixedSingle
            };

            cuadro.Click += onClick;
            return cuadro;
        }

        private void CirculoFondo_Click(object sender, EventArgs e)
        {
            Color colorElegido = (Color)((Panel)sender).Tag;
            GuardarColorEnTheme("fondoprincipal", colorElegido);
            ApplyDarkTheme();
            AjustarElementosRedimension();
        }

        private void CirculoEtiqueta_Click(object sender, EventArgs e)
        {
            Color colorElegido = (Color)((Panel)sender).Tag;
            GuardarColorEnTheme("coloretiqueta", colorElegido);
            ApplyDarkTheme();
            AjustarElementosRedimension();
        }

        private void GuardarColorEnTheme(string clave, Color color)
        {
            string themeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt");
            string colorHex = ColorTranslator.ToHtml(color);

            var lineas = new List<string>();
            bool claveEncontrada = false;

            if (File.Exists(themeFilePath))
            {
                lineas.AddRange(File.ReadAllLines(themeFilePath));
                for (int i = 0; i < lineas.Count; i++)
                {
                    if (lineas[i].Trim().StartsWith(clave + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        lineas[i] = clave + "=" + colorHex;
                        claveEncontrada = true;
                        break;
                    }
                }
            }

            if (!claveEncontrada)
                lineas.Add(clave + "=" + colorHex);

            File.WriteAllLines(themeFilePath, lineas);
        }

        // =========================================================
        // PINTADO
        // =========================================================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_scaledXbg != null)
                e.Graphics.DrawImage(_scaledXbg, offsetCentrado.X, offsetCentrado.Y);
            else if (_cachedXbg != null)
                e.Graphics.DrawImage(_cachedXbg, offsetCentrado.X, offsetCentrado.Y, 1524, 928);

            if (_scaledJoy != null)
                e.Graphics.DrawImage(_scaledJoy, 350 + offsetCentrado.X, 90 + offsetCentrado.Y);
            else if (_cachedJoy != null)
                e.Graphics.DrawImage(_cachedJoy, 350 + offsetCentrado.X, 90 + offsetCentrado.Y, 710, 520);
        }

        private void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            Control groupBox = (Control)sender;
            Color borderColor = (groupBox.Tag is Color) ? (Color)groupBox.Tag : _themeColorMarco;

            if (!groupBox.Enabled)
                borderColor = Color.FromArgb(45, 47, 49);

            using (SolidBrush bgBrush = new SolidBrush(_themeBgPrincipal))
                e.Graphics.FillRectangle(bgBrush, 0, 0, groupBox.Width, groupBox.Font.Height + 5);

            if (!string.IsNullOrEmpty(groupBox.Text))
            {
                Color colorTextoTitulo = groupBox.Enabled
                    ? _themeTextPrincipal
                    : Color.FromArgb(100, 102, 105);

                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Top;
                TextRenderer.DrawText(e.Graphics, groupBox.Text, groupBox.Font,
                    new Point(10, 4), colorTextoTitulo, flags);
            }
        }
        // =========================================================
        // WINDOW_MAIN_LOAD
        // =========================================================

        private void Window_Main_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            numDeadzoneL.AutoSize = false;
            numDeadzoneL.Size = new Size(50, 20);

            numDeadzoneR.AutoSize = false;
            numDeadzoneR.Size = new Size(50, 20);

            barDeadzoneL = new DeadzoneBar
            {
                Name = "barDeadzoneL",
                Location = new Point(65, 32),
                Size = new Size(72, 18),
                MaxValue = 50,
                Value = 32
            };
            deadzonePanel.Controls.Add(barDeadzoneL);

            barDeadzoneR = new DeadzoneBar
            {
                Name = "barDeadzoneR",
                Location = new Point(65, 65),
                Size = new Size(72, 18),
                MaxValue = 50,
                Value = 32
            };
            deadzonePanel.Controls.Add(barDeadzoneR);

            const int MIN_DEADZONE = 32;

            // Controller 1
            xboxControllers[0].configureChatpad(Properties.Settings.Default.ctrl1KeyboardType);
            xboxControllers[0].configureGamepad(Properties.Settings.Default.ctrl1TriggerAsButton);
            xboxControllers[0].mouseModeFlag = Properties.Settings.Default.ctrl1MouseMode;

            if (Properties.Settings.Default.ctrl1DeadzoneL < MIN_DEADZONE)
                Properties.Settings.Default.ctrl1DeadzoneL = MIN_DEADZONE;
            if (Properties.Settings.Default.ctrl1DeadzoneR < MIN_DEADZONE)
                Properties.Settings.Default.ctrl1DeadzoneR = MIN_DEADZONE;

            xboxControllers[0].deadzoneL = (int)Math.Round(Properties.Settings.Default.ctrl1DeadzoneL * 327.67);
            xboxControllers[0].deadzoneR = (int)Math.Round(Properties.Settings.Default.ctrl1DeadzoneR * 327.67);

            // Controller 2
            xboxControllers[1].configureChatpad(Properties.Settings.Default.ctrl2KeyboardType);
            xboxControllers[1].configureGamepad(Properties.Settings.Default.ctrl2TriggerAsButton);
            xboxControllers[1].mouseModeFlag = Properties.Settings.Default.ctrl2MouseMode;

            if (Properties.Settings.Default.ctrl2DeadzoneL < MIN_DEADZONE)
                Properties.Settings.Default.ctrl2DeadzoneL = MIN_DEADZONE;
            if (Properties.Settings.Default.ctrl2DeadzoneR < MIN_DEADZONE)
                Properties.Settings.Default.ctrl2DeadzoneR = MIN_DEADZONE;

            xboxControllers[1].deadzoneL = (int)Math.Round(Properties.Settings.Default.ctrl2DeadzoneL * 327.67);
            xboxControllers[1].deadzoneR = (int)Math.Round(Properties.Settings.Default.ctrl2DeadzoneR * 327.67);

            // Controller 3
            xboxControllers[2].configureChatpad(Properties.Settings.Default.ctrl3KeyboardType);
            xboxControllers[2].configureGamepad(Properties.Settings.Default.ctrl3TriggerAsButton);
            xboxControllers[2].mouseModeFlag = Properties.Settings.Default.ctrl3MouseMode;

            if (Properties.Settings.Default.ctrl3DeadzoneL < MIN_DEADZONE)
                Properties.Settings.Default.ctrl3DeadzoneL = MIN_DEADZONE;
            if (Properties.Settings.Default.ctrl3DeadzoneR < MIN_DEADZONE)
                Properties.Settings.Default.ctrl3DeadzoneR = MIN_DEADZONE;

            xboxControllers[2].deadzoneL = (int)Math.Round(Properties.Settings.Default.ctrl3DeadzoneL * 327.67);
            xboxControllers[2].deadzoneR = (int)Math.Round(Properties.Settings.Default.ctrl3DeadzoneR * 327.67);

            // Controller 4
            xboxControllers[3].configureChatpad(Properties.Settings.Default.ctrl4KeyboardType);
            xboxControllers[3].configureGamepad(Properties.Settings.Default.ctrl4TriggerAsButton);
            xboxControllers[3].mouseModeFlag = Properties.Settings.Default.ctrl4MouseMode;

            if (Properties.Settings.Default.ctrl4DeadzoneL < MIN_DEADZONE)
                Properties.Settings.Default.ctrl4DeadzoneL = MIN_DEADZONE;
            if (Properties.Settings.Default.ctrl4DeadzoneR < MIN_DEADZONE)
                Properties.Settings.Default.ctrl4DeadzoneR = MIN_DEADZONE;

            xboxControllers[3].deadzoneL = (int)Math.Round(Properties.Settings.Default.ctrl4DeadzoneL * 327.67);
            xboxControllers[3].deadzoneR = (int)Math.Round(Properties.Settings.Default.ctrl4DeadzoneR * 327.67);

            xboxControllers[0].registerJoystick(1);
            xboxControllers[1].registerJoystick(2);
            xboxControllers[2].registerJoystick(3);
            xboxControllers[3].registerJoystick(4);

            xboxReceiver = new Receiver(xboxControllers, this);
            xboxReceiver.connectReceiver();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt")))
                logMessage("Tema cargado.");

            this.Controls.Add(lblToggleDeadzone);
            lblToggleDeadzone.BringToFront();
            deadzonePanel.Visible = false;
            ConfigurarToggleDeadzone();

            LoadDeadzonesForControl(1);
            radioCtrl1.Checked = true;

            AjustarElementosRedimension();

            _logVisible = false;
            _colorsVisible = false;

            appLogTextbox.Visible = false;
            SetColorsVisible(false);

            lblglobalKB.Text = "OPCIONES DE TECLADO";
            lblglobalKB.Cursor = Cursors.Hand;

            layoutPanel.Visible = false;

            string currentLayout = Properties.Settings.Default.ctrl1KeyboardType;
            rbQWERTY.Checked = (currentLayout == "Q W E R T Y");
            rbQWERTZ.Checked = (currentLayout == "Q W E R T Z");
            rbAZERTY.Checked = (currentLayout == "A Z E R T Y");

            StyleFlatButton(WP);
            StyleFlatButton(JOY);

            // Inicializar el toast (texto flotante)
            if (lblToast != null)
            {
                lblToast.Text = "";
                CenterToast();
                lblToast.Visible = false;
                lblToast.AutoSize = false;
                lblToast.TextAlign = ContentAlignment.MiddleCenter;
                lblToast.BackColor = Color.Transparent;
                lblToast.ForeColor = Color.White;
                lblToast.Font = new Font(this.Font.FontFamily, 15f, FontStyle.Bold);
                lblToast.Size = new Size(400, 40);

                // Pintado custom con sombra
                lblToast.Paint += (s, pe) =>
                {
                    var lbl = (Label)s;
                    var g = pe.Graphics;

                    if (string.IsNullOrEmpty(_toastText)) return;

                    var rect = new Rectangle(0, 0, lbl.Width, lbl.Height);

                    // Sombra
                    var rectShadow = new Rectangle(2, 2, lbl.Width, lbl.Height);
                    TextRenderer.DrawText(g, _toastText, lbl.Font, rectShadow,
                        Color.Black,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // Texto principal
                    TextRenderer.DrawText(g, _toastText, lbl.Font, rect,
                        lbl.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
            }
        }

        
        private void StyleFlatButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 40);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 20);
            btn.BackColor = Color.Black;
            btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand;
        }

        // =========================================================
        // LAYOUT OPTION
        // =========================================================

        private void layoutOption_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null || !rb.Checked) return;

            string layout = rb.Tag?.ToString();
            if (string.IsNullOrEmpty(layout)) return;

            for (int i = 0; i < 4; i++)
                xboxControllers[i].configureChatpad(layout);

            Properties.Settings.Default.ctrl1KeyboardType = layout;
            Properties.Settings.Default.ctrl2KeyboardType = layout;
            Properties.Settings.Default.ctrl3KeyboardType = layout;
            Properties.Settings.Default.ctrl4KeyboardType = layout;
            Properties.Settings.Default.Save();

            rbQWERTY.Invalidate();
            rbQWERTZ.Invalidate();
            rbAZERTY.Invalidate();
        }

        // =========================================================
        // WINDOW EVENTS
        // =========================================================

        private void Window_Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
                trayIcon.Visible = true;
            }
            else
            {
                AjustarElementosRedimension();
            }
        }

        private void Window_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                trayIcon.Visible = false;

                if (xboxReceiver != null)
                    xboxReceiver.killReceiver();

                Properties.Settings.Default.Save();
            }
            catch
            {
                System.Environment.Exit(0);
            }
            Properties.Settings.Default.Save();
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            RestaurarVentana();
        }

        private void RestaurarVentana()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            _allowExit = true;
            trayIcon.Visible = false;
            this.Close();
        }

        private void appLogTextbox_TextChanged(object sender, EventArgs e)
        {
            appLogTextbox.SelectionStart = appLogTextbox.Text.Length;
            appLogTextbox.ScrollToCaret();
            appLogTextbox.Refresh();
        }

        private void chatpadTextBox_Enter(object sender, EventArgs e)
        {
            chatpadTextBox.TextAlign = HorizontalAlignment.Left;
            chatpadTextBox.Text = "";
            chatpadTextBox.Enter -= chatpadTextBox_Enter;
        }

        // =========================================================
        // MODO MOUSE ↔ GAMEPAD
        // =========================================================

        public void controllerConnected(int ctrl) { }
        public void controllerDisconnected(int ctrl) { }

        public void mouseModeUpdate(int ctrl, bool modeStatus)
        {
            switch (ctrl)
            {
                case 1: Properties.Settings.Default.ctrl1MouseMode = modeStatus; break;
                case 2: Properties.Settings.Default.ctrl2MouseMode = modeStatus; break;
                case 3: Properties.Settings.Default.ctrl3MouseMode = modeStatus; break;
                case 4: Properties.Settings.Default.ctrl4MouseMode = modeStatus; break;
            }

            bool anyMouseModeActive = Properties.Settings.Default.ctrl1MouseMode ||
                                      Properties.Settings.Default.ctrl2MouseMode ||
                                      Properties.Settings.Default.ctrl3MouseMode ||
                                      Properties.Settings.Default.ctrl4MouseMode;

            foreach (var kvp in textosPorModo)
                kvp.Key.Text = anyMouseModeActive ? kvp.Value.mouse : kvp.Value.gamepad;
        }

        public void logMessage(string message)
        {
            // Toast primero: no depende del log
            DetectAndHighlight(message);

            string nueva = "[" + DateTime.Now.ToString("G") + "] - " + message + "\r\n";

            // Tope de tamaño: conserva solo lo más reciente
            const int maxChars = 20000;
            string actual = appLogTextbox.Text;
            if (actual.Length > maxChars)
            {
                int cut = actual.LastIndexOf("\r\n", maxChars, StringComparison.Ordinal);
                actual = actual.Substring(0, cut > 0 ? cut : maxChars);
            }

            appLogTextbox.Text = nueva + actual;
            appLogTextbox.Select(0, 0);
            appLogTextbox.ScrollToCaret();
        }

        // =========================================================
        // TOAST
        // =========================================================

        private void DetectAndHighlight(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (!message.Contains("►")) return;

            int idx = message.IndexOf("►");
            string after = message.Substring(idx + 1).Trim();

            string fullText = after.Split('\r', '\n')[0].Trim();
            if (string.IsNullOrEmpty(fullText)) return;

            string buttonName = fullText.Split(' ', '(')[0].Trim();
            string iconName = buttonName;

            // =========================================================
            // DPAD con direcciones — funciona en modo gamepad Y modo mouse
            // =========================================================
            if (fullText.Contains("ARRIBA+IZQUIERDA"))
                iconName = "DPAD";
            else if (fullText.Contains("ARRIBA+DERECHA"))
                iconName = "DPAD";
            else if (fullText.Contains("ARRIBA") || fullText.Contains("↑"))
                iconName = "DU";
            else if (fullText.Contains("ABAJO") || fullText.Contains("↓"))
                iconName = "DD";
            else if (fullText.Contains("IZQUIERDA") || fullText.Contains("←"))
                iconName = "DL";
            else if (fullText.Contains("DERECHA") || fullText.Contains("→"))
                iconName = "DR";
            if (fullText.Contains("ARRIBA+IZQUIERDA"))
                iconName = fullText.IndexOf("ALT", StringComparison.OrdinalIgnoreCase) >= 0 ? "DALTL" : "DPAD";
            else if (fullText.Contains("ARRIBA+DERECHA"))
                iconName = fullText.IndexOf("ALT", StringComparison.OrdinalIgnoreCase) >= 0 ? "DALTR" : "DPAD";
            // =========================================================
            // Left Stick (LS) con dirección
            // =========================================================
            else if (fullText.StartsWith("LS"))
            {
                if (fullText.Contains("↑")) iconName = "LSU";
                else if (fullText.Contains("↓")) iconName = "LSD";
                else if (fullText.Contains("←")) iconName = "LSL";
                else if (fullText.Contains("→")) iconName = "LSR";
                else iconName = "LS";
            }

            // =========================================================
            // Right Stick (RS) con dirección
            // =========================================================
            else if (fullText.StartsWith("RS"))
            {
                if (fullText.Contains("↑")) iconName = "RSU";
                else if (fullText.Contains("↓")) iconName = "RSD";
                else if (fullText.Contains("←")) iconName = "RSL";
                else if (fullText.Contains("→")) iconName = "RSR";
                else iconName = "RS";
            }

            // =========================================================
            // L3 / R3
            // =========================================================
            else if (fullText.StartsWith("L3"))
                iconName = "L3";
            else if (fullText.StartsWith("R3"))
                iconName = "R3";

            // A, B, X, Y, LT, RT, LB, RB, START, BACK, GUIDE
            // ya coinciden con su nombre de archivo

            ShowToast(fullText, iconName);
        }

        private bool IsAnyMouseModeActive()
        {
            return Properties.Settings.Default.ctrl1MouseMode ||
                   Properties.Settings.Default.ctrl2MouseMode ||
                   Properties.Settings.Default.ctrl3MouseMode ||
                   Properties.Settings.Default.ctrl4MouseMode;
        }

        private void ShowToast(string text, string buttonName = null)
        {
            if (lblToast == null) return;
            if (string.IsNullOrEmpty(text)) return;

            // Cargar el icono según el botón
            if (pbToastIcon != null)
            {
                pbToastIcon.BackColor = Color.Transparent;
                pbToastIcon.SizeMode = PictureBoxSizeMode.Zoom;

                Bitmap icon = LoadToastIcon(buttonName);
                if (icon != null)
                {
                    pbToastIcon.Image?.Dispose();
                    pbToastIcon.Image = icon;
                    pbToastIcon.Visible = true;
                }
                else
                {
                    pbToastIcon.Visible = false;
                }
            }

            // Guardar el texto en variable propia (NO en el label)
            _toastText = text;
            lblToast.Text = "";   // ← el label NO dibuja el texto

            // Recalcular posición: pbToastIcon + lblToast centrados juntos
            CenterToast();

            // Mostrar
            lblToast.Visible = true;
            lblToast.BringToFront();
            lblToast.Invalidate();   // ← forzar repaint para que el Paint dibuje el texto

            if (pbToastIcon != null && pbToastIcon.Visible)
            {
                pbToastIcon.BringToFront();
                pbToastIcon.Location = new Point(lblToast.Left - pbToastIcon.Width - 5, lblToast.Top + 5);
            }

            // Timer para ocultar
            if (_toastTimer == null)
            {
                _toastTimer = new Timer { Interval = 100 };
                _toastTimer.Tick += (s, e) =>
                {
                    _toastRemaining--;
                    if (_toastRemaining <= 0)
                    {
                        _toastTimer.Stop();
                        lblToast.Visible = false;

                        if (pbToastIcon != null)
                        {
                            pbToastIcon.Visible = false;
                            pbToastIcon.Image = null;
                        }
                    }
                };
            }

            _toastRemaining = 15;
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        /// <summary>
        /// Carga el icono asociado al botón, o default si no existe el específico.
        /// Los iconos van en la carpeta "icons/" junto al .exe.
        /// </summary>
        private Bitmap LoadToastIcon(string buttonName)
        {
            try
            {
                string iconsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");
                if (!Directory.Exists(iconsDir)) return null;

                // 1. Intentar con el nombre del botón
                if (!string.IsNullOrEmpty(buttonName))
                {
                    string specific = Path.Combine(iconsDir, buttonName + ".png");
                    if (File.Exists(specific))
                        return new Bitmap(specific);
                }

                // 2. Fallback: default.png
                string fallback = Path.Combine(iconsDir, "default.png");
                if (File.Exists(fallback))
                    return new Bitmap(fallback);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando icono: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Centra horizontalmente el conjunto icono + label en el form.
        /// </summary>
        private void CenterToast()
        {
            if (lblToast == null) return;

            int totalWidth = lblToast.Width;

            if (pbToastIcon != null && pbToastIcon.Visible)
                totalWidth += pbToastIcon.Width + 5;   // 5px de separación

            int startX = (this.ClientSize.Width - totalWidth) / 2;
            lblToast.Location = new Point(startX + (pbToastIcon != null && pbToastIcon.Visible ? pbToastIcon.Width + 5 : 0), lblToast.Location.Y);

            if (pbToastIcon != null && pbToastIcon.Visible)
            {
                pbToastIcon.Location = new Point(startX, lblToast.Location.Y + (lblToast.Height - pbToastIcon.Height) / 2);
            }
        }

        // =========================================================
        // AJUSTE DE ELEMENTOS
        // =========================================================

        private void AjustarElementosRedimension()
        {
            int clientW = this.ClientSize.Width;
            int clientH = this.ClientSize.Height;

            int rightPanelWidth = 300;
            int bottomMargin = 18;

            int gap = -2;
            int rowH = 34;

            int rightPanelX = clientW - rightPanelWidth - 8;
            int iconsColX = rightPanelX + 8;
            int iconsColW = 44;

            int controllers = 4;
            int totalHeight = controllers * (rowH + gap) - gap;

            int bottomGapToLog = -140;

            int groupTop = clientH - bottomMargin - totalHeight - bottomGapToLog;
            groupTop = Math.Min(groupTop, clientH - bottomMargin - totalHeight);
            groupTop = Math.Max(0, groupTop);

            string[] iconNames = { "pictureBox1", "pictureBox2", "pictureBox3", "pictureBox4" };

            for (int i = 0; i < controllers; i++)
            {
                int rowY = groupTop + i * (rowH + gap);

                if (i < iconNames.Length)
                {
                    var icon = this.Controls.Find(iconNames[i], true).FirstOrDefault() as PictureBox;
                    if (icon != null)
                    {
                        int iw = Math.Min(48, Math.Max(24, icon.Width > 0 ? icon.Width : iconsColW));
                        int ih = Math.Min(48, Math.Max(24, icon.Height > 0 ? icon.Height : 32));
                        icon.Size = new Size(iw, ih);
                        icon.Location = new Point(iconsColX, rowY + (rowH - ih) / 2);
                        icon.Visible = true;
                        icon.BringToFront();
                    }
                }
            }
        }

        // =========================================================
        // DEADZONE HANDLERS
        // =========================================================

        private void SetColorsVisible(bool visible)
        {
            themePanel.Visible = visible;
        }

        private void radioCtrl_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null || !rb.Checked) return;

            if (rb == radioCtrl1) LoadDeadzonesForControl(1);
            else if (rb == radioCtrl2) LoadDeadzonesForControl(2);
            else if (rb == radioCtrl3) LoadDeadzonesForControl(3);
            else if (rb == radioCtrl4) LoadDeadzonesForControl(4);

            radioCtrl1.Invalidate();
            radioCtrl2.Invalidate();
            radioCtrl3.Invalidate();
            radioCtrl4.Invalidate();
        }

        private void numDeadzoneL_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressDeadzoneEvents) return;

            int v = (int)numDeadzoneL.Value;
            if (barDeadzoneL != null) barDeadzoneL.Value = v;
            lblDeadzoneL.Text = v + "%";
            int raw = (int)Math.Round(v * 327.67);

            switch (_activeControl)
            {
                case 1: Properties.Settings.Default.ctrl1DeadzoneL = v; xboxControllers[0].deadzoneL = raw; break;
                case 2: Properties.Settings.Default.ctrl2DeadzoneL = v; xboxControllers[1].deadzoneL = raw; break;
                case 3: Properties.Settings.Default.ctrl3DeadzoneL = v; xboxControllers[2].deadzoneL = raw; break;
                case 4: Properties.Settings.Default.ctrl4DeadzoneL = v; xboxControllers[3].deadzoneL = raw; break;
            }
            Properties.Settings.Default.Save();
        }

        private void numDeadzoneR_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressDeadzoneEvents) return;

            int v = (int)numDeadzoneR.Value;
            if (barDeadzoneR != null) barDeadzoneR.Value = v;
            lblDeadzoneR.Text = v + "%";
            int raw = (int)Math.Round(v * 327.67);

            switch (_activeControl)
            {
                case 1: Properties.Settings.Default.ctrl1DeadzoneR = v; xboxControllers[0].deadzoneR = raw; break;
                case 2: Properties.Settings.Default.ctrl2DeadzoneR = v; xboxControllers[1].deadzoneR = raw; break;
                case 3: Properties.Settings.Default.ctrl3DeadzoneR = v; xboxControllers[2].deadzoneR = raw; break;
                case 4: Properties.Settings.Default.ctrl4DeadzoneR = v; xboxControllers[3].deadzoneR = raw; break;
            }
        }

        private void LoadDeadzonesForControl(int ctrl)
        {
            _activeControl = ctrl;

            int dzL, dzR;
            switch (ctrl)
            {
                case 1: dzL = Properties.Settings.Default.ctrl1DeadzoneL; dzR = Properties.Settings.Default.ctrl1DeadzoneR; break;
                case 2: dzL = Properties.Settings.Default.ctrl2DeadzoneL; dzR = Properties.Settings.Default.ctrl2DeadzoneR; break;
                case 3: dzL = Properties.Settings.Default.ctrl3DeadzoneL; dzR = Properties.Settings.Default.ctrl3DeadzoneR; break;
                case 4: dzL = Properties.Settings.Default.ctrl4DeadzoneL; dzR = Properties.Settings.Default.ctrl4DeadzoneR; break;
                default: dzL = 32; dzR = 32; break;
            }

            int rawL = (int)Math.Round(dzL * 327.67);
            int rawR = (int)Math.Round(dzR * 327.67);
            switch (ctrl)
            {
                case 1: xboxControllers[0].deadzoneL = rawL; xboxControllers[0].deadzoneR = rawR; break;
                case 2: xboxControllers[1].deadzoneL = rawL; xboxControllers[1].deadzoneR = rawR; break;
                case 3: xboxControllers[2].deadzoneL = rawL; xboxControllers[2].deadzoneR = rawR; break;
                case 4: xboxControllers[3].deadzoneL = rawL; xboxControllers[3].deadzoneR = rawR; break;
            }

            _suppressDeadzoneEvents = true;
            numDeadzoneL.Value = dzL;
            numDeadzoneR.Value = dzR;
            _suppressDeadzoneEvents = false;

            if (barDeadzoneL != null) barDeadzoneL.Value = dzL;
            if (barDeadzoneR != null) barDeadzoneR.Value = dzR;
            lblDeadzoneL.Text = dzL + "%";
            lblDeadzoneR.Text = dzR + "%";
        }

        // =========================================================
        // TOGGLES Y ESTILOS
        // =========================================================

        private void ConfigurarToggleDeadzone()
        {
            lblToggleLog.Text = "MOSTRAR LOG";
            lblToggleLog.Cursor = Cursors.Hand;

            lblToggleColors.Text = "COLORES DE TEMA";
            lblToggleColors.Cursor = Cursors.Hand;

            lblToggleDeadzone.Cursor = Cursors.Hand;
            lblToggleDeadzone.Click += LblToggleDeadzone_Click;

            SetupStyledRadio(rbQWERTY);
            SetupStyledRadio(rbQWERTZ);
            SetupStyledRadio(rbAZERTY);

            SetupStyledRadio(radioCtrl1);
            SetupStyledRadio(radioCtrl2);
            SetupStyledRadio(radioCtrl3);
            SetupStyledRadio(radioCtrl4);
        }

        private void SetupStyledRadio(RadioButton rb)
        {
            rb.Paint += (s, e) =>
            {
                var radio = (RadioButton)s;
                var g = e.Graphics;

                using (var brush = new SolidBrush(Color.Black))
                    g.FillRectangle(brush, 0, 0, radio.Width, radio.Height);

                Color textColor;
                bool isHovered = radio.ClientRectangle.Contains(radio.PointToClient(Cursor.Position));

                if (radio.Checked)
                    textColor = Color.FromArgb(180, 180, 180);
                else if (isHovered)
                    textColor = Color.FromArgb(140, 140, 140);
                else
                    textColor = Color.FromArgb(90, 90, 90);

                TextRenderer.DrawText(
                    g,
                    radio.Text,
                    radio.Font,
                    new Rectangle(0, 0, radio.Width, radio.Height),
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            rb.MouseEnter += (s, e) => rb.Invalidate();
            rb.MouseLeave += (s, e) => rb.Invalidate();

            rb.FlatStyle = FlatStyle.Flat;
            rb.FlatAppearance.BorderSize = 0;
            rb.UseVisualStyleBackColor = false;
            rb.BackColor = Color.Black;
            rb.ForeColor = Color.White;
        }

        private void LblToggleDeadzone_Click(object sender, EventArgs e)
        {
            deadzonePanel.Visible = !deadzonePanel.Visible;
            lblToggleDeadzone.Text = deadzonePanel.Visible ? "CERRAR" : "OPCIONES DE DEADZONE";
        }

        private void lblToggleLog_Click(object sender, EventArgs e)
        {
            _logVisible = !_logVisible;
            appLogTextbox.Visible = _logVisible;
            lblToggleLog.Text = _logVisible ? "CERRAR" : "MOSTRAR LOG";
        }

        private void lblToggleColors_Click(object sender, EventArgs e)
        {
            _colorsVisible = !_colorsVisible;
            SetColorsVisible(_colorsVisible);
            lblToggleColors.Text = _colorsVisible ? "CERRAR" : "COLORES DE TEMA";
        }

        private void lblglobalKB_Click(object sender, EventArgs e)
        {
            layoutPanel.Visible = !layoutPanel.Visible;

            if (layoutPanel.Visible)
                layoutPanel.BringToFront();

            lblglobalKB.Text = layoutPanel.Visible ? "CERRAR" : "OPCIONES DE TECLADO";
        }

        private void WP_Click(object sender, EventArgs e)
        {
            CambiarImagenFondo(true);
        }

        private void JOY_Click(object sender, EventArgs e)
        {
            CambiarImagenFondo(false);
        }

        // =========================================================
        // STUBS (eventos del Designer que no se usan)
        // =========================================================

        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void Y_Click(object sender, EventArgs e) { }
        private void X_Click(object sender, EventArgs e) { }
        private void L2_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void GUIDE_Click(object sender, EventArgs e) { }
        private void R3_Click(object sender, EventArgs e) { }
        private void A_Click(object sender, EventArgs e) { }
        private void B_Click(object sender, EventArgs e) { }
        private void RS_Click(object sender, EventArgs e) { }
        private void GUIDE_Click_1(object sender, EventArgs e) { }
        private void label26_Click(object sender, EventArgs e) { }
        private void label30_Click(object sender, EventArgs e) { }
        private void BACK_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click_1(object sender, EventArgs e) { }
        private void label27_Click(object sender, EventArgs e) { }
        private void trayIconMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void label29_Click(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void label31_Click(object sender, EventArgs e) { }
        private void lblToggleDeadzone_Click(object sender, EventArgs e) { }
        private void label9_Click(object sender, EventArgs e) { }
        private void radioButton1_CheckedChanged_1(object sender, EventArgs e) { }
        private void lblDeadzoneL_Click(object sender, EventArgs e) { }
        private void lblDeadzoneRTitle_Click(object sender, EventArgs e) { }
        private void lblDeadzoneR_Click(object sender, EventArgs e) { }
        private void lblDeadzoneLTitle_Click(object sender, EventArgs e) { }

        private void btnOpenJoyConfig_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("joy.cpl");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la configuración de dispositivos: " + ex.Message);
            }
        }

        private void START_Click(object sender, EventArgs e)
        {

        }

        private void pbToastIcon_Click(object sender, EventArgs e)
        {

        }
    }
}