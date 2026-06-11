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
        pictureBoxCat.Image = CreateCatImage();
    }

    #endregion

    #region Properties

    private static Version CurrentVersion =>
        Version.Parse(Application.ProductVersion.Split('+')[0]);

    #endregion

    #region Methods

    private async void FormLoadEventHandler(object sender, EventArgs e)
    {
        await RunUpdateCheckAsync(checkRequiredVersion: true);
    }

    private async void FormActivatedEventHandler(object sender, EventArgs e)
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

    private static Bitmap CreateCatImage()
    {
        var bmp = new Bitmap(460, 200);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(100, 149, 237));

        using var catBrush = new SolidBrush(Color.FromArgb(220, 120, 20));
        using var pinkBrush = new SolidBrush(Color.FromArgb(255, 182, 193));
        using var noseBrush = new SolidBrush(Color.DeepPink);
        using var mouthPen = new Pen(Color.FromArgb(120, 60, 60), 2f);
        using var whiskerPen = new Pen(Color.White, 1.5f);

        g.FillEllipse(catBrush, 160, 150, 140, 60);
        g.FillEllipse(catBrush, 155, 28, 150, 138);

        g.FillPolygon(catBrush, new Point[] { new(168, 58), new(163, 5), new(208, 43) });
        g.FillPolygon(pinkBrush, new Point[] { new(172, 54), new(168, 15), new(205, 46) });

        g.FillPolygon(catBrush, new Point[] { new(292, 58), new(297, 5), new(252, 43) });
        g.FillPolygon(pinkBrush, new Point[] { new(288, 54), new(292, 15), new(255, 46) });

        g.FillEllipse(Brushes.White, 186, 82, 32, 24);
        g.FillEllipse(Brushes.White, 242, 82, 32, 24);

        g.FillEllipse(Brushes.Black, 194, 86, 16, 18);
        g.FillEllipse(Brushes.Black, 250, 86, 16, 18);

        g.FillEllipse(Brushes.White, 198, 88, 5, 5);
        g.FillEllipse(Brushes.White, 254, 88, 5, 5);

        g.FillEllipse(noseBrush, 221, 117, 18, 12);

        g.DrawArc(mouthPen, 205, 126, 22, 12, 0, 180);
        g.DrawArc(mouthPen, 233, 126, 22, 12, 0, 180);

        g.DrawLine(whiskerPen, 218, 118, 130, 106);
        g.DrawLine(whiskerPen, 218, 124, 130, 124);
        g.DrawLine(whiskerPen, 218, 130, 130, 142);

        g.DrawLine(whiskerPen, 242, 118, 330, 106);
        g.DrawLine(whiskerPen, 242, 124, 330, 124);
        g.DrawLine(whiskerPen, 242, 130, 330, 142);

        return bmp;
    }

    #endregion
}
