namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;

    public partial class FormMain : Form, IFixMessageViewer
    {
        private readonly FormFixCompare formCompare = new FormFixCompare();
        private readonly List<FixTag> marked = new List<FixTag>();
        private FixMessage last;
        private FixTag selected;
        private IDictionary<FixTag, Rectangle> tagLocations;

        public FormMain()
        {
            InitializeComponent();
            ViewMessage(FixMessage.Test());
        }

        public void ViewMessage(FixMessage msg)
        {
            this.treeView1.Parent = this.tabControl1.SelectedTab;
            string fix = msg.Raw;
            this.textBoxFixMsgHidden.Text = fix;
            this.treeView1.Nodes.Clear();
            foreach (FixTag tag in msg.Tags)
            {
                string label = this.tabControl1.SelectedIndex == 0
                    ? tag.Tag.ToString(CultureInfo.InvariantCulture)
                    : tag.Definition.FieldName;
                TreeNode tagNode = this.treeView1.Nodes.Add(label);
                tagNode.Tag = tag;
                tagNode.Nodes.Add("Value = " + tag.Value);
                tagNode.Nodes.Add("Fieldname = " + tag.Definition.FieldName);
                if (tag.ValueMeaning.Length > 0)
                {
                    tagNode.Nodes.Add("Meaning = " + tag.ValueMeaning);
                }
            }

            if (this.last != msg)
            {
                this.last = msg;
                this.marked.Clear();
                this.selected = null;
            }
            MarkTags();
            this.pictureBoxFix.Refresh();
        }

        private void UpdateClipboard()
        {
            this.textBoxFixMsgHidden.Text = this.last.Raw;
            this.textBoxFixMsgHidden.SelectAll();
            this.textBoxFixMsgHidden.SelectionBackColor = Color.White;

            foreach (FixTag tag in this.last.Tags)
            {
                if ((tag == this.selected) || this.marked.Contains(tag))
                {
                    this.textBoxFixMsgHidden.SelectionStart = tag.Position;
                    this.textBoxFixMsgHidden.SelectionLength = tag.Tag.ToString(CultureInfo.InvariantCulture).Length +
                                                    tag.Value.Length + 1;
                    this.textBoxFixMsgHidden.SelectionBackColor = this.marked.Contains(tag) ? Color.GreenYellow : Color.Magenta;
                }
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FixMessage msg = new FixMessage(this.textBoxFixMsgInput.Text);
                ViewMessage(msg);
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FixMessage msg = new FixMessage(this.textBoxFixMsgInput.Text.Replace('|', (char) 1));
            ViewMessage(msg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBoxFixMsgInput.Clear();
            this.textBoxFixMsgInput.Paste();
            FixMessage msg = new FixMessage(this.textBoxFixMsgInput.Text.Replace('|', (char) 1));
            ViewMessage(msg);
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode tagNode = e.Node;

            while (tagNode.Level > 0)
            {
                tagNode = tagNode.Parent;
            }

            if (tagNode.Tag is FixTag)
            {
                UpdateTagInfo((FixTag) tagNode.Tag);
            }
        }

        private void UpdateTagInfo(FixTag tag)
        {
            this.textBoxTagInfo.Clear();
            if (tag == null) return;

            this.textBoxTagInfo.Text = string.Format("Tag {0}\r\n{1} ({2})\r\n\r\n{3}\r\n\r\n", tag.Tag,
                tag.Definition.FieldName, tag.Definition.DataType, tag.Definition.Description);

            if (tag.Definition.ValidValues.Count > 0)
            {
                string validValues = "Valid values: \r\n";
                tag.Definition.ValidValues.Keys.ToList().ForEach(key =>
                    validValues += string.Format("{0} ({1})\r\n", key, tag.Definition.ValidValues[key]));
                this.textBoxTagInfo.Text += validValues;
            }

            string usedin = string.Empty;
            tag.Definition.Messages.ToList().ForEach(m => usedin += string.Format(" {0}\r\n", m.Name));
            if (usedin.Length > 0)
            {
                this.textBoxTagInfo.Text += string.Format("\r\nUsed in messages:\r\n{0}", usedin);
            }

            this.selected = tag;
            this.pictureBoxFix.Refresh();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewMessage(this.last);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormLogDialog form = new FormLogDialog();

            if (form.ShowDialog() == DialogResult.OK)
            {
                FormLogFile logform = new FormLogFile(this, form.LogFile);
                logform.Show();
            }
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            TagDrawer d = new TagDrawer();
            this.tagLocations = d.Draw(this.last,
                e.Graphics,
                new Rectangle(0, 0, this.pictureBoxFix.Width, this.pictureBoxFix.Height),
                20,
                this.selected,
                this.marked);
            UpdateClipboard();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Clipboard.SetText(this.last.Raw);            
            this.textBoxFixMsgHidden.SelectAll();
            this.textBoxFixMsgHidden.Copy();
        }

        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            FixTag tag = GetFixTagFromGraph(e.Location);

            if (tag == null)
            {
                this.pictureBoxFix.Cursor = Cursors.Arrow;
                this.toolTipTags.Active = false;
            }
            else
            {
                this.pictureBoxFix.Cursor = Cursors.Hand;
                this.toolTipTags.Active = true;
                this.toolTipTags.SetToolTip(this.pictureBoxFix, tag.Definition.Description);
            }
        }

        private FixTag GetFixTagFromGraph(Point p)
        {
            if (this.tagLocations != null)
            {
                foreach (FixTag tag in this.tagLocations.Keys)
                {
                    if (this.tagLocations[tag].Contains(p))
                    {
                        return tag;
                    }
                }
            }
            return null;
        }

        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            FixTag tag = GetFixTagFromGraph(e.Location);
            if (tag == null)
            {
                this.selected = null;
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    SelectTag(tag);
                    if (this.marked.Contains(tag))
                    {
                        this.marked.Remove(tag);
                    }
                    else
                    {
                        this.marked.Add(tag);
                    }
                    this.selected = null;
                }

                if (e.Button == MouseButtons.Left)
                {
                    // control key is held down, copy the tag value to clipboard
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        Clipboard.SetText(tag.Value);
                    }
                    else
                    {
                        SelectTag(tag);
                        this.selected = tag;
                    }
                }
            }
            this.pictureBoxFix.Refresh();
        }

        private void SelectTag(FixTag tag)
        {
            foreach (TreeNode node in this.treeView1.Nodes)
            {
                if (node.Tag == tag)
                {
                    this.treeView1.SelectedNode = node;
                    node.ExpandAll();
                    UpdateTagInfo(tag);
                    return;
                }
            }
        }

        private void MarkTags()
        {
            this.marked.Clear();

            string[] keys = this.textMarking.Text.ToLowerInvariant()
                .Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<FixTag> tags = this.last.Tags
                .ToDictionary(t => t,
                    t => string.Format("{0} {1} {2} {3}", t.Tag, t.Definition.FieldName, t.Value, t.ValueMeaning))
                .Where(t => keys.Any(key => t.Value.ToLowerInvariant().Contains(key)))
                .Select(t => t.Key);

            this.marked.AddRange(tags);
        }

        private void buttonMark_Click(object sender, EventArgs e)
        {
            MarkTags();
            this.pictureBoxFix.Refresh();
        }

        private void textMarking_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonMark_Click(null, null);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.marked.Clear();
            this.pictureBoxFix.Refresh();
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            SetCompare(this.last);
        }

        private void buttonCopyTable_Click(object sender, EventArgs e)
        {
            TableViewer.CopyHtmlToClipboard(this.last);
        }

        #region IFixMessageViewer Members

        public void SetCompare(FixMessage message)
        {
            if (!this.formCompare.Visible)
                this.formCompare.Show();
            if (message != null)
                this.formCompare.SetMessage(message);
        }

        public void ViewCompare(FixMessage msg1, FixMessage msg2)
        {
            if (!this.formCompare.Visible)
                this.formCompare.Show();
            this.formCompare.SetMessage(msg1, msg2);
        }

        #endregion
    }
}