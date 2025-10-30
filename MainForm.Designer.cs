using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PreviewUnblock
{
    /// <summary>
    /// Designer portion of the main form. Defines the UI layout and assigns
    /// control properties. Logic lives in MainForm.cs.
    /// </summary>
    partial class MainForm
    {
        private IContainer components = null!;

        // UI controls
        private Label labelAppName;
        private TextBox textBoxFolder;
        private Button buttonChangeFolder;
        private RichTextBox richTextBoxLog;
        private Label labelStatus;
        private CheckBox checkBoxAgree;
        private Button buttonStartStop;
        private Label labelWarning;

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
        ///  Required method for Designer support – do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            this.labelAppName = new Label();
            this.textBoxFolder = new TextBox();
            this.buttonChangeFolder = new Button();
            this.richTextBoxLog = new RichTextBox();
            this.labelStatus = new Label();
            this.checkBoxAgree = new CheckBox();
            this.buttonStartStop = new Button();
            this.labelWarning = new Label();
            this.SuspendLayout();

            // labelAppName
            this.labelAppName.AutoSize = true;
            this.labelAppName.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.labelAppName.Location = new Point(12, 9);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new Size(180, 30);
            this.labelAppName.TabIndex = 0;
            this.labelAppName.Text = "PreviewUnblock";

            // textBoxFolder
            this.textBoxFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxFolder.Location = new Point(12, 49);
            this.textBoxFolder.Name = "textBoxFolder";
            this.textBoxFolder.ReadOnly = true;
            this.textBoxFolder.Size = new Size(480, 23);
            this.textBoxFolder.TabIndex = 1;

            // buttonChangeFolder
            this.buttonChangeFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.buttonChangeFolder.Location = new Point(498, 48);
            this.buttonChangeFolder.Name = "buttonChangeFolder";
            this.buttonChangeFolder.Size = new Size(90, 25);
            this.buttonChangeFolder.TabIndex = 2;
            this.buttonChangeFolder.Text = "Change...";
            this.buttonChangeFolder.UseVisualStyleBackColor = true;
            this.buttonChangeFolder.Click += new System.EventHandler(this.buttonChangeFolder_Click);

            // labelWarning
            this.labelWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.labelWarning.ForeColor = Color.Maroon;
            this.labelWarning.Location = new Point(12, 78);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new Size(576, 40);
            this.labelWarning.TabIndex = 3;
            this.labelWarning.Text = "Warning: Removing Windows' security flag can expose you to harmful files.\nOnly proceed if you trust the files in this folder.";

            // richTextBoxLog
            this.richTextBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.richTextBoxLog.Location = new Point(12, 121);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new Size(576, 200);
            this.richTextBoxLog.TabIndex = 4;
            this.richTextBoxLog.Text = string.Empty;
            this.richTextBoxLog.WordWrap = false;

            // checkBoxAgree
            this.checkBoxAgree.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.checkBoxAgree.AutoSize = true;
            this.checkBoxAgree.Location = new Point(12, 333);
            this.checkBoxAgree.Name = "checkBoxAgree";
            this.checkBoxAgree.Size = new Size(394, 19);
            this.checkBoxAgree.TabIndex = 5;
            this.checkBoxAgree.Text = "I understand the risk and take responsibility for files opened on this computer.";
            this.checkBoxAgree.UseVisualStyleBackColor = true;
            this.checkBoxAgree.CheckedChanged += new System.EventHandler(this.checkBoxAgree_CheckedChanged);

            // buttonStartStop
            this.buttonStartStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonStartStop.Enabled = false;
            this.buttonStartStop.Location = new Point(498, 328);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new Size(90, 29);
            this.buttonStartStop.TabIndex = 6;
            this.buttonStartStop.Text = "Start";
            this.buttonStartStop.UseVisualStyleBackColor = true;
            this.buttonStartStop.Click += new System.EventHandler(this.buttonStartStop_Click);

            // labelStatus
            this.labelStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = Color.DarkGreen;
            this.labelStatus.Location = new Point(12, 357);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new Size(0, 15);
            this.labelStatus.TabIndex = 7;

            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 380);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonStartStop);
            this.Controls.Add(this.checkBoxAgree);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.labelWarning);
            this.Controls.Add(this.buttonChangeFolder);
            this.Controls.Add(this.textBoxFolder);
            this.Controls.Add(this.labelAppName);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(420, 320);
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "PreviewUnblock";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}