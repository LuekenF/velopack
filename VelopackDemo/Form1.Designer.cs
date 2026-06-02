namespace VelopackDemo;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblVersion = new Label();
        lblStatus = new Label();
        btnCheckUpdate = new Button();
        SuspendLayout();

        lblVersion.AutoSize = true;
        lblVersion.Font = new Font("Segoe UI", 10F);
        lblVersion.Location = new Point(20, 20);
        lblVersion.Name = "lblVersion";
        lblVersion.Text = "Version: ...";

        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.DimGray;
        lblStatus.Location = new Point(20, 100);
        lblStatus.Name = "lblStatus";
        lblStatus.Text = "";

        btnCheckUpdate.Font = new Font("Segoe UI", 9F);
        btnCheckUpdate.Location = new Point(20, 60);
        btnCheckUpdate.Name = "btnCheckUpdate";
        btnCheckUpdate.Size = new Size(160, 30);
        btnCheckUpdate.Text = "Nach Updates suchen";
        btnCheckUpdate.Click += btnCheckUpdate_Click;

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(500, 200);
        Controls.Add(lblVersion);
        Controls.Add(btnCheckUpdate);
        Controls.Add(lblStatus);
        Name = "Form1";
        Text = "VelopackDemo";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblVersion;
    private Label lblStatus;
    private Button btnCheckUpdate;
}
