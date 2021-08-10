//C#
// Magellano Navigation Script v1.0
// SimonSoft 2021
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RazorEnhanced;

namespace Scripts
{
    class Magellano : Form
    {
        private PictureBox cmdR;
        private PictureBox cmdBTM_R;
        private PictureBox cmdBTM;
        private PictureBox cmdBTM_L;
        private PictureBox cmdL;
        private PictureBox cmdUP_L;
        private PictureBox cmdUP_R;
        private PictureBox cmdStop;
        private PictureBox cmdTurnLeft;
        private PictureBox cmdTurnRight;
        private RadioButton speedNormal;
        private RadioButton speedSlow;
        private RadioButton speedOne;
        private PictureBox cmdUP;
        private PictureBox cmdAnchor;

        private enum Orientation
        {
            NORTH,
            EAST,
            SOUTH,
            WEST,
            ERROR
        }

        // Do not change enum elements position
        private enum Commands
        {
            STOP,
            RAISE_ANCHOR,
            DROP_ANCHOR,
            TURN_RIGHT,
            TURN_LEFT,
            TURN_AROUND,
            FORWARD,
            FORWARD_RIGHT,
            RIGHT,
            BACK_RIGHT,
            BACK,
            BACK_LEFT,
            LEFT,
            FORWARD_LEFT,
        }

        private Point3D playerPrevPosition = new Point3D();
        private int tillerManSerial = 0;
        private PictureBox cmd_TurnAround;
        private System.Windows.Forms.Timer timerCheckCollision;
        private IContainer components;
        private CheckBox chkCollision;
        private PictureBox imgRadar;
        private bool anchorDropped = false;

        public Magellano()
        {
            InitializeComponent();
            LoadImages();
            this.Text = this.Text + " - " + Player.Name;
        }

