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
            buttonConnectJoystick = new MaterialSkin.Controls.MaterialFloatingActionButton();
            imageListIcons = new ImageList(components);
            materialCard2 = new MaterialSkin.Controls.MaterialCard();
            labelJoystickStatus = new MaterialSkin.Controls.MaterialLabel();
            buttonConnectSimulator = new MaterialSkin.Controls.MaterialFloatingActionButton();
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            labelAircraftName = new MaterialSkin.Controls.MaterialLabel();
            FlightStatusLabel = new MaterialSkin.Controls.MaterialLabel();
            tabPageEffects = new TabPage();
            flowLayoutPanel1 = new FlowLayoutPanel();
            SwitchCenterSpring = new MaterialSkin.Controls.MaterialSwitch();
            panel2 = new Panel();
            switchDynamicSpring = new MaterialSkin.Controls.MaterialSwitch();
            panel3 = new Panel();
            TrimSwitch = new MaterialSkin.Controls.MaterialSwitch();
            trimPanel = new Panel();
            JoyBtnExplainLabel = new MaterialSkin.Controls.MaterialLabel();
            PitchUpLabel = new MaterialSkin.Controls.MaterialLabel();
            PitchUpTextBox = new MaterialSkin.Controls.MaterialTextBox();
            PitchDownLabel = new MaterialSkin.Controls.MaterialLabel();
            PitchDownTextBox = new MaterialSkin.Controls.MaterialTextBox();
            RollRightTextBox = new MaterialSkin.Controls.MaterialTextBox();
            RollLeftLabel = new MaterialSkin.Controls.MaterialLabel();
            RollRightLabel = new MaterialSkin.Controls.MaterialLabel();
            RollLeftTextBox = new MaterialSkin.Controls.MaterialTextBox();
            switchStickShaker = new MaterialSkin.Controls.MaterialSwitch();
            GearVibratesSwitch = new MaterialSkin.Controls.MaterialSwitch();
            GroundVibrationSwitch = new MaterialSkin.Controls.MaterialSwitch();
            buttonSaveNewProfile = new MaterialSkin.Controls.MaterialButton();
            comboBoxProfiles = new MaterialSkin.Controls.MaterialComboBox();
            tabPageDevices = new TabPage();
            textJoystickInfo = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            buttonRefresh = new MaterialSkin.Controls.MaterialButton();
            comboBoxJoysticks = new MaterialSkin.Controls.MaterialComboBox();
            tabPageSettings = new TabPage();
            cyclicSwitch = new MaterialSkin.Controls.MaterialSwitch();
            cyclicSettingsPanel = new Panel();
            dampingSlider = new MaterialSkin.Controls.MaterialSlider();
            cyclicSpringSlider = new MaterialSkin.Controls.MaterialSlider();
            trimDisconnectLabel = new MaterialSkin.Controls.MaterialLabel();
            trimDisconnectTextBox = new MaterialSkin.Controls.MaterialTextBox();
            trimResetLabel = new MaterialSkin.Controls.MaterialLabel();
            trimResetTextBox = new MaterialSkin.Controls.MaterialTextBox();
            materialTabControl1.SuspendLayout();
            tabPageDashboard.SuspendLayout();
            materialCard2.SuspendLayout();
            materialCard1.SuspendLayout();
            tabPageEffects.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            trimPanel.SuspendLayout();
            tabPageDevices.SuspendLayout();
            cyclicSettingsPanel.SuspendLayout();
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
            materialTabControl1.Location = new Point(0, 64);
            materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(797, 383);
            materialTabControl1.TabIndex = 0;
            // 
            // tabPageDashboard
            // 
            tabPageDashboard.BackColor = Color.WhiteSmoke;
            tabPageDashboard.Controls.Add(buttonConnectJoystick);
            tabPageDashboard.Controls.Add(materialCard2);
            tabPageDashboard.Controls.Add(buttonConnectSimulator);
            tabPageDashboard.Controls.Add(materialCard1);
            tabPageDashboard.ImageKey = "icons8-dashboard-layout-48.png";
            tabPageDashboard.Location = new Point(4, 39);
            tabPageDashboard.Name = "tabPageDashboard";
            tabPageDashboard.Padding = new Padding(3);
            tabPageDashboard.Size = new Size(789, 340);
            tabPageDashboard.TabIndex = 0;
            tabPageDashboard.Text = "Dashboard";
            // 
            // buttonConnectJoystick
            // 
            buttonConnectJoystick.Depth = 0;
            buttonConnectJoystick.ForeColor = SystemColors.ControlText;
            buttonConnectJoystick.Icon = Properties.Resources.icons8_joystick_48;
            buttonConnectJoystick.ImageKey = "icons8-joystick-48.png";
            buttonConnectJoystick.ImageList = imageListIcons;
            buttonConnectJoystick.Location = new Point(298, 114);
            buttonConnectJoystick.MouseState = MaterialSkin.MouseState.HOVER;
            buttonConnectJoystick.Name = "buttonConnectJoystick";
            buttonConnectJoystick.Size = new Size(56, 56);
            buttonConnectJoystick.TabIndex = 3;
            buttonConnectJoystick.UseVisualStyleBackColor = true;
            buttonConnectJoystick.Click += buttonConnectJoystick_Click;
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
            // materialCard2
            // 
            materialCard2.BackColor = Color.FromArgb(255, 255, 255);
            materialCard2.Controls.Add(labelJoystickStatus);
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
            // labelJoystickStatus
            // 
            labelJoystickStatus.AutoSize = true;
            labelJoystickStatus.Depth = 0;
            labelJoystickStatus.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelJoystickStatus.Location = new Point(17, 14);
            labelJoystickStatus.MouseState = MaterialSkin.MouseState.HOVER;
            labelJoystickStatus.Name = "labelJoystickStatus";
            labelJoystickStatus.Size = new Size(141, 19);
            labelJoystickStatus.TabIndex = 0;
            labelJoystickStatus.Text = "No joystick selected";
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
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(labelAircraftName);
            materialCard1.Controls.Add(FlightStatusLabel);
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
            // FlightStatusLabel
            // 
            FlightStatusLabel.AutoSize = true;
            FlightStatusLabel.Depth = 0;
            FlightStatusLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            FlightStatusLabel.Location = new Point(17, 43);
            FlightStatusLabel.MouseState = MaterialSkin.MouseState.HOVER;
            FlightStatusLabel.Name = "FlightStatusLabel";
            FlightStatusLabel.Size = new Size(89, 19);
            FlightStatusLabel.TabIndex = 3;
            FlightStatusLabel.Text = "Flight status";
            // 
            // tabPageEffects
            // 
            tabPageEffects.AutoScroll = true;
            tabPageEffects.BackColor = Color.WhiteSmoke;
            tabPageEffects.Controls.Add(flowLayoutPanel1);
            tabPageEffects.Controls.Add(buttonSaveNewProfile);
            tabPageEffects.Controls.Add(comboBoxProfiles);
            tabPageEffects.ImageKey = "icons8-depth-effect-48.png";
            tabPageEffects.Location = new Point(4, 39);
            tabPageEffects.Name = "tabPageEffects";
            tabPageEffects.Padding = new Padding(3);
            tabPageEffects.Size = new Size(789, 340);
            tabPageEffects.TabIndex = 1;
            tabPageEffects.Text = "Effects";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(SwitchCenterSpring);
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.Controls.Add(panel3);
            flowLayoutPanel1.Controls.Add(trimPanel);
            flowLayoutPanel1.Controls.Add(switchStickShaker);
            flowLayoutPanel1.Controls.Add(GearVibratesSwitch);
            flowLayoutPanel1.Controls.Add(GroundVibrationSwitch);
            flowLayoutPanel1.Controls.Add(cyclicSwitch);
            flowLayoutPanel1.Controls.Add(cyclicSettingsPanel);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(6, 61);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(597, 529);
            flowLayoutPanel1.TabIndex = 15;
            // 
            // SwitchCenterSpring
            // 
            SwitchCenterSpring.AutoSize = true;
            SwitchCenterSpring.BackColor = Color.Transparent;
            SwitchCenterSpring.BackgroundImageLayout = ImageLayout.None;
            SwitchCenterSpring.Checked = true;
            SwitchCenterSpring.CheckState = CheckState.Checked;
            SwitchCenterSpring.Depth = 0;
            SwitchCenterSpring.Location = new Point(0, 0);
            SwitchCenterSpring.Margin = new Padding(0);
            SwitchCenterSpring.MouseLocation = new Point(-1, -1);
            SwitchCenterSpring.MouseState = MaterialSkin.MouseState.HOVER;
            SwitchCenterSpring.Name = "SwitchCenterSpring";
            SwitchCenterSpring.Ripple = true;
            SwitchCenterSpring.Size = new Size(168, 37);
            SwitchCenterSpring.TabIndex = 0;
            SwitchCenterSpring.Text = "Centered spring";
            SwitchCenterSpring.UseVisualStyleBackColor = false;
            SwitchCenterSpring.CheckedChanged += SwitchCenterSpring_CheckedChanged;
            // 
            // panel2
            // 
            panel2.AutoSize = true;
            panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel2.Controls.Add(switchDynamicSpring);
            panel2.Location = new Point(0, 37);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(65, 0, 0, 0);
            panel2.Size = new Size(274, 37);
            panel2.TabIndex = 15;
            // 
            // switchDynamicSpring
            // 
            switchDynamicSpring.AutoSize = true;
            switchDynamicSpring.Depth = 0;
            switchDynamicSpring.Location = new Point(65, 0);
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
            // panel3
            // 
            panel3.AutoSize = true;
            panel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel3.Controls.Add(TrimSwitch);
            panel3.Location = new Point(0, 74);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(65, 0, 0, 0);
            panel3.Size = new Size(223, 37);
            panel3.TabIndex = 16;
            // 
            // TrimSwitch
            // 
            TrimSwitch.AutoSize = true;
            TrimSwitch.Depth = 0;
            TrimSwitch.Location = new Point(65, 0);
            TrimSwitch.Margin = new Padding(0);
            TrimSwitch.MouseLocation = new Point(-1, -1);
            TrimSwitch.MouseState = MaterialSkin.MouseState.HOVER;
            TrimSwitch.Name = "TrimSwitch";
            TrimSwitch.Ripple = true;
            TrimSwitch.Size = new Size(158, 37);
            TrimSwitch.TabIndex = 3;
            TrimSwitch.Text = "Dynamic Trim";
            TrimSwitch.UseVisualStyleBackColor = true;
            // 
            // trimPanel
            // 
            trimPanel.AutoSize = true;
            trimPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            trimPanel.Controls.Add(JoyBtnExplainLabel);
            trimPanel.Controls.Add(PitchUpLabel);
            trimPanel.Controls.Add(PitchUpTextBox);
            trimPanel.Controls.Add(PitchDownLabel);
            trimPanel.Controls.Add(PitchDownTextBox);
            trimPanel.Controls.Add(RollRightTextBox);
            trimPanel.Controls.Add(RollLeftLabel);
            trimPanel.Controls.Add(RollRightLabel);
            trimPanel.Controls.Add(RollLeftTextBox);
            trimPanel.Location = new Point(120, 111);
            trimPanel.Margin = new Padding(120, 0, 3, 0);
            trimPanel.Name = "trimPanel";
            trimPanel.Size = new Size(474, 104);
            trimPanel.TabIndex = 4;
            // 
            // JoyBtnExplainLabel
            // 
            JoyBtnExplainLabel.AutoSize = true;
            JoyBtnExplainLabel.Depth = 0;
            JoyBtnExplainLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            JoyBtnExplainLabel.Location = new Point(3, 0);
            JoyBtnExplainLabel.MouseState = MaterialSkin.MouseState.HOVER;
            JoyBtnExplainLabel.Name = "JoyBtnExplainLabel";
            JoyBtnExplainLabel.Size = new Size(285, 19);
            JoyBtnExplainLabel.TabIndex = 12;
            JoyBtnExplainLabel.Text = "Click in the textbox to set joystick button";
            // 
            // PitchUpLabel
            // 
            PitchUpLabel.AutoSize = true;
            PitchUpLabel.Depth = 0;
            PitchUpLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            PitchUpLabel.Location = new Point(3, 29);
            PitchUpLabel.MouseState = MaterialSkin.MouseState.HOVER;
            PitchUpLabel.Name = "PitchUpLabel";
            PitchUpLabel.Size = new Size(60, 19);
            PitchUpLabel.TabIndex = 4;
            PitchUpLabel.Text = "Pitch Up";
            // 
            // PitchUpTextBox
            // 
            PitchUpTextBox.AnimateReadOnly = false;
            PitchUpTextBox.BorderStyle = BorderStyle.None;
            PitchUpTextBox.Depth = 0;
            PitchUpTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            PitchUpTextBox.LeadingIcon = null;
            PitchUpTextBox.Location = new Point(3, 51);
            PitchUpTextBox.MaxLength = 50;
            PitchUpTextBox.MouseState = MaterialSkin.MouseState.OUT;
            PitchUpTextBox.Multiline = false;
            PitchUpTextBox.Name = "PitchUpTextBox";
            PitchUpTextBox.Size = new Size(111, 50);
            PitchUpTextBox.TabIndex = 5;
            PitchUpTextBox.Text = "";
            PitchUpTextBox.TrailingIcon = null;
            // 
            // PitchDownLabel
            // 
            PitchDownLabel.AutoSize = true;
            PitchDownLabel.Depth = 0;
            PitchDownLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            PitchDownLabel.Location = new Point(121, 29);
            PitchDownLabel.MouseState = MaterialSkin.MouseState.HOVER;
            PitchDownLabel.Name = "PitchDownLabel";
            PitchDownLabel.Size = new Size(82, 19);
            PitchDownLabel.TabIndex = 6;
            PitchDownLabel.Text = "Pitch Down";
            // 
            // PitchDownTextBox
            // 
            PitchDownTextBox.AnimateReadOnly = false;
            PitchDownTextBox.BorderStyle = BorderStyle.None;
            PitchDownTextBox.Depth = 0;
            PitchDownTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            PitchDownTextBox.LeadingIcon = null;
            PitchDownTextBox.Location = new Point(121, 51);
            PitchDownTextBox.MaxLength = 50;
            PitchDownTextBox.MouseState = MaterialSkin.MouseState.OUT;
            PitchDownTextBox.Multiline = false;
            PitchDownTextBox.Name = "PitchDownTextBox";
            PitchDownTextBox.Size = new Size(111, 50);
            PitchDownTextBox.TabIndex = 7;
            PitchDownTextBox.Text = "";
            PitchDownTextBox.TrailingIcon = null;
            // 
            // RollRightTextBox
            // 
            RollRightTextBox.AnimateReadOnly = false;
            RollRightTextBox.BorderStyle = BorderStyle.None;
            RollRightTextBox.Depth = 0;
            RollRightTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            RollRightTextBox.LeadingIcon = null;
            RollRightTextBox.Location = new Point(360, 51);
            RollRightTextBox.MaxLength = 50;
            RollRightTextBox.MouseState = MaterialSkin.MouseState.OUT;
            RollRightTextBox.Multiline = false;
            RollRightTextBox.Name = "RollRightTextBox";
            RollRightTextBox.Size = new Size(111, 50);
            RollRightTextBox.TabIndex = 11;
            RollRightTextBox.Text = "";
            RollRightTextBox.TrailingIcon = null;
            // 
            // RollLeftLabel
            // 
            RollLeftLabel.AutoSize = true;
            RollLeftLabel.Depth = 0;
            RollLeftLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            RollLeftLabel.Location = new Point(241, 29);
            RollLeftLabel.MouseState = MaterialSkin.MouseState.HOVER;
            RollLeftLabel.Name = "RollLeftLabel";
            RollLeftLabel.Size = new Size(60, 19);
            RollLeftLabel.TabIndex = 8;
            RollLeftLabel.Text = "Roll Left";
            // 
            // RollRightLabel
            // 
            RollRightLabel.AutoSize = true;
            RollRightLabel.Depth = 0;
            RollRightLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            RollRightLabel.Location = new Point(360, 29);
            RollRightLabel.MouseState = MaterialSkin.MouseState.HOVER;
            RollRightLabel.Name = "RollRightLabel";
            RollRightLabel.Size = new Size(69, 19);
            RollRightLabel.TabIndex = 10;
            RollRightLabel.Text = "Roll Right";
            // 
            // RollLeftTextBox
            // 
            RollLeftTextBox.AnimateReadOnly = false;
            RollLeftTextBox.BorderStyle = BorderStyle.None;
            RollLeftTextBox.Depth = 0;
            RollLeftTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            RollLeftTextBox.LeadingIcon = null;
            RollLeftTextBox.Location = new Point(241, 51);
            RollLeftTextBox.MaxLength = 50;
            RollLeftTextBox.MouseState = MaterialSkin.MouseState.OUT;
            RollLeftTextBox.Multiline = false;
            RollLeftTextBox.Name = "RollLeftTextBox";
            RollLeftTextBox.Size = new Size(111, 50);
            RollLeftTextBox.TabIndex = 9;
            RollLeftTextBox.Text = "";
            RollLeftTextBox.TrailingIcon = null;
            // 
            // switchStickShaker
            // 
            switchStickShaker.AutoSize = true;
            switchStickShaker.Checked = true;
            switchStickShaker.CheckState = CheckState.Checked;
            switchStickShaker.Depth = 0;
            switchStickShaker.Location = new Point(0, 215);
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
            // GearVibratesSwitch
            // 
            GearVibratesSwitch.AutoSize = true;
            GearVibratesSwitch.Depth = 0;
            GearVibratesSwitch.Location = new Point(0, 252);
            GearVibratesSwitch.Margin = new Padding(0);
            GearVibratesSwitch.MouseLocation = new Point(-1, -1);
            GearVibratesSwitch.MouseState = MaterialSkin.MouseState.HOVER;
            GearVibratesSwitch.Name = "GearVibratesSwitch";
            GearVibratesSwitch.Ripple = true;
            GearVibratesSwitch.Size = new Size(165, 37);
            GearVibratesSwitch.TabIndex = 13;
            GearVibratesSwitch.Text = "Gear vibrations";
            GearVibratesSwitch.UseVisualStyleBackColor = true;
            // 
            // GroundVibrationSwitch
            // 
            GroundVibrationSwitch.AutoSize = true;
            GroundVibrationSwitch.Depth = 0;
            GroundVibrationSwitch.Location = new Point(0, 289);
            GroundVibrationSwitch.Margin = new Padding(0);
            GroundVibrationSwitch.MouseLocation = new Point(-1, -1);
            GroundVibrationSwitch.MouseState = MaterialSkin.MouseState.HOVER;
            GroundVibrationSwitch.Name = "GroundVibrationSwitch";
            GroundVibrationSwitch.Ripple = true;
            GroundVibrationSwitch.Size = new Size(184, 37);
            GroundVibrationSwitch.TabIndex = 14;
            GroundVibrationSwitch.Text = "Ground vibrations";
            GroundVibrationSwitch.UseVisualStyleBackColor = true;
            // 
            // buttonSaveNewProfile
            // 
            buttonSaveNewProfile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSaveNewProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonSaveNewProfile.Depth = 0;
            buttonSaveNewProfile.HighEmphasis = true;
            buttonSaveNewProfile.Icon = null;
            buttonSaveNewProfile.Location = new Point(322, 9);
            buttonSaveNewProfile.Margin = new Padding(4, 6, 4, 6);
            buttonSaveNewProfile.MouseState = MaterialSkin.MouseState.HOVER;
            buttonSaveNewProfile.Name = "buttonSaveNewProfile";
            buttonSaveNewProfile.NoAccentTextColor = Color.Empty;
            buttonSaveNewProfile.Size = new Size(155, 36);
            buttonSaveNewProfile.TabIndex = 0;
            buttonSaveNewProfile.Text = "Save New Profile";
            buttonSaveNewProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonSaveNewProfile.UseAccentColor = false;
            // 
            // comboBoxProfiles
            // 
            comboBoxProfiles.AutoResize = false;
            comboBoxProfiles.BackColor = Color.FromArgb(255, 255, 255);
            comboBoxProfiles.Depth = 0;
            comboBoxProfiles.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxProfiles.DropDownHeight = 174;
            comboBoxProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxProfiles.DropDownWidth = 121;
            comboBoxProfiles.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboBoxProfiles.ForeColor = Color.FromArgb(222, 0, 0, 0);
            comboBoxProfiles.IntegralHeight = false;
            comboBoxProfiles.ItemHeight = 43;
            comboBoxProfiles.Location = new Point(6, 6);
            comboBoxProfiles.MaxDropDownItems = 4;
            comboBoxProfiles.MouseState = MaterialSkin.MouseState.OUT;
            comboBoxProfiles.Name = "comboBoxProfiles";
            comboBoxProfiles.Size = new Size(308, 49);
            comboBoxProfiles.StartIndex = 0;
            comboBoxProfiles.TabIndex = 1;
            // 
            // tabPageDevices
            // 
            tabPageDevices.BackColor = Color.WhiteSmoke;
            tabPageDevices.Controls.Add(textJoystickInfo);
            tabPageDevices.Controls.Add(buttonRefresh);
            tabPageDevices.Controls.Add(comboBoxJoysticks);
            tabPageDevices.ImageKey = "icons8-joystick-48.png";
            tabPageDevices.Location = new Point(4, 39);
            tabPageDevices.Name = "tabPageDevices";
            tabPageDevices.Size = new Size(789, 340);
            tabPageDevices.TabIndex = 2;
            tabPageDevices.Text = "Devices";
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
            tabPageSettings.Size = new Size(789, 340);
            tabPageSettings.TabIndex = 3;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // cyclicSwitch
            // 
            cyclicSwitch.AutoSize = true;
            cyclicSwitch.Depth = 0;
            cyclicSwitch.Location = new Point(0, 326);
            cyclicSwitch.Margin = new Padding(0);
            cyclicSwitch.MouseLocation = new Point(-1, -1);
            cyclicSwitch.MouseState = MaterialSkin.MouseState.HOVER;
            cyclicSwitch.Name = "cyclicSwitch";
            cyclicSwitch.Ripple = true;
            cyclicSwitch.Size = new Size(176, 37);
            cyclicSwitch.TabIndex = 17;
            cyclicSwitch.Text = "Springless cyclic";
            cyclicSwitch.UseVisualStyleBackColor = true;
            // 
            // cyclicSettingsPanel
            // 
            cyclicSettingsPanel.AutoSize = true;
            cyclicSettingsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cyclicSettingsPanel.Controls.Add(trimResetTextBox);
            cyclicSettingsPanel.Controls.Add(trimResetLabel);
            cyclicSettingsPanel.Controls.Add(trimDisconnectTextBox);
            cyclicSettingsPanel.Controls.Add(trimDisconnectLabel);
            cyclicSettingsPanel.Controls.Add(cyclicSpringSlider);
            cyclicSettingsPanel.Controls.Add(dampingSlider);
            cyclicSettingsPanel.Location = new Point(65, 363);
            cyclicSettingsPanel.Margin = new Padding(65, 0, 0, 0);
            cyclicSettingsPanel.Name = "cyclicSettingsPanel";
            cyclicSettingsPanel.Size = new Size(518, 166);
            cyclicSettingsPanel.TabIndex = 18;
            // 
            // dampingSlider
            // 
            dampingSlider.Depth = 0;
            dampingSlider.ForeColor = Color.FromArgb(222, 0, 0, 0);
            dampingSlider.Location = new Point(3, 3);
            dampingSlider.MouseState = MaterialSkin.MouseState.HOVER;
            dampingSlider.Name = "dampingSlider";
            dampingSlider.Size = new Size(512, 40);
            dampingSlider.TabIndex = 1;
            dampingSlider.Text = "Damping";
            dampingSlider.Value = 100;
            // 
            // cyclicSpringSlider
            // 
            cyclicSpringSlider.Depth = 0;
            cyclicSpringSlider.ForeColor = Color.FromArgb(222, 0, 0, 0);
            cyclicSpringSlider.Location = new Point(0, 38);
            cyclicSpringSlider.MouseState = MaterialSkin.MouseState.HOVER;
            cyclicSpringSlider.Name = "cyclicSpringSlider";
            cyclicSpringSlider.Size = new Size(512, 40);
            cyclicSpringSlider.TabIndex = 2;
            cyclicSpringSlider.Text = "Spring     ";
            // 
            // trimDisconnectLabel
            // 
            trimDisconnectLabel.AutoSize = true;
            trimDisconnectLabel.Depth = 0;
            trimDisconnectLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            trimDisconnectLabel.Location = new Point(0, 81);
            trimDisconnectLabel.MouseState = MaterialSkin.MouseState.HOVER;
            trimDisconnectLabel.Name = "trimDisconnectLabel";
            trimDisconnectLabel.Size = new Size(115, 19);
            trimDisconnectLabel.TabIndex = 3;
            trimDisconnectLabel.Text = "Trim disconnect";
            // 
            // trimDisconnectTextBox
            // 
            trimDisconnectTextBox.AnimateReadOnly = false;
            trimDisconnectTextBox.BorderStyle = BorderStyle.None;
            trimDisconnectTextBox.Depth = 0;
            trimDisconnectTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            trimDisconnectTextBox.LeadingIcon = null;
            trimDisconnectTextBox.Location = new Point(3, 113);
            trimDisconnectTextBox.MaxLength = 50;
            trimDisconnectTextBox.MouseState = MaterialSkin.MouseState.OUT;
            trimDisconnectTextBox.Multiline = false;
            trimDisconnectTextBox.Name = "trimDisconnectTextBox";
            trimDisconnectTextBox.Size = new Size(100, 50);
            trimDisconnectTextBox.TabIndex = 4;
            trimDisconnectTextBox.Text = "";
            trimDisconnectTextBox.TrailingIcon = null;
            // 
            // trimResetLabel
            // 
            trimResetLabel.AutoSize = true;
            trimResetLabel.Depth = 0;
            trimResetLabel.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            trimResetLabel.Location = new Point(136, 81);
            trimResetLabel.MouseState = MaterialSkin.MouseState.HOVER;
            trimResetLabel.Name = "trimResetLabel";
            trimResetLabel.Size = new Size(72, 19);
            trimResetLabel.TabIndex = 5;
            trimResetLabel.Text = "Trim reset";
            // 
            // trimResetTextBox
            // 
            trimResetTextBox.AnimateReadOnly = false;
            trimResetTextBox.BorderStyle = BorderStyle.None;
            trimResetTextBox.Depth = 0;
            trimResetTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            trimResetTextBox.LeadingIcon = null;
            trimResetTextBox.Location = new Point(136, 113);
            trimResetTextBox.MaxLength = 50;
            trimResetTextBox.MouseState = MaterialSkin.MouseState.OUT;
            trimResetTextBox.Multiline = false;
            trimResetTextBox.Name = "trimResetTextBox";
            trimResetTextBox.Size = new Size(100, 50);
            trimResetTextBox.TabIndex = 6;
            trimResetTextBox.Text = "";
            trimResetTextBox.TrailingIcon = null;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(materialTabControl1);
            DrawerShowIconsWhenHidden = true;
            DrawerTabControl = materialTabControl1;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Padding = new Padding(0, 64, 3, 3);
            Text = "TDX Air Mechanic";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            materialTabControl1.ResumeLayout(false);
            tabPageDashboard.ResumeLayout(false);
            materialCard2.ResumeLayout(false);
            materialCard2.PerformLayout();
            materialCard1.ResumeLayout(false);
            materialCard1.PerformLayout();
            tabPageEffects.ResumeLayout(false);
            tabPageEffects.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            trimPanel.ResumeLayout(false);
            trimPanel.PerformLayout();
            tabPageDevices.ResumeLayout(false);
            tabPageDevices.PerformLayout();
            cyclicSettingsPanel.ResumeLayout(false);
            cyclicSettingsPanel.PerformLayout();
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
        private MaterialSkin.Controls.MaterialSwitch SwitchCenterSpring;
        private MaterialSkin.Controls.MaterialSwitch switchDynamicSpring;
        private MaterialSkin.Controls.MaterialSwitch switchStickShaker;
        private MaterialSkin.Controls.MaterialMultiLineTextBox textJoystickInfo;
        private MaterialSkin.Controls.MaterialComboBox comboBoxProfiles;
        private MaterialSkin.Controls.MaterialButton buttonSaveNewProfile;
        private MaterialSkin.Controls.MaterialTextBox PitchUpTextBox;
        private MaterialSkin.Controls.MaterialLabel PitchUpLabel;
        private MaterialSkin.Controls.MaterialSwitch TrimSwitch;
        private MaterialSkin.Controls.MaterialTextBox RollRightTextBox;
        private MaterialSkin.Controls.MaterialLabel RollRightLabel;
        private MaterialSkin.Controls.MaterialTextBox RollLeftTextBox;
        private MaterialSkin.Controls.MaterialLabel RollLeftLabel;
        private MaterialSkin.Controls.MaterialTextBox PitchDownTextBox;
        private MaterialSkin.Controls.MaterialLabel PitchDownLabel;
        private MaterialSkin.Controls.MaterialLabel JoyBtnExplainLabel;
        private MaterialSkin.Controls.MaterialSwitch GearVibratesSwitch;
        private MaterialSkin.Controls.MaterialLabel FlightStatusLabel;
        private MaterialSkin.Controls.MaterialSwitch GroundVibrationSwitch;
        private MaterialSkin.Controls.MaterialFloatingActionButton buttonConnectJoystick;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel trimPanel;
        private Panel panel2;
        private Panel panel3;
        private MaterialSkin.Controls.MaterialSwitch cyclicSwitch;
        private Panel cyclicSettingsPanel;
        private MaterialSkin.Controls.MaterialSlider dampingSlider;
        private MaterialSkin.Controls.MaterialSlider cyclicSpringSlider;
        private MaterialSkin.Controls.MaterialLabel trimResetLabel;
        private MaterialSkin.Controls.MaterialTextBox trimDisconnectTextBox;
        private MaterialSkin.Controls.MaterialLabel trimDisconnectLabel;
        private MaterialSkin.Controls.MaterialTextBox trimResetTextBox;
    }
}
