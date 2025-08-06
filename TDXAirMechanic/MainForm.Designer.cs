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
            buttonConnectSimulator = new MaterialSkin.Controls.MaterialFloatingActionButton();
            imageListIcons = new ImageList(components);
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            labelAircraftName = new MaterialSkin.Controls.MaterialLabel();
            tabPageEffects = new TabPage();
            tabPageDevices = new TabPage();
            tabPageSettings = new TabPage();
            materialTabControl1.SuspendLayout();
            tabPageDashboard.SuspendLayout();
            materialCard1.SuspendLayout();
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
            tabPageEffects.ImageKey = "icons8-depth-effect-48.png";
            tabPageEffects.Location = new Point(4, 39);
            tabPageEffects.Name = "tabPageEffects";
            tabPageEffects.Padding = new Padding(3);
            tabPageEffects.Size = new Size(786, 340);
            tabPageEffects.TabIndex = 1;
            tabPageEffects.Text = "Effects";
            tabPageEffects.UseVisualStyleBackColor = true;
            // 
            // tabPageDevices
            // 
            tabPageDevices.ImageKey = "icons8-joystick-48.png";
            tabPageDevices.Location = new Point(4, 39);
            tabPageDevices.Name = "tabPageDevices";
            tabPageDevices.Size = new Size(786, 340);
            tabPageDevices.TabIndex = 2;
            tabPageDevices.Text = "Devices";
            tabPageDevices.UseVisualStyleBackColor = true;
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
            materialTabControl1.ResumeLayout(false);
            tabPageDashboard.ResumeLayout(false);
            materialCard1.ResumeLayout(false);
            materialCard1.PerformLayout();
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
    }
}
