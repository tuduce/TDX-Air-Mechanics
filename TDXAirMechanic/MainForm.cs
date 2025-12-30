using MaterialSkin;
using MaterialSkin.Controls;
using TDXAirMechanic.Model;
using TDXAirMechanic.Services;

namespace TDXAirMechanic
{
    public partial class MainForm : MaterialForm
    {
        private readonly SimConnectService _simConnectService;
        private readonly MechanicService _mechanicServices;
        private readonly IProfileManager _profileManager;
        private readonly CancellationTokenSource _formClosingCts = new CancellationTokenSource();

        private bool _isSimConnectClicked = false;

        private AirplaneProfile? _currentProfile;
        private string? _currentModel;
        private bool _applyingProfile;

        // Prevent joystick acquire before the window is foreground
        private bool _uiReadyForAcquire = false;

        public MainForm(SimConnectService simConnectService, MechanicService mechanicServices, IProfileManager profileManager)
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,
                Primary.BlueGrey900,
                Primary.BlueGrey500,
                Accent.Orange700,
                TextShade.WHITE);

            _simConnectService = simConnectService;
            _mechanicServices = mechanicServices;
            _profileManager = profileManager;

            // Hook joystick selection change
            comboBoxJoysticks.SelectedIndexChanged += comboBoxJoysticks_SelectedIndexChanged;

