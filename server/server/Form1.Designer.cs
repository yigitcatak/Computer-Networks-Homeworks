namespace server
{
    partial class Form1
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
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_start = new System.Windows.Forms.Button();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_num = new System.Windows.Forms.TextBox();
            this.buttonStartGame = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(32, 39);
            this.textBox_port.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(183, 22);
            this.textBox_port.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            // 
            // button_start
            // 
            this.button_start.Location = new System.Drawing.Point(443, 37);
            this.button_start.Margin = new System.Windows.Forms.Padding(2);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(139, 27);
            this.button_start.TabIndex = 2;
            this.button_start.Text = "Start Server";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_listen_Click);
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(32, 116);
            this.logs.Margin = new System.Windows.Forms.Padding(2);
            this.logs.Name = "logs";
            this.logs.ReadOnly = true;
            this.logs.Size = new System.Drawing.Size(550, 335);
            this.logs.TabIndex = 7;
            this.logs.TabStop = false;
            this.logs.Text = "";
            this.logs.TextChanged += new System.EventHandler(this.logs_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(229, 20);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Number of Questions:";
            // 
            // textBox_num
            // 
            this.textBox_num.Location = new System.Drawing.Point(232, 39);
            this.textBox_num.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_num.Name = "textBox_num";
            this.textBox_num.Size = new System.Drawing.Size(183, 22);
            this.textBox_num.TabIndex = 1;
            // 
            // buttonStartGame
            // 
            this.buttonStartGame.Location = new System.Drawing.Point(443, 68);
            this.buttonStartGame.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStartGame.Name = "buttonStartGame";
            this.buttonStartGame.Size = new System.Drawing.Size(139, 27);
            this.buttonStartGame.TabIndex = 10;
            this.buttonStartGame.Text = "Start Game";
            this.buttonStartGame.UseVisualStyleBackColor = true;
            this.buttonStartGame.Click += new System.EventHandler(this.buttonStartGame_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 507);
            this.Controls.Add(this.buttonStartGame);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_num);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_port);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_num;
        private System.Windows.Forms.Button buttonStartGame;
    }
}

