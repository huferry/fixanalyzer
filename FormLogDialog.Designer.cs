namespace FixAnalyzer
{
    partial class FormLogDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioFindLogFile = new System.Windows.Forms.RadioButton();
            this.radioLogFile = new System.Windows.Forms.RadioButton();
            this.radioCMF = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxEnv = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbLast = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbGateway = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioFindLogFile);
            this.groupBox1.Controls.Add(this.radioLogFile);
            this.groupBox1.Controls.Add(this.radioCMF);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(268, 102);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // radioFindLogFile
            // 
            this.radioFindLogFile.AutoSize = true;
            this.radioFindLogFile.Enabled = false;
            this.radioFindLogFile.Location = new System.Drawing.Point(29, 65);
            this.radioFindLogFile.Name = "radioFindLogFile";
            this.radioFindLogFile.Size = new System.Drawing.Size(92, 17);
            this.radioFindLogFile.TabIndex = 2;
            this.radioFindLogFile.Text = "Search log file";
            this.radioFindLogFile.UseVisualStyleBackColor = true;
            // 
            // radioLogFile
            // 
            this.radioLogFile.AutoSize = true;
            this.radioLogFile.Checked = true;
            this.radioLogFile.Location = new System.Drawing.Point(29, 42);
            this.radioLogFile.Name = "radioLogFile";
            this.radioLogFile.Size = new System.Drawing.Size(153, 17);
            this.radioLogFile.TabIndex = 1;
            this.radioLogFile.TabStop = true;
            this.radioLogFile.Text = "Fintegrator/RetailFix log file";
            this.radioLogFile.UseVisualStyleBackColor = true;
            // 
            // radioCMF
            // 
            this.radioCMF.AutoSize = true;
            this.radioCMF.Location = new System.Drawing.Point(29, 19);
            this.radioCMF.Name = "radioCMF";
            this.radioCMF.Size = new System.Drawing.Size(98, 17);
            this.radioCMF.TabIndex = 0;
            this.radioCMF.Text = "Oracle CMF log";
            this.radioCMF.UseVisualStyleBackColor = true;
            this.radioCMF.CheckedChanged += new System.EventHandler(this.radioCMF_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxEnv);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cbLast);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbGateway);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 121);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(269, 132);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CMF Log";
            // 
            // comboBoxEnv
            // 
            this.comboBoxEnv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEnv.FormattingEnabled = true;
            this.comboBoxEnv.Location = new System.Drawing.Point(81, 25);
            this.comboBoxEnv.Name = "comboBoxEnv";
            this.comboBoxEnv.Size = new System.Drawing.Size(164, 21);
            this.comboBoxEnv.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Environment";
            // 
            // cbLast
            // 
            this.cbLast.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLast.FormattingEnabled = true;
            this.cbLast.Location = new System.Drawing.Point(81, 91);
            this.cbLast.Name = "cbLast";
            this.cbLast.Size = new System.Drawing.Size(164, 21);
            this.cbLast.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "For the last";
            // 
            // cbGateway
            // 
            this.cbGateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGateway.FormattingEnabled = true;
            this.cbGateway.Location = new System.Drawing.Point(81, 56);
            this.cbGateway.Name = "cbGateway";
            this.cbGateway.Size = new System.Drawing.Size(164, 21);
            this.cbGateway.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Gateway";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(125, 259);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(206, 259);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "*.log";
            this.openFileDialog1.FileName = "*.log;*.txt";
            this.openFileDialog1.Filter = "(*.log;*.txt)|Log files";
            this.openFileDialog1.Title = "Open Log File";
            // 
            // FormLogDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 294);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLogDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose Log Source";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioLogFile;
        private System.Windows.Forms.RadioButton radioCMF;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbGateway;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbLast;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RadioButton radioFindLogFile;
        private System.Windows.Forms.ComboBox comboBoxEnv;
        private System.Windows.Forms.Label label3;

    }
}