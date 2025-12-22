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
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.Command), data.Command, null);
            }
        }

        private void SimConnectProgressReporter(AirplaneProfile data)
        {
            // This code is guaranteed to run on the UI thread!
            labelAircraftName.Text = data.Model;

            // Update current model and apply profile
            if (!string.IsNullOrWhiteSpace(data.Model))
            {
                if (_currentModel != data.Model)
                {
                    _currentModel = data.Model;

                    // Load profile from disk (fallback to default)
                    _currentProfile = _profileManager.LoadProfileForModel(_currentModel);

                    ApplyProfileToUi(_currentProfile);
                    _mechanicServices.SetActiveProfile(_currentProfile);

                    // Ensure profiles dropdown is populated
                    RefreshProfilesDropdown();
                    SelectProfileInDropdown(_currentProfile.Model);
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
            }
            finally
            {
                _applyingProfile = false;
            }
        }

        private void UpdateCurrentProfileFromUi()
        {
            if (_applyingProfile) return;
            if (string.IsNullOrWhiteSpace(_currentModel)) return;

            _currentProfile ??= new AirplaneProfile { Model = _currentModel };

            _currentProfile.CenteredSpring = SwitchCenterSpring.Checked;
            _currentProfile.DynamicSpring = SwitchCenterSpring.Checked && switchDynamicSpring.Checked; // only valid when centered
            _currentProfile.StickShaker = switchStickShaker.Checked;

            _mechanicServices.SetActiveProfile(_currentProfile);

            // Auto-save on change
            _profileManager.SaveProfile(_currentProfile);
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

            // Ensure dynamic spring visibility reflects center spring state on startup
            switchDynamicSpring.Visible = SwitchCenterSpring.Checked;

            // Populate profiles dropdown from disk
            RefreshProfilesDropdown();
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
                _simConnectService.Start(progress, this.Handle);
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

        private void comboBoxJoysticks_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_uiReadyForAcquire) return;

            var name = comboBoxJoysticks.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(name)) return;

            var info = _mechanicServices.SelectJoystick(name, this.Handle);
            labelJoystickStatus.Text = name;
            textJoystickInfo.Text = info;
        }

        private void RefreshProfilesDropdown()
        {
            var names = _profileManager.ListProfiles();
            comboBoxProfiles.DataSource = null;
            comboBoxProfiles.DataSource = names;
        }

        private void SelectProfileInDropdown(string? model)
        {
            if (string.IsNullOrWhiteSpace(model)) return;
            var names = comboBoxProfiles.DataSource as System.Collections.IList;
            if (names == null) return;
            var idx = names.IndexOf(model);
            if (idx >= 0)
                comboBoxProfiles.SelectedIndex = idx;
        }

        private void comboBoxProfiles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_applyingProfile) return;

            var selectedName = comboBoxProfiles.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName)) return;

            // When user picks a profile, apply (but keep current model in label)
            var profile = _profileManager.LoadProfileForModel(selectedName);
            // Override model to current aircraft if known
            profile.Model = _currentModel ?? selectedName;
            _currentProfile = profile;
            ApplyProfileToUi(profile);
            _mechanicServices.SetActiveProfile(profile);
        }

        private void buttonSaveNewProfile_Click(object? sender, EventArgs e)
        {
            // Saves current UI profile for current aircraft model name
            if (string.IsNullOrWhiteSpace(_currentModel)) return;
            var profile = new AirplaneProfile
            {
                Model = _currentModel,
                CenteredSpring = SwitchCenterSpring.Checked,
                DynamicSpring = SwitchCenterSpring.Checked && switchDynamicSpring.Checked,
                StickShaker = switchStickShaker.Checked
            };
            _profileManager.SaveProfile(profile);
            _currentProfile = profile;
            RefreshProfilesDropdown();
            SelectProfileInDropdown(profile.Model);
        }
    }
}
