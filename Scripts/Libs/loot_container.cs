// You can store your loot list in the StoredData.json file stored in Data folder or RE.
// You can choose to have a global lootlist valid for all characters or a
// character specific lootlist.

// Run the script the first time to let it create a default loot list and a character specific list.
// Then edit the file StoredData.json in Data folder or RE to add or remove items.

using System;
using System.Collections.Generic;
using System.Linq;

//#forcedebug
//#import <stored_data.cs>

namespace RazorEnhanced
{
    internal class LootContainer
    {
        private const int CHEST_MAX_DISTANCE = 1;
        private const int MOVE_ITEMS_DELAY = 700;
        private readonly StoredData json_storedData = new StoredData();

        private readonly int lootBagSerial = 0;
        private readonly List<LootInfo> wantedLootList = new List<LootInfo>();

        private class LootInfo
        {
            public class Property
            {
                public string Name { get; set; } = "";
                public string Value { get; set; } = "";
                public Property(string Name, string Value) 
                {
                    this.Name = Name;
                    this.Value = Value;
                }
            }

            public int ItemID { get; set; } = 0;
            public string Name { get; set; } = "";
            public List<Property> Properties { get; set; } = new List<Property>();
            public LootInfo() { }
            public LootInfo(int itemID)
            {
                ItemID = itemID;
            }
            public void AddProperty(string property, string value)
            {
                Properties.Add(new Property(property, value));
            }
        }

        public LootContainer()
        {
            lootBagSerial = json_storedData.GetData<int>("LootChest.LootBagSerial", StoredData.StoreType.Character);
            if (lootBagSerial == 0)
            {
                Player.HeadMessage(33, "Select a loot bag");
                Item lootBag = Items.FindBySerial(new Target().PromptTarget());
                lootBagSerial = lootBag.Serial;
                if (lootBag == null || !lootBag.IsContainer)
                {
                    Player.HeadMessage(33, "Invalid loot bag selected. Using Backpack this time");
                    lootBagSerial = Player.Backpack.Serial;
                }
                else
                {
                    json_storedData.StoreData(lootBagSerial, "LootChest.LootBagSerial", StoredData.StoreType.Character);
                }
            }

            // Joins Server LootList with a Character specific LootList if exists
            var serverList = json_storedData.GetData<List<LootInfo>>("LootChest.LootList", StoredData.StoreType.Server);
            var charList = json_storedData.GetData<List<LootInfo>>("LootChest.LootList", StoredData.StoreType.Character);

            if (serverList != null)
            {
                wantedLootList.AddRange(serverList);
            }

            if (charList != null)
            {
                wantedLootList.AddRange(charList);
            }

            //wantedLootList.Clear(); // TODO: Remove this line when you have finished testing. only for debug

            // If there is no loot list, add a default one
            if (wantedLootList.Count <= 0)
            {
                wantedLootList.Add(new LootInfo(0x0F0F) { Name = "Star Sapphire" });
                wantedLootList.Add(new LootInfo(0x0F10) { Name = "Emerald" });
                wantedLootList.Add(new LootInfo(0x0F11) { Name = "Sapphire" });
                wantedLootList.Add(new LootInfo(0x0F13) { Name = "Ruby" });
                wantedLootList.Add(new LootInfo(0x0F15) { Name = "Citrine" });
                wantedLootList.Add(new LootInfo(0x0F16) { Name = "Amethyst" });
                wantedLootList.Add(new LootInfo(0x0F18) { Name = "Tourmaline" });
                wantedLootList.Add(new LootInfo(0x0F19) { Name = "Sapphire" });
                wantedLootList.Add(new LootInfo(0x0F21) { Name = "Star Sapphire" });
                wantedLootList.Add(new LootInfo(0x0F25) { Name = "Amber" });
                wantedLootList.Add(new LootInfo(0x0F26) { Name = "Diamond" });

                wantedLootList.Add(new LootInfo(0x0EF3) { Name = "Blank Scroll" });
                wantedLootList.Add(new LootInfo(0x1F59) { Name = "Mark" });
                wantedLootList.Add(new LootInfo(0x1F41) { Name = "Telekinesis" });

                json_storedData.StoreData(wantedLootList, "LootChest.LootList", StoredData.StoreType.Server);

                // Adding gold to the character specific list.
                // This is just an example of how to add items to the character specific list.
                var charSpecificList = new List<LootInfo>
                {
                    new LootInfo() { Name = "Gold Coin" }, // Gold
                    new LootInfo() { Name = "Bracelet", Properties = new List<LootInfo.Property> { new LootInfo.Property("Lesser Artifact","")} } // Bracelet
                };

                //charSpecificList.Add(new LootInfo() { Name = "Bracelet" });
                //charSpecificList.Last().AddProperty("test", "prova");

                json_storedData.StoreData(charSpecificList, "LootChest.LootList", StoredData.StoreType.Character);
                wantedLootList.AddRange(charSpecificList);
            }
        }

