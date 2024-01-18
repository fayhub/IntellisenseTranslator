namespace IntellisenseTranslator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            label2 = new Label();
            folderBrowserDialog1 = new FolderBrowserDialog();
            txtTranslatorFolder = new TextBox();
            butOpenFolder = new Button();
            statusStrip1 = new StatusStrip();
            lblOfflineDict = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            lblStatus = new ToolStripStatusLabel();
            chkTransXmlDict = new CheckBox();
            butUpdateDict = new Button();
            chkReplace = new CheckBox();
            btuStartTranslator = new Button();
            rtbLog = new RichTextBox();
            rtbResult = new RichTextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 24);
            label2.Name = "label2";
            label2.Size = new Size(104, 17);
            label2.TabIndex = 1;
            label2.Text = "需要翻译的路径：";
            // 
            // txtTranslatorFolder
            // 
            txtTranslatorFolder.AllowDrop = true;
            txtTranslatorFolder.Location = new Point(122, 21);
            txtTranslatorFolder.Name = "txtTranslatorFolder";
            txtTranslatorFolder.Size = new Size(431, 23);
            txtTranslatorFolder.TabIndex = 2;
            txtTranslatorFolder.DragDrop += txtTranslatorFolder_DragDrop;
            txtTranslatorFolder.DragEnter += txtTranslatorFolder_DragEnter;
            txtTranslatorFolder.KeyPress += txtTranslatorFolder_KeyPress;
            // 
            // butOpenFolder
            // 
            butOpenFolder.Location = new Point(559, 21);
            butOpenFolder.Name = "butOpenFolder";
            butOpenFolder.Size = new Size(49, 23);
            butOpenFolder.TabIndex = 3;
            butOpenFolder.Text = "选择";
            butOpenFolder.UseVisualStyleBackColor = true;
            butOpenFolder.Click += butOpenFolder_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.AutoSize = false;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblOfflineDict, toolStripStatusLabel1, lblStatus });
            statusStrip1.Location = new Point(0, 731);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(671, 25);
            statusStrip1.TabIndex = 20;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblOfflineDict
            // 
            lblOfflineDict.Name = "lblOfflineDict";
            lblOfflineDict.Size = new Size(104, 20);
            lblOfflineDict.Text = "本地离线字典数：";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(532, 20);
            toolStripStatusLabel1.Spring = true;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(20, 20);
            lblStatus.Text = "卐";
            lblStatus.Paint += lblStatus_Paint;
            // 
            // chkTransXmlDict
            // 
            chkTransXmlDict.AutoSize = true;
            chkTransXmlDict.Checked = true;
            chkTransXmlDict.CheckState = CheckState.Checked;
            chkTransXmlDict.Location = new Point(13, 64);
            chkTransXmlDict.Name = "chkTransXmlDict";
            chkTransXmlDict.Size = new Size(143, 21);
            chkTransXmlDict.TabIndex = 7;
            chkTransXmlDict.Text = "使用字典翻译xml文件";
            chkTransXmlDict.UseVisualStyleBackColor = true;
            // 
            // butUpdateDict
            // 
            butUpdateDict.Location = new Point(372, 62);
            butUpdateDict.Name = "butUpdateDict";
            butUpdateDict.Size = new Size(75, 23);
            butUpdateDict.TabIndex = 8;
            butUpdateDict.Text = "更新字典文件";
            butUpdateDict.UseVisualStyleBackColor = true;
            butUpdateDict.Click += butUpdateDict_Click;
            // 
            // chkReplace
            // 
            chkReplace.AutoSize = true;
            chkReplace.Checked = true;
            chkReplace.CheckState = CheckState.Checked;
            chkReplace.Location = new Point(175, 64);
            chkReplace.Name = "chkReplace";
            chkReplace.Size = new Size(159, 21);
            chkReplace.TabIndex = 9;
            chkReplace.Text = "翻译后自动替换目标文件";
            chkReplace.UseVisualStyleBackColor = true;
            // 
            // btuStartTranslator
            // 
            btuStartTranslator.Location = new Point(478, 62);
            btuStartTranslator.Name = "btuStartTranslator";
            btuStartTranslator.Size = new Size(75, 23);
            btuStartTranslator.TabIndex = 10;
            btuStartTranslator.Text = "开始翻译";
            btuStartTranslator.UseVisualStyleBackColor = true;
            btuStartTranslator.Click += btuStartTranslation_Click;
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Bottom;
            rtbLog.Location = new Point(0, 634);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(671, 97);
            rtbLog.TabIndex = 19;
            rtbLog.Text = "";
            // 
            // rtbResult
            // 
            rtbResult.Dock = DockStyle.Bottom;
            rtbResult.Location = new Point(0, 105);
            rtbResult.Name = "rtbResult";
            rtbResult.Size = new Size(671, 529);
            rtbResult.TabIndex = 18;
            rtbResult.Text = "";
            // 
            // timer1
            // 
            timer1.Tick += Timer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(671, 756);
            Controls.Add(label2);
            Controls.Add(btuStartTranslator);
            Controls.Add(chkReplace);
            Controls.Add(butUpdateDict);
            Controls.Add(chkTransXmlDict);
            Controls.Add(rtbResult);
            Controls.Add(rtbLog);
            Controls.Add(butOpenFolder);
            Controls.Add(txtTranslatorFolder);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Visual Studio 智能感知翻译工具";
            Load += MainForm_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private FolderBrowserDialog folderBrowserDialog1;
        private TextBox txtTranslatorFolder;
        private Button butOpenFolder;
        private RichTextBox rtbResult;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblOfflineDict;
        private CheckBox chkTransXmlDict;
        private Button butUpdateDict;
        private CheckBox chkReplace;
        private Button btuStartTranslator;
        private RichTextBox rtbLog;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer timer1;
    }
}
