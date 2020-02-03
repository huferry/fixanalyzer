namespace FixAnalyzer
{
    using System.Windows.Forms;

    internal class TableViewer
    {
        private FixMessage message;

        public TableViewer(FixMessage message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            string tabel = "Tag\tName\tValue\tDescription\r\n";

            foreach (FixTag tag in this.message.Tags)
            {
                tabel += tag.Tag.ToString() + "\t";

                tabel += tag.Definition.FieldName + "\t";

                tabel += tag.Value + "\t";

                tabel += tag.ValueMeaning + "\t\r\n";
            }
            return tabel;
        }

        public string ToHtml()
        {
            string tabel = "<table cellpadding=2 cellspacing=0 border=1 style=\"background-color:#E0E0F0\">" +
                           "<th>Tag</th><th>FieldName</th><th>Value</th><th>Description</th>\r\n";

            int count = 0;

            foreach (FixTag tag in this.message.Tags)
            {
                string color = count++%2 == 0 ? "white" : "#F0F0F0";

                tabel += "<tr style=\"background-color:" + color + "\"><td>" + tag.Tag.ToString() + "</td>\r\n";

                tabel += "<td>" + tag.Definition.FieldName + "</td>\r\n";

                tabel += "<td>" + tag.Value + "</td>\r\n";

                tabel += "<td>" + tag.ValueMeaning + "</td></tr>\r\n";
            }
            return tabel + "</table>";
        }

        public void ToClipboard()
        {
            Clipboard.SetText(ToString());
        }

        public static void CopyToClipboard(FixMessage msg)
        {
            (new TableViewer(msg)).ToClipboard();
        }

        public static void CopyHtmlToClipboard(FixMessage msg)
        {
            HtmlFragment.CopyToClipboard((new TableViewer(msg)).ToHtml());
        }
    }
}