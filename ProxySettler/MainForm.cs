using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace ProxySettler;

public class MainForm : Form
{
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private const string IpCheckUrl = "https://api.myip.com";

    private static readonly Color TextColor = ColorTranslator.FromHtml("#0E0E10");
    private static readonly Color SubtleColor = Color.FromArgb(255, 145, 145, 150);
    private static readonly Color AccentBlue = ColorTranslator.FromHtml("#2F54EB");
    private static readonly Color ActiveColor = ColorTranslator.FromHtml("#16A34A");
    private static readonly Color InactiveColor = ColorTranslator.FromHtml("#E11D48");
    private static readonly Color DividerColor = Color.FromArgb(255, 231, 231, 235);

    private readonly OutlinedField _proxyField;
    private readonly OutlinedField _portField;
    private readonly RoundedButton _btnToggle;
    private readonly Label _lblStatus;

    private readonly LinkLabel _lnkCheckIp;
    private readonly Label _lblIpValue;
    private readonly Label _lblCountryValue;
    private readonly Label _lblIpError;

    public MainForm()
    {
        Text = "Proxy Settler";
        ClientSize = new Size(420, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9.5f);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;

        var picIcon = new PictureBox
        {
            Left = (420 - 64) / 2,
            Top = 28,
            Width = 64,
            Height = 64,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadEmbeddedIcon(),
        };

        var lblTitle = new Label
        {
            Text = "Proxy Settler",
            Left = 24,
            Top = 104,
            Width = 372,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
        };

        var lblSubtitle = new Label
        {
            Text = "Set your proxy and port",
            Left = 24,
            Top = 140,
            Width = 372,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = SubtleColor,
            Font = new Font("Segoe UI", 10f),
        };

        _proxyField = new OutlinedField("Proxy") { Left = 24, Top = 176, Width = 228, Height = 64 };
        _proxyField.TextBox.PlaceholderText = "0.0.0.0";

        _portField = new OutlinedField("Port", numericOnly: true) { Left = 268, Top = 176, Width = 128, Height = 64 };

        _btnToggle = new RoundedButton
        {
            Text = "Connect",
            Left = 24,
            Top = 256,
            Width = 372,
            Height = 56,
            AccentColor = AccentBlue,
        };
        _btnToggle.Click += BtnToggle_Click;

        _lblStatus = new Label
        {
            Text = "Inactive",
            Left = 24,
            Top = 328,
            Width = 372,
            Height = 22,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = InactiveColor,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
        };

        var divider = new Panel { Left = 24, Top = 372, Width = 372, Height = 1, BackColor = DividerColor };

        _lnkCheckIp = new LinkLabel
        {
            Text = "Check my IP",
            Left = 24,
            Top = 396,
            Width = 372,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            LinkColor = TextColor,
            ActiveLinkColor = TextColor,
            VisitedLinkColor = TextColor,
            LinkBehavior = LinkBehavior.AlwaysUnderline,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
        };
        _lnkCheckIp.LinkClicked += async (_, _) => await CheckIpAsync();

        _lblIpValue = new Label
        {
            Text = "",
            Top = 432,
            Height = 24,
            AutoSize = true,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
        };

        _lblCountryValue = new Label
        {
            Text = "",
            Top = 432,
            Height = 24,
            AutoSize = true,
            Font = new Font("Segoe UI", 11f),
        };

        _lblIpError = new Label
        {
            Text = "",
            Left = 24,
            Top = 458,
            Width = 372,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = InactiveColor,
            Font = new Font("Segoe UI", 8.5f),
        };

        Controls.AddRange(new Control[]
        {
            picIcon, lblTitle, lblSubtitle, _proxyField, _portField, _btnToggle, _lblStatus,
            divider, _lnkCheckIp, _lblIpValue, _lblCountryValue, _lblIpError
        });

        RefreshStatus();
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        // Some APIs (this one included) reject requests with no User-Agent header.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ProxySettler/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        return client;
    }

    private static Image? LoadEmbeddedIcon()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("ProxySettler.Assets.appicon.png");
        if (stream is null)
        {
            return null;
        }

        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }

    private void RefreshStatus()
    {
        var (enabled, server) = ProxyManager.GetStatus();

        // The registry keeps the last-used ProxyServer value even after disconnecting,
        // so this doubles as "remember what I typed last time".
        if (!string.IsNullOrWhiteSpace(server))
        {
            var parts = server.Split(':');
            if (parts.Length == 2)
            {
                _proxyField.TextBox.Text = parts[0];
                _portField.TextBox.Text = parts[1];
            }
        }

        if (enabled)
        {
            _lblStatus.Text = $"Active - {server}";
            _lblStatus.ForeColor = ActiveColor;
            _btnToggle.Text = "Disconnect";
            _btnToggle.Variant = ButtonVariant.Outline;
        }
        else
        {
            _lblStatus.Text = "Inactive";
            _lblStatus.ForeColor = InactiveColor;
            _btnToggle.Text = "Connect";
            _btnToggle.Variant = ButtonVariant.Solid;
        }

        _btnToggle.Invalidate();
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

        var proxy = _proxyField.TextBox.Text.Trim();
        if (string.IsNullOrEmpty(proxy))
        {
            MessageBox.Show(this, "Please enter a proxy address.", "Proxy Settler",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(_portField.TextBox.Text.Trim(), out var port) || port is < 1 or > 65535)
        {
            MessageBox.Show(this, "Please enter a valid port (1-65535).", "Proxy Settler",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            ProxyManager.Connect(proxy, port);
            RefreshStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to set the proxy: {ex.Message}", "Proxy Settler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CheckIpAsync()
    {
        _lnkCheckIp.Enabled = false;
        _lblIpError.Text = "";
        _lblIpValue.Text = "Checking...";
        _lblCountryValue.Text = "";
        CenterIpResult();

        try
        {
            using var response = await HttpClient.GetAsync(IpCheckUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<IpInfo>(json);

            _lblIpValue.Text = string.IsNullOrEmpty(info?.Ip) ? "N/A" : info.Ip;
            _lblCountryValue.Text = !string.IsNullOrEmpty(info?.Country)
                ? string.IsNullOrEmpty(info.CountryCode) ? $"  ({info.Country})" : $"  ({info.Country}, {info.CountryCode})"
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
            CenterIpResult();
            _lnkCheckIp.Enabled = true;
        }
    }

    private void CenterIpResult()
    {
        var combinedWidth = _lblIpValue.Width + _lblCountryValue.Width;
        var startX = (ClientSize.Width - combinedWidth) / 2;
        _lblIpValue.Left = startX;
        _lblCountryValue.Left = _lblIpValue.Right;
    }
}
