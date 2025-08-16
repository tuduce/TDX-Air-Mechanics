namespace TDXAirMechanic
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            tabPageDashboard = new TabPage();
            materialCard2 = new MaterialSkin.Controls.MaterialCard();
            buttonConnectSimulator = new MaterialSkin.Controls.MaterialFloatingActionButton();
            imageListIcons = new ImageList(components);
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            labelJoystickStatus = new MaterialSkin.Controls.MaterialLabel();
            labelAircraftName = new MaterialSkin.Controls.MaterialLabel();
            tabPageEffects = new TabPage();
            switchStickShaker = new MaterialSkin.Controls.MaterialSwitch();
            switchDynamicSpring = new MaterialSkin.Controls.MaterialSwitch();
            switchCenterSpring = new MaterialSkin.Controls.MaterialSwitch();
            tabPageDevices = new TabPage();
            textJoystickInfo = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            buttonRefresh = new MaterialSkin.Controls.MaterialButton();
            comboBoxJoysticks = new MaterialSkin.Controls.MaterialComboBox();
            tabPageSettings = new TabPage();
            materialTabControl1.SuspendLayout();
            tabPageDashboard.SuspendLayout();
            materialCard1.SuspendLayout();
            tabPageEffects.SuspendLayout();
            tabPageDevices.SuspendLayout();
            SuspendLayout();
            // 
            // materialTabControl1
            // 
            materialTabControl1.Controls.Add(tabPageDashboard);
            materialTabControl1.Controls.Add(tabPageEffects);
            materialTabControl1.Controls.Add(tabPageDevices);
            materialTabControl1.Controls.Add(tabPageSettings);
            materialTabControl1.Depth = 0;
            materialTabControl1.Dock = DockStyle.Fill;
            materialTabControl1.ImageList = imageListIcons;
            materialTabControl1.Location = new Point(3, 64);
            materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(794, 383);
            materialTabControl1.TabIndex = 0;
            // 
            // tabPageDashboard
            // 
            tabPageDashboard.Controls.Add(materialCard2);
            tabPageDashboard.Controls.Add(buttonConnectSimulator);
            tabPageDashboard.Controls.Add(materialCard1);
            tabPageDashboard.ImageKey = "icons8-dashboard-layout-48.png";
            tabPageDashboard.Location = new Point(4, 39);
            tabPageDashboard.Name = "tabPageDashboard";
            tabPageDashboard.Padding = new Padding(3);
            tabPageDashboard.Size = new Size(786, 340);
            tabPageDashboard.TabIndex = 0;
            tabPageDashboard.Text = "Dashboard";
            tabPageDashboard.UseVisualStyleBackColor = true;
            // 
            // materialCard2
            // 
            materialCard2.BackColor = Color.FromArgb(255, 255, 255);
            materialCard2.Depth = 0;
            materialCard2.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard2.Location = new Point(17, 104);
            materialCard2.Margin = new Padding(14);
            materialCard2.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard2.Name = "materialCard2";
            materialCard2.Padding = new Padding(14);
            materialCard2.Size = new Size(310, 76);
            materialCard2.TabIndex = 2;
            // 
            // buttonConnectSimulator
            // 
            buttonConnectSimulator.Depth = 0;
            buttonConnectSimulator.Icon = Properties.Resources.icons8_paper_airplane_48;
            buttonConnectSimulator.ImageKey = "icons8-paper-airplane-48.png";
            buttonConnectSimulator.ImageList = imageListIcons;
            buttonConnectSimulator.Location = new Point(298, 23);
            buttonConnectSimulator.MouseState = MaterialSkin.MouseState.HOVER;
            buttonConnectSimulator.Name = "buttonConnectSimulator";
            buttonConnectSimulator.Size = new Size(56, 56);
            buttonConnectSimulator.TabIndex = 1;
            buttonConnectSimulator.UseVisualStyleBackColor = true;
            buttonConnectSimulator.Click += buttonConnectSimulator_Click;
            // 
            // imageListIcons
            // 
            imageListIcons.ColorDepth = ColorDepth.Depth32Bit;
            imageListIcons.ImageStream = (ImageListStreamer)resources.GetObject("imageListIcons.ImageStream");
            imageListIcons.TransparentColor = Color.Transparent;
            imageListIcons.Images.SetKeyName(0, "icons8-settings-48.png");
            imageListIcons.Images.SetKeyName(1, "icons8-depth-effect-48.png");
            imageListIcons.Images.SetKeyName(2, "icons8-dashboard-layout-48.png");
            imageListIcons.Images.SetKeyName(3, "icons8-joystick-48.png");
            imageListIcons.Images.SetKeyName(4, "icons8-paper-airplane-48.png");
            // 
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(labelJoystickStatus);
            materialCard1.Controls.Add(labelAircraftName);
            materialCard1.Depth = 0;
            materialCard1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new Point(17, 14);
            materialCard1.Margin = new Padding(14);
            materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new Padding(14);
            materialCard1.Size = new Size(310, 76);
            materialCard1.TabIndex = 0;
            // 
            // labelJoystickStatus
            // 
            labelJoystickStatus.AutoSize = true;
            labelJoystickStatus.Depth = 0;
            labelJoystickStatus.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelJoystickStatus.Location = new Point(17, 43);
            labelJoystickStatus.MouseState = MaterialSkin.MouseState.HOVER;
            labelJoystickStatus.Name = "labelJoystickStatus";
            labelJoystickStatus.Size = new Size(141, 19);
            labelJoystickStatus.TabIndex = 0;
            labelJoystickStatus.Text = "No joystick selected";
            // 
            // labelAircraftName
            // 
            labelAircraftName.AutoSize = true;
            labelAircraftName.Depth = 0;
            labelAircraftName.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelAircraftName.ImageKey = "(none)";
            labelAircraftName.Location = new Point(17, 14);
            labelAircraftName.MouseState = MaterialSkin.MouseState.HOVER;
            labelAircraftName.Name = "labelAircraftName";
            labelAircraftName.Size = new Size(191, 19);
            labelAircraftName.TabIndex = 0;
            labelAircraftName.Text = "Aircraft profile not selected";
            // 
            // tabPageEffects
            // 
            tabPageEffects.Controls.Add(switchStickShaker);
            tabPageEffects.Controls.Add(switchDynamicSpring);
            tabPageEffects.Controls.Add(switchCenterSpring);
            tabPageEffects.ImageKey = "icons8-depth-effect-48.png";
            tabPageEffects.Location = new Point(4, 39);
            tabPageEffects.Name = "tabPageEffects";
            tabPageEffects.Padding = new Padding(3);
            tabPageEffects.Size = new Size(786, 340);
            tabPageEffects.TabIndex = 1;
            tabPageEffects.Text = "Effects";
            tabPageEffects.UseVisualStyleBackColor = true;
            // 
            // switchStickShaker
            // 
            switchStickShaker.AutoSize = true;
            switchStickShaker.Checked = true;
            switchStickShaker.CheckState = CheckState.Checked;
            switchStickShaker.Depth = 0;
            switchStickShaker.Location = new Point(13, 105);
            switchStickShaker.Margin = new Padding(0);
            switchStickShaker.MouseLocation = new Point(-1, -1);
            switchStickShaker.MouseState = MaterialSkin.MouseState.HOVER;
            switchStickShaker.Name = "switchStickShaker";
            switchStickShaker.Ripple = true;
            switchStickShaker.Size = new Size(144, 37);
            switchStickShaker.TabIndex = 2;
            switchStickShaker.Text = "Stick shaker";
            switchStickShaker.UseVisualStyleBackColor = true;
            // 
            // switchDynamicSpring
            // 
            switchDynamicSpring.AutoSize = true;
            switchDynamicSpring.Depth = 0;
            switchDynamicSpring.Location = new Point(63, 59);
            switchDynamicSpring.Margin = new Padding(0);
            switchDynamicSpring.MouseLocation = new Point(-1, -1);
            switchDynamicSpring.MouseState = MaterialSkin.MouseState.HOVER;
            switchDynamicSpring.Name = "switchDynamicSpring";
            switchDynamicSpring.Ripple = true;
            switchDynamicSpring.Size = new Size(209, 37);
            switchDynamicSpring.TabIndex = 1;
            switchDynamicSpring.Text = "Dynamic spring force";
            switchDynamicSpring.UseVisualStyleBackColor = true;
            // 
            // switchCenterSpring
            // 
            switchCenterSpring.AutoSize = true;
            switchCenterSpring.BackColor = Color.Transparent;
            switchCenterSpring.BackgroundImageLayout = ImageLayout.None;
            switchCenterSpring.Checked = true;
            switchCenterSpring.CheckState = CheckState.Checked;
            switchCenterSpring.Depth = 0;
            switchCenterSpring.Location = new Point(13, 13);
            switchCenterSpring.Margin = new Padding(0);
            switchCenterSpring.MouseLocation = new Point(-1, -1);
            switchCenterSpring.MouseState = MaterialSkin.MouseState.HOVER;
            switchCenterSpring.Name = "switchCenterSpring";
            switchCenterSpring.Ripple = true;
            switchCenterSpring.Size = new Size(168, 37);
            switchCenterSpring.TabIndex = 0;
            switchCenterSpring.Text = "Centered spring";
            switchCenterSpring.UseVisualStyleBackColor = false;
            switchCenterSpring.CheckedChanged += switchCenterSpring_CheckedChanged;
            // 
            // tabPageDevices
            // 
            tabPageDevices.Controls.Add(textJoystickInfo);
            tabPageDevices.Controls.Add(buttonRefresh);
            tabPageDevices.Controls.Add(comboBoxJoysticks);
            tabPageDevices.ImageKey = "icons8-joystick-48.png";
            tabPageDevices.Location = new Point(4, 39);
            tabPageDevices.Name = "tabPageDevices";
            tabPageDevices.Size = new Size(786, 340);
            tabPageDevices.TabIndex = 2;
            tabPageDevices.Text = "Devices";
            tabPageDevices.UseVisualStyleBackColor = true;
            // 
            // textJoystickInfo
            // 
            textJoystickInfo.BackColor = Color.FromArgb(255, 255, 255);
            textJoystickInfo.BorderStyle = BorderStyle.None;
            textJoystickInfo.Depth = 0;
            textJoystickInfo.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            textJoystickInfo.ForeColor = Color.FromArgb(222, 0, 0, 0);
            textJoystickInfo.Location = new Point(17, 86);
            textJoystickInfo.MouseState = MaterialSkin.MouseState.HOVER;
            textJoystickInfo.Name = "textJoystickInfo";
            textJoystickInfo.ReadOnly = true;
            textJoystickInfo.Size = new Size(405, 175);
            textJoystickInfo.TabIndex = 2;
            textJoystickInfo.Text = "";
            // 
            // buttonRefresh
            // 
            buttonRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonRefresh.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonRefresh.Depth = 0;
            buttonRefresh.HighEmphasis = true;
            buttonRefresh.Icon = null;
            buttonRefresh.Location = new Point(338, 24);
            buttonRefresh.Margin = new Padding(4, 6, 4, 6);
            buttonRefresh.MouseState = MaterialSkin.MouseState.HOVER;
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.NoAccentTextColor = Color.Empty;
            buttonRefresh.Size = new Size(84, 36);
            buttonRefresh.TabIndex = 1;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonRefresh.UseAccentColor = false;
            buttonRefresh.UseVisualStyleBackColor = true;
            buttonRefresh.Click += buttonRefresh_Click;
            // 
            // comboBoxJoysticks
            // 
            comboBoxJoysticks.AutoResize = false;
            comboBoxJoysticks.BackColor = Color.FromArgb(255, 255, 255);
            comboBoxJoysticks.Depth = 0;
            comboBoxJoysticks.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxJoysticks.DropDownHeight = 174;
            comboBoxJoysticks.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxJoysticks.DropDownWidth = 121;
            comboBoxJoysticks.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboBoxJoysticks.ForeColor = Color.FromArgb(222, 0, 0, 0);
            comboBoxJoysticks.FormattingEnabled = true;
            comboBoxJoysticks.IntegralHeight = false;
            comboBoxJoysticks.ItemHeight = 43;
            comboBoxJoysticks.Location = new Point(17, 18);
            comboBoxJoysticks.MaxDropDownItems = 4;
            comboBoxJoysticks.MouseState = MaterialSkin.MouseState.OUT;
            comboBoxJoysticks.Name = "comboBoxJoysticks";
            comboBoxJoysticks.Size = new Size(307, 49);
            comboBoxJoysticks.StartIndex = 0;
            comboBoxJoysticks.TabIndex = 0;
            // 
            // tabPageSettings
            // 
            tabPageSettings.ImageKey = "icons8-settings-48.png";
            tabPageSettings.Location = new Point(4, 39);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Size = new Size(786, 340);
            tabPageSettings.TabIndex = 3;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(materialTabControl1);
            DrawerShowIconsWhenHidden = true;
            DrawerTabControl = materialTabControl1;
            Name = "MainForm";
            Text = "TDX Air Mechanic";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            materialTabControl1.ResumeLayout(false);
            tabPageDashboard.ResumeLayout(false);
            materialCard1.ResumeLayout(false);
            materialCard1.PerformLayout();
            tabPageEffects.ResumeLayout(false);
            tabPageEffects.PerformLayout();
            tabPageDevices.ResumeLayout(false);
            tabPageDevices.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private TabPage tabPageDashboard;
        private TabPage tabPageEffects;
        private ImageList imageListIcons;
        private TabPage tabPageDevices;
        private TabPage tabPageSettings;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialLabel labelAircraftName;
        private MaterialSkin.Controls.MaterialFloatingActionButton buttonConnectSimulator;
        private MaterialSkin.Controls.MaterialButton buttonRefresh;
        private MaterialSkin.Controls.MaterialComboBox comboBoxJoysticks;
        private MaterialSkin.Controls.MaterialCard materialCard2;
        private MaterialSkin.Controls.MaterialLabel labelJoystickStatus;
        private MaterialSkin.Controls.MaterialSwitch switchCenterSpring;
        private MaterialSkin.Controls.MaterialSwitch switchDynamicSpring;
        private MaterialSkin.Controls.MaterialSwitch switchStickShaker;
        private MaterialSkin.Controls.MaterialMultiLineTextBox textJoystickInfo;
    }
}