        public void Run()
        {

            //this.Icon = System.Drawing.Icon.FromHandle(Items.GetImage(0x1598, 0).GetHicon());
            this.Icon = System.Drawing.Icon.FromHandle(Ultima.Gumps.GetGump(0x1598).GetHicon());
            Application.EnableVisualStyles();
            Application.Run(this); // This is blocking. Will return only when form is closed

            // NUmeri 0x58f
            // ok 0x81a
            // Radio button 0x7745  / 0x16C3
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdR = new System.Windows.Forms.PictureBox();
            this.cmdBTM_R = new System.Windows.Forms.PictureBox();
            this.cmdBTM = new System.Windows.Forms.PictureBox();
            this.cmdBTM_L = new System.Windows.Forms.PictureBox();
            this.cmdL = new System.Windows.Forms.PictureBox();
            this.cmdUP_L = new System.Windows.Forms.PictureBox();
            this.cmdUP_R = new System.Windows.Forms.PictureBox();
            this.cmdUP = new System.Windows.Forms.PictureBox();
            this.cmdStop = new System.Windows.Forms.PictureBox();
            this.cmdTurnLeft = new System.Windows.Forms.PictureBox();
            this.cmdTurnRight = new System.Windows.Forms.PictureBox();
            this.speedNormal = new System.Windows.Forms.RadioButton();
            this.speedSlow = new System.Windows.Forms.RadioButton();
            this.speedOne = new System.Windows.Forms.RadioButton();
            this.cmdAnchor = new System.Windows.Forms.PictureBox();
            this.cmd_TurnAround = new System.Windows.Forms.PictureBox();
            this.timerCheckCollision = new System.Windows.Forms.Timer(this.components);
            this.chkCollision = new System.Windows.Forms.CheckBox();
            this.imgRadar = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.cmdR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM_L)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP_L)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdTurnLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdTurnRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdAnchor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmd_TurnAround)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgRadar)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdR
            // 
            this.cmdR.BackColor = System.Drawing.Color.Transparent;
            this.cmdR.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdR.Location = new System.Drawing.Point(176, 110);
            this.cmdR.Name = "cmdR";
            this.cmdR.Size = new System.Drawing.Size(50, 50);
            this.cmdR.TabIndex = 16;
            this.cmdR.TabStop = false;
            this.cmdR.Click += new System.EventHandler(this.cmdR_Click);
            // 
            // cmdBTM_R
            // 
            this.cmdBTM_R.BackColor = System.Drawing.Color.Transparent;
            this.cmdBTM_R.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdBTM_R.Location = new System.Drawing.Point(176, 166);
            this.cmdBTM_R.Name = "cmdBTM_R";
            this.cmdBTM_R.Size = new System.Drawing.Size(50, 50);
            this.cmdBTM_R.TabIndex = 15;
            this.cmdBTM_R.TabStop = false;
            this.cmdBTM_R.Click += new System.EventHandler(this.cmdBTM_R_Click);
            // 
            // cmdBTM
            // 
            this.cmdBTM.BackColor = System.Drawing.Color.Transparent;
            this.cmdBTM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdBTM.Location = new System.Drawing.Point(120, 166);
            this.cmdBTM.Name = "cmdBTM";
            this.cmdBTM.Size = new System.Drawing.Size(50, 50);
            this.cmdBTM.TabIndex = 14;
            this.cmdBTM.TabStop = false;
            this.cmdBTM.Click += new System.EventHandler(this.cmdBTM_Click);
            // 
            // cmdBTM_L
            // 
            this.cmdBTM_L.BackColor = System.Drawing.Color.Transparent;
            this.cmdBTM_L.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdBTM_L.Location = new System.Drawing.Point(64, 166);
            this.cmdBTM_L.Name = "cmdBTM_L";
            this.cmdBTM_L.Size = new System.Drawing.Size(50, 50);
            this.cmdBTM_L.TabIndex = 13;
            this.cmdBTM_L.TabStop = false;
            this.cmdBTM_L.Click += new System.EventHandler(this.cmdBTM_L_Click);
            // 
            // cmdL
            // 
            this.cmdL.BackColor = System.Drawing.Color.Transparent;
            this.cmdL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdL.Location = new System.Drawing.Point(64, 110);
            this.cmdL.Name = "cmdL";
            this.cmdL.Size = new System.Drawing.Size(50, 50);
            this.cmdL.TabIndex = 12;
            this.cmdL.TabStop = false;
            this.cmdL.Click += new System.EventHandler(this.cmdL_Click);
            // 
            // cmdUP_L
            // 
            this.cmdUP_L.BackColor = System.Drawing.Color.Transparent;
            this.cmdUP_L.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdUP_L.Location = new System.Drawing.Point(64, 54);
            this.cmdUP_L.Name = "cmdUP_L";
            this.cmdUP_L.Size = new System.Drawing.Size(50, 50);
            this.cmdUP_L.TabIndex = 11;
            this.cmdUP_L.TabStop = false;
            this.cmdUP_L.Click += new System.EventHandler(this.cmdUP_L_Click);
            // 
            // cmdUP_R
            // 
            this.cmdUP_R.BackColor = System.Drawing.Color.Transparent;
            this.cmdUP_R.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdUP_R.Location = new System.Drawing.Point(176, 54);
            this.cmdUP_R.Name = "cmdUP_R";
            this.cmdUP_R.Size = new System.Drawing.Size(50, 50);
            this.cmdUP_R.TabIndex = 10;
            this.cmdUP_R.TabStop = false;
            this.cmdUP_R.Click += new System.EventHandler(this.cmdUP_R_Click);
            // 
            // cmdUP
            // 
            this.cmdUP.BackColor = System.Drawing.Color.Transparent;
            this.cmdUP.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdUP.Location = new System.Drawing.Point(120, 54);
            this.cmdUP.Name = "cmdUP";
            this.cmdUP.Size = new System.Drawing.Size(50, 50);
            this.cmdUP.TabIndex = 9;
            this.cmdUP.TabStop = false;
            this.cmdUP.Click += new System.EventHandler(this.cmdUP_Click);
            // 
            // cmdStop
            // 
            this.cmdStop.BackColor = System.Drawing.Color.Transparent;
            this.cmdStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdStop.Location = new System.Drawing.Point(120, 110);
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.Size = new System.Drawing.Size(50, 50);
            this.cmdStop.TabIndex = 17;
            this.cmdStop.TabStop = false;
            this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
            // 
            // cmdTurnLeft
            // 
            this.cmdTurnLeft.BackColor = System.Drawing.Color.Transparent;
            this.cmdTurnLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTurnLeft.Location = new System.Drawing.Point(17, 125);
            this.cmdTurnLeft.Name = "cmdTurnLeft";
            this.cmdTurnLeft.Size = new System.Drawing.Size(30, 22);
            this.cmdTurnLeft.TabIndex = 18;
            this.cmdTurnLeft.TabStop = false;
            this.cmdTurnLeft.Click += new System.EventHandler(this.cmdTurnLeft_Click);
            // 
            // cmdTurnRight
            // 
            this.cmdTurnRight.BackColor = System.Drawing.Color.Transparent;
            this.cmdTurnRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTurnRight.Location = new System.Drawing.Point(242, 125);
            this.cmdTurnRight.Name = "cmdTurnRight";
            this.cmdTurnRight.Size = new System.Drawing.Size(30, 22);
            this.cmdTurnRight.TabIndex = 19;
            this.cmdTurnRight.TabStop = false;
            this.cmdTurnRight.Click += new System.EventHandler(this.cmdTurnRight_Click);
            // 
            // speedNormal
            // 
            this.speedNormal.AutoSize = true;
            this.speedNormal.Location = new System.Drawing.Point(40, 13);
            this.speedNormal.Name = "speedNormal";
            this.speedNormal.Size = new System.Drawing.Size(74, 21);
            this.speedNormal.TabIndex = 20;
            this.speedNormal.Text = "Normal";
            this.speedNormal.UseVisualStyleBackColor = true;
            // 
            // speedSlow
            // 
            this.speedSlow.AutoSize = true;
            this.speedSlow.Location = new System.Drawing.Point(120, 13);
            this.speedSlow.Name = "speedSlow";
            this.speedSlow.Size = new System.Drawing.Size(58, 21);
            this.speedSlow.TabIndex = 21;
            this.speedSlow.Text = "Slow";
            this.speedSlow.UseVisualStyleBackColor = true;
            // 
            // speedOne
            // 
            this.speedOne.AutoSize = true;
            this.speedOne.Checked = true;
            this.speedOne.Location = new System.Drawing.Point(188, 13);
            this.speedOne.Name = "speedOne";
            this.speedOne.Size = new System.Drawing.Size(56, 21);
            this.speedOne.TabIndex = 22;
            this.speedOne.TabStop = true;
            this.speedOne.Text = "One";
            this.speedOne.UseVisualStyleBackColor = true;
            // 
            // cmdAnchor
            // 
            this.cmdAnchor.BackColor = System.Drawing.Color.Transparent;
            this.cmdAnchor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAnchor.Location = new System.Drawing.Point(8, 166);
            this.cmdAnchor.Name = "cmdAnchor";
            this.cmdAnchor.Size = new System.Drawing.Size(50, 50);
            this.cmdAnchor.TabIndex = 23;
            this.cmdAnchor.TabStop = false;
            this.cmdAnchor.Click += new System.EventHandler(this.cmdAnchor_Click);
            // 
            // cmd_TurnAround
            // 
            this.cmd_TurnAround.BackColor = System.Drawing.Color.Transparent;
            this.cmd_TurnAround.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmd_TurnAround.Location = new System.Drawing.Point(232, 166);
            this.cmd_TurnAround.Name = "cmd_TurnAround";
            this.cmd_TurnAround.Size = new System.Drawing.Size(50, 50);
            this.cmd_TurnAround.TabIndex = 24;
            this.cmd_TurnAround.TabStop = false;
            this.cmd_TurnAround.Click += new System.EventHandler(this.cmd_TurnAround_Click);
            // 
            // timerCheckCollision
            // 
            this.timerCheckCollision.Enabled = true;
            this.timerCheckCollision.Interval = 500;
            this.timerCheckCollision.Tick += new System.EventHandler(this.timerCheckCollision_Tick);
            // 
            // chkCollision
            // 
            this.chkCollision.AutoSize = true;
            this.chkCollision.Checked = true;
            this.chkCollision.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCollision.Location = new System.Drawing.Point(67, 233);
            this.chkCollision.Name = "chkCollision";
            this.chkCollision.Size = new System.Drawing.Size(158, 21);
            this.chkCollision.TabIndex = 25;
            this.chkCollision.Text = "Stop before collision";
            this.chkCollision.UseVisualStyleBackColor = true;
            // 
            // imgRadar
            // 
            this.imgRadar.BackColor = System.Drawing.Color.Transparent;
            this.imgRadar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.imgRadar.Location = new System.Drawing.Point(293, 12);
            this.imgRadar.Name = "imgRadar";
            this.imgRadar.Size = new System.Drawing.Size(242, 242);
            this.imgRadar.TabIndex = 26;
            this.imgRadar.TabStop = false;
            // 
            // Magellano
            // 
            this.ClientSize = new System.Drawing.Size(548, 276);
            this.Controls.Add(this.imgRadar);
            this.Controls.Add(this.chkCollision);
            this.Controls.Add(this.cmd_TurnAround);
            this.Controls.Add(this.cmdAnchor);
            this.Controls.Add(this.speedOne);
            this.Controls.Add(this.speedSlow);
            this.Controls.Add(this.speedNormal);
            this.Controls.Add(this.cmdTurnRight);
            this.Controls.Add(this.cmdTurnLeft);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.cmdR);
            this.Controls.Add(this.cmdBTM_R);
            this.Controls.Add(this.cmdBTM);
            this.Controls.Add(this.cmdBTM_L);
            this.Controls.Add(this.cmdL);
            this.Controls.Add(this.cmdUP_L);
            this.Controls.Add(this.cmdUP_R);
            this.Controls.Add(this.cmdUP);
            this.Name = "Magellano";
            this.Text = "Magellano - Ship Navigator";
            ((System.ComponentModel.ISupportInitialize)(this.cmdR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBTM_L)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP_L)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdUP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdTurnLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdTurnRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdAnchor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmd_TurnAround)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgRadar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void LoadImages()
        {
            /*
            System.Drawing.Bitmap backgroud = Ultima.Gumps.GetGump(0xB01);
            this.BackgroundImage = backgroud;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            */

            this.cmdUP.BackgroundImage = Ultima.Gumps.GetGump(0x1194);
            this.cmdUP_R.BackgroundImage = Ultima.Gumps.GetGump(0x1195);
            this.cmdR.BackgroundImage = Ultima.Gumps.GetGump(0x1196);
            this.cmdBTM_R.BackgroundImage = Ultima.Gumps.GetGump(0x1197);
            this.cmdBTM.BackgroundImage = Ultima.Gumps.GetGump(0x1198);
            this.cmdBTM_L.BackgroundImage = Ultima.Gumps.GetGump(0x1199);
            this.cmdL.BackgroundImage = Ultima.Gumps.GetGump(0x119A);
            this.cmdUP_L.BackgroundImage = Ultima.Gumps.GetGump(0x119B);
            this.cmdStop.BackgroundImage = Ultima.Gumps.GetGump(0x1B79);
            this.cmdStop.BackgroundImageLayout = ImageLayout.Stretch;

            this.cmdTurnLeft.BackgroundImage = Ultima.Gumps.GetGump(0xFAF);
            this.cmdTurnRight.BackgroundImage = Ultima.Gumps.GetGump(0xFA6);

            this.cmdAnchor.BackgroundImage = Items.GetImage(0x14F7);
            this.cmdAnchor.BackgroundImageLayout = ImageLayout.Stretch;

            this.cmd_TurnAround.BackgroundImage = Ultima.Gumps.GetGump(0xEC);
            this.cmd_TurnAround.BackgroundImage = Ultima.Gumps.GetGump(0x9C6A);
            this.cmd_TurnAround.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void cmdStop_Click(object sender, EventArgs e)
        {
            SendBoatCommand(Commands.STOP, false);
        }
        private void cmdAnchor_Click(object sender, EventArgs e)
        {
            if (anchorDropped == true)
            {
                SendBoatCommand(Commands.RAISE_ANCHOR, false);
                anchorDropped = false;
            }
            else
            {
                SendBoatCommand(Commands.DROP_ANCHOR, false);
                anchorDropped = true;
            }
        }
        private void cmdUP_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.FORWARD_LEFT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.BACK_LEFT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.BACK_RIGHT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.FORWARD_RIGHT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdUP_R_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.FORWARD);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.LEFT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.BACK);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.RIGHT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdR_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.FORWARD_RIGHT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.FORWARD_LEFT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.BACK_LEFT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.BACK_RIGHT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdBTM_R_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.RIGHT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.FORWARD);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.LEFT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.BACK);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdBTM_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.BACK_RIGHT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.FORWARD_RIGHT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.FORWARD_LEFT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.BACK_LEFT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdBTM_L_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.BACK);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.RIGHT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.FORWARD);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.LEFT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdL_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.BACK_LEFT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.BACK_RIGHT);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.FORWARD_RIGHT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.FORWARD_LEFT);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdUP_L_Click(object sender, EventArgs e)
        {
            Orientation orientation = GetOrientation();

            switch (orientation)
            {
                case Orientation.NORTH:
                    SendBoatCommand(Commands.LEFT);
                    break;
                case Orientation.EAST:
                    SendBoatCommand(Commands.BACK);
                    break;
                case Orientation.SOUTH:
                    SendBoatCommand(Commands.RIGHT);
                    break;
                case Orientation.WEST:
                    SendBoatCommand(Commands.FORWARD);
                    break;
                case Orientation.ERROR:
                    Player.HeadMessage(33, "Something went wrong getting orientation");
                    SendBoatCommand(Commands.STOP, false);
                    break;
                default:
                    break;
            }
        }
        private void cmdTurnLeft_Click(object sender, EventArgs e)
        {
            SendBoatCommand(Commands.TURN_LEFT, false);
        }
        private void cmdTurnRight_Click(object sender, EventArgs e)
        {
            SendBoatCommand(Commands.TURN_RIGHT, false);
        }
        private void cmd_TurnAround_Click(object sender, EventArgs e)
        {
            SendBoatCommand(Commands.TURN_AROUND, false);
        }

        private Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            rotatedImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                // Rotate
                g.RotateTransform(angle);
                // Restore rotation point in the matrix
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                // Draw the image on the bitmap
                g.DrawImage(bmp, new Point(0, 0));
            }

            return rotatedImage;
        }
        private Orientation GetOrientation()
        {
            if (tillerManSerial == 0)
            {
                tillerManSerial = Items.FindBySerial(new Target().PromptTarget("Target tillerman")).Serial;
            }

            Item tiller = Items.FindBySerial(tillerManSerial);
            if (tiller == null)
            {
                return Orientation.ERROR;
            }

            Orientation ret = Orientation.ERROR;
            switch (tiller.ItemID)
            {
                case 0x3E4B:
                    ret = Orientation.SOUTH;
                    break;
                case 0x3E55:
                    ret = Orientation.EAST;
                    break;
                case 0x3E4E:
                    ret = Orientation.NORTH;
                    break;
                case 0x3E50:
                    ret = Orientation.WEST;
                    break;
            }
            return ret;
        }
        private void SendBoatCommand(Commands command, bool directionCommand = true)
        {
            // Converts enum to string
            string boatCommand = Enum.GetName(typeof(Commands), command).Replace("_", " ").ToLower();

            if (speedOne.Checked && directionCommand)
            {
                boatCommand = $"{boatCommand} One";
            }
            else if (speedSlow.Checked && directionCommand)
            {
                boatCommand = $"Slow {boatCommand}";
            }

            // Capital first letter of each word
            boatCommand = Regex.Replace(boatCommand, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            Player.ChatSay(33, boatCommand);
        }
        private void timerCheckCollision_Tick(object sender, EventArgs e)
        {
            Point3D position = Player.Position;
            if (position.Equals(playerPrevPosition)) { return; } // If player doesn't move I skip
            playerPrevPosition = position; // Upgrade prev position

            const int size = 6; // Size of Radar
            Bitmap bmp = new Bitmap(44 * size, 44 * size);
            Graphics g = Graphics.FromImage(bmp);

            int row = 0;
            int column = 0;
            for (int x = position.X - size; x < position.X + size; x++)
            {
                for (int y = position.Y - size; y < position.Y + size; y++)
                {
                    var landInfo = Statics.GetStaticsLandInfo(x, y, Player.Map);
                    Bitmap img = Ultima.Art.GetLand(landInfo.StaticID);

                    var staticInfo = Statics.GetStaticsTileInfo(x, y, Player.Map);
                    if (staticInfo.Count > 0)
                    {
                        img = Ultima.Art.GetStatic(staticInfo[staticInfo.Count - 1].StaticID);
                        string tileName = Statics.GetTileName(staticInfo[staticInfo.Count - 1].StaticID).ToLower();

                        // If there is a static item need to stop the ship
                        if (chkCollision.Checked && tileName != "water")
                        {
                            SendBoatCommand(Commands.STOP, false);
                        }
                    }
                    else
                    {
                        bool isWet = Statics.GetLandFlag(landInfo.StaticID, "Wet");
                        if (!isWet)
                        {
                            string debugLandName = Statics.GetLandName(landInfo.StaticID);
                            // If land is not wet need to stop the ship
                            if (chkCollision.Checked)
                            {
                                SendBoatCommand(Commands.STOP, false);
                            }
                        }
                    }

                    int x1 = column * 22;
                    int y1 = row * 22;

                    img = RotateImage(img, -45);
                    g.DrawImage(img, x1, y1, img.Width, img.Height);
                    row++;
                }
                column++;
                row = 0;
            }


            Bitmap RadarGump = RotateImage(new Bitmap(Ultima.Gumps.GetGump(0x1392), 44 * size, 44 * size), -45);
            RadarGump.MakeTransparent(RadarGump.GetPixel(RadarGump.Width / 2, RadarGump.Height / 2));
            g.DrawImage(RadarGump, 0, 0);

            var ship = RotateImage(Items.GetImage(0x14F4), -45);
            g.DrawImage(ship, (bmp.Width / 2) - (ship.Width / 2), (bmp.Height / 2) - (ship.Height / 2), ship.Width, ship.Height);

            bmp = RotateImage(bmp, 45);
            
            imgRadar.BackgroundImage = bmp;
            imgRadar.BackgroundImageLayout = ImageLayout.Stretch;
        }

    }
}
