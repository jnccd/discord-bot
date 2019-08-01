namespace Hoppy_Fields
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.BLearn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BRun = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // BLearn
            // 
            this.BLearn.Location = new System.Drawing.Point(12, 429);
            this.BLearn.Name = "BLearn";
            this.BLearn.Size = new System.Drawing.Size(75, 23);
            this.BLearn.TabIndex = 0;
            this.BLearn.Text = "Learn";
            this.BLearn.UseVisualStyleBackColor = true;
            this.BLearn.Click += new System.EventHandler(this.BLearn_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(795, 410);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseDown);
            // 
            // BRun
            // 
            this.BRun.Location = new System.Drawing.Point(93, 429);
            this.BRun.Name = "BRun";
            this.BRun.Size = new System.Drawing.Size(75, 23);
            this.BRun.TabIndex = 2;
            this.BRun.Text = "Run";
            this.BRun.UseVisualStyleBackColor = true;
            this.BRun.Click += new System.EventHandler(this.BRun_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 464);
            this.Controls.Add(this.BRun);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.BLearn);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BLearn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button BRun;
    }
}

