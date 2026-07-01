using System.Drawing.Drawing2D;

namespace ProxySettler;

internal enum ButtonVariant
{
    /// <summary>Filled with <see cref="RoundedButton.AccentColor"/>, white text.</summary>
    Solid,

    /// <summary>White fill, gray border, dark text.</summary>
    Outline
}

/// <summary>A flat, 16px-rounded button drawn against a solid parent background.</summary>
internal sealed class RoundedButton : Button
{
    private const int CornerRadius = 16;

    private static readonly Color TextColor = ColorTranslator.FromHtml("#0E0E10");
    private static readonly Color OutlineBorderColor = Color.FromArgb(255, 214, 214, 219);
    private static readonly Color OutlineHoverFill = Color.FromArgb(255, 246, 246, 248);
    private static readonly Color DisabledColor = Color.FromArgb(255, 200, 200, 204);

    private bool _hovering;
    private bool _pressed;

    public ButtonVariant Variant { get; set; } = ButtonVariant.Solid;
    public Color AccentColor { get; set; } = ColorTranslator.FromHtml("#2F54EB");

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.White);

        using var path = RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);

        Color fill, border, text;
        if (Variant == ButtonVariant.Solid)
        {
            text = Color.White;
            border = Enabled ? Shade(AccentColor, _pressed ? -0.18f : _hovering ? -0.08f : 0f) : DisabledColor;
            fill = border;
        }
        else
        {
            text = Enabled ? TextColor : DisabledColor;
            border = Enabled ? OutlineBorderColor : DisabledColor;
            fill = _pressed ? Color.FromArgb(255, 238, 238, 241) : _hovering ? OutlineHoverFill : Color.White;
        }

        using (var brush = new SolidBrush(fill))
        {
            g.FillPath(brush, path);
        }

        using (var pen = new Pen(border, 1f))
        {
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static Color Shade(Color color, float amount)
    {
        int Adjust(int channel) => Math.Clamp((int)(channel + amount * 255), 0, 255);
        return Color.FromArgb(color.A, Adjust(color.R), Adjust(color.G), Adjust(color.B));
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovering = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovering = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _pressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _pressed = false;
        Invalidate();
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
