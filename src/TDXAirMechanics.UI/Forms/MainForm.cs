using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.Logging;
using System.Drawing.Drawing2D;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.UI.Services;

namespace TDXAirMechanics.UI.Forms;

/// <summary>
/// Main application form for TDX Air Mechanics
/// </summary>
public partial class MainForm : MaterialForm
{
    readonly MaterialSkinManager materialSkinManager;

    private readonly ILogger<MainForm> _logger;
    private readonly IApplicationService _applicationService;
    private readonly IStatusService _statusService;
    private readonly IConfigurationManager _configurationManager;
    private readonly System.Windows.Forms.Timer _updateTimer;
    private NotifyIcon? _notifyIcon;
    private bool _closeToTrayOnExit = true; // Default value

#if DEBUG
    // Parameterless constructor for Windows Forms Designer support
    public MainForm() : this(null!, null!, null!, null!)
    {
        // This constructor is only for the designer and should not be used at runtime.
    }
#endif

    public MainForm(
        ILogger<MainForm> logger,
        IApplicationService applicationService,
        IStatusService statusService,
        IConfigurationManager configurationManager)
    {
        _logger = logger;
        _applicationService = applicationService;
        _statusService = statusService;
        _configurationManager = configurationManager;
        
        InitializeComponent();

        materialSkinManager = MaterialSkinManager.Instance;
        // materialSkinManager.EnforceBackcolorOnAllComponents = true;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(
            Primary.BlueGrey800, 
            Primary.BlueGrey900, 
            Primary.BlueGrey500, 
            Accent.Orange700, // LightBlue200, 
            TextShade.WHITE);

