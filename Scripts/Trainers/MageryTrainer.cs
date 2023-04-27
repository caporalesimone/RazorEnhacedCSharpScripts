/************************************************************************
 * Magery Trainer: Simple Magery Trainer
 * Dynamic
 * **********************************************************************/

namespace SharpRE.Scripts
{
    using System;
    using System.Windows.Forms;
    using System.Threading.Tasks;
    using System.Linq;
    using RazorEnhanced;

    class MageryTrainer : Form
    {
        #region Designer
        private CheckBox cbxUseMed;
        private Label label1;
        private TextBox tbxMeditateAt;
        private GroupBox groupBox2;
        private Label lblTotalGains;
        private Label label7;
        private Label lblCurrentSkill;
        private Label lbl;
        private Label lblStartSkill;
        private Label label2;
        private Button btnStart;
        private Label lblStatus;
        private GroupBox groupBox1;

        private void InitializeComponent()
        {
            this.cbxUseMed = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxMeditateAt = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblTotalGains = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblCurrentSkill = new System.Windows.Forms.Label();
            this.lbl = new System.Windows.Forms.Label();
            this.lblStartSkill = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbxUseMed
            // 
            this.cbxUseMed.AutoSize = true;
            this.cbxUseMed.Location = new System.Drawing.Point(13, 30);
            this.cbxUseMed.Name = "cbxUseMed";
            this.cbxUseMed.Size = new System.Drawing.Size(124, 21);
            this.cbxUseMed.TabIndex = 0;
            this.cbxUseMed.Text = "Use Meditation";
            this.cbxUseMed.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Meditate At:";
            // 
            // tbxMeditateAt
            // 
            this.tbxMeditateAt.Location = new System.Drawing.Point(156, 65);
            this.tbxMeditateAt.Name = "tbxMeditateAt";
            this.tbxMeditateAt.Size = new System.Drawing.Size(45, 22);
            this.tbxMeditateAt.TabIndex = 2;
            this.tbxMeditateAt.Text = "25";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxUseMed);
            this.groupBox1.Controls.Add(this.tbxMeditateAt);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 103);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Meditation";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblTotalGains);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.lblCurrentSkill);
            this.groupBox2.Controls.Add(this.lbl);
            this.groupBox2.Controls.Add(this.lblStartSkill);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(231, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(230, 145);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statistics";
            // 
            // lblTotalGains
            // 
            this.lblTotalGains.AutoSize = true;
            this.lblTotalGains.Location = new System.Drawing.Point(157, 111);
            this.lblTotalGains.Name = "lblTotalGains";
            this.lblTotalGains.Size = new System.Drawing.Size(16, 17);
            this.lblTotalGains.TabIndex = 5;
            this.lblTotalGains.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 111);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 17);
            this.label7.TabIndex = 4;
            this.label7.Text = "Total Gains:";
            // 
            // lblCurrentSkill
            // 
            this.lblCurrentSkill.AutoSize = true;
            this.lblCurrentSkill.Location = new System.Drawing.Point(157, 78);
            this.lblCurrentSkill.Name = "lblCurrentSkill";
            this.lblCurrentSkill.Size = new System.Drawing.Size(16, 17);
            this.lblCurrentSkill.TabIndex = 3;
            this.lblCurrentSkill.Text = "0";
            // 
            // lbl
            // 
            this.lbl.AutoSize = true;
            this.lbl.Location = new System.Drawing.Point(13, 78);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(88, 17);
            this.lbl.TabIndex = 2;
            this.lbl.Text = "Current Skill:";
            // 
            // lblStartSkill
            // 
            this.lblStartSkill.AutoSize = true;
            this.lblStartSkill.Location = new System.Drawing.Point(157, 45);
            this.lblStartSkill.Name = "lblStartSkill";
            this.lblStartSkill.Size = new System.Drawing.Size(16, 17);
            this.lblStartSkill.TabIndex = 1;
            this.lblStartSkill.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Starting Skill:";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(386, 155);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 34);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 167);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(79, 17);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Status: N/A";
            // 
            // MageryTrainer
            // 
            this.ClientSize = new System.Drawing.Size(475, 206);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "MageryTrainer";
            this.Text = "Magery Trainer v1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MageryTrainer_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        int Mana
        {
            get
            {
                var perc = (double)Player.Mana / Player.ManaMax * 100;
                return (int)perc;
            }
        }
        double SkillValue
        {
            get
            {
                return Player.GetSkillValue("Magery");
            }
        }

        bool _running = false;
        DateTime _started;
        int _MedAttemptDelay = 14;

        public MageryTrainer()
        {
            InitializeComponent();
        }


        public void Run()
        {
            Application.EnableVisualStyles();
            Application.Run(this);

        }


        void Meditate()
        {
            bool meditating = false;

            double stamp = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            DateTime nextTry = DateTime.Now;

            lblStatus.Text = "Status: Trying Meditation";
            while (Mana < 99)
            {
                if (!_running)
                    break;

                if (!meditating)
                {
                    if (DateTime.Now > nextTry)
                    {
                        Player.UseSkill("Meditation");

                        nextTry = DateTime.Now.AddSeconds(_MedAttemptDelay);

                        Misc.Pause(100);

                        var entries = Journal.GetJournalEntry(stamp);

                        if (entries.Count > 0)
                        {
                            if (entries.Exists(x => x.Text == "You enter a meditative trance."))
                            {
                                meditating = true;
                                lblStatus.Text = "Status: Meditating";
                            }

                            stamp = entries.Last().Timestamp;
                        }
                    }
                }
                Misc.Pause(100);
            }
        }


        private void btnStart_Click(object sender, System.EventArgs e)
        {
            if (_running)
                _running = false;
            else
                _running = true;

            if (_running)
            {
                Misc.SendMessage("Started");

                btnStart.Text = "Stop";

                Task.Run(() =>
                {
                    _started = DateTime.Now;
                    lblStartSkill.Text = Player.GetSkillValue("Magery").ToString();
                    while (_running)
                    {
                        if (cbxUseMed.Checked && Mana <= Convert.ToInt32(tbxMeditateAt.Text))
                            Meditate();

                        lblStatus.Text = "Status: Casting ";
                        if (SkillValue >= 30 && SkillValue < 45)
                        {
                            string spell = "Bless";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Target.WaitForTarget(1025);
                            Target.Self();
                        }
                        else if (SkillValue >= 45 && SkillValue < 55)
                        {
                            string spell = "Greater Heal";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Target.WaitForTarget(1275);
                            Target.Self();
                        }
                        else if (SkillValue >= 55 && SkillValue < 65)
                        {
                            string spell = "Magic Reflection";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Misc.Pause(1525);
                        }
                        else if (SkillValue >= 65 && SkillValue < 75)
                        {
                            string spell = "Invisibility";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Target.WaitForTarget(1775);
                            Target.Self();
                        }
                        else if (SkillValue >= 75 && SkillValue < 90)
                        {
                            string spell = "Mana Vampire";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Target.WaitForTarget(2025);
                            Target.Self();
                        }
                        else if (SkillValue >= 90 && SkillValue < 120)
                        {
                            string spell = "Earthquake";
                            lblStatus.Text += spell;
                            Spells.Cast(spell);
                            Misc.Pause(2500);
                        }
                        lblCurrentSkill.Text = Player.GetSkillValue("Magery").ToString();

                        var startSkill = Convert.ToDouble(lblStartSkill.Text);
                        var results = SkillValue - startSkill;
                        lblTotalGains.Text = results.ToString("0.0");

                        Misc.Pause(100);
                    }
                    Misc.SendMessage("Exited");
                    lblStatus.Text = "Status: N/A";
                });
            }
            else
            {
                btnStart.Text = "Start";
            }               
        }

        private void MageryTrainer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _running = false;
            Misc.SendMessage("Exited");
        }
    }
}