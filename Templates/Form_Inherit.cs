//C#
using System;
using System.Windows.Forms;
using Assistant;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;


namespace RazorEnhanced
{
    public class Form_Inherit : Form
    {
        private Button btn2;
        private Button button3;
        private Button button4;

        public Form_Inherit()
        {
            InitializeComponent();
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form_Inherit());
        }

        private void InitializeComponent()
        {
            this.btn2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(24, 43);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(123, 38);
            this.btn2.TabIndex = 0;
            this.btn2.Text = "SendMessage";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(153, 43);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(123, 39);
            this.button3.TabIndex = 1;
            this.button3.Text = "My Name";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(282, 43);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(123, 39);
            this.button4.TabIndex = 2;
            this.button4.Text = "HeadMessage";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form_Inherit
            // 
            this.ClientSize = new System.Drawing.Size(679, 385);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btn2);
            this.Name = "Form_Inherit";
            this.ResumeLayout(false);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Misc.SendMessage("button2_Clicks");
        }

        private void button3_Click(object sender, EventArgs e)
        {
           Misc.SendMessage("Hello, my name is " + Player.Name);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Player.HeadMessage(22, "HeadMessage");
        }
    }
}