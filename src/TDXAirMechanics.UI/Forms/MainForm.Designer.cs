namespace TDXAirMechanics.UI.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;        /// <summary>
                                                                           /// Clean up any resources being used.
                                                                           /// </summary>
                                                                           /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        // Dispose method is implemented in MainForm.cs

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label2 = new Label();
            forceYLabel = new Label();
            buttonSimConnected = new Button();
            forceXLabel = new Label();
            labelJoystickSelected = new Label();
            forceYProgressBar = new ProgressBar();
            buttonJoystickSelected = new Button();
            forceXProgressBar = new ProgressBar();
            headingLabel = new Label();
            altitudeLabel = new Label();
            airspeedLabel = new Label();
            disconnectButton = new Button();
            connectButton = new Button();
            joystickStatusLabel = new Label();
            matLabelMSFSConnected = new MaterialSkin.Controls.MaterialLabel();
            closeToTrayCheckBox = new CheckBox();
            enableForceFeedbackCheckBox = new CheckBox();
            forceMultiplierLabel = new Label();
            forceMultiplierTrackBar = new TrackBar();
            selectDeviceButton = new Button();
            refreshDevicesButton = new Button();
            joystickListBox = new ListBox();
            mainTabControl = new MaterialSkin.Controls.MaterialTabControl();
            tabPageDashboard = new TabPage();
            materialCardSimulator = new MaterialSkin.Controls.MaterialCard();
            pictureBox1 = new PictureBox();
            materialCardJoystick = new MaterialSkin.Controls.MaterialCard();
            pictureBoxJoystick = new PictureBox();
            tabPageEffects = new TabPage();
            switchShakeOnStallWarning = new MaterialSkin.Controls.MaterialSwitch();
            switchDynamicControlForces = new MaterialSkin.Controls.MaterialSwitch();
            tabPageDevice = new TabPage();
            tabPageSettings = new TabPage();
            iconsList = new ImageList(components);
            ((System.ComponentModel.ISupportInitialize)forceMultiplierTrackBar).BeginInit();
            mainTabControl.SuspendLayout();
            tabPageDashboard.SuspendLayout();
            materialCardSimulator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            materialCardJoystick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxJoystick).BeginInit();
            tabPageEffects.SuspendLayout();
            tabPageDevice.SuspendLayout();
            tabPageSettings.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(101, 416);
            label2.Name = "label2";
            label2.Size = new Size(93, 17);
            label2.TabIndex = 3;
            label2.Text = "Sim connected";
            // 
            // forceYLabel
            // 
            forceYLabel.AutoSize = true;
            forceYLabel.Location = new Point(40, 333);
            forceYLabel.Name = "forceYLabel";
            forceYLabel.Size = new Size(54, 17);
            forceYLabel.TabIndex = 3;
            forceYLabel.Text = "Force Y:";
            // 
            // buttonSimConnected
            // 
            buttonSimConnected.BackColor = Color.Tomato;
            buttonSimConnected.FlatAppearance.BorderColor = Color.Firebrick;
            buttonSimConnected.FlatAppearance.BorderSize = 3;
            buttonSimConnected.FlatStyle = FlatStyle.Flat;
            buttonSimConnected.Location = new Point(156, 37);
            buttonSimConnected.Name = "buttonSimConnected";
            buttonSimConnected.Size = new Size(19, 21);
            buttonSimConnected.TabIndex = 2;
            buttonSimConnected.UseVisualStyleBackColor = false;
            // 
            // forceXLabel
            // 
            forceXLabel.AutoSize = true;
            forceXLabel.Location = new Point(40, 289);
            forceXLabel.Name = "forceXLabel";
            forceXLabel.Size = new Size(55, 17);
            forceXLabel.TabIndex = 2;
            forceXLabel.Text = "Force X:";
            // 
            // labelJoystickSelected
            // 
            labelJoystickSelected.AutoSize = true;
            labelJoystickSelected.Location = new Point(78, 14);
            labelJoystickSelected.Name = "labelJoystickSelected";
            labelJoystickSelected.Size = new Size(124, 17);
            labelJoystickSelected.TabIndex = 1;
            labelJoystickSelected.Text = "No joystick selected";
            // 
            // forceYProgressBar
            // 
            forceYProgressBar.Location = new Point(110, 328);
            forceYProgressBar.Margin = new Padding(3, 2, 3, 2);
            forceYProgressBar.Maximum = 200;
            forceYProgressBar.Name = "forceYProgressBar";
            forceYProgressBar.Size = new Size(350, 25);
            forceYProgressBar.TabIndex = 1;
            // 
            // buttonJoystickSelected
            // 
            buttonJoystickSelected.BackColor = Color.Tomato;
            buttonJoystickSelected.Dock = DockStyle.Left;
            buttonJoystickSelected.FlatAppearance.BorderColor = Color.Firebrick;
            buttonJoystickSelected.FlatAppearance.BorderSize = 3;
            buttonJoystickSelected.FlatStyle = FlatStyle.Flat;
            buttonJoystickSelected.Location = new Point(14, 14);
            buttonJoystickSelected.Name = "buttonJoystickSelected";
            buttonJoystickSelected.Size = new Size(10, 47);
            buttonJoystickSelected.TabIndex = 0;
            buttonJoystickSelected.UseVisualStyleBackColor = false;
            // 
            // forceXProgressBar
            // 
            forceXProgressBar.Location = new Point(110, 286);
            forceXProgressBar.Margin = new Padding(3, 2, 3, 2);
            forceXProgressBar.Maximum = 200;
            forceXProgressBar.Name = "forceXProgressBar";
            forceXProgressBar.Size = new Size(350, 25);
            forceXProgressBar.TabIndex = 0;
            // 
            // headingLabel
            // 
            headingLabel.AutoSize = true;
            headingLabel.Location = new Point(391, 217);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(76, 17);
            headingLabel.TabIndex = 2;
            headingLabel.Text = "Heading: 0°";
            // 
            // altitudeLabel
            // 
            altitudeLabel.AutoSize = true;
            altitudeLabel.Location = new Point(391, 191);
            altitudeLabel.Name = "altitudeLabel";
            altitudeLabel.Size = new Size(78, 17);
            altitudeLabel.TabIndex = 1;
            altitudeLabel.Text = "Altitude: 0 ft";
            // 
            // airspeedLabel
            // 
            airspeedLabel.AutoSize = true;
            airspeedLabel.Location = new Point(391, 166);
            airspeedLabel.Name = "airspeedLabel";
            airspeedLabel.Size = new Size(94, 17);
            airspeedLabel.TabIndex = 0;
            airspeedLabel.Text = "Airspeed: 0 kts";
            // 
            // disconnectButton
            // 
            disconnectButton.Enabled = false;
            disconnectButton.Location = new Point(136, 234);
            disconnectButton.Margin = new Padding(3, 2, 3, 2);
            disconnectButton.Name = "disconnectButton";
            disconnectButton.Size = new Size(88, 25);
            disconnectButton.TabIndex = 3;
            disconnectButton.Text = "Disconnect";
            disconnectButton.UseVisualStyleBackColor = true;
            disconnectButton.Click += DisconnectButton_Click;
            // 
            // connectButton
            // 
            connectButton.Location = new Point(40, 234);
            connectButton.Margin = new Padding(3, 2, 3, 2);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(88, 25);
            connectButton.TabIndex = 2;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += ConnectButton_Click;
            // 
            // joystickStatusLabel
            // 
            joystickStatusLabel.Location = new Point(40, 191);
            joystickStatusLabel.Name = "joystickStatusLabel";
            joystickStatusLabel.Size = new Size(140, 17);
            joystickStatusLabel.TabIndex = 1;
            joystickStatusLabel.Text = "Joystick: Not Selected";
            // 
            // matLabelMSFSConnected
            // 
            matLabelMSFSConnected.AutoSize = true;
            matLabelMSFSConnected.Depth = 0;
            matLabelMSFSConnected.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            matLabelMSFSConnected.Location = new Point(33, 15);
            matLabelMSFSConnected.MouseState = MaterialSkin.MouseState.HOVER;
            matLabelMSFSConnected.Name = "matLabelMSFSConnected";
            matLabelMSFSConnected.Size = new Size(142, 19);
            matLabelMSFSConnected.TabIndex = 0;
            matLabelMSFSConnected.Text = "MSFS disconnected";
            // 
            // closeToTrayCheckBox
            // 
            closeToTrayCheckBox.AutoSize = true;
            closeToTrayCheckBox.Location = new Point(28, 110);
            closeToTrayCheckBox.Margin = new Padding(3, 2, 3, 2);
            closeToTrayCheckBox.Name = "closeToTrayCheckBox";
            closeToTrayCheckBox.Size = new Size(244, 21);
            closeToTrayCheckBox.TabIndex = 0;
            closeToTrayCheckBox.Text = "Minimize to system tray when closing";
            closeToTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // enableForceFeedbackCheckBox
            // 
            enableForceFeedbackCheckBox.AutoSize = true;
            enableForceFeedbackCheckBox.Checked = true;
            enableForceFeedbackCheckBox.CheckState = CheckState.Checked;
            enableForceFeedbackCheckBox.Location = new Point(404, 36);
            enableForceFeedbackCheckBox.Margin = new Padding(3, 2, 3, 2);
            enableForceFeedbackCheckBox.Name = "enableForceFeedbackCheckBox";
            enableForceFeedbackCheckBox.Size = new Size(161, 21);
            enableForceFeedbackCheckBox.TabIndex = 2;
            enableForceFeedbackCheckBox.Text = "Enable Force Feedback";
            enableForceFeedbackCheckBox.UseVisualStyleBackColor = true;
            // 
            // forceMultiplierLabel
            // 
            forceMultiplierLabel.AutoSize = true;
            forceMultiplierLabel.Location = new Point(28, 11);
            forceMultiplierLabel.Name = "forceMultiplierLabel";
            forceMultiplierLabel.Size = new Size(138, 17);
            forceMultiplierLabel.TabIndex = 1;
            forceMultiplierLabel.Text = "Force Multiplier: 100%";
            // 
            // forceMultiplierTrackBar
            // 
            forceMultiplierTrackBar.Location = new Point(28, 36);
            forceMultiplierTrackBar.Margin = new Padding(3, 2, 3, 2);
            forceMultiplierTrackBar.Maximum = 200;
            forceMultiplierTrackBar.Name = "forceMultiplierTrackBar";
            forceMultiplierTrackBar.Size = new Size(350, 45);
            forceMultiplierTrackBar.TabIndex = 0;
            forceMultiplierTrackBar.TickFrequency = 20;
            forceMultiplierTrackBar.Value = 100;
            // 
            // selectDeviceButton
            // 
            selectDeviceButton.Location = new Point(355, 64);
            selectDeviceButton.Margin = new Padding(3, 2, 3, 2);
            selectDeviceButton.Name = "selectDeviceButton";
            selectDeviceButton.Size = new Size(144, 32);
            selectDeviceButton.TabIndex = 2;
            selectDeviceButton.Text = "Select";
            selectDeviceButton.UseVisualStyleBackColor = true;
            // 
            // refreshDevicesButton
            // 
            refreshDevicesButton.Location = new Point(355, 13);
            refreshDevicesButton.Margin = new Padding(3, 2, 3, 2);
            refreshDevicesButton.Name = "refreshDevicesButton";
            refreshDevicesButton.Size = new Size(144, 32);
            refreshDevicesButton.TabIndex = 1;
            refreshDevicesButton.Text = "Refresh";
            refreshDevicesButton.UseVisualStyleBackColor = true;
            // 
            // joystickListBox
            // 
            joystickListBox.FormattingEnabled = true;
            joystickListBox.Location = new Point(12, 13);
            joystickListBox.Margin = new Padding(3, 2, 3, 2);
            joystickListBox.Name = "joystickListBox";
            joystickListBox.Size = new Size(293, 157);
            joystickListBox.TabIndex = 0;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(tabPageDashboard);
            mainTabControl.Controls.Add(tabPageEffects);
            mainTabControl.Controls.Add(tabPageDevice);
            mainTabControl.Controls.Add(tabPageSettings);
            mainTabControl.Depth = 0;
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.ImageList = iconsList;
            mainTabControl.Location = new Point(3, 64);
            mainTabControl.MouseState = MaterialSkin.MouseState.HOVER;
            mainTabControl.Multiline = true;
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(929, 673);
            mainTabControl.TabIndex = 1;
            // 
            // tabPageDashboard
            // 
            tabPageDashboard.Controls.Add(materialCardSimulator);
            tabPageDashboard.Controls.Add(materialCardJoystick);
            tabPageDashboard.Controls.Add(label2);
            tabPageDashboard.Controls.Add(headingLabel);
            tabPageDashboard.Controls.Add(forceYLabel);
            tabPageDashboard.Controls.Add(disconnectButton);
            tabPageDashboard.Controls.Add(altitudeLabel);
            tabPageDashboard.Controls.Add(forceXLabel);
            tabPageDashboard.Controls.Add(forceYProgressBar);
            tabPageDashboard.Controls.Add(airspeedLabel);
            tabPageDashboard.Controls.Add(connectButton);
            tabPageDashboard.Controls.Add(forceXProgressBar);
            tabPageDashboard.Controls.Add(joystickStatusLabel);
            tabPageDashboard.ImageKey = "icons8-dashboard-layout-48.png";
            tabPageDashboard.Location = new Point(4, 39);
            tabPageDashboard.Name = "tabPageDashboard";
            tabPageDashboard.Padding = new Padding(3);
            tabPageDashboard.Size = new Size(921, 630);
            tabPageDashboard.TabIndex = 0;
            tabPageDashboard.Text = "Dashboard";
            tabPageDashboard.UseVisualStyleBackColor = true;
            // 
            // materialCardSimulator
            // 
            materialCardSimulator.BackColor = Color.FromArgb(255, 255, 255);
            materialCardSimulator.Controls.Add(pictureBox1);
            materialCardSimulator.Controls.Add(buttonSimConnected);
            materialCardSimulator.Controls.Add(matLabelMSFSConnected);
            materialCardSimulator.Depth = 0;
            materialCardSimulator.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCardSimulator.Location = new Point(260, 17);
            materialCardSimulator.Margin = new Padding(14);
            materialCardSimulator.MouseState = MaterialSkin.MouseState.HOVER;
            materialCardSimulator.Name = "materialCardSimulator";
            materialCardSimulator.Padding = new Padding(14);
            materialCardSimulator.Size = new Size(250, 75);
            materialCardSimulator.TabIndex = 5;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Right;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(188, 14);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(48, 47);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // materialCardJoystick
            // 
            materialCardJoystick.BackColor = Color.FromArgb(255, 255, 255);
            materialCardJoystick.Controls.Add(pictureBoxJoystick);
            materialCardJoystick.Controls.Add(labelJoystickSelected);
            materialCardJoystick.Controls.Add(buttonJoystickSelected);
            materialCardJoystick.Depth = 0;
            materialCardJoystick.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCardJoystick.Location = new Point(17, 17);
            materialCardJoystick.Margin = new Padding(14);
            materialCardJoystick.MouseState = MaterialSkin.MouseState.HOVER;
            materialCardJoystick.Name = "materialCardJoystick";
            materialCardJoystick.Padding = new Padding(14);
            materialCardJoystick.Size = new Size(225, 75);
            materialCardJoystick.TabIndex = 4;
            // 
            // pictureBoxJoystick
            // 
            pictureBoxJoystick.Dock = DockStyle.Left;
            pictureBoxJoystick.Image = (Image)resources.GetObject("pictureBoxJoystick.Image");
            pictureBoxJoystick.Location = new Point(24, 14);
            pictureBoxJoystick.Name = "pictureBoxJoystick";
            pictureBoxJoystick.Size = new Size(48, 47);
            pictureBoxJoystick.TabIndex = 0;
            pictureBoxJoystick.TabStop = false;
            // 
            // tabPageEffects
            // 
            tabPageEffects.BackColor = Color.LightGray;
            tabPageEffects.Controls.Add(switchShakeOnStallWarning);
            tabPageEffects.Controls.Add(switchDynamicControlForces);
            tabPageEffects.ImageKey = "icons8-depth-effect-48.png";
            tabPageEffects.Location = new Point(4, 39);
            tabPageEffects.Name = "tabPageEffects";
            tabPageEffects.Padding = new Padding(3);
            tabPageEffects.Size = new Size(921, 630);
            tabPageEffects.TabIndex = 1;
            tabPageEffects.Text = "Effects";
            // 
            // switchShakeOnStallWarning
            // 
            switchShakeOnStallWarning.AutoSize = true;
            switchShakeOnStallWarning.Depth = 0;
            switchShakeOnStallWarning.Location = new Point(20, 52);
            switchShakeOnStallWarning.Margin = new Padding(0);
            switchShakeOnStallWarning.MouseLocation = new Point(-1, -1);
            switchShakeOnStallWarning.MouseState = MaterialSkin.MouseState.HOVER;
            switchShakeOnStallWarning.Name = "switchShakeOnStallWarning";
            switchShakeOnStallWarning.Ripple = true;
            switchShakeOnStallWarning.Size = new Size(256, 37);
            switchShakeOnStallWarning.TabIndex = 1;
            switchShakeOnStallWarning.Text = "Shake stick on stall warning";
            switchShakeOnStallWarning.UseVisualStyleBackColor = true;
            // 
            // switchDynamicControlForces
            // 
            switchDynamicControlForces.AutoSize = true;
            switchDynamicControlForces.Checked = true;
            switchDynamicControlForces.CheckState = CheckState.Checked;
            switchDynamicControlForces.Depth = 0;
            switchDynamicControlForces.Location = new Point(20, 15);
            switchDynamicControlForces.Margin = new Padding(0);
            switchDynamicControlForces.MouseLocation = new Point(-1, -1);
            switchDynamicControlForces.MouseState = MaterialSkin.MouseState.HOVER;
            switchDynamicControlForces.Name = "switchDynamicControlForces";
            switchDynamicControlForces.Ripple = true;
            switchDynamicControlForces.Size = new Size(272, 37);
            switchDynamicControlForces.TabIndex = 0;
            switchDynamicControlForces.Text = "Enable dynamic control forces";
            switchDynamicControlForces.UseVisualStyleBackColor = true;
            // 
            // tabPageDevice
            // 
            tabPageDevice.Controls.Add(selectDeviceButton);
            tabPageDevice.Controls.Add(joystickListBox);
            tabPageDevice.Controls.Add(refreshDevicesButton);
            tabPageDevice.ImageKey = "icons8-joystick-48.png";
            tabPageDevice.Location = new Point(4, 39);
            tabPageDevice.Name = "tabPageDevice";
            tabPageDevice.Size = new Size(921, 630);
            tabPageDevice.TabIndex = 2;
            tabPageDevice.Text = "Device";
            tabPageDevice.UseVisualStyleBackColor = true;
            // 
            // tabPageSettings
            // 
            tabPageSettings.Controls.Add(closeToTrayCheckBox);
            tabPageSettings.Controls.Add(enableForceFeedbackCheckBox);
            tabPageSettings.Controls.Add(forceMultiplierLabel);
            tabPageSettings.Controls.Add(forceMultiplierTrackBar);
            tabPageSettings.ImageKey = "icons8-settings-48.png";
            tabPageSettings.Location = new Point(4, 39);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Size = new Size(921, 630);
            tabPageSettings.TabIndex = 3;
            tabPageSettings.Text = "Options";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // iconsList
            // 
            iconsList.ColorDepth = ColorDepth.Depth32Bit;
            iconsList.ImageStream = (ImageListStreamer)resources.GetObject("iconsList.ImageStream");
            iconsList.TransparentColor = Color.Transparent;
            iconsList.Images.SetKeyName(0, "icons8-settings-48.png");
            iconsList.Images.SetKeyName(1, "icons8-depth-effect-48.png");
            iconsList.Images.SetKeyName(2, "icons8-dashboard-layout-48.png");
            iconsList.Images.SetKeyName(3, "icons8-joystick-48.png");
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(935, 740);
            Controls.Add(mainTabControl);
            DrawerShowIconsWhenHidden = true;
            DrawerTabControl = mainTabControl;
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(527, 346);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TDX Air Mechanics";
            ((System.ComponentModel.ISupportInitialize)forceMultiplierTrackBar).EndInit();
            mainTabControl.ResumeLayout(false);
            tabPageDashboard.ResumeLayout(false);
            tabPageDashboard.PerformLayout();
            materialCardSimulator.ResumeLayout(false);
            materialCardSimulator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            materialCardJoystick.ResumeLayout(false);
            materialCardJoystick.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxJoystick).EndInit();
            tabPageEffects.ResumeLayout(false);
            tabPageEffects.PerformLayout();
            tabPageDevice.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            tabPageSettings.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Label joystickStatusLabel;
        private ProgressBar forceXProgressBar;
        private ProgressBar forceYProgressBar;
        private Label forceXLabel;
        private Label forceYLabel;
        private Label airspeedLabel;
        private Label altitudeLabel;
        private Label headingLabel;
        private TrackBar forceMultiplierTrackBar;
        private Label forceMultiplierLabel;
        private CheckBox enableForceFeedbackCheckBox;
        private CheckBox closeToTrayCheckBox;
private ListBox joystickListBox;
        private Button refreshDevicesButton;
        private Button selectDeviceButton;
        private Button connectButton;
        private Button disconnectButton;
        private Button buttonSimConnected;
        private Label labelJoystickSelected;
        private Button buttonJoystickSelected;
        private Label label2;
        private MaterialSkin.Controls.MaterialTabControl mainTabControl;
        private TabPage tabPageDashboard;
        private TabPage tabPageEffects;
        private TabPage tabPageDevice;
        private TabPage tabPageSettings;
        private ImageList iconsList;
        private MaterialSkin.Controls.MaterialCard materialCardJoystick;
        private PictureBox pictureBoxJoystick;
        private MaterialSkin.Controls.MaterialCard materialCardSimulator;
        private PictureBox pictureBox1;
        private MaterialSkin.Controls.MaterialLabel matLabelMSFSConnected;
        private MaterialSkin.Controls.MaterialSwitch switchDynamicControlForces;
        private MaterialSkin.Controls.MaterialSwitch switchShakeOnStallWarning;
    }
}
