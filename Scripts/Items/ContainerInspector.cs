//C#

//
// Bulk Item Inspector
//  This scripts inspects a container and show items properties in a table
// 
// Developed by SimonSoft on Demise Server - 2021
//
//

//#forcedebug 
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
        private Button cmdScanContainer;
        private DataGridView dataGrid;
        private Button cmdGetSelected;
        private readonly List<object> lstData = new List<object>();
        private Item container;

        private System.Drawing.Font defaultFontRegular = new System.Drawing.Font("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        private System.Drawing.Font defaultFontBold = new System.Drawing.Font("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        public ContainerInspector()
        {
            InitializeComponent();
            this.Text = this.Text + " - " + Player.Name;
        }

        public void Run()
        {
            ConfigureTable();

            Application.EnableVisualStyles();
            Application.Run(this); // This is blocking. Will return only when form is closed
        }

        private void InitializeComponent()
        {
            this.cmdScanContainer = new System.Windows.Forms.Button();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.cmdGetSelected = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
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
            this.cmdScanContainer.Click += new System.EventHandler(this.CmdScanContainer_Click);
            // 
            // dataGrid
            // 
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(12, 44);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.RowHeadersWidth = 51;
            this.dataGrid.Size = new System.Drawing.Size(1039, 551);
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
            this.cmdGetSelected.Click += new System.EventHandler(this.CmdGetSelected_Click);
            // 
            // ContainerInspector
            // 
            this.ClientSize = new System.Drawing.Size(1063, 607);
            this.Controls.Add(this.cmdGetSelected);
            this.Controls.Add(this.dataGrid);
            this.Controls.Add(this.cmdScanContainer);
            this.Name = "ContainerInspector";
            this.Text = "Container Inspector";
            this.Resize += new System.EventHandler(this.BulkItemsInspector_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        private void CmdScanContainer_Click(object sender, EventArgs e)
        {
            lstData.Clear();
            container = Items.FindBySerial(new Target().PromptTarget("Select a container"));
            if (container != null && (container.IsContainer || container.IsCorpse))
            {
                Items.UseItem(container);
                Misc.Pause(800);
                Player.HeadMessage(33, "Scanning container...");
                var found = FindItems(container, true);
                if (found != null && found.Count > 0)
                {
                    foreach (var itm in found)
                    {
                        if (itm.IsContainer) { continue; } // Skip containers
                        if (itm.ItemID == 0x2259) { continue; } // Skip book of bods

                        var item = ParseProperties(itm.Properties.Select(p =>
                        {
                            string propName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToString().ToLower());
                            var skip = new[] { "crafted by", "recovered from" };
                            if (skip.Any(propName.Contains)) return null; // Skiping unuseful properties
                            return propName;
                        }).Where(p => p != null).ToList());

                        item.Serial = "0x" + itm.Serial.ToString("X");
                        lstData.Add(item);
                    }
                }

                Player.HeadMessage(33, "Creating Table...");
                if (lstData.Count == 0)
                {
                    Misc.SendMessage("Nothing found", 33);
                    return;
                }

                DataTable table = new DataTable();
                
                table.Columns.Add("Serial", typeof(string));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Quality", typeof(string));
                table.Columns.Add("QualityColor", typeof(string));

                Player.HeadMessage(33, "Updating Columns...");
                foreach (UOObject item in lstData.Cast<UOObject>())
                {
                    DataRow row = table.NewRow();
                    row["Serial"] = item.Serial;
                    row["Name"] = item.Name;
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
                    if (row.Cells["Serial"].Value == null)
                        continue;

                    var serial = row.Cells["Serial"].Value;
                    var name = row.Cells["Name"].Value;

                    if (row.Cells["QualityColor"].Value.ToString() == "") continue;
                    row.Cells["Quality"].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(row.Cells["QualityColor"].Value.ToString());
                }

                dataGrid.Columns["Quality"].DefaultCellStyle.Font = defaultFontBold;
                dataGrid.Visible = true;
            }

        }

        private List<Item> FindItems(Item container, bool recursive = true)
        {
            List<Item> itemList = new List<Item>();

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

        private void ConfigureTable()
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

        private void BulkItemsInspector_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            dataGrid.Size = new System.Drawing.Size(control.Size.Width - 50, control.Size.Height - 100);
        }

        private void CmdGetSelected_Click(object sender, EventArgs e)
        {
            if (container == null) return;
            if (dataGrid.SelectedRows.Count == 0) return;

            var row = dataGrid.SelectedRows;
            string serial = row[0].Cells["Serial"].Value.ToString();
            Items.Move(Convert.ToInt32(serial, 16), Player.Backpack.Serial, 1);
        }

        private class UOObject
        {
            public class ObjectProperty
            {
                public string PropertyName { get; set; }
                public int Value { get; set; } = 0;
                public int MaxValue { get; set; } = 0;
                public bool IsFlag { get; set; }
                public bool IsPercent { get; set;}
            }

            public string Serial { get; set; }
            public string Name { get; set; }
            public string Quality { get; set; }
            public string QualityColor { get; set; }
            public List<ObjectProperty> Properties { get; set; } = new List<ObjectProperty>();
        }

        private static UOObject ParseProperties(List<string> properties)
        {
            UOObject uoObject = new()
            {
                Name = properties[0] // Set Name
            };

            // Set Quality and QualityColor
            string lastProperty = properties[properties.Count - 1];
            if (lastProperty.ToLower().Contains("<basefont"))
            {
                int start = lastProperty.IndexOf(">") + 1;
                int end = lastProperty.Length;
                uoObject.Quality = lastProperty.Substring(start, end - start).Trim();

                int colorStart = lastProperty.ToLower().IndexOf("color=") + 6;
                int colorEnd = lastProperty.IndexOf(">");
                uoObject.QualityColor = lastProperty.Substring(colorStart, colorEnd - colorStart).ToLower();
            }
            else
            {
                uoObject.Quality = null;
                uoObject.QualityColor = null;
            }

            int numProperties = lastProperty.ToLower().Contains("<basefont") ? properties.Count - 1 : properties.Count;

            // Set Properties
            for (int i = 1; i < numProperties; i++)
            {
                string property = properties[i];
                UOObject.ObjectProperty objectProperty = new()
                {
                    PropertyName = Regex.Match(property, @"^[^\d:+-]+").Value.Trim(), // Set PropertyName
                    IsFlag = true, // Set IsBoolean by default true
                    IsPercent = property.Contains("%")
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

                uoObject.Properties.Add(objectProperty);
            }

            return uoObject;
        }
    }
}