        try
        {
            this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "Resources", "app_icon.ico"));
        }
        catch (System.Exception)
        {
            // Icon loading failed - application will use default icon
        }
        InitializeUI();

        // Setup update timer
        _updateTimer = new System.Windows.Forms.Timer();
        _updateTimer.Interval = 100; // 10 Hz update rate
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        // Subscribe to status changes
        _statusService.StatusChanged += OnStatusChanged;
    }

    private void SetPictureBoxJoystickIconColor(Color color)
    {
        if (pictureBoxJoystick.Image == null)
            return;
        var original = (Bitmap)pictureBoxJoystick.Image;
        var recolored = new Bitmap(original.Width, original.Height);
        using (var g = Graphics.FromImage(recolored))
        {
            g.Clear(Color.Transparent);
            var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
            {
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {color.R/255f, color.G/255f, color.B/255f, 0, 1}
            });
            var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
        pictureBoxJoystick.Image = recolored;
    }

    private void ApplyDarkModeToJoystickIcon()
    {
        // Use the MaterialSkin text color for dark mode
        if (materialSkinManager.Theme == MaterialSkinManager.Themes.DARK)
        {
            SetPictureBoxJoystickIconColor(materialSkinManager.ColorScheme.TextColor);
        }
        else
        {
            // Optionally, set to a different color for light mode
            SetPictureBoxJoystickIconColor(Color.Black);
        }
    }

    private void InitializeUI()
    {
        _logger.LogInformation("Initializing main form UI");

        // Load configuration
        LoadConfiguration();

        // Setup system tray icon
        SetupSystemTray();

        // Setup UI event handlers
        SetupEventHandlers();

        // Handle form closing to minimize to tray instead
        this.FormClosing += MainForm_FormClosing;
        this.WindowState = FormWindowState.Normal;
        this.ShowInTaskbar = true;
        ApplyDarkModeToJoystickIcon();
    }

    private void SetupEventHandlers()
    {
        // Force settings events
        forceMultiplierTrackBar.ValueChanged += ForceMultiplierTrackBar_ValueChanged;
        enableForceFeedbackCheckBox.CheckedChanged += EnableForceFeedbackCheckBox_CheckedChanged;

        // General settings events
        closeToTrayCheckBox.CheckedChanged += CloseToTrayCheckBox_CheckedChanged;

        // Tab control events
        mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;

        // Device management events
        refreshDevicesButton.Click += RefreshDevicesButton_Click;
        selectDeviceButton.Click += SelectDeviceButton_Click;
    }

    private void SetupSystemTray()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
        _notifyIcon.Text = "TDX Air Mechanics";
        _notifyIcon.Visible = false;

        // Create context menu for system tray
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
        _notifyIcon.ContextMenuStrip = contextMenu;

        // Double-click to show window
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Activate();
        _notifyIcon!.Visible = false;
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        Application.Exit();
    }

    private async void LoadConfiguration()
    {
        try
        {
            var config = await _configurationManager.LoadConfigurationAsync();
            _closeToTrayOnExit = config.General.CloseToTrayOnExit;
            
            // Update UI to reflect loaded configuration
            closeToTrayCheckBox.Checked = _closeToTrayOnExit;
            
            _logger.LogInformation("Configuration loaded: CloseToTrayOnExit = {CloseToTrayOnExit}", _closeToTrayOnExit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration, using default settings");
            _closeToTrayOnExit = true; // Default fallback
            closeToTrayCheckBox.Checked = _closeToTrayOnExit;
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Check user preference for close behavior
        if (e.CloseReason == CloseReason.UserClosing)
        {
            if (_closeToTrayOnExit)
            {
                // Minimize to tray instead of closing
                e.Cancel = true;
                this.Hide();
                _notifyIcon!.Visible = true;
                _notifyIcon.ShowBalloonTip(2000, "TDX Air Mechanics", 
                    "Application minimized to system tray. Right-click the tray icon to exit.", ToolTipIcon.Info);
            }
            else
            {
                // Close the application completely
                ExitApplication();
            }
        }
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Update UI with current status
        UpdateStatusDisplay();
    }

    private void UpdateJoystickSelectedIndicator()
    {
        // Check if a force-feedback joystick is selected
        var isJoystickConnected = _applicationService.IsJoystickConnected;
        var joystickName = _applicationService.SelectedJoystickName;

        if (isJoystickConnected && !string.IsNullOrEmpty(joystickName))
        {
            buttonJoystickSelected.BackColor = Color.LimeGreen;
            buttonJoystickSelected.FlatAppearance.BorderColor = Color.DarkGreen;
            labelJoystickSelected.Text = joystickName;
        }
        else
        {
            buttonJoystickSelected.BackColor = Color.Tomato;
            buttonJoystickSelected.FlatAppearance.BorderColor = Color.Firebrick;
            labelJoystickSelected.Text = "No joystick selected";
        }
    }

    private void UpdateStatusDisplay()
    {
        // Update connection status
        var isSimConnected = _applicationService.IsSimConnectConnected;
        var isJoystickConnected = _applicationService.IsJoystickConnected;

        msfsStatusLabel.Text = $"MSFS: {(isSimConnected ? "Connected" : "Disconnected")}";
        joystickStatusLabel.Text = $"Joystick: {(isJoystickConnected ? "Connected" : "Not Selected")}";

        // Update connection button states
        connectButton.Enabled = !isSimConnected;
        connectButton.Text = "Connect";
        disconnectButton.Enabled = isSimConnected;
        disconnectButton.Text = "Disconnect";

        // Update flight data if available
        var flightData = _applicationService.CurrentFlightData;
        if (flightData != null)
        {
            airspeedLabel.Text = $"Airspeed: {flightData.AirspeedKnots:F0} kts";
            altitudeLabel.Text = $"Altitude: {flightData.AltitudeFeet:F0} ft";
            headingLabel.Text = $"Heading: {flightData.HeadingDegrees:F0}°";
        }

        // Update force display
        var currentForces = _applicationService.CurrentForces;
        if (currentForces != null)
        {
            // Convert force values from -1.0/+1.0 range to 0-200 ProgressBar range
            // -1.0 maps to 0, 0.0 maps to 100, +1.0 maps to 200
            forceXProgressBar.Value = (int)((currentForces.ForceX + 1.0) * 100);
            forceYProgressBar.Value = (int)((currentForces.ForceY + 1.0) * 100);
        }
        // Update joystick selected indicator
        UpdateJoystickSelectedIndicator();
    }

    private void ForceMultiplierTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        var value = forceMultiplierTrackBar.Value;
        forceMultiplierLabel.Text = $"Force Multiplier: {value}%";
        _applicationService.SetForceMultiplier(value);
    }

    private void EnableForceFeedbackCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        var enabled = enableForceFeedbackCheckBox.Checked;
        _applicationService.SetForceFeedbackEnabled(enabled);
        _logger.LogInformation("Force feedback {Status}", enabled ? "enabled" : "disabled");
    }

    private async void CloseToTrayCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        try
        {
            _closeToTrayOnExit = closeToTrayCheckBox.Checked;
            
            // Save the configuration with the new setting
            var config = await _configurationManager.LoadConfigurationAsync();
            config.General.CloseToTrayOnExit = _closeToTrayOnExit;
            await _configurationManager.SaveConfigurationAsync(config);
            
            _logger.LogInformation("Close to tray setting changed to: {CloseToTrayOnExit}", _closeToTrayOnExit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save close to tray setting");
            // Revert the checkbox state if save failed
            closeToTrayCheckBox.Checked = _closeToTrayOnExit;
        }
    }

    private async void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Check if the devices tab was selected (assuming it's at index 2: Status, Settings, Devices)
        if (mainTabControl.SelectedIndex == 2) // Devices tab
        {
            await RefreshDevicesIfNeeded();
        }
    }

    private async Task RefreshDevicesIfNeeded()
    {
        // Only refresh if the list is empty or contains the "No devices found" message
        if (joystickListBox.Items.Count == 0 || 
            (joystickListBox.Items.Count == 1 && joystickListBox.Items[0].ToString() == "No devices found"))
        {
            _logger.LogInformation("Auto-refreshing device list on devices tab activation");
            
            // Show a subtle indication that auto-refresh is happening
            joystickListBox.Items.Clear();
            joystickListBox.Items.Add("Scanning for devices...");
            
            await RefreshDevices(showErrorDialog: false);
        }
    }

    private async Task RefreshDevices(bool showErrorDialog = false)
    {
        try
        {
            var wasEnabled = refreshDevicesButton.Enabled;
            refreshDevicesButton.Enabled = false;
            joystickListBox.Items.Clear();

            var devices = await _applicationService.GetAvailableDevicesAsync();
            foreach (var device in devices)
            {
                joystickListBox.Items.Add(device);
            }

            if (devices.Count == 0)
            {
                joystickListBox.Items.Add("No devices found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing device list");
            // Only show message box if requested (manual refresh)
            if (showErrorDialog)
            {
                MessageBox.Show($"Error refreshing devices: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            refreshDevicesButton.Enabled = true;
        }
    }

    private async void RefreshDevicesButton_Click(object? sender, EventArgs e)
    {
        await RefreshDevices(showErrorDialog: true);
    }

    private async void SelectDeviceButton_Click(object? sender, EventArgs e)
    {
        if (joystickListBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a device first.", "No Device Selected", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            selectDeviceButton.Enabled = false;
            var deviceName = joystickListBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(deviceName))
            {
                await _applicationService.SelectDeviceAsync(deviceName, this.Handle);
                MessageBox.Show($"Selected device: {deviceName}", "Device Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Update indicator immediately after selection
                UpdateJoystickSelectedIndicator();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting device");
            MessageBox.Show($"Error selecting device: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            selectDeviceButton.Enabled = true;
        }
    }

    private async void ConnectButton_Click(object? sender, EventArgs e)
    {
        try
        {
            connectButton.Enabled = false;
            connectButton.Text = "Connecting...";
            
            _logger.LogInformation("User requested SimConnect connection");
            var success = await _applicationService.ConnectToSimulatorAsync();
            
            if (success)
            {
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                MessageBox.Show("Connected to flight simulator successfully!", "Connection Successful", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                connectButton.Enabled = true;
                connectButton.Text = "Connect";
                MessageBox.Show("Failed to connect to flight simulator. Please ensure MSFS is running and try again.", 
                    "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual SimConnect connection");
            connectButton.Enabled = true;
            connectButton.Text = "Connect";
            MessageBox.Show($"Error connecting to simulator: {ex.Message}", "Connection Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void DisconnectButton_Click(object? sender, EventArgs e)
    {
        try
        {
            disconnectButton.Enabled = false;
            disconnectButton.Text = "Disconnecting...";
            
            _logger.LogInformation("User requested SimConnect disconnection");
            await _applicationService.DisconnectFromSimulatorAsync();
            
            connectButton.Enabled = true;
            connectButton.Text = "Connect";
            disconnectButton.Enabled = false;
            disconnectButton.Text = "Disconnect";
            
            MessageBox.Show("Disconnected from flight simulator.", "Disconnected", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SimConnect disconnection");
            disconnectButton.Enabled = true;
            disconnectButton.Text = "Disconnect";
            MessageBox.Show($"Error disconnecting from simulator: {ex.Message}", "Disconnection Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnStatusChanged(object? sender, StatusChangedEventArgs e)
    {
        // Handle status changes from the application service
        Invoke(() =>
        {
            _logger.LogDebug("Status changed: {Status}", e.Message);
            // Update UI based on status change
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _updateTimer?.Dispose();
            _notifyIcon?.Dispose();
            _statusService.StatusChanged -= OnStatusChanged;
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