        // Must be public so can be called from other scripts
        public bool Loot(int chestSerial)
        {
            Player.HeadMessage(50, "Starting Looting");

            Item chest = Items.FindBySerial(chestSerial);
            if (chest == null || !chest.IsContainer)
            {
                Player.HeadMessage(33, "Target is not a container");
                return false;
            }

            if (Distance(Player.Position, chest.GetWorldPosition()) > CHEST_MAX_DISTANCE)
            {
                Player.HeadMessage(33, "Chest is too far. You must be closer");
                return false;
            }

            Items.UseItem(chest);
            Misc.Pause(800);

            List<Item> chestItems = FindItems(chest, true);
            Misc.Pause(200);

            foreach (Item loot in chestItems)
            {
                if (MustBeLooted(loot))
                {
                    Player.HeadMessage(33, "Looting: " + loot.Name);
                    Items.Move(loot.Serial, lootBagSerial, 0);
                    Misc.Pause(MOVE_ITEMS_DELAY);
                }
            }
            return true;
        }

        private static int Distance(Point3D from, Point3D to)
        {
            int xDelta = Math.Abs(from.X - to.X);
            int yDelta = Math.Abs(from.Y - to.Y);

            return xDelta > yDelta ? xDelta : yDelta;
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

        private bool MustBeLooted(Item item)
        {
            foreach (LootInfo lootInfo in wantedLootList)
            {
                bool validID = false; 
                bool validName = false;
                bool validProperties = false;

                // If ID is 0 means Ignore ID so it will pass
                if (lootInfo.ItemID == 0) validID = true;
                // If Name is empty means Ignore Name so it will pass
                if (string.IsNullOrEmpty(lootInfo.Name)) validID = true;
                // If no properties means Ignore Properties so it will pass
                if (lootInfo.Properties.Count == 0) validProperties = true;

                if ((lootInfo.ItemID == item.ItemID) || (lootInfo.ItemID == 0))
                {
                    validID = true;
                }

                if (lootInfo.Name != "" && item.Name.ToLower().Contains(lootInfo.Name.ToLower()))
                {
                    validName = true;
                }

                if (lootInfo.Properties.Count > 0)
                {
                    int cntValidProps = 0;
                    foreach (LootInfo.Property property in lootInfo.Properties)
                    {
                        if (item.Properties.Any(prop => prop.ToString().ToLower().Contains(property.Name.ToLower())))
                        {
                            // Property without a value then take it withot any other check
                            if (string.IsNullOrEmpty(property.Value))
                            {
                                cntValidProps++;
                                continue;
                            }

                            // Property with a value then check the value
                            var value = Items.GetPropValue(item.Serial, property.Name);
                            var parsed = double.TryParse(property.Value, out double searchValue);
                            if (parsed && (value >= searchValue))
                            {
                                cntValidProps++;
                                continue;
                            }
                        }
                    }
                    if (cntValidProps == lootInfo.Properties.Count)
                    {
                        validProperties = true;
                    }
                }

                if ((validID == true) && (validName == true) && (validProperties == true))
                {
                    return true;
                }
            }
            return false;
        }
    }
}