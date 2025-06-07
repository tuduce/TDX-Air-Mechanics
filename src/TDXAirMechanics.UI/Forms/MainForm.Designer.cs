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
            this.mainTabControl = new TabControl();
            this.statusTabPage = new TabPage();
            this.settingsTabPage = new TabPage();
            this.devicesTabPage = new TabPage();
            this.statusPanel = new Panel();            this.connectionStatusGroup = new GroupBox();
            this.msfsStatusLabel = new Label();
            this.joystickStatusLabel = new Label();
            this.connectButton = new Button();
            this.disconnectButton = new Button();
            this.forcePanel = new Panel();
            this.forceDisplayGroup = new GroupBox();
            this.forceXProgressBar = new ProgressBar();
            this.forceYProgressBar = new ProgressBar();
            this.forceXLabel = new Label();
            this.forceYLabel = new Label();
            this.flightDataGroup = new GroupBox();
            this.airspeedLabel = new Label();
            this.altitudeLabel = new Label();
            this.headingLabel = new Label();
            this.settingsPanel = new Panel();
            this.forceSettingsGroup = new GroupBox();
            this.forceMultiplierTrackBar = new TrackBar();
            this.forceMultiplierLabel = new Label();
            this.enableForceFeedbackCheckBox = new CheckBox();
            this.devicesPanel = new Panel();
            this.joystickListGroup = new GroupBox();
            this.joystickListBox = new ListBox();
            this.refreshDevicesButton = new Button();
            this.selectDeviceButton = new Button();
            this.mainTabControl.SuspendLayout();
            this.statusTabPage.SuspendLayout();
            this.settingsTabPage.SuspendLayout();
            this.devicesTabPage.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.connectionStatusGroup.SuspendLayout();
            this.forcePanel.SuspendLayout();
            this.forceDisplayGroup.SuspendLayout();
            this.flightDataGroup.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.forceSettingsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.forceMultiplierTrackBar)).BeginInit();
            this.devicesPanel.SuspendLayout();
            this.joystickListGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.statusTabPage);
            this.mainTabControl.Controls.Add(this.settingsTabPage);
            this.mainTabControl.Controls.Add(this.devicesTabPage);
            this.mainTabControl.Dock = DockStyle.Fill;
            this.mainTabControl.Location = new Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new Size(900, 700);
            this.mainTabControl.TabIndex = 0;
            // 
            // statusTabPage
            // 
            this.statusTabPage.Controls.Add(this.forcePanel);
            this.statusTabPage.Controls.Add(this.statusPanel);
            this.statusTabPage.Location = new Point(4, 29);
            this.statusTabPage.Name = "statusTabPage";
            this.statusTabPage.Padding = new Padding(3);
            this.statusTabPage.Size = new Size(892, 667);
            this.statusTabPage.TabIndex = 0;
            this.statusTabPage.Text = "Status";
            this.statusTabPage.UseVisualStyleBackColor = true;
            // 
            // settingsTabPage
            // 
            this.settingsTabPage.Controls.Add(this.settingsPanel);
            this.settingsTabPage.Location = new Point(4, 29);
            this.settingsTabPage.Name = "settingsTabPage";
            this.settingsTabPage.Padding = new Padding(3);
            this.settingsTabPage.Size = new Size(892, 667);
            this.settingsTabPage.TabIndex = 1;
            this.settingsTabPage.Text = "Settings";
            this.settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // devicesTabPage
            // 
            this.devicesTabPage.Controls.Add(this.devicesPanel);
            this.devicesTabPage.Location = new Point(4, 29);
            this.devicesTabPage.Name = "devicesTabPage";
            this.devicesTabPage.Padding = new Padding(3);
            this.devicesTabPage.Size = new Size(892, 667);
            this.devicesTabPage.TabIndex = 2;
            this.devicesTabPage.Text = "Devices";
            this.devicesTabPage.UseVisualStyleBackColor = true;
            // 
            // statusPanel
            // 
            this.statusPanel.Controls.Add(this.flightDataGroup);
            this.statusPanel.Controls.Add(this.connectionStatusGroup);
            this.statusPanel.Dock = DockStyle.Top;
            this.statusPanel.Location = new Point(3, 3);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new Size(886, 200);
            this.statusPanel.TabIndex = 0;
            //            // connectionStatusGroup
            // 
            this.connectionStatusGroup.Controls.Add(this.disconnectButton);
            this.connectionStatusGroup.Controls.Add(this.connectButton);
            this.connectionStatusGroup.Controls.Add(this.joystickStatusLabel);
            this.connectionStatusGroup.Controls.Add(this.msfsStatusLabel);
            this.connectionStatusGroup.Dock = DockStyle.Left;
            this.connectionStatusGroup.Location = new Point(0, 0);
            this.connectionStatusGroup.Name = "connectionStatusGroup";
            this.connectionStatusGroup.Size = new Size(300, 200);
            this.connectionStatusGroup.TabIndex = 0;
            this.connectionStatusGroup.TabStop = false;
            this.connectionStatusGroup.Text = "Connection Status";
            // 
            // msfsStatusLabel
            // 
            this.msfsStatusLabel.AutoSize = true;
            this.msfsStatusLabel.Location = new Point(20, 40);
            this.msfsStatusLabel.Name = "msfsStatusLabel";
            this.msfsStatusLabel.Size = new Size(150, 20);
            this.msfsStatusLabel.TabIndex = 0;
            this.msfsStatusLabel.Text = "MSFS: Disconnected";
            // 
            // joystickStatusLabel
            //            this.joystickStatusLabel.AutoSize = true;
            this.joystickStatusLabel.Location = new Point(20, 70);
            this.joystickStatusLabel.Name = "joystickStatusLabel";
            this.joystickStatusLabel.Size = new Size(160, 20);
            this.joystickStatusLabel.TabIndex = 1;
            this.joystickStatusLabel.Text = "Joystick: Not Selected";
            // 
            // connectButton
            // 
            this.connectButton.Location = new Point(20, 120);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new Size(100, 30);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new EventHandler(this.ConnectButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new Point(130, 120);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new Size(100, 30);
            this.disconnectButton.TabIndex = 3;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Click += new EventHandler(this.DisconnectButton_Click);
            // 
            // forcePanel
            // 
            this.forcePanel.Controls.Add(this.forceDisplayGroup);
            this.forcePanel.Dock = DockStyle.Fill;
            this.forcePanel.Location = new Point(3, 203);
            this.forcePanel.Name = "forcePanel";
            this.forcePanel.Size = new Size(886, 461);
            this.forcePanel.TabIndex = 1;
            // 
            // forceDisplayGroup
            // 
            this.forceDisplayGroup.Controls.Add(this.forceYLabel);
            this.forceDisplayGroup.Controls.Add(this.forceXLabel);
            this.forceDisplayGroup.Controls.Add(this.forceYProgressBar);
            this.forceDisplayGroup.Controls.Add(this.forceXProgressBar);
            this.forceDisplayGroup.Dock = DockStyle.Fill;
            this.forceDisplayGroup.Location = new Point(0, 0);
            this.forceDisplayGroup.Name = "forceDisplayGroup";
            this.forceDisplayGroup.Size = new Size(886, 461);
            this.forceDisplayGroup.TabIndex = 0;
            this.forceDisplayGroup.TabStop = false;
            this.forceDisplayGroup.Text = "Force Feedback";
            //            // forceXProgressBar
            // 
            this.forceXProgressBar.Location = new Point(100, 50);
            this.forceXProgressBar.Maximum = 200;
            this.forceXProgressBar.Minimum = 0;
            this.forceXProgressBar.Name = "forceXProgressBar";
            this.forceXProgressBar.Size = new Size(400, 30);
            this.forceXProgressBar.TabIndex = 0;
            //            // forceYProgressBar
            // 
            this.forceYProgressBar.Location = new Point(100, 100);
            this.forceYProgressBar.Maximum = 200;
            this.forceYProgressBar.Minimum = 0;
            this.forceYProgressBar.Name = "forceYProgressBar";
            this.forceYProgressBar.Size = new Size(400, 30);
            this.forceYProgressBar.TabIndex = 1;
            // 
            // forceXLabel
            // 
            this.forceXLabel.AutoSize = true;
            this.forceXLabel.Location = new Point(20, 55);
            this.forceXLabel.Name = "forceXLabel";
            this.forceXLabel.Size = new Size(60, 20);
            this.forceXLabel.TabIndex = 2;
            this.forceXLabel.Text = "Force X:";
            // 
            // forceYLabel
            // 
            this.forceYLabel.AutoSize = true;
            this.forceYLabel.Location = new Point(20, 105);
            this.forceYLabel.Name = "forceYLabel";
            this.forceYLabel.Size = new Size(60, 20);
            this.forceYLabel.TabIndex = 3;
            this.forceYLabel.Text = "Force Y:";
            // 
            // flightDataGroup
            // 
            this.flightDataGroup.Controls.Add(this.headingLabel);
            this.flightDataGroup.Controls.Add(this.altitudeLabel);
            this.flightDataGroup.Controls.Add(this.airspeedLabel);
            this.flightDataGroup.Dock = DockStyle.Fill;
            this.flightDataGroup.Location = new Point(300, 0);
            this.flightDataGroup.Name = "flightDataGroup";
            this.flightDataGroup.Size = new Size(586, 200);
            this.flightDataGroup.TabIndex = 1;
            this.flightDataGroup.TabStop = false;
            this.flightDataGroup.Text = "Flight Data";
            // 
            // airspeedLabel
            // 
            this.airspeedLabel.AutoSize = true;
            this.airspeedLabel.Location = new Point(20, 40);
            this.airspeedLabel.Name = "airspeedLabel";
            this.airspeedLabel.Size = new Size(120, 20);
            this.airspeedLabel.TabIndex = 0;
            this.airspeedLabel.Text = "Airspeed: 0 kts";
            // 
            // altitudeLabel
            // 
            this.altitudeLabel.AutoSize = true;
            this.altitudeLabel.Location = new Point(20, 70);
            this.altitudeLabel.Name = "altitudeLabel";
            this.altitudeLabel.Size = new Size(110, 20);
            this.altitudeLabel.TabIndex = 1;
            this.altitudeLabel.Text = "Altitude: 0 ft";
            // 
            // headingLabel
            // 
            this.headingLabel.AutoSize = true;
            this.headingLabel.Location = new Point(20, 100);
            this.headingLabel.Name = "headingLabel";
            this.headingLabel.Size = new Size(110, 20);
            this.headingLabel.TabIndex = 2;
            this.headingLabel.Text = "Heading: 0°";
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.forceSettingsGroup);
            this.settingsPanel.Dock = DockStyle.Fill;
            this.settingsPanel.Location = new Point(3, 3);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new Size(886, 661);
            this.settingsPanel.TabIndex = 0;
            // 
            // forceSettingsGroup
            // 
            this.forceSettingsGroup.Controls.Add(this.enableForceFeedbackCheckBox);
            this.forceSettingsGroup.Controls.Add(this.forceMultiplierLabel);
            this.forceSettingsGroup.Controls.Add(this.forceMultiplierTrackBar);
            this.forceSettingsGroup.Dock = DockStyle.Top;
            this.forceSettingsGroup.Location = new Point(0, 0);
            this.forceSettingsGroup.Name = "forceSettingsGroup";
            this.forceSettingsGroup.Size = new Size(886, 150);
            this.forceSettingsGroup.TabIndex = 0;
            this.forceSettingsGroup.TabStop = false;
            this.forceSettingsGroup.Text = "Force Feedback Settings";
            // 
            // forceMultiplierTrackBar
            // 
            this.forceMultiplierTrackBar.Location = new Point(20, 70);
            this.forceMultiplierTrackBar.Maximum = 200;
            this.forceMultiplierTrackBar.Minimum = 0;
            this.forceMultiplierTrackBar.Name = "forceMultiplierTrackBar";
            this.forceMultiplierTrackBar.Size = new Size(400, 56);
            this.forceMultiplierTrackBar.TabIndex = 0;
            this.forceMultiplierTrackBar.TickFrequency = 20;
            this.forceMultiplierTrackBar.Value = 100;
            // 
            // forceMultiplierLabel
            // 
            this.forceMultiplierLabel.AutoSize = true;
            this.forceMultiplierLabel.Location = new Point(20, 40);
            this.forceMultiplierLabel.Name = "forceMultiplierLabel";
            this.forceMultiplierLabel.Size = new Size(180, 20);
            this.forceMultiplierLabel.TabIndex = 1;
            this.forceMultiplierLabel.Text = "Force Multiplier: 100%";
            // 
            // enableForceFeedbackCheckBox
            // 
            this.enableForceFeedbackCheckBox.AutoSize = true;
            this.enableForceFeedbackCheckBox.Checked = true;
            this.enableForceFeedbackCheckBox.CheckState = CheckState.Checked;
            this.enableForceFeedbackCheckBox.Location = new Point(450, 70);
            this.enableForceFeedbackCheckBox.Name = "enableForceFeedbackCheckBox";
            this.enableForceFeedbackCheckBox.Size = new Size(190, 24);
            this.enableForceFeedbackCheckBox.TabIndex = 2;
            this.enableForceFeedbackCheckBox.Text = "Enable Force Feedback";
            this.enableForceFeedbackCheckBox.UseVisualStyleBackColor = true;
            // 
            // devicesPanel
            // 
            this.devicesPanel.Controls.Add(this.joystickListGroup);
            this.devicesPanel.Dock = DockStyle.Fill;
            this.devicesPanel.Location = new Point(3, 3);
            this.devicesPanel.Name = "devicesPanel";
            this.devicesPanel.Size = new Size(886, 661);
            this.devicesPanel.TabIndex = 0;
            // 
            // joystickListGroup
            // 
            this.joystickListGroup.Controls.Add(this.selectDeviceButton);
            this.joystickListGroup.Controls.Add(this.refreshDevicesButton);
            this.joystickListGroup.Controls.Add(this.joystickListBox);
            this.joystickListGroup.Dock = DockStyle.Fill;
            this.joystickListGroup.Location = new Point(0, 0);
            this.joystickListGroup.Name = "joystickListGroup";
            this.joystickListGroup.Size = new Size(886, 661);
            this.joystickListGroup.TabIndex = 0;
            this.joystickListGroup.TabStop = false;
            this.joystickListGroup.Text = "Available Joysticks";
            // 
            // joystickListBox
            // 
            this.joystickListBox.FormattingEnabled = true;
            this.joystickListBox.Location = new Point(20, 40);
            this.joystickListBox.Name = "joystickListBox";
            this.joystickListBox.Size = new Size(600, 400);
            this.joystickListBox.TabIndex = 0;
            // 
            // refreshDevicesButton
            // 
            this.refreshDevicesButton.Location = new Point(650, 40);
            this.refreshDevicesButton.Name = "refreshDevicesButton";
            this.refreshDevicesButton.Size = new Size(120, 40);
            this.refreshDevicesButton.TabIndex = 1;
            this.refreshDevicesButton.Text = "Refresh";
            this.refreshDevicesButton.UseVisualStyleBackColor = true;
            // 
            // selectDeviceButton
            // 
            this.selectDeviceButton.Location = new Point(650, 100);
            this.selectDeviceButton.Name = "selectDeviceButton";
            this.selectDeviceButton.Size = new Size(120, 40);
            this.selectDeviceButton.TabIndex = 2;
            this.selectDeviceButton.Text = "Select";
            this.selectDeviceButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 700);
            this.Controls.Add(this.mainTabControl);
            this.MinimumSize = new Size(600, 400);
            this.Name = "MainForm";            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "TDX Air Mechanics";
            try 
            {
                this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "Resources", "app_icon.ico"));
            }
            catch (System.Exception)
            {
                // Icon loading failed - application will use default icon
            }
            this.mainTabControl.ResumeLayout(false);
            this.statusTabPage.ResumeLayout(false);
            this.settingsTabPage.ResumeLayout(false);
            this.devicesTabPage.ResumeLayout(false);
            this.statusPanel.ResumeLayout(false);
            this.connectionStatusGroup.ResumeLayout(false);
            this.connectionStatusGroup.PerformLayout();
            this.forcePanel.ResumeLayout(false);
            this.forceDisplayGroup.ResumeLayout(false);
            this.forceDisplayGroup.PerformLayout();
            this.flightDataGroup.ResumeLayout(false);
            this.flightDataGroup.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.forceSettingsGroup.ResumeLayout(false);
            this.forceSettingsGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.forceMultiplierTrackBar)).EndInit();
            this.devicesPanel.ResumeLayout(false);
            this.joystickListGroup.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TabControl mainTabControl;
        private TabPage statusTabPage;
        private TabPage settingsTabPage;
        private TabPage devicesTabPage;
        private Panel statusPanel;
        private GroupBox connectionStatusGroup;
        private Label msfsStatusLabel;
        private Label joystickStatusLabel;
        private Panel forcePanel;
        private GroupBox forceDisplayGroup;
        private ProgressBar forceXProgressBar;
        private ProgressBar forceYProgressBar;
        private Label forceXLabel;
        private Label forceYLabel;
        private GroupBox flightDataGroup;
        private Label airspeedLabel;
        private Label altitudeLabel;
        private Label headingLabel;
        private Panel settingsPanel;
        private GroupBox forceSettingsGroup;
        private TrackBar forceMultiplierTrackBar;
        private Label forceMultiplierLabel;
        private CheckBox enableForceFeedbackCheckBox;
        private Panel devicesPanel;
        private GroupBox joystickListGroup;        private ListBox joystickListBox;
        private Button refreshDevicesButton;
        private Button selectDeviceButton;
        private Button connectButton;
        private Button disconnectButton;
    }
}
