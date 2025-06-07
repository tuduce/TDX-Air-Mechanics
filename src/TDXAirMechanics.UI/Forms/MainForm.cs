using Microsoft.Extensions.Logging;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.UI.Services;

namespace TDXAirMechanics.UI.Forms;

/// <summary>
/// Main application form for TDX Air Mechanics
/// </summary>
public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly IApplicationService _applicationService;
    private readonly IStatusService _statusService;
    private readonly System.Windows.Forms.Timer _updateTimer;
    private NotifyIcon? _notifyIcon;    public MainForm(
        ILogger<MainForm> logger,
        IApplicationService applicationService,
        IStatusService statusService)
    {
        _logger = logger;
        _applicationService = applicationService;
        _statusService = statusService;
        
        InitializeComponent();
        InitializeUI();

        // Setup update timer
        _updateTimer = new System.Windows.Forms.Timer();
        _updateTimer.Interval = 100; // 10 Hz update rate
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        // Subscribe to status changes
        _statusService.StatusChanged += OnStatusChanged;
    }

    private void InitializeUI()
    {
        _logger.LogInformation("Initializing main form UI");

        // Setup system tray icon
        SetupSystemTray();

        // Setup UI event handlers
        SetupEventHandlers();

        // Handle form closing to minimize to tray instead
        this.FormClosing += MainForm_FormClosing;
        this.WindowState = FormWindowState.Normal;
        this.ShowInTaskbar = true;
    }

    private void SetupEventHandlers()
    {
        // Force settings events
        forceMultiplierTrackBar.ValueChanged += ForceMultiplierTrackBar_ValueChanged;
        enableForceFeedbackCheckBox.CheckedChanged += EnableForceFeedbackCheckBox_CheckedChanged;

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

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Minimize to tray instead of closing
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
            _notifyIcon!.Visible = true;
            _notifyIcon.ShowBalloonTip(2000, "TDX Air Mechanics", 
                "Application minimized to system tray", ToolTipIcon.Info);
        }
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Update UI with current status
        UpdateStatusDisplay();
    }    private void UpdateStatusDisplay()
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
        }        // Update force display
        var currentForces = _applicationService.CurrentForces;
        if (currentForces != null)
        {
            // Convert force values from -1.0/+1.0 range to 0-200 ProgressBar range
            // -1.0 maps to 0, 0.0 maps to 100, +1.0 maps to 200
            forceXProgressBar.Value = (int)((currentForces.ForceX + 1.0) * 100);
            forceYProgressBar.Value = (int)((currentForces.ForceY + 1.0) * 100);
        }
    }private void ForceMultiplierTrackBar_ValueChanged(object? sender, EventArgs e)
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
    }    private async void RefreshDevicesButton_Click(object? sender, EventArgs e)
    {
        try
        {
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
            MessageBox.Show($"Error refreshing devices: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            refreshDevicesButton.Enabled = true;
        }
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
                await _applicationService.SelectDeviceAsync(deviceName);
                MessageBox.Show($"Selected device: {deviceName}", "Device Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        }    }

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
    }    protected override void Dispose(bool disposing)
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
