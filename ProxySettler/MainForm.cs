using System.Net.Http;
using System.Text.Json;

namespace ProxySettler;

public class MainForm : Form
{
    private static readonly HttpClient HttpClient = new();
    private const string IpCheckUrl = "https://api.myip.com";

    private readonly TextBox _txtProxy;
    private readonly NumericUpDown _numPort;
    private readonly Button _btnConnect;
    private readonly Button _btnDisconnect;
    private readonly Label _lblStatus;

    private readonly Button _btnCheckIp;
    private readonly Label _lblIpValue;
    private readonly Label _lblCountryValue;
    private readonly Label _lblIpError;

    public MainForm()
    {
        Text = "Proxy Settler";
        ClientSize = new Size(450, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var grpProxy = new GroupBox
        {
            Text = "Proxy Configuration",
            Left = 15,
            Top = 15,
            Width = 420,
            Height = 175
        };

        var lblProxy = new Label { Text = "Proxy address:", Left = 15, Top = 32, Width = 100 };
        _txtProxy = new TextBox { Left = 120, Top = 28, Width = 280, PlaceholderText = "e.g. 127.0.0.1" };

        var lblPort = new Label { Text = "Port:", Left = 15, Top = 67, Width = 100 };
        _numPort = new NumericUpDown { Left = 120, Top = 63, Width = 100, Minimum = 1, Maximum = 65535, Value = 8080 };

        _btnConnect = new Button { Text = "Connect", Left = 120, Top = 100, Width = 130, Height = 32 };
        _btnConnect.Click += BtnConnect_Click;

        _btnDisconnect = new Button { Text = "Disconnect", Left = 260, Top = 100, Width = 140, Height = 32 };
        _btnDisconnect.Click += BtnDisconnect_Click;

        _lblStatus = new Label
        {
            Text = "Status: unknown",
            Left = 15,
            Top = 145,
            Width = 390,
            Font = new Font(Font, FontStyle.Bold)
        };

        grpProxy.Controls.AddRange(new Control[]
        {
            lblProxy, _txtProxy, lblPort, _numPort, _btnConnect, _btnDisconnect, _lblStatus
        });

        var grpIp = new GroupBox
        {
            Text = "IP Check",
            Left = 15,
            Top = 200,
            Width = 420,
            Height = 145
        };

        _btnCheckIp = new Button { Text = "Check My IP", Left = 15, Top = 28, Width = 140, Height = 32 };
        _btnCheckIp.Click += BtnCheckIp_Click;

        var lblIpCaption = new Label { Text = "Public IP:", Left = 15, Top = 75, Width = 90 };
        _lblIpValue = new Label { Text = "-", Left = 110, Top = 75, Width = 290, Font = new Font(Font, FontStyle.Bold) };

        var lblCountryCaption = new Label { Text = "Country:", Left = 15, Top = 100, Width = 90 };
        _lblCountryValue = new Label { Text = "-", Left = 110, Top = 100, Width = 290 };

        _lblIpError = new Label { Text = "", Left = 15, Top = 122, Width = 390, ForeColor = Color.Firebrick };

        grpIp.Controls.AddRange(new Control[]
        {
            _btnCheckIp, lblIpCaption, _lblIpValue, lblCountryCaption, _lblCountryValue, _lblIpError
        });

        Controls.Add(grpProxy);
        Controls.Add(grpIp);

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var (enabled, server) = ProxyManager.GetStatus();

        if (enabled && !string.IsNullOrWhiteSpace(server))
        {
            _lblStatus.Text = $"Status: Active ({server})";
            _lblStatus.ForeColor = Color.DarkGreen;

            var parts = server.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var parsedPort))
            {
                _txtProxy.Text = parts[0];
                _numPort.Value = Math.Clamp(parsedPort, (int)_numPort.Minimum, (int)_numPort.Maximum);
            }
        }
        else
        {
            _lblStatus.Text = "Status: Inactive";
            _lblStatus.ForeColor = Color.Firebrick;
        }
    }

    private void BtnConnect_Click(object? sender, EventArgs e)
    {
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

    private void BtnDisconnect_Click(object? sender, EventArgs e)
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
    }

    private async void BtnCheckIp_Click(object? sender, EventArgs e)
    {
        _btnCheckIp.Enabled = false;
        _lblIpError.Text = "";
        _lblIpValue.Text = "Checking...";
        _lblCountryValue.Text = "-";

        try
        {
            using var response = await HttpClient.GetAsync(IpCheckUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<IpInfo>(json);

            _lblIpValue.Text = string.IsNullOrEmpty(info?.Ip) ? "N/A" : info.Ip;

            if (!string.IsNullOrEmpty(info?.Country))
            {
                _lblCountryValue.Text = string.IsNullOrEmpty(info.CountryCode)
                    ? info.Country
                    : $"{info.Country} ({info.CountryCode})";
            }
            else
            {
                _lblCountryValue.Text = "N/A";
            }
        }
        catch (Exception ex)
        {
            _lblIpValue.Text = "-";
            _lblCountryValue.Text = "-";
            _lblIpError.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _btnCheckIp.Enabled = true;
        }
    }
}
