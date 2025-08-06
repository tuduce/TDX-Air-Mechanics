using MaterialSkin;
using MaterialSkin.Controls;
using TDXAirMechanic.Model;
using TDXAirMechanic.Services;

namespace TDXAirMechanic
{
    public partial class MainForm : MaterialForm
    {
        private SimConnectService? _simConnectService;
        private readonly CancellationTokenSource _formClosingCts = new CancellationTokenSource();

        private bool _isSimConnectClicked = false;

        public MainForm()
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

            _simConnectService = new SimConnectService();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cleanly stop the background thread on form close
            _simConnectService?.Dispose();
        }

        private void buttonConnectSimulator_Click(object sender, EventArgs e)
        {
            // Toggle _isSimConnectClicked to prevent multiple clicks
            _isSimConnectClicked = !_isSimConnectClicked;

            if (_isSimConnectClicked)
            {
                // Create a new manager if the old one was disposed
                if (_simConnectService == null)
                {
                    _simConnectService = new SimConnectService();
                }

                var progress = new Progress<AirplaneProfile>(data =>
                {
                    // This code is guaranteed to run on the UI thread!
                    labelAircraftName.Text = data.Model;
                });

                // The Start method no longer needs the window handle
                _simConnectService.Start(progress, this.Handle);
            }
            else
            {
                // Just call Dispose() to handle everything.
                _simConnectService?.Dispose();
                _simConnectService = null; // Set to null to allow reconnection
            }
        }
    }
}
