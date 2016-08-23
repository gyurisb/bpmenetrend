namespace BkvDbConvertGUI
{
    partial class MainForm
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
            this.startBtn = new System.Windows.Forms.Button();
            this.stopBtn = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.log = new System.Windows.Forms.Label();
            this.currentTaskLabel = new System.Windows.Forms.Label();
            this.edit = new System.Windows.Forms.Button();
            this.skipCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.selectedFile = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(46, 20);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 0;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // stopBtn
            // 
            this.stopBtn.BackColor = System.Drawing.Color.Red;
            this.stopBtn.Location = new System.Drawing.Point(144, 20);
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(22, 23);
            this.stopBtn.TabIndex = 1;
            this.stopBtn.UseVisualStyleBackColor = false;
            this.stopBtn.Visible = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 63);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(486, 23);
            this.progressBar.TabIndex = 2;
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(12, 106);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(125, 13);
            this.log.TabIndex = 4;
            this.log.Text = "The program has started.";
            // 
            // currentTaskLabel
            // 
            this.currentTaskLabel.AutoSize = true;
            this.currentTaskLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentTaskLabel.Location = new System.Drawing.Point(12, 93);
            this.currentTaskLabel.Name = "currentTaskLabel";
            this.currentTaskLabel.Size = new System.Drawing.Size(63, 13);
            this.currentTaskLabel.TabIndex = 5;
            this.currentTaskLabel.Text = "Welcome!";
            // 
            // edit
            // 
            this.edit.Location = new System.Drawing.Point(423, 12);
            this.edit.Name = "edit";
            this.edit.Size = new System.Drawing.Size(75, 23);
            this.edit.TabIndex = 6;
            this.edit.Text = "edit links";
            this.edit.UseVisualStyleBackColor = true;
            this.edit.Click += new System.EventHandler(this.edit_Click);
            // 
            // skipCheckBox
            // 
            this.skipCheckBox.AutoSize = true;
            this.skipCheckBox.Location = new System.Drawing.Point(309, 16);
            this.skipCheckBox.Name = "skipCheckBox";
            this.skipCheckBox.Size = new System.Drawing.Size(108, 17);
            this.skipCheckBox.TabIndex = 7;
            this.skipCheckBox.Text = "skip downloading";
            this.skipCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(306, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Selected:";
            // 
            // selectedFile
            // 
            this.selectedFile.AutoSize = true;
            this.selectedFile.Location = new System.Drawing.Point(364, 38);
            this.selectedFile.Name = "selectedFile";
            this.selectedFile.Size = new System.Drawing.Size(31, 13);
            this.selectedFile.TabIndex = 9;
            this.selectedFile.Text = "none";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 334);
            this.Controls.Add(this.selectedFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.skipCheckBox);
            this.Controls.Add(this.edit);
            this.Controls.Add(this.currentTaskLabel);
            this.Controls.Add(this.log);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.stopBtn);
            this.Controls.Add(this.startBtn);
            this.Name = "MainForm";
            this.Text = "Database creater";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Button stopBtn;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label log;
        private System.Windows.Forms.Label currentTaskLabel;
        private System.Windows.Forms.Button edit;
        private System.Windows.Forms.CheckBox skipCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label selectedFile;
    }
}

