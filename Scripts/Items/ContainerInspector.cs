// Container Inspector
// This scripts inspects a container and show items properties in a table
// 
// Copyright Caporale Simone - 2024

//-#-f-o-r-cedebug 
using Assistant;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RazorEnhanced
{
    class ContainerInspector : Form
    {
        #region User Interface
        private Button cmdScanContainer;
        private DataGridView dataGrid;
        private Button cmdGetSelected;
        private PictureBox pictureBoxSelectedObj;
        private ListBox selectedPropList;

        private readonly System.Drawing.Font defaultFontRegular = new("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        private readonly System.Drawing.Font defaultFontBold = new("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        private CheckBox checkRecursiveScan;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatus_Result;
        #endregion

        #region Global Variables
        private readonly List<UOObject> scannedItemsList = new();
        #endregion

        #region Constructor, Run and Initializations
        public ContainerInspector()
        {
            InitializeComponent();
            this.Text = this.Text + " - " + Player.Name;
        }
        public void Run()
        {
            InitializeOtherFormComponentsAndEvents();

            Application.EnableVisualStyles();
            Application.Run(this); // This is blocking. Will return only when form is closed
        }
        private void InitializeComponent()
        {
            this.cmdScanContainer = new System.Windows.Forms.Button();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.cmdGetSelected = new System.Windows.Forms.Button();
            this.pictureBoxSelectedObj = new System.Windows.Forms.PictureBox();
            this.selectedPropList = new System.Windows.Forms.ListBox();
            this.checkRecursiveScan = new System.Windows.Forms.CheckBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus_Result = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectedObj)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdScanContainer
            // 
            this.cmdScanContainer.Location = new System.Drawing.Point(14, 12);
            this.cmdScanContainer.Name = "cmdScanContainer";
            this.cmdScanContainer.Size = new System.Drawing.Size(133, 26);
            this.cmdScanContainer.TabIndex = 0;
            this.cmdScanContainer.Text = "Scan Container";
            this.cmdScanContainer.UseVisualStyleBackColor = true;
            // 
            // dataGrid
            // 
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(174, 44);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.RowHeadersWidth = 51;
            this.dataGrid.Size = new System.Drawing.Size(877, 547);
            this.dataGrid.TabIndex = 1;
            // 
            // cmdGetSelected
            // 
            this.cmdGetSelected.Location = new System.Drawing.Point(153, 12);
            this.cmdGetSelected.Name = "cmdGetSelected";
            this.cmdGetSelected.Size = new System.Drawing.Size(109, 26);
            this.cmdGetSelected.TabIndex = 2;
            this.cmdGetSelected.Text = "Get Selected";
            this.cmdGetSelected.UseVisualStyleBackColor = true;
            // 
            // pictureBoxSelectedObj
            // 
            this.pictureBoxSelectedObj.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxSelectedObj.Location = new System.Drawing.Point(57, 55);
            this.pictureBoxSelectedObj.Name = "pictureBoxSelectedObj";
            this.pictureBoxSelectedObj.Size = new System.Drawing.Size(50, 50);
            this.pictureBoxSelectedObj.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxSelectedObj.TabIndex = 3;
            this.pictureBoxSelectedObj.TabStop = false;
            // 
            // selectedPropList
            // 
            this.selectedPropList.FormattingEnabled = true;
            this.selectedPropList.ItemHeight = 20;
            this.selectedPropList.Location = new System.Drawing.Point(13, 123);
            this.selectedPropList.Name = "selectedPropList";
            this.selectedPropList.Size = new System.Drawing.Size(155, 444);
            this.selectedPropList.TabIndex = 4;
            // 
            // checkRecursiveScan
            // 
            this.checkRecursiveScan.AutoSize = true;
            this.checkRecursiveScan.Checked = true;
            this.checkRecursiveScan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecursiveScan.Location = new System.Drawing.Point(268, 14);
            this.checkRecursiveScan.Name = "checkRecursiveScan";
            this.checkRecursiveScan.Size = new System.Drawing.Size(146, 24);
            this.checkRecursiveScan.TabIndex = 5;
            this.checkRecursiveScan.Text = "Recursive Scan";
            this.checkRecursiveScan.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus_Result});
            this.statusStrip.Location = new System.Drawing.Point(0, 575);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1072, 32);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatus_Result
            // 
            this.toolStripStatus_Result.Name = "toolStripStatus_Result";
            this.toolStripStatus_Result.Size = new System.Drawing.Size(60, 25);
            this.toolStripStatus_Result.Text = "Ready";
            // 
            // ContainerInspector
            // 
            this.ClientSize = new System.Drawing.Size(1072, 607);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.checkRecursiveScan);
            this.Controls.Add(this.selectedPropList);
            this.Controls.Add(this.pictureBoxSelectedObj);
            this.Controls.Add(this.cmdGetSelected);
            this.Controls.Add(this.dataGrid);
            this.Controls.Add(this.cmdScanContainer);
            this.Name = "ContainerInspector";
            this.Text = "Container Inspector";
            this.Resize += new System.EventHandler(this.BulkItemsInspector_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectedObj)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void InitializeOtherFormComponentsAndEvents()
        {
            // Tooltips
            ToolTip toolTip_checkRecursiveScan = new()
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip_checkRecursiveScan.SetToolTip(this.checkRecursiveScan, "Search inside all containers");

            // Initialize DataGrid
            ConfigureDataGrid();

            // Events
            this.cmdGetSelected.Click += new System.EventHandler(this.CmdGetSelected_Click);
            this.cmdScanContainer.Click += new System.EventHandler(this.CmdScanContainer_Click);
            this. dataGrid.SelectionChanged += new System.EventHandler(this.DataGrid_SelectionChanged);
        }
        private void ConfigureDataGrid()
        {
            // Imposta colonne della griglia.
            dataGrid.Columns.Clear();

            // This will speedup grid rendering
            Type dgvType = dataGrid.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dataGrid, true, null);

            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; // Imposta la larghezza delle colonne in modo che contenga tutto il testo
            dataGrid.ColumnHeadersDefaultCellStyle.Font = defaultFontBold;
            dataGrid.Font = defaultFontRegular;
            dataGrid.BackgroundColor = System.Drawing.Color.DarkGray;
            dataGrid.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGrid.DefaultCellStyle.BackColor = System.Drawing.Color.Black;
            dataGrid.ReadOnly = true;
        }
        #endregion

        #region Buttons Events Click
        private void CmdScanContainer_Click(object sender, EventArgs e)
        {
            var window_state = this.WindowState;
            this.WindowState = FormWindowState.Minimized;
            
            ResetLeftPanel();
            TargetContainerAndUpdateObjectsList(checkRecursiveScan.Checked);
            RefreshDataGrid();
            UpdateStatusBar();

            this.WindowState = window_state;
        }
        private void CmdGetSelected_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count == 0) return;

            var row = dataGrid.SelectedRows;
            string serial = row[0].Cells["Serial"].Value.ToString();
            Items.Move(Convert.ToInt32(serial, 16), Player.Backpack.Serial, 1);
            dataGrid.Rows.Remove(row[0]);
        }
        #endregion

        #region Other Events
        private void DataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                try
                {
                    Item selected = Items.FindBySerial(Convert.ToInt32(dataGrid.SelectedRows[0].Cells["Serial"].Value.ToString(), 16));
                    pictureBoxSelectedObj.Image = Items.GetImage(selected.ItemID);
                    //pictureBoxSelectedObj.SizeMode = PictureBoxSizeMode.StretchImage;

                    selectedPropList.Items.Clear();
                    foreach (var prop in selected.Properties)
                    {
                        string rawProp = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(prop.ToString().ToLower());
                        if (rawProp.ToLower().Contains("<basefont"))
                        {
                            int start = rawProp.IndexOf(">") + 1;
                            int end = rawProp.Length;
                            rawProp = rawProp.Substring(start, end - start).Trim();
                        }
                        selectedPropList.Items.Add(rawProp);
                    }
                }
                catch
                {

                }
            }
        }
        private void BulkItemsInspector_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            dataGrid.Size = new System.Drawing.Size(control.Size.Width - 200, control.Size.Height - 100);
        }
        #endregion

        #region Items Data Model
        private class UOObject
        {
            public class ObjectProperty
            {
                public string PropertyName { get; set; }
                public int Value { get; set; } = 0;
                public int MaxValue { get; set; } = 0;
                public bool IsFlag { get; set; }
                public bool IsPercent { get; set;}
                public string RawDescription { get; set; }
            }

            public string Serial { get; }
            public string Name { get; }
            public string Quality { get; } = null;
            public string QualityColor { get; } = null;
            public string Layer { get; } // Place where item is used: Ring, Bracelet, Torso ...s
            public List<ObjectProperty> Properties { get; } = new List<ObjectProperty>();
            public UOObject(string serial, string layer, List<string> properties)
            {
                Serial = serial;
                Layer = layer;
                Name = properties[0]; // Name is always the first property

                // Check Quality and QualityColor
                string lastProperty = properties[properties.Count - 1];
                if (lastProperty.ToLower().Contains("<basefont"))
                {
                    int start = lastProperty.IndexOf(">") + 1;
                    int end = lastProperty.Length;
                    Quality = lastProperty.Substring(start, end - start).Trim();

                    int colorStart = lastProperty.ToLower().IndexOf("color=") + 6;
                    int colorEnd = lastProperty.IndexOf(">");
                    QualityColor = lastProperty.Substring(colorStart, colorEnd - colorStart).ToLower();
                }

                int numProperties = lastProperty.ToLower().Contains("<basefont") ? properties.Count - 1 : properties.Count;

                // Set Properties
                for (int i = 1; i < numProperties; i++)
                {
                    if (properties[i].Contains("crafted by") || properties[i].Contains("recovered from")) continue; // Skip "crafted by", "recovered from"

                    string property = properties[i];
                    UOObject.ObjectProperty objectProperty = new()
                    {
                        PropertyName = Regex.Match(property, @"^[^\d:+-]+").Value.Trim(), // Set PropertyName
                        IsFlag = true, // Set IsBoolean by default true
                        IsPercent = property.Contains("%"),
                        RawDescription = property
                    };

                    // Set Value and MaxValue
                    MatchCollection matches = Regex.Matches(property, @"[-+]?\d+");
                    if (matches.Count == 1)
                    {
                        //objectProperty.Value = property.EndsWith("%") ? matches[0].Value + "%" : matches[0].Value;
                        objectProperty.Value = int.Parse(matches[0].Value);
                        objectProperty.IsFlag = false;
                    }
                    else if (matches.Count == 2)
                    {
                        objectProperty.Value = int.Parse(matches[0].Value);
                        objectProperty.MaxValue = int.Parse(matches[1].Value);
                        objectProperty.IsFlag = false;
                    }

                    Properties.Add(objectProperty);
                }
            }
        }
        #endregion

        #region Other private functions
        private List<Item> FindItems(Item container, bool recursive = true)
        {
            List<Item> itemList = new();

            foreach (Item item in container.Contains)
            {
                //if (itemIDs.Contains(item.ItemID))
                {
                    itemList.Add(item);
                }
            }

            if (recursive)
            {
                List<Item> subcontainers = container.Contains.Select(sublist => sublist).Where(item => item.IsContainer).ToList();

                foreach (Item bag in subcontainers)
                {
                    if (bag.ItemID == 0x2259) { continue; } // If is a Book of BOD skip
                    Items.UseItem(bag);
                    Misc.Pause(800);
                    List<Item> itemInSubContainer = FindItems(bag, true);
                    itemList.AddRange(itemInSubContainer);
                }
            }

            return itemList;
        }
        private void TargetContainerAndUpdateObjectsList(bool recursiveScan)
        {
            scannedItemsList.Clear();
            Item container = Items.FindBySerial(new Target().PromptTarget("Select a container"));

            if ((container == null) || ((container.IsContainer == false) && (container.IsCorpse == false)))
            {
                Player.HeadMessage(33, "This is not a valid container. Select only containers or corpses");
                return;
            }

            Items.UseItem(container);
            Misc.Pause(800);
            Player.HeadMessage(33, "Scanning container...");
            var found = FindItems(container, recursiveScan);
            if (found != null && found.Count > 0)
            {
                foreach (var itm in found)
                {
                    if (itm.IsContainer) { continue; } // Skip other containers
                    if (itm.ItemID == 0x2259) { continue; } // Skip book of bods

                    string serial = "0x" + itm.Serial.ToString("X");
                    string layer = itm.Layer.ToString();
                    UOObject item = new(serial, layer, itm.Properties.Select(p =>
                    {
                        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToString().ToLower());
                    }).ToList());

                    scannedItemsList.Add(item);
                }
            }

            if (scannedItemsList.Count == 0)
            {
                Misc.SendMessage("Nothing found", 33);
                return;
            }

        }
        #endregion

        #region Other UI Functions
        private void RefreshDataGrid()
        {
            Player.HeadMessage(33, "Creating Table...");
            DataTable table = new();

            table.Columns.Add("Serial", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Layer", typeof(string));
            table.Columns.Add("Quality", typeof(string));
            table.Columns.Add("QualityColor", typeof(string));

            Player.HeadMessage(33, "Updating Columns...");
            foreach (UOObject item in scannedItemsList.Cast<UOObject>())
            {
                DataRow row = table.NewRow();
                row["Serial"] = item.Serial;
                row["Name"] = item.Name;
                row["Layer"] = item.Layer;
                row["Quality"] = item.Quality;
                row["QualityColor"] = item.QualityColor;

                foreach (var prop in item.Properties)
                {
                    string colName = prop.PropertyName + (prop.IsPercent ? " %" : "");
                    if (!table.Columns.Contains(colName))
                    {
                        table.Columns.Add(colName, prop.IsFlag == true ? typeof(bool) : prop.Value.GetType());
                    }
                    row[colName] = (prop.IsFlag) ? true : prop.Value;
                }

                table.Rows.Add(row);
            }

            dataGrid.Visible = false;
            dataGrid.DataSource = table;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.ReadOnly = true;

            Player.HeadMessage(33, "Last Fixes...");
            dataGrid.Columns["QualityColor"].Visible = false;

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.Cells["QualityColor"].Value == null) continue;
                //if (row.Cells["QualityColor"].Value.ToString() == "") continue;
                row.Cells["Quality"].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(row.Cells["QualityColor"].Value.ToString());
            }

            dataGrid.Columns["Quality"].DefaultCellStyle.Font = defaultFontBold;
            dataGrid.Visible = true;

            if (dataGrid.Rows.Count > 0)
                dataGrid.Rows[0].Selected = true;
        }
        private void ResetLeftPanel()
        {
            pictureBoxSelectedObj.Image = null;
            selectedPropList.Items.Clear();
        }
        private void UpdateStatusBar()
        {
            int number_of_fixed_columsn = 4; // Serial, Name, Layer, QualityColor (hidden)
            toolStripStatus_Result.Text = "Items found: " + scannedItemsList.Count + " - Proerties: " + (dataGrid.ColumnCount - number_of_fixed_columsn).ToString();
        }

        #endregion
    }
}