            // Hook profile dropdown
            comboBoxProfiles.SelectedIndexChanged += comboBoxProfiles_SelectedIndexChanged;
            buttonSaveNewProfile.Click += buttonSaveNewProfile_Click;
        }

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            _uiReadyForAcquire = true;
            // _mechanicServices.LoadJoysticks();

            // If nothing is selected yet, select the first item now (we are foreground)
            if (comboBoxJoysticks.Items.Count > 0 && comboBoxJoysticks.SelectedIndex < 0)
            {
                comboBoxJoysticks.SelectedIndex = 0;
            }
        }

        private void MechanicProgressReporter(MechanicProgress data)
        {
            switch (data.Command)
            {
                case MechanicProgressCommand.SetStatus:
                    // This code is guaranteed to run on the UI thread!
                    labelJoystickStatus.Text = data.Status;
                    break;
                case MechanicProgressCommand.SetJoysticks:
                    // Update joystick list
                    comboBoxJoysticks.DataSource = null;
                    comboBoxJoysticks.DataSource = data.Joysticks;
                    comboBoxJoysticks.SelectedIndex = 0;
                    break;
                case MechanicProgressCommand.SetFlightStatus:
                    FlightStatusLabel.Text = data.Status;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.Command), data.Command, null);
            }
        }

        private void RefreshProfilesDropdown()
        {
            var names = _profileManager.ListProfiles();
            _applyingProfile = true;
            try
            {
                comboBoxProfiles.DataSource = null;
                comboBoxProfiles.DataSource = names;
            }
            finally
            {
                _applyingProfile = false;
            }
        }

        private void SelectProfileInDropdown(string? model)
        {
            if (string.IsNullOrWhiteSpace(model)) return;
            var names = comboBoxProfiles.DataSource as System.Collections.IList;
            if (names == null) return;
            var idx = names.IndexOf(model);
            if (idx >= 0)
            {
                _applyingProfile = true;
                try
                {
                    comboBoxProfiles.SelectedIndex = idx;
                }
                finally
                {
                    _applyingProfile = false;
                }
            }
        }

        private void SimConnectProgressReporter(AirplaneProfile data)
        {
            // This code is guaranteed to run on the UI thread!
            labelAircraftName.Text = data.Model;

            // When simulator connects and model changes, auto-select its profile
            if (!string.IsNullOrWhiteSpace(data.Model))
            {
                if (_currentModel != data.Model)
                {
                    _currentModel = data.Model;

                    // Load profile from disk (fallback to default)
                    _currentProfile = _profileManager.LoadProfileForModel(_currentModel);

                    // Apply and update dropdown selection to match the aircraft model
                    _applyingProfile = true;
                    try
                    {
                        ApplyProfileToUi(_currentProfile);
                        _mechanicServices.SetActiveProfile(_currentProfile);

                        RefreshProfilesDropdown();
                        SelectProfileInDropdown(_currentProfile.Model);
                    }
                    finally
                    {
                        _applyingProfile = false;
                    }
                }
            }
        }

        private void ApplyProfileToUi(AirplaneProfile profile)
        {
            _applyingProfile = true;
            try
            {
                SwitchCenterSpring.Checked = profile.CenteredSpring;
                switchDynamicSpring.Visible = SwitchCenterSpring.Checked;
                switchDynamicSpring.Checked = profile.DynamicSpring && SwitchCenterSpring.Checked;
                switchStickShaker.Checked = profile.StickShaker;
                GearVibratesSwitch.Checked = profile.GearVibration;
                GroundVibrationSwitch.Checked = profile.GroundVibration;

                // Trim
                TrimSwitch.Checked = profile.TrimEnabled;
                PitchUpTextBox.Text = profile.PitchTrimUpButton >= 0 ? profile.PitchTrimUpButton.ToString() : string.Empty;
                PitchDownTextBox.Text = profile.PitchTrimDownButton >= 0 ? profile.PitchTrimDownButton.ToString() : string.Empty;
                RollLeftTextBox.Text = profile.RollTrimLeftButton >= 0 ? profile.RollTrimLeftButton.ToString() : string.Empty;
                RollRightTextBox.Text = profile.RollTrimRightButton >= 0 ? profile.RollTrimRightButton.ToString() : string.Empty;

                // Trim controls visibility follows centered spring
                UpdateTrimControlsVisibility();
            }
            finally
            {
                _applyingProfile = false;
            }
        }

        private void UpdateTrimControlsVisibility()
        {
            bool visible = SwitchCenterSpring.Checked;
            TrimSwitch.Visible = visible;
            JoyBtnExplainLabel.Visible = visible;
            PitchUpLabel.Visible = visible;
            PitchDownLabel.Visible = visible;
            RollLeftLabel.Visible = visible;
            RollRightLabel.Visible = visible;
            PitchUpTextBox.Visible = visible;
            PitchDownTextBox.Visible = visible;
            RollLeftTextBox.Visible = visible;
            RollRightTextBox.Visible = visible;
        }

        private static int ParseButtonIndex(MaterialTextBox textBox)
        {
            if (int.TryParse(textBox.Text, out var idx) && idx >= 0)
                return idx;
            return -1;
        }

        private void UpdateCurrentProfileFromUi()
        {
            if (_applyingProfile) return;
            if (string.IsNullOrWhiteSpace(_currentModel)) return;

            _currentProfile ??= new AirplaneProfile { Model = _currentModel };

            _currentProfile.CenteredSpring = SwitchCenterSpring.Checked;
            _currentProfile.DynamicSpring = SwitchCenterSpring.Checked && switchDynamicSpring.Checked; // only valid when centered
            _currentProfile.StickShaker = switchStickShaker.Checked;
            _currentProfile.GearVibration = GearVibratesSwitch.Checked;
            _currentProfile.GroundVibration = GroundVibrationSwitch.Checked;

            // Trim
            _currentProfile.TrimEnabled = TrimSwitch.Checked;
            _currentProfile.PitchTrimUpButton = ParseButtonIndex(PitchUpTextBox);
            _currentProfile.PitchTrimDownButton = ParseButtonIndex(PitchDownTextBox);
            _currentProfile.RollTrimLeftButton = ParseButtonIndex(RollLeftTextBox);
            _currentProfile.RollTrimRightButton = ParseButtonIndex(RollRightTextBox);

            _mechanicServices.SetActiveProfile(_currentProfile);

            // Auto-save on change
            _profileManager.SaveProfile(_currentProfile);
        }

        private void WireTrimCaptureHandlers()
        {
            PitchUpTextBox.GotFocus += (s, e) => BeginJoystickCaptureForTextBox(PitchUpTextBox);
            PitchDownTextBox.GotFocus += (s, e) => BeginJoystickCaptureForTextBox(PitchDownTextBox);
            RollLeftTextBox.GotFocus += (s, e) => BeginJoystickCaptureForTextBox(RollLeftTextBox);
            RollRightTextBox.GotFocus += (s, e) => BeginJoystickCaptureForTextBox(RollRightTextBox);

            PitchUpTextBox.LostFocus += (s, e) => _mechanicServices.CancelButtonCapture();
            PitchDownTextBox.LostFocus += (s, e) => _mechanicServices.CancelButtonCapture();
            RollLeftTextBox.LostFocus += (s, e) => _mechanicServices.CancelButtonCapture();
            RollRightTextBox.LostFocus += (s, e) => _mechanicServices.CancelButtonCapture();
        }

        private void BeginJoystickCaptureForTextBox(MaterialTextBox textBox)
        {
            // Clear current content to indicate capture mode
            textBox.Text = string.Empty;
            _mechanicServices.BeginButtonCapture(idx =>
            {
                // This callback executes on the mechanic background thread.
                // Marshal back to UI thread to update the textbox and profile.
                if (textBox.IsHandleCreated)
                {
                    textBox.Invoke(new Action(() =>
                    {
                        textBox.Text = idx.ToString();
                        UpdateCurrentProfileFromUi();
                    }));
                }
            });
        }

        // MainForm_Load is called when the form is loaded
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize the mechanic services
            var progress = new Progress<MechanicProgress>(MechanicProgressReporter);
            _mechanicServices?.Start(progress);

            // Hook change events for effects controls
            switchDynamicSpring.CheckedChanged += switchDynamicSpring_CheckedChanged;
            switchStickShaker.CheckedChanged += switchStickShaker_CheckedChanged;
            GearVibratesSwitch.CheckedChanged += GearVibratesSwitch_CheckedChanged;
            GroundVibrationSwitch.CheckedChanged += GroundVibrationSwitch_CheckedChanged;

            TrimSwitch.CheckedChanged += TrimSwitch_CheckedChanged;
            PitchUpTextBox.TextChanged += TrimTextBox_TextChanged;
            PitchDownTextBox.TextChanged += TrimTextBox_TextChanged;
            RollLeftTextBox.TextChanged += TrimTextBox_TextChanged;
            RollRightTextBox.TextChanged += TrimTextBox_TextChanged;

            // Wire capture handlers for trim assignment
            WireTrimCaptureHandlers();

            // Ensure dynamic spring visibility reflects center spring state on startup
            switchDynamicSpring.Visible = SwitchCenterSpring.Checked;
            UpdateTrimControlsVisibility();

            // Populate profiles dropdown from disk
            RefreshProfilesDropdown();

            // Initialize flight status label
            FlightStatusLabel.Text = "No Flight Loaded";
        }

        private void TrimSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void TrimTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cleanly stop the background thread on form close
            _simConnectService?.Dispose();
            _mechanicServices?.Dispose();
        }

        private void buttonConnectSimulator_Click(object sender, EventArgs e)
        {
            // Toggle _isSimConnectClicked to prevent multiple clicks
            _isSimConnectClicked = !_isSimConnectClicked;

            if (_isSimConnectClicked)
            {
                var progress = new Progress<AirplaneProfile>(SimConnectProgressReporter);
                var flightStatus = new Progress<MechanicProgress>(MechanicProgressReporter);
                _simConnectService.Start(progress, this.Handle, flightStatus);
            }
            else
            {
                _simConnectService.Stop();
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            _mechanicServices.LoadJoysticks();
        }

        private void SwitchCenterSpring_CheckedChanged(object sender, EventArgs e)
        {
            // Show/hide dynamic spring switch based on center spring state
            switchDynamicSpring.Visible = SwitchCenterSpring.Checked;
            if (!switchDynamicSpring.Visible)
            {
                // Optional: reset its state when hidden
                switchDynamicSpring.Checked = false;
            }

            // Trim controls visibility follows centered spring
            UpdateTrimControlsVisibility();

            UpdateCurrentProfileFromUi();
        }

        private void switchDynamicSpring_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void switchStickShaker_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void GearVibratesSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void GroundVibrationSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProfileFromUi();
        }

        private void comboBoxJoysticks_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_uiReadyForAcquire) return;

            var name = comboBoxJoysticks.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(name)) return;

            var info = _mechanicServices.SelectJoystick(name, this.Handle);
            labelJoystickStatus.Text = name;
            textJoystickInfo.Text = info;
        }

        private void comboBoxProfiles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_applyingProfile) return;

            var selectedName = comboBoxProfiles.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName)) return;

            // Load and apply the explicitly selected profile; do not override its Model
            var profile = _profileManager.LoadProfileForModel(selectedName);
            _currentProfile = profile;
            ApplyProfileToUi(profile);
            _mechanicServices.SetActiveProfile(profile);
        }

        private void buttonSaveNewProfile_Click(object? sender, EventArgs e)
        {
            // Saves a new profile. If simulator has an active model, save under that model name; otherwise use the selected profile name.
            var selectedName = comboBoxProfiles.SelectedItem as string;
            var targetName = !string.IsNullOrWhiteSpace(_currentModel) ? _currentModel : selectedName;
            if (string.IsNullOrWhiteSpace(targetName)) return;

            var profile = new AirplaneProfile
            {
                Model = targetName!,
                CenteredSpring = SwitchCenterSpring.Checked,
                DynamicSpring = SwitchCenterSpring.Checked && switchDynamicSpring.Checked,
                StickShaker = switchStickShaker.Checked,
                GearVibration = GearVibratesSwitch.Checked,
                GroundVibration = GroundVibrationSwitch.Checked,
                TrimEnabled = TrimSwitch.Checked,
                PitchTrimUpButton = ParseButtonIndex(PitchUpTextBox),
                PitchTrimDownButton = ParseButtonIndex(PitchDownTextBox),
                RollTrimLeftButton = ParseButtonIndex(RollLeftTextBox),
                RollTrimRightButton = ParseButtonIndex(RollRightTextBox)
            };
            _profileManager.SaveProfile(profile);
            _currentProfile = profile;
            RefreshProfilesDropdown();
            SelectProfileInDropdown(profile.Model);
        }
    }
}
