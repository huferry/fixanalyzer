namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    internal class TagDrawer
    {
        private Pen GetPen()
        {
            return new Pen(Color.Black);
        }

        private Font GetFont()
        {
            return new Font(FontFamily.GenericSansSerif, 8);
        }

        private string GetValue(FixTag tag)
        {
            string value = tag.ValueMeaning == ""
                ? tag.Value
                : tag.Value + " (" + tag.ValueMeaning + ")";
            return string.Format("{0}.{1}={2}", tag.Tag, tag.Definition.FieldName, value);
        }

        private Size GetSize(FixTag tag, int height, Graphics g, Font font)
        {
            SizeF textSize = g.MeasureString(GetValue(tag), font);
            return new Size(Convert.ToInt32(textSize.Width + 2*Math.Sqrt(height)) + 10, height);
        }

        public Rectangle Draw(FixTag tag, Graphics graph, Point startPoint, int height, Color color)
        {
            return Draw(tag, graph, startPoint, height, color, true);
        }

        public Rectangle Draw(FixTag tag, Graphics graph, Point startPoint, int height, Color color, bool alignLeft)
        {
            Point corner = new Point(startPoint.X, startPoint.Y);
            Pen pen = GetPen();
            Font font = GetFont();
            string value = GetValue(tag);
            SizeF textSize = graph.MeasureString(value, font);

            if (!alignLeft)
            {
                corner.X -= GetSize(tag, height, graph, font).Width;
            }

            DrawBorder(
                graph,
                corner,
                GetSize(tag, height, graph, font),
                color
                );

            int dx = Convert.ToInt32(Math.Sqrt(height));
            int dy = Convert.ToInt32((height - textSize.Height)/2);

            graph.DrawString(value, font, pen.Brush, new PointF(corner.X + dx + 6, corner.Y + dy));

            return new Rectangle(corner, GetSize(tag, height, graph, font));
        }

        public IDictionary<FixTag, Rectangle> Draw(FixMessage message, Graphics g, Rectangle rect,
            int height, FixTag activeTag)
        {
            return Draw(message, g, rect, height, activeTag, null);
        }

        public IDictionary<FixTag, Rectangle> Draw(FixMessage message, Graphics g, Rectangle rect,
            int height, FixTag activeTag, IEnumerable<FixTag> marked)
        {
            Dictionary<FixTag, Rectangle> map = new Dictionary<FixTag, Rectangle>();

            Font font = GetFont();

            Point p = new Point(rect.Left, rect.Top);

            foreach (FixTag tag in message.Tags)
            {
                Size size = GetSize(tag, height, g, font);

                if (p.X + size.Width > rect.Width)
                {
                    p.X = rect.Left;
                    p.Y += height + 4;
                }

                FixTag[] fixTags = marked == null ? new FixTag[0] : marked.ToArray();
                Color color = fixTags.Contains(tag)
                    ? Color.GreenYellow
                    : (activeTag == tag ? Color.Magenta : Color.SkyBlue);

                Rectangle r = Draw(tag, g, p, height, color);
                p.X += size.Width + 2;

                map.Add(tag, r);
            }
            return map;
        }

        private void DrawBorder(Graphics g, Point topLeft, Size size, Color background)
        {
            List<Point> hexagon = new List<Point>();

            int dx = Convert.ToInt32(Math.Sqrt(size.Width));
            int dy = size.Height/2;

            hexagon.Add(new Point(topLeft.X + dx, topLeft.Y));
            hexagon.Add(new Point(topLeft.X + size.Width - dx, topLeft.Y));
            hexagon.Add(new Point(topLeft.X + size.Width, topLeft.Y + dy));
            hexagon.Add(new Point(topLeft.X + size.Width - dx, topLeft.Y + size.Height));
            hexagon.Add(new Point(topLeft.X + dx, topLeft.Y + size.Height));
            hexagon.Add(new Point(topLeft.X, topLeft.Y + dy));

            Pen pen = new Pen(Color.Black);
            Brush brush = new SolidBrush(background);
            g.FillPolygon(brush, hexagon.ToArray());
            g.DrawPolygon(pen, hexagon.ToArray());
        }
    }
}