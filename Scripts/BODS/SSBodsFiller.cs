//C#

//
// SSBodsFiller
//  This is an automation tool for fill Tailoring and Blacksmith BODs
//  It has been developed and tested on Demise Server.
//  It can run also on other servers but BodCraftableDatabase.cs file must be updated correctly
// 
// Developed by SimonSoft - 2021
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RazorEnhanced;
using Scripts.Libs;

//#import <bod_libs/Bod.cs>
//#import <bod_libs/BodCraftableDatabase.cs>
//#import <../Libs/common.cs>
//#import <../Libs/logger.cs>
//#import <../Libs/stored_data.cs>

namespace BODS
{
    class SSBodsFiller : Form
    {
        private const string scriptName = "SSBodsFiller by SimonSoft - 2021";
        private const string json_secure_container = "SSBODsFiller_SecureContainer"; // JSON Key with the latest secure container serial
        private const string json_tailor_talisman = "SSBODsFiller_TailorTalisman";   // JSON Key with the latest tailoring talisman serial
        private const string json_blacksmith_talisman = "SSBODsFiller_BlackSmithTalisman";   // JSON Key with the latest blacksmithy talisman serial

        private const int delayUseItem = 600;
        private const int delayDragItem = 600;
        private const int delayGump = 15000;

        private bool forcedStop = false;

        private int secureFilledBodContainerSerial = 0;
        private int secureResourceContainerSerial = 0;

        private int talismanTailoringSerial = 0;
        private int talismanBlackSmithSerial = 0;

        private ListBox lstBODs;

        private Button btnAddSingle;
        private Button btnDelSingle;
        private Button btnClearAll;
        private Button btnAddResourceContainer;
        private Button btnAddAll;
        private Button btnAddStoreBODsContainer;
        private Button btnStart;
        private Button btnStop;

        private Label lbl_1;
        private Label lblSecureContainer;
        private Label lbl_2;
        private Label lblStoreBODs;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripProgressBar progressBar;

        private ToolStripStatusLabel lblCrafting;
        private PictureBox pictureBox;
        private Button btnCutter;
        private Button btnSmelter;
        private Button btnTailorTalisman;
        private Button btnBlacksmithyTalisman;
        private Label label1;
        private Label label2;
        private Label lblTailoringTalisman;
        private Label lblBlackSmithyTalisman;
        

