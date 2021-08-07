//C#

// Remember always to add the needed reference to the project

using Assistant;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace RazorEnhanced
{
    public class FormTemplate
    {
        private Form form;
        private Button btnSayName;
        private TextBox txtMessage;

        public FormTemplate() 
        {
            Log("Starting Script");
        }

        private void Log(object messageString)
        {
            Misc.SendMessage(messageString, 201);
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            this.form.Dispose();
            this.form = null;
        }

        private void OnResizeForm(object sender, EventArgs e)
        {
        }

        private void BtnSayName_Click(object sender, EventArgs e)
        {
            Misc.SendMessage("Hello, my name is " + Player.Name);
            Player.HeadMessage(33, "Hello, my name is " + Player.Name);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
            if (e.KeyCode == Keys.Enter)
            {
                Player.HeadMessage(44, txtMessage.Text);
            }
        }

        private void CreateForm()
        {
            this.form = new Form
            {
                Text = "Form Template - " + Player.Name,
                HelpButton = false,
                MinimizeBox = true,
                MaximizeBox = false,
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition = FormStartPosition.CenterScreen,
                Opacity = 100,
                TopMost = true,
                ActiveControl = null,
                Visible = true
            };
            this.form.FormClosed += new FormClosedEventHandler(this.OnFormClosed);
            this.form.Resize += new EventHandler(this.OnResizeForm);

            btnSayName = new Button
            {
                Size = new Size(80, 40),
                Location = new Point(30, 30),
                Text = "Say your name!",
            };
            btnSayName.Click += new EventHandler(BtnSayName_Click);


            txtMessage = new TextBox
            {
                Size = new Size(200, 20),
                Location = new Point(30, 80),
                Text = "Send this message pressing enter here"
            };
            txtMessage.KeyDown += new KeyEventHandler(TxtMessage_KeyDown);

            this.form.Controls.Add(btnSayName);
            this.form.Controls.Add(txtMessage);
            this.form.Show();
        }

        public void Run()
        {
            CreateForm();
            while (form != null)
            {
                Application.DoEvents();
            }
        }
    }
}
