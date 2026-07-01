using System.Drawing.Drawing2D;

namespace ProxySettler;

/// <summary>A rounded, bordered text field with a small floating caption above the value,
/// matching Material-style "outlined" inputs. Border turns blue while focused.</summary>
internal sealed class OutlinedField : Panel
{
    private const int CornerRadius = 12;

    private static readonly Color TextColor = ColorTranslator.FromHtml("#0E0E10");
    private static readonly Color CaptionColor = Color.FromArgb(255, 145, 145, 150);
    private static readonly Color BorderColor = Color.FromArgb(255, 214, 214, 219);
    private static readonly Color FocusColor = ColorTranslator.FromHtml("#2F54EB");

    public TextBox TextBox { get; }

    public OutlinedField(string caption, bool numericOnly = false)
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        // Inset docked children by 2px so they don't paint over the border drawn at the panel's edge.
        Padding = new Padding(2);
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

        var lblCaption = new Label
        {
            Text = caption,
            Dock = DockStyle.Top,
            Height = 20,
            Padding = new Padding(12, 6, 0, 0),
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = CaptionColor,
        };

        TextBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11f),
            ForeColor = TextColor,
        };

        if (numericOnly)
        {
            TextBox.MaxLength = 5;
            TextBox.KeyPress += (_, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
        }

        var textHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 0, 12, 8),
            BackColor = Color.White,
        };
        textHost.Controls.Add(TextBox);

        Controls.Add(textHost);
        Controls.Add(lblCaption);

        TextBox.Enter += (_, _) => Invalidate();
        TextBox.Leave += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var focused = TextBox.Focused;
        var borderColor = focused ? FocusColor : BorderColor;
        using var path = RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        using var pen = new Pen(borderColor, focused ? 1.6f : 1f);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
