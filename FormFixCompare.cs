using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FixAnalyzer
{
    public partial class FormFixCompare : Form
    {
        FixComparer comp = null;

        private int last_set = 1;

        public FormFixCompare()
        {
            InitializeComponent();
            textBox1.Text = "8=FIX.4.235=D34=049=BNKPETST56=PEBNKTST57=EUROPORT11=299194530711=0346091863=021=2100=N55=HPQ48=US428236103322=4205=00207=N54=1114=N60=20100903-13:38:4838=50040=115=USD59=077=O203=0204=0";
            textBox2.Text = "8=FIX.4.235=D34=049=ALXPETST56=PEALXTST11=423500571109=42350057281021=2100=TO55=CA14012848=CA42979J107522=4167=CS54=160=20100812-14:07:4538=10040=115=CAD59=0204=0";
            button3_Click(null, null);
        }

        public void SetMessage(FixMessage msg)
        {
            last_set = last_set == 0 ? 1 : 0;

            switch (last_set)
            {
                case 0:
                    textBox1.Text = msg.Raw;
                    break;
                case 1:
                    textBox2.Text = msg.Raw;
                    button3_Click(null, null);
                    break;
            }
        }

        public void SetMessage(FixMessage msg1, FixMessage msg2)
        {
            last_set = 1;

            textBox1.Text = msg1.Raw;
            textBox2.Text = msg2.Raw;
            button3_Click(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            comp = new FixComparer(textBox1.Text, textBox2.Text);
            pictureBox1.Refresh();
 
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int mid = pictureBox1.Width / 2;
            if (comp != null)
            {
                TagDrawer d = new TagDrawer();

                int i = 1;

                var tags2 = comp.Fix2.Tags.ToList();

                foreach (int tag_number in comp.GetAllFixTagNumbers())
                {
                    Color c = Color.LightBlue;

                    if (comp.Fix1[tag_number] != null)
                    {

                        var search = tags2.Where(x => x.Tag == tag_number);
                        FixTag tag2 = search.Count() == 0 ? null : search.First();
                        tags2.Remove(tag2);

                        if (tag2 == null)
                            c = Color.LightPink;
                        else
                        {
                            if (comp.Fix1[tag_number].Value != tag2.Value)
                                c = Color.LightSalmon;
                        }
                        d.Draw(comp.Fix1[tag_number], e.Graphics, new Point(mid - 10, i * 22), 18, c, false);
                    }
                    else
                        c = Color.LightPink;

                    if (comp.Fix2[tag_number] != null)
                    {                        
                        d.Draw(comp.Fix2[tag_number], e.Graphics, new Point(mid + 10, i * 22), 18, c);                        
                    }
                    i++;

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = Clipboard.GetText();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = Clipboard.GetText();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void FormFixCompare_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }
}
