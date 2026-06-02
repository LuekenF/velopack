using System.Text.Json;
using Velopack;
using Velopack.Sources;

namespace VelopackDemo;

public partial class Form1 : Form
{
    private static readonly HttpClient Http = new();
    private const string MinVersionUrl = "https://raw.githubusercontent.com/LuekenF/velopack/main/min-version.json";
    private const string GithubRepo = "https://github.com/LuekenF/velopack";

    private DateTime _lastUpdateCheckDate = DateTime.MinValue;

    public Form1()
    {
        InitializeComponent();
        var v = Version.Parse(Application.ProductVersion);
        lblVersion.Text = $"Version: {v.Major}.{v.Minor}.{v.Build}";
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        await CheckForRequiredUpdateAsync();
    }

    private async void Form1_Activated(object sender, EventArgs e)
    {
        if (_lastUpdateCheckDate.Date < DateTime.Today)
        {
            _lastUpdateCheckDate = DateTime.Today;
            await CheckForUpdatesAsync(silent: true);
        }
    }

    private async Task CheckForRequiredUpdateAsync()
    {
        try
        {
            var json = await Http.GetStringAsync(MinVersionUrl);
            var doc = JsonDocument.Parse(json);
            var minStr = doc.RootElement.GetProperty("minimumRequiredVersion").GetString();

            var current = Version.Parse(Application.ProductVersion);
            var minimum = Version.Parse(minStr + ".0");

            if (current < minimum)
            {
                btnCheckUpdate.Enabled = false;
                lblStatus.Text = "Pflichtupdate wird heruntergeladen...";

                var mgr = new UpdateManager(new GithubSource(GithubRepo, null, false));
                var updateInfo = await mgr.CheckForUpdatesAsync();

                if (updateInfo != null)
                {
                    await mgr.DownloadUpdatesAsync(updateInfo);
                    lblStatus.Text = "Update bereit. App wird neu gestartet...";
                    mgr.ApplyUpdatesAndRestart(updateInfo);
                }
            }
        }
        catch { }
    }

    private async Task CheckForUpdatesAsync(bool silent = false)
    {
        if (!silent)
        {
            btnCheckUpdate.Enabled = false;
            lblStatus.Text = "Suche nach Updates...";
        }

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

            lblStatus.Text = $"Update verfügbar: {updateInfo.TargetFullRelease.Version} – lade herunter...";
            await mgr.DownloadUpdatesAsync(updateInfo);
            lblStatus.Text = "Update bereit. App wird neu gestartet...";
            mgr.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            if (!silent)
                lblStatus.Text = $"Fehler: {ex.Message}";
        }
        finally
        {
            if (!silent)
                btnCheckUpdate.Enabled = true;
        }
    }

    private async void btnCheckUpdate_Click(object sender, EventArgs e)
    {
        await CheckForUpdatesAsync(silent: false);
    }
}
