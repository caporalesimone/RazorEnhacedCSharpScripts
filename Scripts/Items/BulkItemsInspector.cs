//C#

//
// Bulk Item Inspector
//  This scripts inspects a container and show items properties in a table
// 
// Developed by SimonSoft on Demise Server - 2021
//

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
    class BulkItemsInspector : Form
    {
        private Button cmdScanContainer;
        private DataGridView dataGrid;
        private Button cmdGetSelected;
        private List<object> lstData = new List<object>();
        private Item container;

        public BulkItemsInspector()
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
            this.cmdScanContainer.Click += new System.EventHandler(this.cmdScanContainer_Click);
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
            this.cmdGetSelected.Click += new System.EventHandler(this.cmdGetSelected_Click);
            // 
            // BulkItemsInspector
            // 
            this.ClientSize = new System.Drawing.Size(1063, 607);
            this.Controls.Add(this.cmdGetSelected);
            this.Controls.Add(this.dataGrid);
            this.Controls.Add(this.cmdScanContainer);
            this.Name = "BulkItemsInspector";
            this.Text = "Bulk Items Inspector";
            this.Resize += new System.EventHandler(this.BulkItemsInspector_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        public DataTable ToDataTable<T>(dynamic items)
        {

            DataTable dtDataTable = new DataTable();
            if (items.Count == 0) return dtDataTable;

            ((IEnumerable)items[0]).Cast<dynamic>().Select(p => p.Name).ToList().ForEach(col => { dtDataTable.Columns.Add(col); });

            ((IEnumerable)items).Cast<dynamic>().ToList().
                ForEach(data =>
                {
                    DataRow dr = dtDataTable.NewRow();
                    ((IEnumerable)data).Cast<dynamic>().ToList().ForEach(Col => { dr[Col.Name] = Col.Value; });
                    dtDataTable.Rows.Add(dr);
                });
            return dtDataTable;
        }

        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }


        private void cmdScanContainer_Click(object sender, EventArgs e)
        {
            DataTable table = new DataTable();

            Dictionary<string, Tuple<string, System.Type>> replace_str = new Dictionary<string, Tuple<string, System.Type>>()
            {
                { "faster casting", new Tuple<string, System.Type>("FC", typeof(int)) },
                { "faster cast recovery", new Tuple<string, System.Type>("FCR", typeof(int)) },
                { "spell damage increase", new Tuple<string, System.Type>("Spell Damage", typeof(int)) },

                { "lower mana cost", new Tuple<string, System.Type>("LMC", typeof(int))},
                { "mana regenration", new Tuple<string, System.Type>("Mana Regen.", typeof(int))},
                { "stamina regenration", new Tuple<string, System.Type>("Stamina Regen.", typeof(int))},

                { "lower reagent cost", new Tuple<string, System.Type>("LRC", typeof(int))},

                { "luck", new Tuple<string, System.Type>("luck", typeof(int))},

                { "night sight", new Tuple<string, System.Type>("Night Sight", typeof(bool))},
                { "blessed", new Tuple<string, System.Type>("Blessed", typeof(bool))},

                { "physical resist", new Tuple<string, System.Type>("Phys. Res.", typeof(int)) },
                { "fire resist", new Tuple<string, System.Type>("Fire Res.", typeof(int)) },
                { "cold resist", new Tuple<string, System.Type>("Cold Res.", typeof(int)) },
                { "poison resist", new Tuple<string, System.Type>("Poison Res.", typeof(int)) },
                { "energy resist", new Tuple<string, System.Type>("Energy Res.", typeof(int)) },

                { "damage increase", new Tuple<string, System.Type>("damage", typeof(int)) },
                { "hit chance increase", new Tuple<string, System.Type>("hit %", typeof(int)) },

                { "defense chance increase", new Tuple<string, System.Type>("defence", typeof(int)) },

                { "enhance potions", new Tuple<string, System.Type>("Enhance pots", typeof(int)) },

                { "strength bonus", new Tuple<string, System.Type>("STR", typeof(int)) },
                { "intelligence bonus", new Tuple<string, System.Type>("INT", typeof(int)) },
                { "dexterity bonus", new Tuple<string, System.Type>("DEX", typeof(int)) },

                { "hit point increase", new Tuple<string, System.Type>("Hit", typeof(int)) },
                { "mana increase", new Tuple<string, System.Type>("Mana", typeof(int)) },
                { "stamina increase", new Tuple<string, System.Type>("Stamina", typeof(int)) },

                { "self repair", new Tuple<string, System.Type>("Repair", typeof(int)) },

                { "hit point regeneration", new Tuple<string, System.Type>("Hit. Regen.", typeof(int)) },
                { "stamina regeneration", new Tuple<string, System.Type>("Stam. Regen.", typeof(int)) },
                { "mana regeneration", new Tuple<string, System.Type>("Mana Regen.", typeof(int)) },

                { "reflect physical damage", new Tuple<string, System.Type>("Reflect Phy.", typeof(int)) },

                { "lower requirements", new Tuple<string, System.Type>("Lower Req.", typeof(int)) },
                { "mage weapon", new Tuple<string, System.Type>("Mage Weap.", typeof(string)) },
                { "mage armor", new Tuple<string, System.Type>("Mage Armor", typeof(bool)) },

                { "spell channeling", new Tuple<string, System.Type>("Spell Chann.", typeof(bool)) },

                { "one-handed weapon", new Tuple<string, System.Type>("1 Handed", typeof(bool)) },
                { "two-handed weapon", new Tuple<string, System.Type>("2 Handed", typeof(bool)) },

                { "physical damage", new Tuple<string, System.Type>("Phy Dmg", typeof(int)) },
                { "cold damage", new Tuple<string, System.Type>("Cold Dmg", typeof(int)) },
                { "fire damage", new Tuple<string, System.Type>("Fire Dmg", typeof(int)) },
                { "weapon damage", new Tuple<string, System.Type>("Weapon Dmg", typeof(string)) },
                { "poison damage", new Tuple<string, System.Type>("Poison Dmg", typeof(int)) },
                { "energy damage", new Tuple<string, System.Type>("Energy Dmg", typeof(int)) },
                { "chaos damage", new Tuple<string, System.Type>("Chaos Dmg", typeof(int)) },

                { "weapon speed", new Tuple<string, System.Type>("Weapon Speed", typeof(string)) },

                { "hit energy area", new Tuple<string, System.Type>("Hit Energy Area", typeof(int)) },
                { "hit physical area", new Tuple<string, System.Type>("Hit Phi. Area", typeof(int)) },
                { "hit poison area", new Tuple<string, System.Type>("Poison Area", typeof(int)) },
                { "hit cold area", new Tuple<string, System.Type>("Cold Area", typeof(int)) },
                { "hit fire area", new Tuple<string, System.Type>("Cold Area", typeof(int)) },

                { "hit fireball", new Tuple<string, System.Type>("Hit Fireball", typeof(int)) },
                { "hit magic arrow", new Tuple<string, System.Type>("Hit Magic Arrow", typeof(int)) },
                { "hit dispel", new Tuple<string, System.Type>("Hit Dispel", typeof(int)) },
                { "hit lightning", new Tuple<string, System.Type>("Hit Lightning", typeof(int)) },
                { "hit harm", new Tuple<string, System.Type>("Hit Harm", typeof(int)) },

                { "hit mana leech", new Tuple<string, System.Type>("Mana Leech", typeof(int)) },
                { "hit life leech", new Tuple<string, System.Type>("Life Leech", typeof(int)) },
                { "hit stamina leech", new Tuple<string, System.Type>("Stamina Leech", typeof(int)) },

                { "hit lower defense", new Tuple<string, System.Type>("Hit Lower Def.", typeof(int)) },
                { "hit lower attack", new Tuple<string, System.Type>("Hit Lower Atk.", typeof(int)) },

                { "swing speed increase", new Tuple<string, System.Type>("Swing Speed Increase", typeof(int)) },

                { "balanced", new Tuple<string, System.Type>("Balanced", typeof(bool)) },

                { "exceptional", new Tuple<string, System.Type>("Except.", typeof(bool)) },
                { "elves only", new Tuple<string, System.Type>("Elves", typeof(bool)) },
                { "use best weapon skill", new Tuple<string, System.Type>("Best Skill", typeof(bool)) },

                { "cursed", new Tuple<string, System.Type>("Cursed", typeof(bool)) },

                { "slayer", new Tuple<string, System.Type>("Slayer", typeof(string)) },
                { "spells", new Tuple<string, System.Type>("Spells", typeof(int)) },
                { "uses remaining:", new Tuple<string, System.Type>("Uses", typeof(int)) },
                { "charges:", new Tuple<string, System.Type>("Charges", typeof(string)) }, // Not int because wands pros is for example: Greater Healing charges: 99

                { "range", new Tuple<string, System.Type>("Range", typeof(int)) },
                { "velocity", new Tuple<string, System.Type>("Velocity", typeof(int)) },

                { "alchemy bonus:", new Tuple<string, System.Type>("Alchemy", typeof(int)) },
                { "tailoring bonus:", new Tuple<string, System.Type>("Tailor", typeof(int)) },

                { "skill:", new Tuple<string, System.Type>("Bonus", typeof(string)) },

                { "skill required:", new Tuple<string, System.Type>("Skill", typeof(string)) },
                { "artifact rarity", new Tuple<string, System.Type>("Art. Rarity", typeof(int)) },
                

            };

            // Carica i dati nella lista
            container = Items.FindBySerial(new Target().PromptTarget("Select a container"));
            if (container != null && (container.IsContainer || container.IsCorpse))
            {
                Items.UseItem(container);
                Misc.Pause(800);
                var found = FindItems(container, true); // Bracelet
                if (found != null && found.Count > 0)
                {
                    table.Columns.Add("Serial", typeof(string));
                    table.Columns.Add("Name", typeof(string));

                    foreach (var itm in found)
                    {
                        if (itm.Properties.Count < 4) { continue; }
                        if (itm.IsContainer) { continue; }
                        if (itm.ItemID == 0x2259) { continue; } // Skip book of bods

                        DataRow row = table.NewRow();
                        row["Serial"] = "0x" + itm.Serial.ToString("X");
                        row["Name"] = itm.Properties[0].ToString();
                        

                        for (int i = 1; i < itm.Properties.Count; i++)
                        {
                            System.Type type = typeof(int);
                            string str = itm.Properties[i].ToString().ToLower();
                            string column = "";

                            var skip = new[] { "insured", "weight", "crafted by" ,
                                               "strength requirement", "durability", "contents",
                                               "recovered from",
                                               "deeds in book:", "book name:" };

                            if (skip.Any(str.Contains))
                            {
                                continue;
                            }

                            if (str.Contains("+"))
                            {
                                column = "Skill";
                                type = typeof(string);
                            }
                            else
                            {
                                foreach (var key in replace_str.Keys)
                                {
                                    if (str.Contains(key))
                                    {
                                        column = replace_str[key].Item1;
                                        type = replace_str[key].Item2;
                                        str = str.Replace("%", "");
                                        str = str.Replace(key, "");
                                        break;
                                    }
                                }
                            }

                            if (column != "")
                            {
                                if (!table.Columns.Contains(column))
                                {
                                    table.Columns.Add(column, type);
                                }
                                if (type == typeof(bool))
                                {
                                    row[column] = true;
                                }
                                else
                                {
                                    row[column] = str;
                                }
                            }
                            else
                            {
                                Misc.SendMessage(itm.Name + " unknown property " + str, 33);
                            }


                        }
                        table.Rows.Add(row);
                    }

                }

            }

            if (table.Rows.Count == 0)
            {
                Misc.SendMessage("Nothing found", 33);
                return;
            }

            dataGrid.DataSource = table;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.ReadOnly = true;

            foreach (DataGridViewColumn col in dataGrid.Columns)
            {
                foreach (var key in replace_str)
                {
                    if (key.Value.Item1 == col.Name)
                    {
                        col.ToolTipText = Regex.Replace(key.Key, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells ;
                        continue;
                    }
                }
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
        }

        private void BulkItemsInspector_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            dataGrid.Size = new System.Drawing.Size(control.Size.Width - 50, control.Size.Height - 100);
        }

        private void cmdGetSelected_Click(object sender, EventArgs e)
        {
            if (container == null) return;
            if (dataGrid.SelectedRows.Count == 0) return;

            var row = dataGrid.SelectedRows;
            string serial = row[0].Cells["Serial"].Value.ToString();
            Items.Move(Convert.ToInt32(serial, 16), Player.Backpack.Serial, 1);
        }
    }
}
