using System.Text.Json;
using Velopack;
using Velopack.Sources;

namespace VelopackDemo;

public partial class Form1 : Form
{
    #region Fields

    private static readonly HttpClient Http = new();
    private const string MinVersionUrl = "https://raw.githubusercontent.com/LuekenF/velopack/main/min-version.json";
    private const string GithubRepo = "https://github.com/LuekenF/velopack";

    private DateTime _lastUpdateCheckDate = DateTime.MinValue;
    private bool _isUpdating = false;

    #endregion

    #region Ctors

    public Form1()
    {
        InitializeComponent();
        var v = CurrentVersion;
        lblVersion.Text = $"Version: {v.Major}.{v.Minor}.{v.Build}";
    }

    #endregion

    #region Properties

    private static Version CurrentVersion =>
        Version.Parse(Application.ProductVersion.Split('+')[0]);

    #endregion

    #region Methods

    private async void Form1_LoadEventHandler(object sender, EventArgs e)
    {
        await RunUpdateCheckAsync(checkRequiredVersion: true);
    }

    private async void Form1_ActivatedEventHandler(object sender, EventArgs e)
    {
        if (_lastUpdateCheckDate.Date >= DateTime.Today) return;
        _lastUpdateCheckDate = DateTime.Today;
        await RunUpdateCheckAsync(checkRequiredVersion: false);
    }

    private async void BtnCheckUpdateClickEventHandler(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        _isUpdating = true;
        btnCheckUpdate.Enabled = false;
        lblStatus.Text = "Suche nach Updates...";

        try
        {
            await ApplyUpdateAsync(silent: false);
        }
        finally
        {
            _isUpdating = false;
            btnCheckUpdate.Enabled = true;
        }
    }

    private async Task RunUpdateCheckAsync(bool checkRequiredVersion)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            if (checkRequiredVersion && await IsForcedUpdateRequiredAsync())
            {
                btnCheckUpdate.Enabled = false;
                lblStatus.Text = "Pflichtupdate wird heruntergeladen...";
                await ApplyUpdateAsync();
                return;
            }

            await ApplyUpdateAsync(silent: true);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async Task<bool> IsForcedUpdateRequiredAsync()
    {
        try
        {
            var json = await Http.GetStringAsync(MinVersionUrl);
            var doc = JsonDocument.Parse(json);
            var minStr = doc.RootElement.GetProperty("minimumRequiredVersion").GetString();
            var current = CurrentVersion;
            var minimum = Version.Parse(minStr + ".0");
            return current < minimum;
        }
        catch
        {
            return false;
        }
    }

    private async Task ApplyUpdateAsync(bool silent = false)
    {
        try
        {
            var mgr = new UpdateManager(new GithubSource(GithubRepo, null, false));
            var updateInfo = await mgr.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                if (!silent)
                    lblStatus.Text = "Kein Update verfügbar.";
                return;
            }

            lblStatus.Text = $"Update {updateInfo.TargetFullRelease.Version} wird heruntergeladen...";
            await mgr.DownloadUpdatesAsync(updateInfo);
            lblStatus.Text = "Update bereit. App wird neu gestartet...";
            mgr.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            if (!silent)
                lblStatus.Text = $"Fehler: {ex.Message}";
        }
    }

    #endregion
}
