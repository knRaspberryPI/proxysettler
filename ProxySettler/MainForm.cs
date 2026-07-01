using System.Net.Http;
using System.Text.Json;

namespace ProxySettler;

public class MainForm : Form
{
    private static readonly HttpClient HttpClient = new();
    private const string IpCheckUrl = "https://api.myip.com";

    private static readonly Color TextColor = ColorTranslator.FromHtml("#0E0E10");
    private static readonly Color SubtleColor = Color.FromArgb(255, 120, 120, 124);
    private static readonly Color ActiveColor = Color.FromArgb(255, 20, 130, 70);
    private static readonly Color InactiveColor = Color.FromArgb(255, 176, 45, 45);
    private static readonly Color DividerColor = Color.FromArgb(255, 229, 229, 231);

    private readonly TextBox _txtProxy;
    private readonly NumericUpDown _numPort;
    private readonly RoundedButton _btnToggle;
    private readonly Label _lblStatus;

    private readonly RoundedButton _btnCheckIp;
    private readonly Label _lblIpValue;
    private readonly Label _lblCountryValue;
    private readonly Label _lblIpError;

    public MainForm()
    {
        Text = "Proxy Settler";
        ClientSize = new Size(420, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9.5f);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;

        var lblProxyCaption = new Label { Text = "PROXY ADDRESS", Left = 24, Top = 24, Width = 220, ForeColor = SubtleColor, Font = new Font("Segoe UI", 8f) };
        _txtProxy = new TextBox { Left = 24, Top = 44, Width = 220, Font = new Font("Segoe UI", 10f), PlaceholderText = "e.g. 127.0.0.1" };

        var lblPortCaption = new Label { Text = "PORT", Left = 260, Top = 24, Width = 136, ForeColor = SubtleColor, Font = new Font("Segoe UI", 8f) };
        _numPort = new NumericUpDown { Left = 260, Top = 44, Width = 136, Minimum = 1, Maximum = 65535, Value = 8080, Font = new Font("Segoe UI", 10f) };

        _btnToggle = new RoundedButton { Text = "Connect", Left = 24, Top = 84, Width = 372, Height = 44 };
        _btnToggle.Click += BtnToggle_Click;

        _lblStatus = new Label
        {
            Text = "Inactive",
            Left = 24,
            Top = 140,
            Width = 372,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = InactiveColor,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        var divider = new Panel { Left = 24, Top = 176, Width = 372, Height = 1, BackColor = DividerColor };

        _btnCheckIp = new RoundedButton { Text = "Check My IP", Left = 24, Top = 196, Width = 160, Height = 38 };
        _btnCheckIp.Click += BtnCheckIp_Click;

        _lblIpValue = new Label
        {
            Text = "",
            Left = 24,
            Top = 248,
            Height = 22,
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };

        _lblCountryValue = new Label
        {
            Text = "",
            Top = 248,
            Height = 22,
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f)
        };

        _lblIpError = new Label { Text = "", Left = 24, Top = 274, Width = 372, ForeColor = InactiveColor, Font = new Font("Segoe UI", 8.5f) };

        Controls.AddRange(new Control[]
        {
            lblProxyCaption, _txtProxy, lblPortCaption, _numPort, _btnToggle, _lblStatus,
            divider, _btnCheckIp, _lblIpValue, _lblCountryValue, _lblIpError
        });

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var (enabled, server) = ProxyManager.GetStatus();

        // The registry keeps the last-used ProxyServer value even after disconnecting,
        // so this doubles as "remember what I typed last time".
        if (!string.IsNullOrWhiteSpace(server))
        {
            var parts = server.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var parsedPort))
            {
                _txtProxy.Text = parts[0];
                _numPort.Value = Math.Clamp(parsedPort, (int)_numPort.Minimum, (int)_numPort.Maximum);
            }
        }

        if (enabled)
        {
            _lblStatus.Text = $"Active — {server}";
            _lblStatus.ForeColor = ActiveColor;
            _btnToggle.Text = "Disconnect";
        }
        else
        {
            _lblStatus.Text = "Inactive";
            _lblStatus.ForeColor = InactiveColor;
            _btnToggle.Text = "Connect";
        }
    }

    private void BtnToggle_Click(object? sender, EventArgs e)
    {
        var (enabled, _) = ProxyManager.GetStatus();

        if (enabled)
        {
            try
            {
                ProxyManager.Disconnect();
                RefreshStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to disable the proxy: {ex.Message}", "Proxy Settler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;
        }

        var proxy = _txtProxy.Text.Trim();
        if (string.IsNullOrEmpty(proxy))
        {
            MessageBox.Show(this, "Please enter a proxy address.", "Proxy Settler",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            ProxyManager.Connect(proxy, (int)_numPort.Value);
            RefreshStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to set the proxy: {ex.Message}", "Proxy Settler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnCheckIp_Click(object? sender, EventArgs e)
    {
        _btnCheckIp.Enabled = false;
        _lblIpError.Text = "";
        _lblIpValue.Text = "Checking...";
        _lblCountryValue.Text = "";

        try
        {
            using var response = await HttpClient.GetAsync(IpCheckUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<IpInfo>(json);

            _lblIpValue.Text = string.IsNullOrEmpty(info?.Ip) ? "N/A" : info.Ip;
            _lblCountryValue.Text = !string.IsNullOrEmpty(info?.Country)
                ? string.IsNullOrEmpty(info.CountryCode) ? $" ({info.Country})" : $" ({info.Country}, {info.CountryCode})"
                : "";
        }
        catch (Exception ex)
        {
            _lblIpValue.Text = "";
            _lblCountryValue.Text = "";
            _lblIpError.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _lblCountryValue.Left = _lblIpValue.Right;
            _btnCheckIp.Enabled = true;
        }
    }
}
