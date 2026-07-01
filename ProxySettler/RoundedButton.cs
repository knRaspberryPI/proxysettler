using System.Drawing.Drawing2D;

namespace ProxySettler;

/// <summary>A flat, 16px-rounded button drawn against a solid parent background.</summary>
internal sealed class RoundedButton : Button
{
    private const int CornerRadius = 16;

    private static readonly Color NormalColor = ColorTranslator.FromHtml("#0E0E10");
    private static readonly Color HoverColor = Color.FromArgb(255, 40, 40, 44);
    private static readonly Color PressedColor = Color.Black;
    private static readonly Color DisabledColor = Color.FromArgb(255, 176, 176, 179);

    private Color _fillColor = NormalColor;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.White);

        var fillColor = Enabled ? _fillColor : DisabledColor;
        using var path = RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        using var brush = new SolidBrush(fillColor);
        g.FillPath(brush, path);

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _fillColor = HoverColor;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _fillColor = NormalColor;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _fillColor = PressedColor;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _fillColor = ClientRectangle.Contains(PointToClient(MousePosition)) ? HoverColor : NormalColor;
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