        public SSBodsFiller()
        {
            InitializeComponent();
            this.Text = scriptName + " - " + Player.Name;
            secureResourceContainerSerial = StoredData.GetData<int>(json_secure_container, false);

            if (secureResourceContainerSerial != 0)
            {
                Item target = Items.FindBySerial(secureResourceContainerSerial);
                if (target != null && target.IsContainer == true)
                {
                    lblSecureContainer.Text = target.Properties[0].ToString();
                }
            }

            talismanTailoringSerial = StoredData.GetData<int>(json_tailor_talisman, false);
            lblTailoringTalisman.Text = (talismanTailoringSerial == 0) ? "NO" : "YES"; 
            
            talismanBlackSmithSerial = StoredData.GetData<int>(json_blacksmith_talisman, false);
            lblBlackSmithyTalisman.Text = (talismanBlackSmithSerial == 0) ? "NO" : "YES";
        }
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.Run(this); // This is blocking. Will return only when form is closed
        }
        private void InitializeComponent()
        {
            this.btnAddAll = new System.Windows.Forms.Button();
            this.lstBODs = new System.Windows.Forms.ListBox();
            this.btnAddSingle = new System.Windows.Forms.Button();
            this.btnDelSingle = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnAddResourceContainer = new System.Windows.Forms.Button();
            this.lbl_1 = new System.Windows.Forms.Label();
            this.lblSecureContainer = new System.Windows.Forms.Label();
            this.btnAddStoreBODsContainer = new System.Windows.Forms.Button();
            this.lbl_2 = new System.Windows.Forms.Label();
            this.lblStoreBODs = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.lblCrafting = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.btnCutter = new System.Windows.Forms.Button();
            this.btnSmelter = new System.Windows.Forms.Button();
            this.btnTailorTalisman = new System.Windows.Forms.Button();
            this.btnBlacksmithyTalisman = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTailoringTalisman = new System.Windows.Forms.Label();
            this.lblBlackSmithyTalisman = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAddAll
            // 
            this.btnAddAll.Location = new System.Drawing.Point(112, 246);
            this.btnAddAll.Name = "btnAddAll";
            this.btnAddAll.Size = new System.Drawing.Size(64, 29);
            this.btnAddAll.TabIndex = 0;
            this.btnAddAll.Text = "add all";
            this.btnAddAll.UseVisualStyleBackColor = true;
            this.btnAddAll.Click += new System.EventHandler(this.btnAddAll_Click);
            this.btnAddAll.MouseHover += new System.EventHandler(this.btnAddAll_MouseHover);
            // 
            // lstBODs
            // 
            this.lstBODs.FormattingEnabled = true;
            this.lstBODs.ItemHeight = 16;
            this.lstBODs.Location = new System.Drawing.Point(12, 12);
            this.lstBODs.Name = "lstBODs";
            this.lstBODs.Size = new System.Drawing.Size(240, 212);
            this.lstBODs.TabIndex = 1;
            this.lstBODs.SelectedIndexChanged += new System.EventHandler(this.lstBODs_SelectedIndexChanged);
            // 
            // btnAddSingle
            // 
            this.btnAddSingle.Location = new System.Drawing.Point(12, 246);
            this.btnAddSingle.Name = "btnAddSingle";
            this.btnAddSingle.Size = new System.Drawing.Size(44, 29);
            this.btnAddSingle.TabIndex = 2;
            this.btnAddSingle.Text = "+";
            this.btnAddSingle.UseVisualStyleBackColor = true;
            this.btnAddSingle.Click += new System.EventHandler(this.btnAddSingle_Click);
            this.btnAddSingle.MouseHover += new System.EventHandler(this.btnAddSingle_MouseHover);
            // 
            // btnDelSingle
            // 
            this.btnDelSingle.Location = new System.Drawing.Point(62, 246);
            this.btnDelSingle.Name = "btnDelSingle";
            this.btnDelSingle.Size = new System.Drawing.Size(44, 29);
            this.btnDelSingle.TabIndex = 3;
            this.btnDelSingle.Text = "-";
            this.btnDelSingle.UseVisualStyleBackColor = true;
            this.btnDelSingle.Click += new System.EventHandler(this.btnDel_Click);
            this.btnDelSingle.MouseHover += new System.EventHandler(this.btnDel_MouseHover);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(182, 246);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(70, 29);
            this.btnClearAll.TabIndex = 4;
            this.btnClearAll.Text = "clear list";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClear_Click);
            this.btnClearAll.MouseHover += new System.EventHandler(this.btnClear_MouseHover);
            // 
            // btnAddResourceContainer
            // 
            this.btnAddResourceContainer.Enabled = false;
            this.btnAddResourceContainer.Location = new System.Drawing.Point(261, 28);
            this.btnAddResourceContainer.Name = "btnAddResourceContainer";
            this.btnAddResourceContainer.Size = new System.Drawing.Size(118, 24);
            this.btnAddResourceContainer.TabIndex = 5;
            this.btnAddResourceContainer.Text = "Add resources container";
            this.btnAddResourceContainer.UseVisualStyleBackColor = true;
            this.btnAddResourceContainer.Click += new System.EventHandler(this.btnResources_Click);
            this.btnAddResourceContainer.MouseHover += new System.EventHandler(this.btnResources_MouseHover);
            // 
            // lbl_1
            // 
            this.lbl_1.AutoSize = true;
            this.lbl_1.Location = new System.Drawing.Point(259, 12);
            this.lbl_1.Name = "lbl_1";
            this.lbl_1.Size = new System.Drawing.Size(57, 17);
            this.lbl_1.TabIndex = 6;
            this.lbl_1.Text = "Secure:";
            // 
            // lblSecureContainer
            // 
            this.lblSecureContainer.AutoSize = true;
            this.lblSecureContainer.Location = new System.Drawing.Point(300, 12);
            this.lblSecureContainer.Name = "lblSecureContainer";
            this.lblSecureContainer.Size = new System.Drawing.Size(56, 17);
            this.lblSecureContainer.TabIndex = 7;
            this.lblSecureContainer.Text = "<none>";
            // 
            // btnAddStoreBODsContainer
            // 
            this.btnAddStoreBODsContainer.Enabled = false;
            this.btnAddStoreBODsContainer.Location = new System.Drawing.Point(403, 28);
            this.btnAddStoreBODsContainer.Name = "btnAddStoreBODsContainer";
            this.btnAddStoreBODsContainer.Size = new System.Drawing.Size(119, 24);
            this.btnAddStoreBODsContainer.TabIndex = 8;
            this.btnAddStoreBODsContainer.Text = "Store finished BODs";
            this.btnAddStoreBODsContainer.UseVisualStyleBackColor = true;
            this.btnAddStoreBODsContainer.Click += new System.EventHandler(this.btnStoreBODs_Click);
            this.btnAddStoreBODsContainer.MouseHover += new System.EventHandler(this.btnStoreBODs_MouseHover);
            // 
            // lbl_2
            // 
            this.lbl_2.AutoSize = true;
            this.lbl_2.Location = new System.Drawing.Point(400, 12);
            this.lbl_2.Name = "lbl_2";
            this.lbl_2.Size = new System.Drawing.Size(87, 17);
            this.lbl_2.TabIndex = 9;
            this.lbl_2.Text = "Store BODs:";
            // 
            // lblStoreBODs
            // 
            this.lblStoreBODs.AutoSize = true;
            this.lblStoreBODs.Location = new System.Drawing.Point(465, 12);
            this.lblStoreBODs.Name = "lblStoreBODs";
            this.lblStoreBODs.Size = new System.Drawing.Size(56, 17);
            this.lblStoreBODs.TabIndex = 10;
            this.lblStoreBODs.Text = "<none>";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progressBar,
            this.lblCrafting});
            this.statusStrip.Location = new System.Drawing.Point(0, 292);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(566, 26);
            this.statusStrip.TabIndex = 11;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(90, 20);
            this.lblStatus.Text = "Select BODs";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 18);
            this.progressBar.Visible = false;
            // 
            // lblCrafting
            // 
            this.lblCrafting.Name = "lblCrafting";
            this.lblCrafting.Size = new System.Drawing.Size(0, 20);
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(369, 197);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(80, 27);
            this.btnStart.TabIndex = 12;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(455, 197);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 27);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(262, 164);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(60, 60);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 14;
            this.pictureBox.TabStop = false;
            // 
            // btnCutter
            // 
            this.btnCutter.Location = new System.Drawing.Point(355, 246);
            this.btnCutter.Name = "btnCutter";
            this.btnCutter.Size = new System.Drawing.Size(80, 27);
            this.btnCutter.TabIndex = 15;
            this.btnCutter.Text = "Cut items";
            this.btnCutter.UseVisualStyleBackColor = true;
            this.btnCutter.Click += new System.EventHandler(this.btnCutter_Click);
            // 
            // btnSmelter
            // 
            this.btnSmelter.Location = new System.Drawing.Point(262, 246);
            this.btnSmelter.Name = "btnSmelter";
            this.btnSmelter.Size = new System.Drawing.Size(93, 27);
            this.btnSmelter.TabIndex = 16;
            this.btnSmelter.Text = "Smelt items";
            this.btnSmelter.UseVisualStyleBackColor = true;
            this.btnSmelter.Click += new System.EventHandler(this.btnSmelter_Click);
            // 
            // btnTailorTalisman
            // 
            this.btnTailorTalisman.Location = new System.Drawing.Point(262, 85);
            this.btnTailorTalisman.Name = "btnTailorTalisman";
            this.btnTailorTalisman.Size = new System.Drawing.Size(118, 24);
            this.btnTailorTalisman.TabIndex = 17;
            this.btnTailorTalisman.Text = "Tailoring talisman";
            this.btnTailorTalisman.UseVisualStyleBackColor = true;
            this.btnTailorTalisman.Click += new System.EventHandler(this.btnTailorTalisman_Click);
            this.btnTailorTalisman.MouseHover += new System.EventHandler(this.btnTailorTalisman_MouseHover);
            // 
            // btnBlacksmithyTalisman
            // 
            this.btnBlacksmithyTalisman.Location = new System.Drawing.Point(403, 85);
            this.btnBlacksmithyTalisman.Name = "btnBlacksmithyTalisman";
            this.btnBlacksmithyTalisman.Size = new System.Drawing.Size(118, 24);
            this.btnBlacksmithyTalisman.TabIndex = 18;
            this.btnBlacksmithyTalisman.Text = "Blackmithy talisman";
            this.btnBlacksmithyTalisman.UseVisualStyleBackColor = true;
            this.btnBlacksmithyTalisman.Click += new System.EventHandler(this.btnBlacksmithyTalisman_Click);
            this.btnBlacksmithyTalisman.MouseHover += new System.EventHandler(this.btnBlacksmithyTalisman_MouseHover);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(259, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 19;
            this.label1.Text = "Selected:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(400, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 17);
            this.label2.TabIndex = 20;
            this.label2.Text = "Selected:";
            // 
            // lblTailoringTalisman
            // 
            this.lblTailoringTalisman.AutoSize = true;
            this.lblTailoringTalisman.Location = new System.Drawing.Point(306, 69);
            this.lblTailoringTalisman.Name = "lblTailoringTalisman";
            this.lblTailoringTalisman.Size = new System.Drawing.Size(29, 17);
            this.lblTailoringTalisman.TabIndex = 21;
            this.lblTailoringTalisman.Text = "NO";
            // 
            // lblBlackSmithyTalisman
            // 
            this.lblBlackSmithyTalisman.AutoSize = true;
            this.lblBlackSmithyTalisman.Location = new System.Drawing.Point(447, 69);
            this.lblBlackSmithyTalisman.Name = "lblBlackSmithyTalisman";
            this.lblBlackSmithyTalisman.Size = new System.Drawing.Size(29, 17);
            this.lblBlackSmithyTalisman.TabIndex = 22;
            this.lblBlackSmithyTalisman.Text = "NO";
            // 
            // SSBodsFiller
            // 
            this.ClientSize = new System.Drawing.Size(566, 318);
            this.Controls.Add(this.lblBlackSmithyTalisman);
            this.Controls.Add(this.lblTailoringTalisman);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBlacksmithyTalisman);
            this.Controls.Add(this.btnTailorTalisman);
            this.Controls.Add(this.btnSmelter);
            this.Controls.Add(this.btnCutter);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.lblStoreBODs);
            this.Controls.Add(this.lbl_2);
            this.Controls.Add(this.btnAddStoreBODsContainer);
            this.Controls.Add(this.lblSecureContainer);
            this.Controls.Add(this.lbl_1);
            this.Controls.Add(this.btnAddResourceContainer);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnDelSingle);
            this.Controls.Add(this.btnAddSingle);
            this.Controls.Add(this.lstBODs);
            this.Controls.Add(this.btnAddAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SSBodsFiller";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.BODs_Tailor_Crafter_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #region EVENTS HANDLERS
        private void OnFormClosed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void btnAddAll_Click(object sender, EventArgs e)
        {
            btnAddAll.Enabled = false;
            lstBODs.Items.Clear();

            List<Item> foundItems = Common.FindItems(0x2258, Player.Backpack, false, true, false); // BOD
            Common.Pause(100);
            foreach (Item itm in foundItems)
            {
                Bod bod = new Bod(itm);
                AddBODtoListIfValid(bod);
            }

            if (lstBODs.Items.Count > 0)
            {
                lstBODs.SelectedIndex = 0;
                SetEnabledButtonsSelectContainers(true);
            }

            btnAddAll.Enabled = true;
        }
        private void btnAddSingle_Click(object sender, EventArgs e)
        {
            Item itm = Items.FindBySerial(new Target().PromptTarget("Select a BOD"));
            if (itm != null && itm.ItemID == 0x2258)
            {
                Bod bod = new Bod(itm);
                if (AddBODtoListIfValid(bod))
                {
                    SetEnabledButtonsSelectContainers(true);
                }
            }
            else
            {
                MessageBox.Show("This is not a BOD", scriptName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log("This is not a BOD");
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            lstBODs.Items.Clear();
            SetEnabledButtonsSelectContainers(false);
        }
        private void BODs_Tailor_Crafter_Load(object sender, EventArgs e)
        {
            //System.Diagnostics.Debugger.Break();
            this.Icon = Icon.FromHandle(Items.GetImage(0x2258, 0).GetHicon());

            //pictureBox.Image = Items.GetImage(0x13cd);
        }
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (lstBODs.Items.Count > 0)
            {
                int idx = lstBODs.SelectedIndex;
                lstBODs.Items.RemoveAt(idx);
                if (lstBODs.Items.Count > 0)
                {
                    if (idx > 0 && (idx < lstBODs.Items.Count))
                    {
                        lstBODs.SelectedIndex = idx;
                    }
                    else if (idx == lstBODs.Items.Count)
                    {
                        lstBODs.SelectedIndex = idx - 1;
                    }
                    else
                    {
                        lstBODs.SelectedIndex = 0;
                    }
                }
                else
                {
                    SetEnabledButtonsSelectContainers(false);
                    lblStatus.Text = "Select BODs";
                }
            }
        }
        private void btnResources_Click(object sender, EventArgs e)
        {
            bool targetValid = false;
            int containerSerial = new Target().PromptTarget("Select secure container");

            if (containerSerial > 0)
            {
                Item target = Items.FindBySerial(containerSerial);
                if (target != null && target.IsContainer == true)
                {
                    secureResourceContainerSerial = target.Serial;
                    lblSecureContainer.Text = target.Properties[0].ToString();
                    targetValid = true;
                    StoredData.StoreData(secureResourceContainerSerial, json_secure_container, false);
                }
                else
                {
                    if (containerSerial == Player.Serial)
                    {
                        secureResourceContainerSerial = Player.Backpack.Serial;
                        lblSecureContainer.Text = "Player's Backpack";
                        targetValid = true;
                        StoredData.StoreData(secureResourceContainerSerial, json_secure_container, false);
                    }
                    else
                    {
                        MessageBox.Show("Target is not a container", scriptName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.Log("Target is not a container");
                    }
                }
            }
            else
            {
                Logger.Log("Target is not a container");
            }

            if (targetValid)
            {
                btnStart.Enabled = true;
                btnStop.Enabled = true;
            }

        }
        private void btnStoreBODs_Click(object sender, EventArgs e)
        {
            int containerSerial = new Target().PromptTarget("Select where store BODs");

            if (containerSerial > 0)
            {
                Item target = Items.FindBySerial(containerSerial);

                if (target != null && (target.IsContainer == true || target.ItemID == 0x2259))
                {
                    if ((target.ItemID == 0x2259) && (target.RootContainer != Player.Backpack.Serial))
                    {
                        MessageBox.Show("Targetted Book of BODs is not in player backpack", scriptName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.Log("Targetted Book of BODs is not in player backpack");
                        lblStoreBODs.Text = "<none>";
                        secureFilledBodContainerSerial = 0;
                    }
                    else
                    {
                        secureFilledBodContainerSerial = target.Serial;
                        lblStoreBODs.Text = target.Properties[0].ToString();
                    }
                }
                else
                {
                    if (containerSerial == Player.Serial)
                    {
                        secureFilledBodContainerSerial = Player.Backpack.Serial;
                        lblStoreBODs.Text = "Player's Backpack";
                    }
                    else
                    {
                        MessageBox.Show("Target is not a container or a book of BODs", scriptName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.Log("Target is not a container or a book of BODs");
                        lblStoreBODs.Text = "<none>";
                        secureFilledBodContainerSerial = 0;
                    }
                }
            }
            else
            {
                Logger.Log("Target is not a container");
            }
        }
        private void btnTailorTalisman_Click(object sender, EventArgs e)
        {
            int talismanSerial = new Target().PromptTarget("Select Tailor Talisman");
            if (talismanSerial == 0)
            {
                Logger.MessageBox("Wrong selection for Tailor Talisman", Logger.MESSAGEBOX_TYPE.ERROR);
                return;
            }

            Item talisman = Items.FindBySerial(talismanSerial);
            if (talisman != null)
            {
                List<int> validTalismans = new List<int>() {
                    0x2F59, // Snake
                    0x2F58, // Shield
                    0x2F5B, // Y
                    0x2F5A, // ? 
                };

                if (!validTalismans.Contains(talisman.ItemID))
                {
                    Logger.MessageBox("Not a valid Talisman!", Logger.MESSAGEBOX_TYPE.ERROR);
                    return;
                }

                bool isTailor = false;
                foreach (var prop in talisman.Properties)
                {
                    if (prop.ToString().ToLower().Contains("tailoring"))
                    {
                        isTailor = true;
                        break;
                    }
                }

                if (!isTailor)
                {
                    Logger.MessageBox("Not a valid Tailoring Talisman!", Logger.MESSAGEBOX_TYPE.ERROR);
                    return;
                }

                talismanTailoringSerial = talismanSerial;
                StoredData.StoreData(talismanTailoringSerial, json_tailor_talisman, false);
                lblTailoringTalisman.Text = "YES";
            }
        }
        private void btnBlacksmithyTalisman_Click(object sender, EventArgs e)
        {
            int talismanSerial = new Target().PromptTarget("Select Blacksmithy Talisman");
            if (talismanSerial == 0)
            {
                Logger.MessageBox("Wrong selection for Blacksmithy Talisman", Logger.MESSAGEBOX_TYPE.ERROR);
                return;
            }

            Item talisman = Items.FindBySerial(talismanSerial);
            if (talisman != null)
            {
                List<int> validTalismans = new List<int>() {
                    0x2F59, // Snake
                    0x2F58, // Shield
                    0x2F5B, // Y
                    0x2F5A, // ? 
                };

                if (!validTalismans.Contains(talisman.ItemID))
                {
                    Logger.MessageBox("Not a valid Talisman!", Logger.MESSAGEBOX_TYPE.ERROR);
                    return;
                }

                bool isBlackSmith = false;
                foreach (var prop in talisman.Properties)
                {
                    if (prop.ToString().ToLower().Contains("blacksmithing"))
                    {
                        isBlackSmith = true;
                        break;
                    }
                }

                if (!isBlackSmith)
                {
                    Logger.MessageBox("Not a valid Blacksmithing Talisman!", Logger.MESSAGEBOX_TYPE.ERROR);
                    return;
                }

                talismanBlackSmithSerial = talismanSerial;
                StoredData.StoreData(talismanBlackSmithSerial, json_blacksmith_talisman, false);
                lblBlackSmithyTalisman.Text = "YES";
            }
        }
        private void btnTailorTalisman_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Target a Tailoring talisman to be equipped";
        }
        private void btnBlacksmithyTalisman_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Target a Blacksmithing talisman to be equipped";
        }
        private void btnAddSingle_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Target one bulk of orders deed";
        }
        private void btnDel_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Remove selected bulk of orders deed";
        }
        private void btnAddAll_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Add all bulk of orders deed in player's backpack";
        }
        private void btnClear_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Removes all bulk of orders from the list";
        }
        private void btnResources_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Target a resource container or player";
        }
        private void btnStoreBODs_MouseHover(object sender, EventArgs e)
        {
            lblStatus.Text = "Target a | Container | Book of BODs | Player | for store filled BODs";
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            SetEnabledAllButtons(false);
            btnStop.Enabled = true;
            forcedStop = false;
            StartCrafting();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            SetEnabledAllButtons(true);
            if (lstBODs.Items.Count > 0)
            {
                btnAddResourceContainer.Enabled = true;
                btnAddStoreBODsContainer.Enabled = true;
            }
            forcedStop = true;
        }
        private void btnSmelter_Click(object sender, EventArgs e)
        {
            if (Gumps.HasGump())
            {
                Gumps.CloseGump(Gumps.CurrentGump());
                Common.Pause(500);
            }

            int serial = new Target().PromptTarget("Select item to smelt");
            Item targetted = Items.FindBySerial(serial);
            List<Item> foundItems = Common.FindItems(targetted.ItemID, Player.Backpack, false, false, false);
            Common.Pause(100);
            if (foundItems != null)
            {
                SmeltItems(foundItems, 600, BodCraftableDatabase.DestroyToolFromMaterial("iron"));
            }
        }
        private void btnCutter_Click(object sender, EventArgs e)
        {
            if (Gumps.HasGump())
            {
                Gumps.CloseGump(Gumps.CurrentGump());
                Common.Pause(500);
            }

            int serial = new Target().PromptTarget("Select item to cut");
            Item targetted = Items.FindBySerial(serial);
            List<Item> foundItems = Common.FindItems(targetted.ItemID, Player.Backpack, false, false, false);
            Common.Pause(100);
            if (foundItems != null)
            {
                CutItems(foundItems, 600, BodCraftableDatabase.DestroyToolFromMaterial("cloth"));
            }
        }
        private void lstBODs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lstBox = (ListBox)sender;

            if (lstBox.SelectedIndex >= 0)
            {
                Bod bod = lstBox.SelectedItem as Bod;
                if (bod.Craftables.Count > 0) ShowBodCraftableInfo(bod.Craftables[0].ItemToDo);
            }
        }

        #endregion

        private bool SmeltItems(List<Item> itemsToBeSmelt, uint requiredPause, List<int> toolList)
        {
            Items.Filter forgeFilter = new Items.Filter
            {
                Graphics = new List<int>() {    0x0FB1, // Small Forge
                                                0x1992, 0x1996, 0x198A, // Large Forge
                                                0x197E, 0x19A2, 0x199E,
                                                0x2DD8, // Elven Forge
                },
                RangeMax = 2,
                Enabled = true
            };

            List<Item> forge = Items.ApplyFilter(forgeFilter);
            if (forge == null || forge.Count <= 0)
            {
                Logger.MessageBox("Must be near a forge", Logger.MESSAGEBOX_TYPE.ERROR);
                return false;
            }

            int tool = GetNeededTool(toolList, secureResourceContainerSerial);
            if (tool <= 0)
            {
                Logger.MessageBox("Unable to find the tool for recycle item", Logger.MESSAGEBOX_TYPE.ERROR);
                return false;
            }

            foreach (var toBeSmet in itemsToBeSmelt)
            {
                if (Gumps.HasGump())
                {
                    Gumps.CloseGump(Gumps.CurrentGump());
                    Common.Pause(100);
                }
                Items.UseItem(tool);
                while (!Gumps.HasGump())
                {
                    Common.Pause(10);
                }
                var gump = Gumps.CurrentGump();
                Gumps.SendAction(gump, 14); // Smelt Action
                Common.Pause(requiredPause);
                Target.WaitForTarget(delayGump);
                Target.TargetExecute(toBeSmet);
                while (!Gumps.HasGump())
                {
                    Common.Pause(10);
                }
                Gumps.CloseGump(gump);
                Common.Pause(300);
            }
            Common.Pause(300); // Not necessary but for safety I add some sleep
            return true;
        }
        private bool CutItems(List<Item> itemsToBeCut, uint requiredPause, List<int> toolList)
        {
            List<Item> tools = Common.FindItems(toolList, Player.Backpack, false, false, false);
            if (tools == null || tools.Count <= 0)
            {
                Logger.MessageBox("Unable to find the tool for recycle item", Logger.MESSAGEBOX_TYPE.ERROR);
                return false;
            }

            foreach (Item item in itemsToBeCut)
            {
                Items.UseItem(tools[0]);
                Target.WaitForTarget(delayGump);
                Target.TargetExecute(item.Serial);
                Common.Pause(requiredPause);
            }

            return true;
        }
        private void ShowBodCraftableInfo(BodCraftable craftable)
        {
            pictureBox.Image = Items.GetImage(craftable.GraphicID);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Common.Pause(100);
        }
        private void SetEnabledButtonsSelectContainers(bool enabled)
        {
            btnAddResourceContainer.Enabled = enabled;
            btnAddStoreBODsContainer.Enabled = enabled;
            progressBar.Visible = !enabled;

            if (enabled == true)
            {
                if (secureResourceContainerSerial > 0)
                {
                    btnStart.Enabled = true;
                }
            }

            Application.DoEvents();
        }
        private void SetEnabledAllButtons(bool enabled)
        {
            btnAddSingle.Enabled = enabled;
            btnDelSingle.Enabled = enabled;
            btnAddAll.Enabled = enabled;
            btnClearAll.Enabled = enabled;

            if (enabled == false)
            {
                btnAddResourceContainer.Enabled = false;
                btnAddStoreBODsContainer.Enabled = false;
            }

            btnStart.Enabled = enabled;
            btnStop.Enabled = enabled;
        }

        // Adds a bot to the list if it considered a valid BOD
        private bool AddBODtoListIfValid(Bod bod)
        {
            bool added = false;
            if (bod.Size == Bod.BodSizeEnum.SMALL) // && !bod.isFilled())
            {
                bool alreadyAdded = lstBODs.Items.OfType<Bod>().Where(i => i.Serial == bod.Serial).ToList().Count > 0;
                if (!alreadyAdded)
                {
                    lstBODs.Items.Add(bod);
                    lstBODs.SelectedIndex = 0;
                    added = true;
                }
                else
                {
                    lblStatus.Text = "This BOD is already in list";
                }
            }

            return added;
        }
        private bool CheckBeforeCrafting()
        {
            bool accessible = false;

            Item resources = Items.FindBySerial(secureResourceContainerSerial);

            if (resources != null)
            {
                if (resources.RootContainer == Player.Serial)
                {
                    accessible = true;
                }
                else
                {
                    int serial = resources.RootContainer;
                    // It's the root container
                    if (serial == 0)
                    {
                        serial = resources.Serial;
                    }

                    if (Player.InRangeItem(serial, 2) == true)

                        accessible = true;
                }
            }

            if (!accessible) return false; // Exit because secureResourceContainer is unaccessible

            if (secureFilledBodContainerSerial > 0)
            {
                accessible = false;

                Item filled = Items.FindBySerial(secureFilledBodContainerSerial);
                if (filled != null)
                {
                    if (filled.RootContainer == Player.Serial)
                    {
                        accessible = true;
                    }
                    else if (Player.InRangeItem(resources.RootContainer, 2) == true)
                    {
                        accessible = true;
                    }
                }
            }
            else
            {
                // if secureFilledBodContainer is empty player backpack will be used
                secureFilledBodContainerSerial = Player.Backpack.Serial;
            }

            return accessible;
        }
        private int GetNeededTool(List<int> itemID, int containerSerial)
        {
            foreach (int id in itemID)
            {
                // Checking 1st in player backpack
                var items = Common.FindItems(id, Player.Backpack, false, true, true);
                if (items != null && items.Count > 0)
                {
                    return items[0].Serial;
                }

                // If nothing found in player container checking in resources container
                Item container = Items.FindBySerial(containerSerial);

                foreach (var item in container.Contains)
                {
                    if (item.ItemID == id)
                    {
                        // Drag item into Player Backpack
                        Items.Move(item.Serial, Player.Backpack.Serial, 0);
                        Common.Pause(delayDragItem);
                        return item.Serial;
                    }
                }
            }
            return 0;
        }
        private bool GetNeededResources(List<BodCraftable.Resource> resources, int itemsQuantity)
        {
            List<(Item itm, int qty)> neededResources = new List<(Item i, int qty)>();

            foreach (var res in resources)
            {
                // Find item in player's backpack
                Item itm = Items.FindByID(res.graphicID, res.color, Player.Backpack.Serial);

                // If there is at least material for 1 item I skip, in this way should be a little faster
                // if ((itm == null) || itm.Amount < (res.quantity * itemsQuantity))
                if ((itm == null) || itm.Amount < res.quantity)
                {
                    // Not found in backpack or not enough amount. Try to find in resources container
                    itm = Items.FindByID(res.graphicID, res.color, secureResourceContainerSerial);
                    if ((itm == null) || itm.Amount < (res.quantity * itemsQuantity))
                    {
                        // Unable to find everywhere enough resources then return false
                        Logger.Log("Missing resource: " + res.quantity.ToString() + " " + res.material, Logger.COLORS.RED);
                        return false;
                    }
                    // If found in secureContainer then i have to move to Backpack but i do it only if all neened resources exists
                    neededResources.Add((itm, (int)(res.quantity * itemsQuantity * 1.5))); // Adding 1.5 * of needed items for fails
                                                                                           //neededResources.Add((itm, res.quantity * itemsQuantity));
                }

            }

            // All needed resources found. Now i grab from secure container
            foreach (var (itm, qty) in neededResources)
            {
                Logger.Log("Doing restock");
                // Drag item into Player Backpack
                Items.Move(itm, Player.Backpack.Serial, qty);
                Common.Pause(delayDragItem);
            }

            return true;
        }
        private void PutBackRemainingResources(List<BodCraftable.Resource> resources)
        {
            if (secureResourceContainerSerial != Player.Backpack.Serial)
            {
                foreach (var res in resources)
                {
                    Item itm = Items.FindByID(res.graphicID, res.color, Player.Backpack.Serial);
                    if (itm != null)
                    {
                        Items.Move(itm, secureResourceContainerSerial, itm.Amount);
                        Common.Pause(delayDragItem);
                    }
                }
            }

        }
        private int CountCraftedItems(int itemID, bool isCountExceptionalOnly)
        {
            List<Item> crafted = Common.FindItems(itemID, Player.Backpack, isCountExceptionalOnly, false, false);
            return crafted.Count;
        }
        private bool RecycleNotExceptionalItems(BodCraftable craftable)
        {
            List<Item> toBeRecycled = Common.FindItemsNotExceptionalInBackpack(craftable.GraphicID);
            if (toBeRecycled == null || toBeRecycled.Count <= 0)
            {
                // No item found. Probabily a fail in craft
                return true;
            }

            bool bRet;

            switch (craftable.RequiredSkill)
            {
                case BodCraftableDatabase.Skill.BLACKSMITHY:
                    bRet = SmeltItems(toBeRecycled, craftable.DelayNeededForDestroy, craftable.ToolNeededForDestroy);
                    break;
                case BodCraftableDatabase.Skill.TAILORING:
                    bRet = CutItems(toBeRecycled, craftable.DelayNeededForDestroy, craftable.ToolNeededForDestroy);
                    break;

                case BodCraftableDatabase.Skill.UNKNOWN:
                default:
                    Logger.MessageBox("Unable to unserstand the skill for Recycle " + craftable.Name, Logger.MESSAGEBOX_TYPE.ERROR);
                    bRet = false;
                    break;
            }
            return bRet;
        }
        bool CheckIfGumpHasCorrectMaterialSelected(string material)
        {
            // Checking if material in gump is correctly selected
            string craftableResource = material.ToLower();
            bool isMaterialSelected = false;
            foreach (string line in Gumps.LastGumpGetLineList())
            {
                string cleanLine = line;
                cleanLine = cleanLine.Replace("(~1_AMT~)", "").Trim();
                cleanLine = cleanLine.Replace("HIDES", "").Trim();
                cleanLine = cleanLine.Replace("/", "").Trim();

                if (cleanLine.ToLower() == craftableResource)
                {
                    isMaterialSelected = true;
                    break;
                }
            }
            return isMaterialSelected;
        }
        bool CraftItem(BodCraftable todo, int quantity, bool isExceptional)
        {
            foreach (var prop in Player.Backpack.Properties)
            {
                if (prop.ToString().Contains("125/125"))
                {
                    Logger.Log("Backpack is FULL", Logger.COLORS.RED);
                    Logger.MessageBox("Backpack is FULL", Logger.MESSAGEBOX_TYPE.ERROR);
                    return false;
                }
            }

            int tool = GetNeededTool(todo.ToolsNeeded, secureResourceContainerSerial);
            if (tool > 0)
            {
                if (!GetNeededResources(todo.ResourceList, quantity))
                {
                    // Resources not found
                    return false;
                }

                Items.UseItem(tool);
                while (!Gumps.HasGump())
                {
                    Common.Pause(10);
                }
                var gump = Gumps.CurrentGump();

                if (!CheckIfGumpHasCorrectMaterialSelected(todo.ResourceList[0].material))
                {
                    Gumps.SendAction(gump, 7); // Selecting Material List
                    Gumps.WaitForGump(gump, delayGump);
                    Gumps.SendAction(gump, BodCraftableDatabase.Instance.GumpButtonForMaterial(todo.ResourceList[0].material));
                    Gumps.WaitForGump(gump, delayGump);
                    Common.Pause(300);
                }

                Journal.Clear();
                Gumps.SendAction(gump, todo.GumpButtons.category);
                Gumps.WaitForGump(gump, delayGump);
                Gumps.SendAction(gump, todo.GumpButtons.selection);

                // Instead of a wait for gump i wait untill a gump is present and I check if the item has worn out
                int safeexit = 150; // 30"
                while (!Gumps.HasGump() && safeexit > 0)
                {
                    safeexit--;
                    Common.Pause(100);
                    if (Journal.Search("have worn out your"))
                    {
                        break;
                    }
                }
                if (safeexit <= 0) return false; // Something gone wrong

                // TODO: Change this pause counting items in backpack instead
                Common.Pause(400); // Need to wait for item to appear in backpack

                if (isExceptional && !Common.FindTextInLastGump("you create an exceptional quality item"))
                {
                    Logger.Log("Crafted a non exceptional item. Try to recycle it");
                    bool done = RecycleNotExceptionalItems(todo);
                    if (!done) return false;
                    Common.Pause(500);
                }

                progressBar.Value++;
                Application.DoEvents();
            }
            else
            {
                Logger.Log("Unable to find tools");
                return false;
            }

            return true;
        }
        bool CraftBodItems(Bod bod)
        {
            if (bod.Size != Bod.BodSizeEnum.SMALL) return false; // Just to be sure is not a Large Bod

            BodCraftable todo = bod.Craftables[0].ItemToDo;
            int amountDone = bod.Craftables[0].qty;

            if (amountDone >= bod.AmountMax)
            {
                Logger.Log("Bod already filled");
                return true;
            }

            progressBar.Maximum = bod.AmountMax;

            bool done = false;
            while (!done && !forcedStop)
            {
                Common.Pause(10); // Safety Sleep

                int crafted = CountCraftedItems(todo.GraphicID, bod.IsExceptional);
                if (crafted + amountDone >= bod.AmountMax)
                {
                    done = true;
                    break;
                }
                progressBar.Value = crafted;

                lblCrafting.Text = todo.Name + ((bod.IsExceptional == true) ? " [E]" : "") + ": " + (amountDone + crafted + 1).ToString() + "/" + bod.AmountMax.ToString();
                Logger.Log("Crafting: " + lblCrafting.Text);

                //Number of items written into the bod - items aldready added to bod - items crafted in backpack
                bool status = CraftItem(todo, bod.AmountMax - amountDone - crafted, bod.IsExceptional);
                if (status == false)
                {
                    break;
                }

                //Pause(delayUseItem);
                Application.DoEvents();
            }
            // TODO: Controllare la rottura dello strumento

            if (done == false)
            {
                // Something wrong exit
                return false;
            }

            // Move resources back if still on backpack
            PutBackRemainingResources(todo.ResourceList);
            Gumps.CloseGump(Gumps.CurrentGump());
            return true;
        }
        bool FillTheBod(Bod bod)
        {
            if (bod.IsFilled) { return true; }

            Common.Pause(1000); // Just a safe wait

            int qtyInBod = bod.Craftables[0].qty;

            List<Item> crafted = Common.FindItems(bod.Craftables[0].ItemToDo.GraphicID, Player.Backpack, bod.IsExceptional, false);

            if (crafted == null)
            {
                Logger.Log("Unable to fill the bod", Logger.COLORS.RED);
                return false;
            }
            else if (crafted.Count <= 0)
            {
                return true;
            }

            Items.UseItem(bod.Serial);
            while (!Gumps.HasGump())
            {
                Common.Pause(10);
            }
            var gump = Gumps.CurrentGump();

            // For fill a bod I need to count how much items i have to target
            // If they are less than needed i fill them anyway
            // The targeting procedure is a little tricky:
            //   - the combine button must be "pressed" only one time then target untill the bod is filled
            //   - at the last target the waitfortarget would fail so i target n-1 items with wait then
            //      the last is only an execute without wait forever

            Gumps.SendAction(gump, 2); // Combine Button
            Target.WaitForTarget(delayGump);

            int numOfTargets = Math.Min(crafted.Count, bod.AmountMax - qtyInBod);
            for (int i = 0; i < numOfTargets - 1; i++)
            {
                Target.TargetExecute(crafted[i].Serial);
                Target.WaitForTarget(delayGump);
            }
            Target.TargetExecute(crafted[numOfTargets - 1].Serial);
            Common.Pause(500); // Safe sleep

            Gumps.SendAction(gump, 1); // Close the fill bod gump

            if (qtyInBod + crafted.Count < bod.AmountMax)
            {
                Logger.Log("WARNING: BOD NOT FILLED", Logger.COLORS.RED);
                return false;
            }

            bod.UpdateBodInfos();

            return true;
        }
        bool StoreTheBod(Bod bod)
        {
            if (secureFilledBodContainerSerial <= 0)
            {
                return true;
            }

            Item bodItem = Items.FindBySerial(bod.Serial);
            if (bodItem != null)
            {
                Common.Pause(500); // Safe sleep
                Items.Move(bod.Serial, secureFilledBodContainerSerial, 1);
                Common.Pause(500);
                if (Gumps.HasGump())
                {
                    Gumps.CloseGump(Gumps.CurrentGump());
                }
            }
            else
            {
                Logger.Log("Unable to store the bod", Logger.COLORS.RED);
                return false;
            }

            return true;
        }

        private void StartCrafting()
        {
            // Verificare che il fill container sia ancora nel player.
            // if ((target.ItemID == 0x2259) && (target.RootContainer != Player.Serial))

            progressBar.Visible = true;
            progressBar.Maximum = lstBODs.Items.Count;
            progressBar.Value = 0;
            lblStatus.Text = "Crafting";
            Application.DoEvents();

            if (CheckBeforeCrafting() == false)
            {
                lblStatus.Text = "Secure container is inaccessible";
                Logger.LogHead("Secure container is inaccessible", Logger.COLORS.RED);
                SetEnabledButtonsSelectContainers(true);
                btnStart.Enabled = true;
                return;
            }

            if (secureResourceContainerSerial != Player.Backpack.Serial)
            {
                Items.UseItem(secureResourceContainerSerial);
                Common.Pause(delayUseItem);
            }

            // I craft always the first of the list so I don't use any counter
            // index is always 0 and I remove the crafted that is in poistion 0 untill the collection is empty
            while (lstBODs.Items.Count > 0)
            {
                Bod bod = (Bod)lstBODs.Items[0];
                lstBODs.SelectedIndex = 0;

                var status = CraftBodItems(bod);
                if (status == false)
                {
                    Logger.MessageBox("Unable to fill the bod", Logger.MESSAGEBOX_TYPE.ERROR);
                    break;
                }
                else
                {
                    FillTheBod(bod);
                    StoreTheBod(bod);
                    lstBODs.Items.RemoveAt(0);
                }
            }

            progressBar.Visible = false;
            lblStatus.Text = "Done";
            lblCrafting.Text = "";
            lstBODs.Items.Clear();
            SetEnabledAllButtons(true);
            btnStart.Enabled = false;
            btnStop.Enabled = false;
        }
    }

}

