// Container Inspector
// This scripts inspects a container and show items properties in a table
// 
// Copyright Caporale Simone - 2024

//#forcedebug 
//#assembly <Newtonsoft.Json.dll>

/*
Troubleshooting for the error: "Error (CS0006) at line 0: Metadata file 'Newtonsoft.Json.dll' could not be found"
1- Locate the file Assemblies.cfg in the RazorEnhanced folder
2- Open the file with a text editor
3- Add Newtonsoft.Json.dll at the end of the file and save
4- Delete the line #assembly <Newtonsoft.Json.dll> from this script
5- Restart Razor Enhanced and the script
 */

/*
    Version 1.0: 
        - Initial release (Tnx to @Denzen)
    Version 1.1:
        - Removed the container check because jewelry boxes are not considered containers (Valid for UoAlive server). (Tnx to @BigDa)
    Version 1.2:
        - Now, when the filter is applied, only the properties actually available are shown and not all of them
        - Fixed a bug on the filter button. Now the filter should work correctly.
    Version 1.3:
        - Added a button to export the table to a JSON file.
        - Added a button to import the table from a JSON file.
        - Now scanning a new container will not clear the table but items will be added to the existing list
        - Added a button to clear the table
        - Added a button to show hidden columns
        - If an item is too far (more than 2 tiles), a message will be shown and the root container will say "I'm the container"
    Version 1.4:
        - Changed the exported JSON structure to include columns visibility.
    Version 1.5:
        - Changed the exported JSON structure to include columns order.
    Version 1.6:
        - Previous export and import button renamed to Save and Load Project.
        - Added a button to export the table into a CSV file.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace RazorEnhanced
{
    class ContainerInspector : Form
    {
        private const string version = "1.6";

        #region User Interface
        private readonly System.Drawing.Font defaultFontRegular = new("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        private readonly System.Drawing.Font defaultFontBold = new("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        private Button cmdScanContainer;
        private DataGridView dataGrid;
        private Button cmdGetSelected;
        private PictureBox pictureBoxSelectedObj;
        private CheckBox checkRecursiveScan;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatus_Result;
        private CheckedListBox checkedListBox_ColumnsFilter;
        private Label lblColumnsFilter;
        private Button cmdFilter;
        private RadioButton radioButton_OR;
        private RadioButton radioButton_AND;
        private RichTextBox Txt_SelectedItmProp;
        private DataTable originalTableWithoutFilters;
        private Button cmdSaveProject;
        private Button cmdImportProject;
        #endregion

        #region Global Variables
        private List<UOObject> scannedItemsList = new();
        private List<UOObject> scannedItemsList_CopyForFiltering = new();
        private Button cmdShowHiddenColumns;
        private Button cmdClearTable;
        private Button cmdExportCSV;
        private int specialColumnsCount;
        #endregion

        #region ExportedSturctures
        private class ColumnInfo
        {
            public byte ColumnIndex { get; set; }
            public byte DisplayIndex { get; set; }
            public string ColumnName { get; set; }
            public bool IsVisible { get; set; }
        }

        private class ExportedData
        {
            public List<ColumnInfo> ColumnsInfo { get; set; }
            public List<UOObject> ItemsTable { get; set; }
        }
        #endregion

        #region Constructor, Run and Initializations
        public ContainerInspector()
        {
            InitializeComponent();
            this.Text = this.Text + " - " + Player.Name + " - Version: " + version;
        }
        public void Run()
        {
            this.Icon = System.Drawing.Icon.FromHandle(Items.GetImage(0x2235, 0).GetHicon());
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
            this.checkRecursiveScan = new System.Windows.Forms.CheckBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus_Result = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkedListBox_ColumnsFilter = new System.Windows.Forms.CheckedListBox();
            this.lblColumnsFilter = new System.Windows.Forms.Label();
            this.cmdFilter = new System.Windows.Forms.Button();
            this.radioButton_OR = new System.Windows.Forms.RadioButton();
            this.radioButton_AND = new System.Windows.Forms.RadioButton();
            this.Txt_SelectedItmProp = new System.Windows.Forms.RichTextBox();
            this.cmdSaveProject = new System.Windows.Forms.Button();
            this.cmdImportProject = new System.Windows.Forms.Button();
            this.cmdShowHiddenColumns = new System.Windows.Forms.Button();
            this.cmdClearTable = new System.Windows.Forms.Button();
            this.cmdExportCSV = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectedObj)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdScanContainer
            // 
            this.cmdScanContainer.Location = new System.Drawing.Point(12, 12);
            this.cmdScanContainer.Name = "cmdScanContainer";
            this.cmdScanContainer.Size = new System.Drawing.Size(133, 26);
            this.cmdScanContainer.TabIndex = 0;
            this.cmdScanContainer.Text = "Scan Container";
            this.cmdScanContainer.UseVisualStyleBackColor = true;
            this.cmdScanContainer.Click += new System.EventHandler(this.CmdScanContainer_Click);
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(174, 44);
            this.dataGrid.MultiSelect = false;
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersWidth = 51;
            this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGrid.Size = new System.Drawing.Size(1072, 665);
            this.dataGrid.TabIndex = 1;
            this.dataGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGrid_CellFormatting);
            this.dataGrid.ColumnHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGrid_ColumnHeaderMouseDoubleClick);
            this.dataGrid.SelectionChanged += new System.EventHandler(this.DataGrid_SelectionChanged);
            // 
            // cmdGetSelected
            // 
            this.cmdGetSelected.Location = new System.Drawing.Point(153, 12);
            this.cmdGetSelected.Name = "cmdGetSelected";
            this.cmdGetSelected.Size = new System.Drawing.Size(120, 26);
            this.cmdGetSelected.TabIndex = 2;
            this.cmdGetSelected.Text = "Grab Selected";
            this.cmdGetSelected.UseVisualStyleBackColor = true;
            this.cmdGetSelected.Click += new System.EventHandler(this.CmdGetSelected_Click);
            // 
            // pictureBoxSelectedObj
            // 
            this.pictureBoxSelectedObj.BackColor = System.Drawing.Color.Black;
            this.pictureBoxSelectedObj.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxSelectedObj.Location = new System.Drawing.Point(49, 44);
            this.pictureBoxSelectedObj.Name = "pictureBoxSelectedObj";
            this.pictureBoxSelectedObj.Size = new System.Drawing.Size(73, 73);
            this.pictureBoxSelectedObj.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSelectedObj.TabIndex = 3;
            this.pictureBoxSelectedObj.TabStop = false;
            // 
            // checkRecursiveScan
            // 
            this.checkRecursiveScan.AutoSize = true;
            this.checkRecursiveScan.Checked = true;
            this.checkRecursiveScan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecursiveScan.Location = new System.Drawing.Point(412, 13);
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
            this.statusStrip.Location = new System.Drawing.Point(0, 712);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1258, 32);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatus_Result
            // 
            this.toolStripStatus_Result.Name = "toolStripStatus_Result";
            this.toolStripStatus_Result.Size = new System.Drawing.Size(60, 25);
            this.toolStripStatus_Result.Text = "Ready";
            // 
            // checkedListBox_ColumnsFilter
            // 
            this.checkedListBox_ColumnsFilter.FormattingEnabled = true;
            this.checkedListBox_ColumnsFilter.Location = new System.Drawing.Point(12, 416);
            this.checkedListBox_ColumnsFilter.Name = "checkedListBox_ColumnsFilter";
            this.checkedListBox_ColumnsFilter.Size = new System.Drawing.Size(155, 234);
            this.checkedListBox_ColumnsFilter.TabIndex = 7;
            // 
            // lblColumnsFilter
            // 
            this.lblColumnsFilter.AutoSize = true;
            this.lblColumnsFilter.Location = new System.Drawing.Point(8, 393);
            this.lblColumnsFilter.Name = "lblColumnsFilter";
            this.lblColumnsFilter.Size = new System.Drawing.Size(110, 20);
            this.lblColumnsFilter.TabIndex = 8;
            this.lblColumnsFilter.Text = "Columns Filter";
            this.lblColumnsFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmdFilter
            // 
            this.cmdFilter.Location = new System.Drawing.Point(14, 676);
            this.cmdFilter.Name = "cmdFilter";
            this.cmdFilter.Size = new System.Drawing.Size(151, 33);
            this.cmdFilter.TabIndex = 9;
            this.cmdFilter.Text = "Apply Filter";
            this.cmdFilter.UseVisualStyleBackColor = true;
            this.cmdFilter.Click += new System.EventHandler(this.CmdFilter_Click);
            // 
            // radioButton_OR
            // 
            this.radioButton_OR.AutoSize = true;
            this.radioButton_OR.Checked = true;
            this.radioButton_OR.Location = new System.Drawing.Point(14, 655);
            this.radioButton_OR.Name = "radioButton_OR";
            this.radioButton_OR.Size = new System.Drawing.Size(58, 24);
            this.radioButton_OR.TabIndex = 10;
            this.radioButton_OR.TabStop = true;
            this.radioButton_OR.Text = "OR";
            this.radioButton_OR.UseVisualStyleBackColor = true;
            this.radioButton_OR.CheckedChanged += new System.EventHandler(this.RadioButton_OR_CheckedChanged);
            // 
            // radioButton_AND
            // 
            this.radioButton_AND.AutoSize = true;
            this.radioButton_AND.Location = new System.Drawing.Point(100, 655);
            this.radioButton_AND.Name = "radioButton_AND";
            this.radioButton_AND.Size = new System.Drawing.Size(68, 24);
            this.radioButton_AND.TabIndex = 11;
            this.radioButton_AND.Text = "AND";
            this.radioButton_AND.UseVisualStyleBackColor = true;
            this.radioButton_AND.CheckedChanged += new System.EventHandler(this.RadioButton_AND_CheckedChanged);
            // 
            // Txt_SelectedItmProp
            // 
            this.Txt_SelectedItmProp.BackColor = System.Drawing.Color.Black;
            this.Txt_SelectedItmProp.Location = new System.Drawing.Point(12, 123);
            this.Txt_SelectedItmProp.Name = "Txt_SelectedItmProp";
            this.Txt_SelectedItmProp.Size = new System.Drawing.Size(156, 267);
            this.Txt_SelectedItmProp.TabIndex = 12;
            this.Txt_SelectedItmProp.Text = "";
            // 
            // cmdSaveProject
            // 
            this.cmdSaveProject.Location = new System.Drawing.Point(801, 11);
            this.cmdSaveProject.Name = "cmdSaveProject";
            this.cmdSaveProject.Size = new System.Drawing.Size(120, 26);
            this.cmdSaveProject.TabIndex = 13;
            this.cmdSaveProject.Text = "Save Project";
            this.cmdSaveProject.UseVisualStyleBackColor = true;
            this.cmdSaveProject.Click += new System.EventHandler(this.CmdSaveProject_Click);
            // 
            // cmdImportProject
            // 
            this.cmdImportProject.Location = new System.Drawing.Point(927, 11);
            this.cmdImportProject.Name = "cmdImportProject";
            this.cmdImportProject.Size = new System.Drawing.Size(120, 26);
            this.cmdImportProject.TabIndex = 14;
            this.cmdImportProject.Text = "Import Project";
            this.cmdImportProject.UseVisualStyleBackColor = true;
            this.cmdImportProject.Click += new System.EventHandler(this.CmdImportProject_Click);
            // 
            // cmdShowHiddenColumns
            // 
            this.cmdShowHiddenColumns.Location = new System.Drawing.Point(608, 11);
            this.cmdShowHiddenColumns.Name = "cmdShowHiddenColumns";
            this.cmdShowHiddenColumns.Size = new System.Drawing.Size(187, 26);
            this.cmdShowHiddenColumns.TabIndex = 15;
            this.cmdShowHiddenColumns.Text = "Show Hidden Columns";
            this.cmdShowHiddenColumns.UseVisualStyleBackColor = true;
            this.cmdShowHiddenColumns.Click += new System.EventHandler(this.CmdShowHiddenColumns_Click);
            // 
            // cmdClearTable
            // 
            this.cmdClearTable.Location = new System.Drawing.Point(279, 11);
            this.cmdClearTable.Name = "cmdClearTable";
            this.cmdClearTable.Size = new System.Drawing.Size(120, 26);
            this.cmdClearTable.TabIndex = 16;
            this.cmdClearTable.Text = "Clear Table";
            this.cmdClearTable.UseVisualStyleBackColor = true;
            this.cmdClearTable.Click += new System.EventHandler(this.CmdClearTable_Click);
            // 
            // cmdExportCSV
            // 
            this.cmdExportCSV.Location = new System.Drawing.Point(1126, 11);
            this.cmdExportCSV.Name = "cmdExportCSV";
            this.cmdExportCSV.Size = new System.Drawing.Size(120, 26);
            this.cmdExportCSV.TabIndex = 17;
            this.cmdExportCSV.Text = "Export CSV";
            this.cmdExportCSV.UseVisualStyleBackColor = true;
            this.cmdExportCSV.Click += new System.EventHandler(this.cmdExportCSV_Click);
            // 
            // ContainerInspector
            // 
            this.ClientSize = new System.Drawing.Size(1258, 744);
            this.Controls.Add(this.cmdExportCSV);
            this.Controls.Add(this.cmdClearTable);
            this.Controls.Add(this.cmdShowHiddenColumns);
            this.Controls.Add(this.cmdImportProject);
            this.Controls.Add(this.cmdSaveProject);
            this.Controls.Add(this.Txt_SelectedItmProp);
            this.Controls.Add(this.radioButton_AND);
            this.Controls.Add(this.radioButton_OR);
            this.Controls.Add(this.cmdFilter);
            this.Controls.Add(this.lblColumnsFilter);
            this.Controls.Add(this.checkedListBox_ColumnsFilter);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.checkRecursiveScan);
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
                AutoPopDelay = 2000,
                InitialDelay = 50,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip_checkRecursiveScan.SetToolTip(this.checkRecursiveScan, "Search inside all containers");
            
            ToolTip toolTipUnHideButton = new()
            {
                AutoPopDelay = 2000,
                InitialDelay = 50,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip_checkRecursiveScan.SetToolTip(this.cmdShowHiddenColumns, "To hide a column double click the header");

            // Initialize DataGrid
            ConfigureDataGrid();
        }
        private void ConfigureDataGrid()
        {
            // Imposta colonne della griglia.
            dataGrid.Columns.Clear();

            // This will speedup grid rendering
            Type dgvType = dataGrid.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dataGrid, true, null);

            dataGrid.ColumnHeadersDefaultCellStyle.Font = defaultFontBold;
            dataGrid.Font = defaultFontRegular;
            dataGrid.BackgroundColor = System.Drawing.Color.DarkGray;
            dataGrid.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGrid.DefaultCellStyle.BackColor = System.Drawing.Color.Black;
            dataGrid.AllowUserToOrderColumns = true;
        }
        #endregion

        #region Buttons Events Click
        private void CmdScanContainer_Click(object sender, EventArgs e)
        {
            var window_state = this.WindowState;
            this.WindowState = FormWindowState.Minimized;

            TargetContainerAndUpdateObjectsList(checkRecursiveScan.Checked);
            UpdateAllUI();
            originalTableWithoutFilters = (dataGrid.DataSource as DataTable).Copy();

            this.WindowState = window_state;
        }
        private void CmdGetSelected_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count == 0) return;

            var row = dataGrid.SelectedRows;
            int serial = Convert.ToInt32(row[0].Cells["Serial"].Value.ToString(), 16);
            Item item = Items.FindBySerial(serial);

            var distance = Player.DistanceTo(item);
            if (distance > 2)
            {
                Player.HeadMessage(33, "Item is too far away");
                Items.Message(item.RootContainer, 33, "I'm the container");
                return;
            }

            Items.Move(serial, Player.Backpack.Serial, 1);
            dataGrid.Rows.Remove(row[0]);
        }
        private void CmdFilter_Click(object sender, EventArgs e)
        {
            List<string> selectedProperties = new();

            for (int i = 0; i < checkedListBox_ColumnsFilter.Items.Count; i++)
            {
                if (checkedListBox_ColumnsFilter.GetItemChecked(i))
                {
                    selectedProperties.Add(checkedListBox_ColumnsFilter.Items[i].ToString());
                }
            }

            List<DataRow> rowsToHide = new();

            if (selectedProperties.Count > 0)
            {
                DataTable table = originalTableWithoutFilters.Copy();

                if (radioButton_AND.Checked)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (string property in selectedProperties)
                        {
                            if (row[property].ToString() == "")
                            {
                                rowsToHide.Add(row);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataRow row in table.Rows)
                    {
                        bool bValid = false;
                        foreach (string property in selectedProperties)
                        {
                            string value = row[property].ToString();
                            if (value != "")
                            {
                                bValid = true;
                                break;
                            }
                        }
                        if (!bValid) rowsToHide.Add(row);
                    }
                }
            }

            scannedItemsList = new(scannedItemsList_CopyForFiltering);

            foreach (var row in rowsToHide)
            {
                string serial = row["Serial"].ToString();
                scannedItemsList.RemoveAll(item => item.Serial == serial);
            }

            UpdateAllUI();
        }
        private void CmdSaveProject_Click(object sender, EventArgs e)
        {
            string file = FileInputBox.Show("Save this project into a file", "Insert filename that will be stored in DATA\nfolder of Razor Enhanced.\n", "ContainerInspectorProject.json", ".json", this);
            if (string.IsNullOrEmpty(file)) return;

            var columnsInfo = new List<ColumnInfo>();
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                columnsInfo.Add(new ColumnInfo
                {
                    ColumnIndex = (byte)column.Index,
                    DisplayIndex = (byte)column.DisplayIndex,
                    ColumnName = column.Name,
                    IsVisible = column.Visible
                });
            }

            var exportedData = new ExportedData
            {
                ColumnsInfo = columnsInfo,
                ItemsTable = scannedItemsList
            };

            string _data_folder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            FileInfo fileName = new(Path.Combine(_data_folder, file));

            //string json = JsonConvert.SerializeObject(scannedItemsList, Formatting.Indented);
            string json = JsonConvert.SerializeObject(exportedData, Formatting.Indented);

            File.WriteAllText(fileName.FullName, json);
            MessageBox.Show("JSON File exported in path: " + fileName.FullName, "File Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void CmdImportProject_Click(object sender, EventArgs e)
        {
            string file = FileInputBox.Show("Load an existing project from file", "Insert filename that will be imported\nfrom DATA folder of Razor Enhanced.\n", "ContainerInspectorProject.json", ".json", this);
            if (string.IsNullOrEmpty(file)) return;

            string _data_folder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            FileInfo fileName = new(Path.Combine(_data_folder, file));

            if (File.Exists(fileName.FullName))
            {
                string json = File.ReadAllText(fileName.FullName);

                var deserializedData = JsonConvert.DeserializeObject<ExportedData>(json);

                scannedItemsList = deserializedData.ItemsTable;
                scannedItemsList_CopyForFiltering = new List<UOObject>(scannedItemsList);
                UpdateAllUI();

                // Recover columns visibility
                foreach (var columnInfo in deserializedData.ColumnsInfo)
                {
                    dataGrid.Columns[columnInfo.ColumnIndex].Visible = columnInfo.IsVisible;
                    dataGrid.Columns[columnInfo.ColumnIndex].DisplayIndex = columnInfo.DisplayIndex;
                }

                MessageBox.Show("JSON File Imported", "File Imported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("File not found: " + fileName.FullName, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CmdShowHiddenColumns_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                if (!column.Visible)
                {
                    column.Visible = true;
                }
            }
        }
        private void CmdClearTable_Click(object sender, EventArgs e)
        {
            scannedItemsList.Clear();
            UpdateAllUI();
            originalTableWithoutFilters = (dataGrid.DataSource as DataTable).Copy();
        }
        private void cmdExportCSV_Click(object sender, EventArgs e)
        {
            string file = FileInputBox.Show("Export data into a CSV file", "Insert filename that will be stored in DATA\nfolder of Razor Enhanced.\n", "ContainerInspectorExport.csv", ".csv", this);
            if (string.IsNullOrEmpty(file)) return;

            DataTable table = (DataTable)dataGrid.DataSource;

            StringBuilder csvContent = new StringBuilder();

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            csvContent.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                csvContent.AppendLine(string.Join(",", fields));
            }

            string _data_folder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            FileInfo fileName = new(Path.Combine(_data_folder, file));
            File.WriteAllText(fileName.FullName, csvContent.ToString());
        }

        #endregion

        #region Other Events
        private void DataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                try
                {
                    string serial = dataGrid.SelectedRows[0].Cells["Serial"].Value.ToString();
                    UOObject foundItem = scannedItemsList.Find(match: item => item.Serial == serial);
                    pictureBoxSelectedObj.Image = Items.GetImage(foundItem.ItemID, foundItem.Hue);

                    Txt_SelectedItmProp.Clear();
                    Txt_SelectedItmProp.Text = "";
                    Txt_SelectedItmProp.SelectionAlignment = HorizontalAlignment.Center;
                    Txt_SelectedItmProp.SelectionFont = defaultFontBold;
                    Txt_SelectedItmProp.SelectionColor = Color.Yellow;
                    Txt_SelectedItmProp.AppendText(foundItem.Name + "\n");
                    Txt_SelectedItmProp.SelectionFont = defaultFontRegular;

                    for (int i = 0; i < foundItem.Properties.Count; i++)
                    {
                        string rawProp = foundItem.Properties[i].RawDescription;
                        Txt_SelectedItmProp.SelectionStart = Txt_SelectedItmProp.TextLength;
                        Txt_SelectedItmProp.SelectionColor = Color.White;
                        Txt_SelectedItmProp.SelectionLength = 0;
                        Txt_SelectedItmProp.AppendText(rawProp + "\n");
                    }

                    Txt_SelectedItmProp.SelectionStart = Txt_SelectedItmProp.TextLength;
                    Txt_SelectedItmProp.SelectionLength = 0;
                    Txt_SelectedItmProp.SelectionColor = System.Drawing.ColorTranslator.FromHtml(foundItem.QualityColor);
                    Txt_SelectedItmProp.AppendText(foundItem.Quality + "\n");
                }
                catch
                {

                }
            }
        }
        private void DataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dataGrid.Columns[e.ColumnIndex].Name == "Name")
            {
                if (e.Value != null)
                {
                    e.CellStyle.Font = defaultFontBold;
                }
            }

            if (this.dataGrid.Columns[e.ColumnIndex].Name == "Quality")
            {
                if (e.Value != null)
                {
                    string qualityColor = dataGrid.Rows[e.RowIndex].Cells["QualityColor"].Value.ToString();
                    e.CellStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml(qualityColor);
                    e.CellStyle.Font = defaultFontBold;
                }
            }
        }
        private void DataGrid_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGrid.Columns[e.ColumnIndex].Visible = false;
        }
        private void BulkItemsInspector_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            dataGrid.Size = new System.Drawing.Size(control.Size.Width - 200, control.Size.Height - 100);
        }
        private void RadioButton_AND_CheckedChanged(object sender, EventArgs e)
        {
            radioButton_OR.Checked = !radioButton_AND.Checked;
        }
        private void RadioButton_OR_CheckedChanged(object sender, EventArgs e)
        {
            radioButton_AND.Checked = !radioButton_OR.Checked;
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
                public bool IsPercent { get; set; }
                public string RawDescription { get; set; }
            }

            public string Serial { get; set; }
            public string Name { get; set; }
            public string Quality { get; set; } = null;
            public string QualityColor { get; set; } = null;
            public int Hue { get; set; }
            public int ItemID { get; set; }
            public string Layer { get; set; } // Place where item is used: Ring, Bracelet, Torso ...s
            public List<ObjectProperty> Properties { get; set; } = new List<ObjectProperty>();
            public UOObject(Item item)
            {
                if (item == null) return;

                List<string> properties = item.Properties.Select(p =>
                {
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToString().ToLower());
                }).ToList();

                Serial = "0x" + item.Serial.ToString("X");
                Layer = item.Layer;
                Name = properties[0]; // Name is always the first property
                Hue = item.Hue;
                ItemID = item.ItemID;

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

                    // Ignore error o unexpected properties. Eg: Chivalry book that shows "10 Spells"
                    // in this case I'm expecting a string before a number and the regex fails with an empty string
                    // I don't care must about this kind of properties
                    if (objectProperty.PropertyName == "") continue;

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
            //scannedItemsList.Clear();
            Item container = Items.FindBySerial(new Target().PromptTarget("Select a container"));

            //Removed the container check because jewelry boxes are not considered containers
            if (container == null)
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

                    UOObject item = new(itm);
                    scannedItemsList.Add(item);
                }
            }

            // Create a copy of the list to be used for filtering
            scannedItemsList_CopyForFiltering = new List<UOObject>(scannedItemsList);

            if (scannedItemsList.Count == 0)
            {
                Misc.SendMessage("Nothing found", 33);
                return;
            }

        }
        #endregion

        #region Other UI Functions
        private void UpdateAllUI()
        {
            ResetDetailsPanel();
            RefreshDataGrid();
            UpdateCheckedListBoxFilters();
        }
        private void RefreshDataGrid()
        {
            Player.HeadMessage(33, "Creating Table...");
            DataTable table = new();

            table.Columns.Add("Serial", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Layer", typeof(string));
            table.Columns.Add("Quality", typeof(string));
            table.Columns.Add("QualityColor", typeof(string));
            specialColumnsCount = 4; // Quality is not considered a special column

            foreach (UOObject item in scannedItemsList)
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
            dataGrid.Columns["QualityColor"].Visible = false;

            // This messy section is used to sort the columns in a specific order
            /////////////////////////////////////////
            var list = from DataGridViewColumn c in dataGrid.Columns
                       orderby c.HeaderText == "Serial" ? 0 :
                               c.HeaderText == "Name" ? 1 :
                               c.HeaderText == "Layer" ? 2 :
                               c.HeaderText == "Quality" ? 3 :
                               c.HeaderText == "QualityColor" ? 4 :
                               5, c.HeaderText
                       select c;

            int i = 0;
            foreach (DataGridViewColumn c in list)
            {
                c.DisplayIndex = i++;
            }
            /////////////////////////////////////////

            dataGrid.Visible = true;

            if (dataGrid.Rows.Count > 0)
                dataGrid.Rows[0].Selected = true;

            UpdateStatusBar();
        }
        private void ResetDetailsPanel()
        {
            pictureBoxSelectedObj.Image = null;
            Txt_SelectedItmProp.Clear();
        }
        private void UpdateStatusBar()
        {
            toolStripStatus_Result.Text = "Items found: " + scannedItemsList.Count + " - Proerties: " + (dataGrid.ColumnCount - specialColumnsCount).ToString();
        }
        private void UpdateCheckedListBoxFilters()
        {
            // Store checked items
            List<string> checkedItems = new();
            foreach (var item in checkedListBox_ColumnsFilter.CheckedItems)
            {
                checkedItems.Add(item.ToString());
            }

            checkedListBox_ColumnsFilter.Items.Clear();

            // Get the list of column names from the dataGrid
            List<string> columnNames = dataGrid.Columns.Cast<DataGridViewColumn>()
                .Select(column => column.Name)
                .OrderBy(name => name)
                .ToList();

            foreach (var column in columnNames)
            {
                List<string> columnsToIgnore = new() { "Serial", "Name", "Layer", "Quality", "QualityColor" };
                if (columnsToIgnore.Contains(column)) continue;

                bool checkedItem = checkedItems.Contains(column);
                checkedListBox_ColumnsFilter.Items.Add(column, checkedItem);
            }
        }
        #endregion

        #region FileInputBox
        public class FileInputBox
        {
            public static string Show(string title, string description, string text, string fileExtension, Form parentForm)
            {
                using var form = new Form();
                form.Icon = System.Drawing.Icon.FromHandle(Items.GetImage(0x2235, 0).GetHicon());
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedSingle;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                form.Text = title;
                form.Width = 250;
                form.Height = 130;

                var label = new Label()
                {
                    Text = description,
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    TextAlign = ContentAlignment.TopLeft,
                    Width = form.Width - 20,
                    //Margin = new Padding(10, 10, 10, 10)
                };

                var textBox = new TextBox()
                {
                    Dock = DockStyle.Fill,
                    Text = text,
                    Top = 20
                    
                };

                var okButton = new Button()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Left,
                    Width = 100
                };

                var cancelButton = new Button()
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Dock = DockStyle.Right,
                    Width = 100
                };

                var buttonPanel = new Panel()
                {
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                buttonPanel.Controls.Add(okButton);
                buttonPanel.Controls.Add(cancelButton);

                form.Controls.Add(textBox);
                form.Controls.Add(label);
                form.Controls.Add(buttonPanel);

                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    if (textBox.Text.EndsWith(fileExtension))
                    {
                        return textBox.Text;
                    }
                    else
                    {
                        return textBox.Text + fileExtension;
                    }
                }

                return null;
            }
        }



        #endregion
    }
}
