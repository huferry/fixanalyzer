namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public partial class FormLogDialog : Form
    {
        private static int lastSelectedEnv = Settings.Instance.LogDialog.LastEnvironment;
        private static int lastSelectedGat = Settings.Instance.LogDialog.LastGateway;
        private static int lastSelectedTime = Settings.Instance.LogDialog.LastTime;
        private FixLogFile fixlogfile;
        private readonly IDictionary<string, TimeSpan> timespan = new Dictionary<string, TimeSpan>();

        public FormLogDialog()
        {
            InitializeComponent();
            IEnumerable<Gateway> gateways = (IEnumerable<Gateway>) Enum.GetValues(typeof (Gateway));

            foreach (Gateway g in gateways.OrderBy(x => x.ToString()))
            {
                this.cbGateway.Items.Add(g);
            }
            this.cbGateway.SelectedIndex = Math.Max(lastSelectedGat, 0);

            this.timespan.Add("10 minutes", TimeSpan.FromMinutes(10));
            this.timespan.Add("30 minutes", TimeSpan.FromMinutes(30));
            this.timespan.Add("1 hour", TimeSpan.FromHours(1));
            this.timespan.Add("4 hours", TimeSpan.FromHours(4));
            this.timespan.Add("8 hours", TimeSpan.FromHours(8));
            this.timespan.Add("1 day", TimeSpan.FromDays(1));
            this.timespan.Add("2 days", TimeSpan.FromDays(2));
            this.timespan.Add("4 days", TimeSpan.FromDays(4));
            this.timespan.Add("1 week", TimeSpan.FromDays(7));
            this.timespan.Add("1 month", TimeSpan.FromDays(30));
            this.timespan.Add("3 months", TimeSpan.FromDays(93));
            this.timespan.Add("6 months", TimeSpan.FromDays(356/2));
            this.timespan.Add("1 year", TimeSpan.FromDays(356));

            foreach (string key in this.timespan.Keys)
                this.cbLast.Items.Add(key);
            this.cbLast.SelectedIndex = Math.Max(0, lastSelectedTime);

            this.comboBoxEnv.Items.AddRange(DbSettings.GetAll().ToArray());
            this.comboBoxEnv.SelectedIndex = Math.Max(lastSelectedEnv, 0);
        }

        public FixLogFile LogFile
        {
            get { return this.fixlogfile; }
        }

        private void radioCMF_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox2.Enabled = this.radioCMF.Checked;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.radioCMF.Checked)
            {
                Gateway gw = (Gateway) this.cbGateway.SelectedItem;
                TimeSpan span = this.timespan[this.cbLast.Text];
                DbSettings setting = (DbSettings) this.comboBoxEnv.SelectedItem;
                lastSelectedEnv = this.comboBoxEnv.SelectedIndex;
                lastSelectedGat = this.cbGateway.SelectedIndex;
                lastSelectedTime = this.cbLast.SelectedIndex;
                this.fixlogfile = FixLogFile.FromDatabase(gw, setting, span);
            }
            else
            {
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.fixlogfile = FixLogFile.FromCmfLog(this.openFileDialog1.FileName);
                }
                else
                    DialogResult = DialogResult.Cancel;
            }

            Settings.Instance.LogDialog.LastEnvironment = lastSelectedEnv;
            Settings.Instance.LogDialog.LastGateway = lastSelectedGat;
            Settings.Instance.LogDialog.LastTime = lastSelectedTime;
        }
    }
}