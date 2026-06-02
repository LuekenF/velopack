using Velopack;
using Velopack.Sources;

namespace VelopackDemo;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        lblVersion.Text = $"Version: {Application.ProductVersion}";
    }

    private async void btnCheckUpdate_Click(object sender, EventArgs e)
    {
        btnCheckUpdate.Enabled = false;
        lblStatus.Text = "Suche nach Updates...";

        try
        {
            var mgr = new UpdateManager(new GithubSource("https://github.com/LuekenF/velopack", null, false));
            var updateInfo = await mgr.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                lblStatus.Text = "Kein Update verfügbar.";
                return;
            }

            lblStatus.Text = $"Update gefunden: {updateInfo.TargetFullRelease.Version} – lade herunter...";
            await mgr.DownloadUpdatesAsync(updateInfo);

            lblStatus.Text = "Update bereit. App wird neu gestartet...";
            mgr.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Fehler: {ex.Message}";
        }
        finally
        {
            btnCheckUpdate.Enabled = true;
        }
    }
}
