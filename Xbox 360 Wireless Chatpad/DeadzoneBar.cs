using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Barra visual que muestra un valor de deadzone (0-50).
    /// No es interactiva — solo dibuja. El control real es el NumericUpDown.
    /// </summary>
    internal sealed class DeadzoneBar : Panel
    {
        private int _value;
        public int MaxValue { get; set; } = 50;

        public Color BarColor { get; set; } = Color.FromArgb(0, 128, 0);
        public Color TrackColor { get; set; } = Color.FromArgb(40, 40, 40);
        public Color BorderColor { get; set; } = Color.FromArgb(60, 64, 67);

        public int Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                _value = Math.Max(0, Math.Min(MaxValue, value));
                Invalidate();
            }
        }

        public DeadzoneBar()
        {
            SetStyle(ControlStyles.UserPaint
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(30, 31, 32);
            Height = 18;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            int w = Width;
            int h = Height;

            // Fondo del track
            using (var bg = new SolidBrush(TrackColor))
                g.FillRectangle(bg, 0, 0, w, h);

            // Progreso
            float pct = MaxValue > 0 ? (float)_value / MaxValue : 0f;
            int progressWidth = (int)(w * pct);
            if (progressWidth > 0)
            {
                using (var fill = new SolidBrush(BarColor))
                    g.FillRectangle(fill, 0, 0, progressWidth, h);
            }

            // Borde
            using (var pen = new Pen(BorderColor, 1))
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
        }
    }
}