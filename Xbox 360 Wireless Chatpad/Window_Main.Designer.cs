using System.Drawing;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    partial class Window_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.appLogTextbox = new System.Windows.Forms.TextBox();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chatpadTextBox = new System.Windows.Forms.TextBox();
            this.B = new System.Windows.Forms.Label();
            this.BACK = new System.Windows.Forms.Label();
            this.GUIDE = new System.Windows.Forms.Label();
            this.LT = new System.Windows.Forms.Label();
            this.X = new System.Windows.Forms.Label();
            this.RT = new System.Windows.Forms.Label();
            this.Y = new System.Windows.Forms.Label();
            this.LB = new System.Windows.Forms.Label();
            this.LS = new System.Windows.Forms.Label();
            this.L3 = new System.Windows.Forms.Label();
            this.A = new System.Windows.Forms.Label();
            this.RB = new System.Windows.Forms.Label();
            this.DPAD = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.START = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.btnOpenJoyConfig = new System.Windows.Forms.PictureBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.lblToggleDeadzone = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblglobalKB = new System.Windows.Forms.Label();
            this.lblToggleLog = new System.Windows.Forms.Label();
            this.lblToggleColors = new System.Windows.Forms.Label();
            this.layoutPanel = new System.Windows.Forms.Panel();
            this.rbAZERTY = new System.Windows.Forms.RadioButton();
            this.rbQWERTZ = new System.Windows.Forms.RadioButton();
            this.rbQWERTY = new System.Windows.Forms.RadioButton();
            this.themePanel = new System.Windows.Forms.Panel();
            this.WP = new System.Windows.Forms.Button();
            this.JOY = new System.Windows.Forms.Button();
            this.deadzonePanel = new System.Windows.Forms.Panel();
            this.radioCtrl3 = new System.Windows.Forms.RadioButton();
            this.radioCtrl2 = new System.Windows.Forms.RadioButton();
            this.numDeadzoneR = new System.Windows.Forms.NumericUpDown();
            this.lblDeadzoneR = new System.Windows.Forms.Label();
            this.lblDeadzoneRTitle = new System.Windows.Forms.Label();
            this.numDeadzoneL = new System.Windows.Forms.NumericUpDown();
            this.lblDeadzoneL = new System.Windows.Forms.Label();
            this.lblDeadzoneLTitle = new System.Windows.Forms.Label();
            this.radioCtrl4 = new System.Windows.Forms.RadioButton();
            this.radioCtrl1 = new System.Windows.Forms.RadioButton();
            this.lblToast = new System.Windows.Forms.Label();
            this.pbToastIcon = new System.Windows.Forms.PictureBox();
            this.object_49b36b00_93ca_44b2_821f_3748e0116215 = new System.Windows.Forms.Label();
            this.trayIconMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnOpenJoyConfig)).BeginInit();
            this.layoutPanel.SuspendLayout();
            this.themePanel.SuspendLayout();
            this.deadzonePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadzoneR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadzoneL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbToastIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // appLogTextbox
            // 
            this.appLogTextbox.BackColor = System.Drawing.Color.Black;
            this.appLogTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.appLogTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.appLogTextbox.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appLogTextbox.ForeColor = System.Drawing.Color.White;
            this.appLogTextbox.Location = new System.Drawing.Point(346, 650);
            this.appLogTextbox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.appLogTextbox.Multiline = true;
            this.appLogTextbox.Name = "appLogTextbox";
            this.appLogTextbox.ReadOnly = true;
            this.appLogTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.appLogTextbox.Size = new System.Drawing.Size(346, 97);
            this.appLogTextbox.TabIndex = 5;
            this.appLogTextbox.TextChanged += new System.EventHandler(this.appLogTextbox_TextChanged);
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayIconMenu;
            this.trayIcon.Text = "Xbox 360 Wireless Chatpad";
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new System.EventHandler(this.trayIcon_DoubleClick);
            // 
            // trayIconMenu
            // 
            this.trayIconMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.trayIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitMenuItem});
            this.trayIconMenu.Name = "trayIconMenu";
            this.trayIconMenu.Size = new System.Drawing.Size(103, 28);
            this.trayIconMenu.Opening += new System.ComponentModel.CancelEventHandler(this.trayIconMenu_Opening);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(102, 24);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // chatpadTextBox
            // 
            this.chatpadTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.chatpadTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chatpadTextBox.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.chatpadTextBox.Location = new System.Drawing.Point(31, 48);
            this.chatpadTextBox.Margin = new System.Windows.Forms.Padding(5, 1, 5, 1);
            this.chatpadTextBox.Name = "chatpadTextBox";
            this.chatpadTextBox.Size = new System.Drawing.Size(284, 23);
            this.chatpadTextBox.TabIndex = 3;
            this.chatpadTextBox.Text = "-Test Chatpad Here-";
            this.chatpadTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chatpadTextBox.Enter += new System.EventHandler(this.chatpadTextBox_Enter);
            // 
            // B
            // 
            this.B.BackColor = System.Drawing.Color.Green;
            this.B.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.B.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.B.Location = new System.Drawing.Point(15, 456);
            this.B.Name = "B";
            this.B.Size = new System.Drawing.Size(57, 15);
            this.B.TabIndex = 58;
            this.B.Text = "RIGHT CLICK";
            this.B.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.B.UseMnemonic = false;
            // 
            // BACK
            // 
            this.BACK.BackColor = System.Drawing.Color.Green;
            this.BACK.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BACK.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.BACK.Location = new System.Drawing.Point(15, 36);
            this.BACK.Name = "BACK";
            this.BACK.Size = new System.Drawing.Size(57, 15);
            this.BACK.TabIndex = 59;
            this.BACK.Text = "DELETE";
            this.BACK.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BACK.UseMnemonic = false;
            this.BACK.Click += new System.EventHandler(this.BACK_Click);
            // 
            // GUIDE
            // 
            this.GUIDE.BackColor = System.Drawing.Color.Green;
            this.GUIDE.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GUIDE.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.GUIDE.Location = new System.Drawing.Point(14, 68);
            this.GUIDE.Name = "GUIDE";
            this.GUIDE.Size = new System.Drawing.Size(57, 15);
            this.GUIDE.TabIndex = 60;
            this.GUIDE.Text = "GAMEPAD/MOUSE";
            this.GUIDE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.GUIDE.UseMnemonic = false;
            this.GUIDE.Click += new System.EventHandler(this.GUIDE_Click_1);
            // 
            // LT
            // 
            this.LT.BackColor = System.Drawing.Color.Green;
            this.LT.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LT.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.LT.Location = new System.Drawing.Point(0, 93);
            this.LT.Name = "LT";
            this.LT.Size = new System.Drawing.Size(57, 15);
            this.LT.TabIndex = 61;
            this.LT.Text = "CTRL";
            this.LT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LT.UseMnemonic = false;
            // 
            // X
            // 
            this.X.BackColor = System.Drawing.Color.Green;
            this.X.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.X.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.X.Location = new System.Drawing.Point(14, 538);
            this.X.Name = "X";
            this.X.Size = new System.Drawing.Size(57, 15);
            this.X.TabIndex = 62;
            this.X.Text = "LEFT CLICK";
            this.X.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.X.UseMnemonic = false;
            // 
            // RT
            // 
            this.RT.BackColor = System.Drawing.Color.Green;
            this.RT.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RT.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.RT.Location = new System.Drawing.Point(15, 289);
            this.RT.Name = "RT";
            this.RT.Size = new System.Drawing.Size(57, 15);
            this.RT.TabIndex = 63;
            this.RT.Text = "ALT + TAB";
            this.RT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RT.UseMnemonic = false;
            // 
            // Y
            // 
            this.Y.BackColor = System.Drawing.Color.Green;
            this.Y.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Y.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Y.Location = new System.Drawing.Point(15, 410);
            this.Y.Name = "Y";
            this.Y.Size = new System.Drawing.Size(57, 15);
            this.Y.TabIndex = 64;
            this.Y.Text = "SCROLL UP";
            this.Y.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Y.UseMnemonic = false;
            // 
            // LB
            // 
            this.LB.BackColor = System.Drawing.Color.Green;
            this.LB.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LB.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.LB.Location = new System.Drawing.Point(14, 111);
            this.LB.Name = "LB";
            this.LB.Size = new System.Drawing.Size(57, 15);
            this.LB.TabIndex = 65;
            this.LB.Text = "ESCAPE";
            this.LB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB.UseMnemonic = false;
            // 
            // LS
            // 
            this.LS.BackColor = System.Drawing.Color.Green;
            this.LS.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LS.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.LS.Location = new System.Drawing.Point(14, 135);
            this.LS.Name = "LS";
            this.LS.Size = new System.Drawing.Size(57, 15);
            this.LS.TabIndex = 66;
            this.LS.Text = "MOUSE MOVE";
            this.LS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LS.UseMnemonic = false;
            this.LS.Click += new System.EventHandler(this.label30_Click);
            // 
            // L3
            // 
            this.L3.BackColor = System.Drawing.Color.Green;
            this.L3.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.L3.Location = new System.Drawing.Point(14, 180);
            this.L3.Name = "L3";
            this.L3.Size = new System.Drawing.Size(57, 15);
            this.L3.TabIndex = 67;
            this.L3.Text = "MINIMIZAR";
            this.L3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.L3.UseMnemonic = false;
            // 
            // A
            // 
            this.A.BackColor = System.Drawing.Color.Green;
            this.A.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.A.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.A.Location = new System.Drawing.Point(14, 497);
            this.A.Name = "A";
            this.A.Size = new System.Drawing.Size(57, 15);
            this.A.TabIndex = 68;
            this.A.Text = "SCROLL DOWN";
            this.A.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.A.UseMnemonic = false;
            // 
            // RB
            // 
            this.RB.BackColor = System.Drawing.Color.Green;
            this.RB.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RB.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.RB.Location = new System.Drawing.Point(14, 327);
            this.RB.Name = "RB";
            this.RB.Size = new System.Drawing.Size(57, 15);
            this.RB.TabIndex = 70;
            this.RB.Text = "SCROLL DOWN";
            this.RB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RB.UseMnemonic = false;
            // 
            // DPAD
            // 
            this.DPAD.BackColor = System.Drawing.Color.Green;
            this.DPAD.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DPAD.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.DPAD.Location = new System.Drawing.Point(14, 219);
            this.DPAD.Name = "DPAD";
            this.DPAD.Size = new System.Drawing.Size(57, 15);
            this.DPAD.TabIndex = 71;
            this.DPAD.Text = "DPAD";
            this.DPAD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DPAD.UseMnemonic = false;
            // 
            // label26
            // 
            this.label26.BackColor = System.Drawing.Color.Green;
            this.label26.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label26.Location = new System.Drawing.Point(1286, 632);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(98, 15);
            this.label26.TabIndex = 72;
            this.label26.Text = " ";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label26.UseMnemonic = false;
            this.label26.Click += new System.EventHandler(this.label26_Click);
            // 
            // START
            // 
            this.START.BackColor = System.Drawing.Color.Black;
            this.START.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.START.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.START.Location = new System.Drawing.Point(1127, 36);
            this.START.Name = "START";
            this.START.Size = new System.Drawing.Size(243, 29);
            this.START.TabIndex = 74;
            this.START.Text = "MOUSE/GAMEPAD";
            this.START.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.START.UseMnemonic = false;
            this.START.Click += new System.EventHandler(this.START_Click);
            // 
            // label25
            // 
            this.label25.BackColor = System.Drawing.Color.Green;
            this.label25.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label25.Location = new System.Drawing.Point(14, 356);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(57, 15);
            this.label25.TabIndex = 75;
            this.label25.Text = "VOL + / -";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label25.UseMnemonic = false;
            // 
            // btnOpenJoyConfig
            // 
            this.btnOpenJoyConfig.Image = global::Xbox360WirelessChatpad.Properties.Resources.RW4ESm;
            this.btnOpenJoyConfig.Location = new System.Drawing.Point(760, 43);
            this.btnOpenJoyConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.btnOpenJoyConfig.Name = "btnOpenJoyConfig";
            this.btnOpenJoyConfig.Size = new System.Drawing.Size(94, 23);
            this.btnOpenJoyConfig.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.btnOpenJoyConfig.TabIndex = 30;
            this.btnOpenJoyConfig.TabStop = false;
            this.btnOpenJoyConfig.Click += new System.EventHandler(this.btnOpenJoyConfig_Click);
            // 
            // label27
            // 
            this.label27.BackColor = System.Drawing.Color.Green;
            this.label27.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label27.Location = new System.Drawing.Point(15, 384);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(57, 15);
            this.label27.TabIndex = 76;
            this.label27.Text = "MEDIA PREV - NEXT";
            this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label27.UseMnemonic = false;
            this.label27.Click += new System.EventHandler(this.label27_Click);
            // 
            // label29
            // 
            this.label29.BackColor = System.Drawing.Color.Green;
            this.label29.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label29.Location = new System.Drawing.Point(0, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(1587, 23);
            this.label29.TabIndex = 78;
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label29.UseMnemonic = false;
            this.label29.Click += new System.EventHandler(this.label29_Click);
            // 
            // lblToggleDeadzone
            // 
            this.lblToggleDeadzone.BackColor = System.Drawing.Color.Green;
            this.lblToggleDeadzone.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblToggleDeadzone.Location = new System.Drawing.Point(693, 747);
            this.lblToggleDeadzone.Name = "lblToggleDeadzone";
            this.lblToggleDeadzone.Size = new System.Drawing.Size(345, 25);
            this.lblToggleDeadzone.TabIndex = 41;
            this.lblToggleDeadzone.Text = "SENSIBILIDAD STICKS";
            this.lblToggleDeadzone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblToggleDeadzone.Click += new System.EventHandler(this.lblToggleDeadzone_Click);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Green;
            this.label7.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(285, 184);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 15);
            this.label7.TabIndex = 40;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Green;
            this.label1.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(285, 278);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 15);
            this.label1.TabIndex = 34;
            // 
            // lblglobalKB
            // 
            this.lblglobalKB.BackColor = System.Drawing.Color.Green;
            this.lblglobalKB.Location = new System.Drawing.Point(1039, 747);
            this.lblglobalKB.Name = "lblglobalKB";
            this.lblglobalKB.Size = new System.Drawing.Size(345, 25);
            this.lblglobalKB.TabIndex = 32;
            this.lblglobalKB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblglobalKB.Click += new System.EventHandler(this.lblglobalKB_Click);
            // 
            // lblToggleLog
            // 
            this.lblToggleLog.BackColor = System.Drawing.Color.Green;
            this.lblToggleLog.Location = new System.Drawing.Point(347, 747);
            this.lblToggleLog.Name = "lblToggleLog";
            this.lblToggleLog.Size = new System.Drawing.Size(345, 25);
            this.lblToggleLog.TabIndex = 44;
            this.lblToggleLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblToggleLog.Click += new System.EventHandler(this.lblToggleLog_Click);
            // 
            // lblToggleColors
            // 
            this.lblToggleColors.BackColor = System.Drawing.Color.Green;
            this.lblToggleColors.Location = new System.Drawing.Point(1, 747);
            this.lblToggleColors.Name = "lblToggleColors";
            this.lblToggleColors.Size = new System.Drawing.Size(345, 25);
            this.lblToggleColors.TabIndex = 45;
            this.lblToggleColors.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblToggleColors.Click += new System.EventHandler(this.lblToggleColors_Click);
            // 
            // layoutPanel
            // 
            this.layoutPanel.BackColor = System.Drawing.Color.Black;
            this.layoutPanel.Controls.Add(this.rbAZERTY);
            this.layoutPanel.Controls.Add(this.rbQWERTZ);
            this.layoutPanel.Controls.Add(this.rbQWERTY);
            this.layoutPanel.Controls.Add(this.chatpadTextBox);
            this.layoutPanel.Location = new System.Drawing.Point(1039, 650);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.Size = new System.Drawing.Size(345, 97);
            this.layoutPanel.TabIndex = 89;
            this.layoutPanel.Visible = false;
            // 
            // rbAZERTY
            // 
            this.rbAZERTY.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbAZERTY.Checked = true;
            this.rbAZERTY.FlatAppearance.BorderSize = 0;
            this.rbAZERTY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbAZERTY.ForeColor = System.Drawing.Color.White;
            this.rbAZERTY.Location = new System.Drawing.Point(259, 0);
            this.rbAZERTY.Name = "rbAZERTY";
            this.rbAZERTY.Size = new System.Drawing.Size(86, 30);
            this.rbAZERTY.TabIndex = 2;
            this.rbAZERTY.TabStop = true;
            this.rbAZERTY.Tag = "A Z E R T Y";
            this.rbAZERTY.Text = "AZERTY";
            this.rbAZERTY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbAZERTY.UseVisualStyleBackColor = true;
            // 
            // rbQWERTZ
            // 
            this.rbQWERTZ.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbQWERTZ.Checked = true;
            this.rbQWERTZ.FlatAppearance.BorderSize = 0;
            this.rbQWERTZ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbQWERTZ.ForeColor = System.Drawing.Color.White;
            this.rbQWERTZ.Location = new System.Drawing.Point(130, 0);
            this.rbQWERTZ.Name = "rbQWERTZ";
            this.rbQWERTZ.Size = new System.Drawing.Size(86, 30);
            this.rbQWERTZ.TabIndex = 1;
            this.rbQWERTZ.TabStop = true;
            this.rbQWERTZ.Tag = "Q W E R T Z";
            this.rbQWERTZ.Text = "QWERTZ";
            this.rbQWERTZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbQWERTZ.UseVisualStyleBackColor = true;
            // 
            // rbQWERTY
            // 
            this.rbQWERTY.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbQWERTY.Checked = true;
            this.rbQWERTY.FlatAppearance.BorderSize = 0;
            this.rbQWERTY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbQWERTY.ForeColor = System.Drawing.Color.White;
            this.rbQWERTY.Location = new System.Drawing.Point(0, 0);
            this.rbQWERTY.Name = "rbQWERTY";
            this.rbQWERTY.Size = new System.Drawing.Size(86, 30);
            this.rbQWERTY.TabIndex = 0;
            this.rbQWERTY.TabStop = true;
            this.rbQWERTY.Tag = "Q W E R T Y";
            this.rbQWERTY.Text = "QWERTY";
            this.rbQWERTY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbQWERTY.UseVisualStyleBackColor = true;
            this.rbQWERTY.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged_1);
            // 
            // themePanel
            // 
            this.themePanel.BackColor = System.Drawing.Color.Black;
            this.themePanel.Controls.Add(this.WP);
            this.themePanel.Controls.Add(this.JOY);
            this.themePanel.Location = new System.Drawing.Point(3, 650);
            this.themePanel.Name = "themePanel";
            this.themePanel.Size = new System.Drawing.Size(343, 97);
            this.themePanel.TabIndex = 91;
            this.themePanel.Visible = false;
            // 
            // WP
            // 
            this.WP.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.WP.FlatAppearance.BorderSize = 0;
            this.WP.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.WP.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.WP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WP.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.WP.Location = new System.Drawing.Point(214, 62);
            this.WP.Name = "WP";
            this.WP.Size = new System.Drawing.Size(131, 25);
            this.WP.TabIndex = 89;
            this.WP.Text = "CAMBIAR FONDO";
            this.WP.UseVisualStyleBackColor = true;
            // 
            // JOY
            // 
            this.JOY.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.JOY.FlatAppearance.BorderSize = 0;
            this.JOY.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.JOY.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.JOY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.JOY.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.JOY.Location = new System.Drawing.Point(214, 22);
            this.JOY.Name = "JOY";
            this.JOY.Size = new System.Drawing.Size(131, 25);
            this.JOY.TabIndex = 88;
            this.JOY.Text = "IMAGEN CENTRAL";
            this.JOY.UseVisualStyleBackColor = true;
            // 
            // deadzonePanel
            // 
            this.deadzonePanel.BackColor = System.Drawing.Color.Black;
            this.deadzonePanel.Controls.Add(this.radioCtrl3);
            this.deadzonePanel.Controls.Add(this.radioCtrl2);
            this.deadzonePanel.Controls.Add(this.numDeadzoneR);
            this.deadzonePanel.Controls.Add(this.lblDeadzoneR);
            this.deadzonePanel.Controls.Add(this.lblDeadzoneRTitle);
            this.deadzonePanel.Controls.Add(this.numDeadzoneL);
            this.deadzonePanel.Controls.Add(this.lblDeadzoneL);
            this.deadzonePanel.Controls.Add(this.lblDeadzoneLTitle);
            this.deadzonePanel.Controls.Add(this.radioCtrl4);
            this.deadzonePanel.Controls.Add(this.radioCtrl1);
            this.deadzonePanel.Location = new System.Drawing.Point(692, 650);
            this.deadzonePanel.Name = "deadzonePanel";
            this.deadzonePanel.Size = new System.Drawing.Size(346, 97);
            this.deadzonePanel.TabIndex = 93;
            this.deadzonePanel.Visible = false;
            // 
            // radioCtrl3
            // 
            this.radioCtrl3.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioCtrl3.AutoSize = true;
            this.radioCtrl3.FlatAppearance.BorderSize = 0;
            this.radioCtrl3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioCtrl3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCtrl3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.radioCtrl3.Location = new System.Drawing.Point(168, -1);
            this.radioCtrl3.Name = "radioCtrl3";
            this.radioCtrl3.Size = new System.Drawing.Size(27, 29);
            this.radioCtrl3.TabIndex = 13;
            this.radioCtrl3.TabStop = true;
            this.radioCtrl3.Text = "3";
            this.radioCtrl3.UseVisualStyleBackColor = true;
            // 
            // radioCtrl2
            // 
            this.radioCtrl2.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioCtrl2.AutoSize = true;
            this.radioCtrl2.FlatAppearance.BorderSize = 0;
            this.radioCtrl2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioCtrl2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCtrl2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.radioCtrl2.Location = new System.Drawing.Point(134, -1);
            this.radioCtrl2.Name = "radioCtrl2";
            this.radioCtrl2.Size = new System.Drawing.Size(27, 29);
            this.radioCtrl2.TabIndex = 12;
            this.radioCtrl2.TabStop = true;
            this.radioCtrl2.Text = "2";
            this.radioCtrl2.UseVisualStyleBackColor = true;
            // 
            // numDeadzoneR
            // 
            this.numDeadzoneR.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numDeadzoneR.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numDeadzoneR.Location = new System.Drawing.Point(202, 55);
            this.numDeadzoneR.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numDeadzoneR.Name = "numDeadzoneR";
            this.numDeadzoneR.Size = new System.Drawing.Size(57, 25);
            this.numDeadzoneR.TabIndex = 19;
            this.numDeadzoneR.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // lblDeadzoneR
            // 
            this.lblDeadzoneR.AutoSize = true;
            this.lblDeadzoneR.Font = new System.Drawing.Font("Impact", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeadzoneR.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblDeadzoneR.Location = new System.Drawing.Point(266, 48);
            this.lblDeadzoneR.Name = "lblDeadzoneR";
            this.lblDeadzoneR.Size = new System.Drawing.Size(60, 42);
            this.lblDeadzoneR.TabIndex = 20;
            this.lblDeadzoneR.Text = "8%";
            // 
            // lblDeadzoneRTitle
            // 
            this.lblDeadzoneRTitle.AutoSize = true;
            this.lblDeadzoneRTitle.Font = new System.Drawing.Font("Impact", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeadzoneRTitle.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblDeadzoneRTitle.Location = new System.Drawing.Point(132, 54);
            this.lblDeadzoneRTitle.Name = "lblDeadzoneRTitle";
            this.lblDeadzoneRTitle.Size = new System.Drawing.Size(70, 29);
            this.lblDeadzoneRTitle.TabIndex = 18;
            this.lblDeadzoneRTitle.Text = "RIGHT";
            this.lblDeadzoneRTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numDeadzoneL
            // 
            this.numDeadzoneL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numDeadzoneL.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numDeadzoneL.Location = new System.Drawing.Point(202, 22);
            this.numDeadzoneL.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numDeadzoneL.Name = "numDeadzoneL";
            this.numDeadzoneL.Size = new System.Drawing.Size(57, 25);
            this.numDeadzoneL.TabIndex = 16;
            this.numDeadzoneL.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // lblDeadzoneL
            // 
            this.lblDeadzoneL.AutoSize = true;
            this.lblDeadzoneL.Font = new System.Drawing.Font("Impact", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeadzoneL.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblDeadzoneL.Location = new System.Drawing.Point(266, 15);
            this.lblDeadzoneL.Name = "lblDeadzoneL";
            this.lblDeadzoneL.Size = new System.Drawing.Size(60, 42);
            this.lblDeadzoneL.TabIndex = 17;
            this.lblDeadzoneL.Text = "8%";
            // 
            // lblDeadzoneLTitle
            // 
            this.lblDeadzoneLTitle.AutoSize = true;
            this.lblDeadzoneLTitle.Font = new System.Drawing.Font("Impact", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeadzoneLTitle.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblDeadzoneLTitle.Location = new System.Drawing.Point(146, 22);
            this.lblDeadzoneLTitle.Name = "lblDeadzoneLTitle";
            this.lblDeadzoneLTitle.Size = new System.Drawing.Size(53, 29);
            this.lblDeadzoneLTitle.TabIndex = 15;
            this.lblDeadzoneLTitle.Text = "LEFT";
            this.lblDeadzoneLTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radioCtrl4
            // 
            this.radioCtrl4.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioCtrl4.AutoSize = true;
            this.radioCtrl4.FlatAppearance.BorderSize = 0;
            this.radioCtrl4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioCtrl4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCtrl4.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.radioCtrl4.Location = new System.Drawing.Point(202, -1);
            this.radioCtrl4.Name = "radioCtrl4";
            this.radioCtrl4.Size = new System.Drawing.Size(27, 29);
            this.radioCtrl4.TabIndex = 14;
            this.radioCtrl4.TabStop = true;
            this.radioCtrl4.Text = "4";
            this.radioCtrl4.UseVisualStyleBackColor = true;
            // 
            // radioCtrl1
            // 
            this.radioCtrl1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioCtrl1.AutoSize = true;
            this.radioCtrl1.FlatAppearance.BorderSize = 0;
            this.radioCtrl1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioCtrl1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCtrl1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.radioCtrl1.Location = new System.Drawing.Point(101, -1);
            this.radioCtrl1.Name = "radioCtrl1";
            this.radioCtrl1.Size = new System.Drawing.Size(27, 29);
            this.radioCtrl1.TabIndex = 11;
            this.radioCtrl1.TabStop = true;
            this.radioCtrl1.Text = "1";
            this.radioCtrl1.UseVisualStyleBackColor = true;
            // 
            // lblToast
            // 
            this.lblToast.AutoSize = true;
            this.lblToast.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToast.ForeColor = System.Drawing.Color.White;
            this.lblToast.Location = new System.Drawing.Point(608, 580);
            this.lblToast.Name = "lblToast";
            this.lblToast.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.lblToast.Size = new System.Drawing.Size(37, 39);
            this.lblToast.TabIndex = 95;
            this.lblToast.Text = "\"\"";
            this.lblToast.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblToast.Visible = false;
            // 
            // pbToastIcon
            // 
            this.pbToastIcon.BackColor = System.Drawing.Color.Transparent;
            this.pbToastIcon.Location = new System.Drawing.Point(507, 93);
            this.pbToastIcon.Name = "pbToastIcon";
            this.pbToastIcon.Size = new System.Drawing.Size(46, 40);
            this.pbToastIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbToastIcon.TabIndex = 97;
            this.pbToastIcon.TabStop = false;
            this.pbToastIcon.Visible = false;
            this.pbToastIcon.Click += new System.EventHandler(this.pbToastIcon_Click);
            // 
            // object_49b36b00_93ca_44b2_821f_3748e0116215
            // 
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.BackColor = System.Drawing.Color.Green;
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.Font = new System.Drawing.Font("Microsoft Uighur", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.Location = new System.Drawing.Point(105, 61);
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.Name = "object_49b36b00_93ca_44b2_821f_3748e0116215";
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.Size = new System.Drawing.Size(57, 15);
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.TabIndex = 74;
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.UseMnemonic = false;
            this.object_49b36b00_93ca_44b2_821f_3748e0116215.Visible = false;
            // 
            // Window_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.ClientSize = new System.Drawing.Size(1392, 773);
            this.Controls.Add(this.btnOpenJoyConfig);
            this.Controls.Add(this.pbToastIcon);
            this.Controls.Add(this.lblToast);
            this.Controls.Add(this.themePanel);
            this.Controls.Add(this.layoutPanel);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.START);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.DPAD);
            this.Controls.Add(this.RB);
            this.Controls.Add(this.A);
            this.Controls.Add(this.L3);
            this.Controls.Add(this.LS);
            this.Controls.Add(this.LB);
            this.Controls.Add(this.Y);
            this.Controls.Add(this.RT);
            this.Controls.Add(this.X);
            this.Controls.Add(this.LT);
            this.Controls.Add(this.GUIDE);
            this.Controls.Add(this.B);
            this.Controls.Add(this.lblToggleColors);
            this.Controls.Add(this.lblToggleLog);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblglobalKB);
            this.Controls.Add(this.appLogTextbox);
            this.Controls.Add(this.deadzonePanel);
            this.Controls.Add(this.lblToggleDeadzone);
            this.Controls.Add(this.BACK);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "Window_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XBC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Window_Main_FormClosing);
            this.Load += new System.EventHandler(this.Window_Main_Load);
            this.Resize += new System.EventHandler(this.Window_Main_Resize);
            this.trayIconMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnOpenJoyConfig)).EndInit();
            this.layoutPanel.ResumeLayout(false);
            this.layoutPanel.PerformLayout();
            this.themePanel.ResumeLayout(false);
            this.deadzonePanel.ResumeLayout(false);
            this.deadzonePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadzoneR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadzoneL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbToastIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox appLogTextbox;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayIconMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.TextBox chatpadTextBox;
        private System.Windows.Forms.PictureBox btnOpenJoyConfig;
        private System.Windows.Forms.Label B;
        private System.Windows.Forms.Label BACK;
        private System.Windows.Forms.Label GUIDE;
        private System.Windows.Forms.Label LT;
        private System.Windows.Forms.Label X;
        private System.Windows.Forms.Label RT;
        private System.Windows.Forms.Label Y;
        private System.Windows.Forms.Label LB;
        private System.Windows.Forms.Label LS;
        private System.Windows.Forms.Label L3;
        private System.Windows.Forms.Label A;
        private System.Windows.Forms.Label RB;
        private System.Windows.Forms.Label DPAD;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label START;
        private System.Windows.Forms.Label label25;
        private Label label27;
        private Label label29;
        private Label lblToggleDeadzone;
        private Label label7;
        private Label label1;
        private Label lblglobalKB;
        private Label lblToggleLog;
        private Label lblToggleColors;
        private Panel layoutPanel;
        private RadioButton rbQWERTY;
        private RadioButton rbQWERTZ;
        private RadioButton rbAZERTY;
        private Panel themePanel;
        private Button WP;
        private Button JOY;
        private Panel deadzonePanel;
        private NumericUpDown numDeadzoneR;
        private Label lblDeadzoneR;
        private Label lblDeadzoneRTitle;
        private NumericUpDown numDeadzoneL;
        private Label lblDeadzoneL;
        private Label lblDeadzoneLTitle;
        private RadioButton radioCtrl4;
        private RadioButton radioCtrl3;
        private RadioButton radioCtrl2;
        private RadioButton radioCtrl1;
        private Label lblToast;
        private PictureBox pbToastIcon;
        private Label object_49b36b00_93ca_44b2_821f_3748e0116215;
    }
}