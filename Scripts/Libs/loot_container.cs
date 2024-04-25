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
        private const int MOVE_ITEMS_DELAY = 600;
        private readonly StoredData json_storedData = new StoredData();

        private readonly int lootBagSerial = 0;
        private readonly List<LootInfo> wantedLootList = new List<LootInfo>();

        private class LootInfo
        {
            public int ItemID { get; set; } = 0;
            public string Name { get; set; } = "";
            public List<(string, string)> Properties { get; set; } = new List<(string, string)>();
            public LootInfo() { }
            public LootInfo(int itemID)
            {
                ItemID = itemID;
            }
            public void AddProperty(string property, string value)
            {
                Properties.Add((property, value));
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

            // If there is no loot list, add a default one
            if (wantedLootList.Count == 0)
            {
                wantedLootList.Add(new LootInfo(0x0F10)); // Emerald
                wantedLootList.Add(new LootInfo(0x0F13)); // Ruby
                wantedLootList.Add(new LootInfo(0x0F15)); // Citrine
                wantedLootList.Add(new LootInfo(0x0F16)); // Amethyst
                wantedLootList.Add(new LootInfo(0x0F11)); // Sapphire
                wantedLootList.Add(new LootInfo(0x0F19)); // Sapphire
                wantedLootList.Add(new LootInfo(0x0F0F)); // Star sapphire
                wantedLootList.Add(new LootInfo(0x0F21)); // Star sapphire
                wantedLootList.Add(new LootInfo(0x0F25)); // Amber
                wantedLootList.Add(new LootInfo(0x0F26)); // Diamond
                wantedLootList.Add(new LootInfo(0x0F18)); // Tourmaline

                json_storedData.StoreData(wantedLootList, "LootChest.LootList", StoredData.StoreType.Server);

                // Adding gold to the character specific list.
                // This is just an example of how to add items to the character specific list.
                var charSpecificList = new List<LootInfo>
                {
                    new LootInfo() { Name = "Gold Coin" } // Gold
                };

                json_storedData.StoreData(charSpecificList, "LootChest.LootList", StoredData.StoreType.Character);
                wantedLootList.Concat(charSpecificList);
            }
        }

        /*
        public void Run()
        {
            Target target = new Target();

            Player.HeadMessage(50, "Target a Chest");

            int chestSerial = target.PromptTarget();
            if (chestSerial == 0)
            {
                Player.HeadMessage(33, "No target selected");
                return;
            }

            Loot(chestSerial);
        }
        */

        // Must be public so can be called from other scripts
        public bool Loot(int chestSerial)
        {
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
            Misc.Pause(1000);

            List<Item> chestItems = FindItems(chest, true);

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

            return (xDelta > yDelta ? xDelta : yDelta);
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

                // If ItemID is 0 means Ignore ItemID so it will pass
                // If a specific ID is set, check if the item ID is the same
                if (lootInfo.ItemID == item.ItemID)
                {
                    validID = true;
                } 

                if (lootInfo.Name != "" && item.Name.ToLower().Contains(lootInfo.Name.ToLower()))
                {
                    validName = true;
                }

                // If Name is empty means Ignore Name so it will pass
                // If a specific name is set, check if the item name contains it (case insensitive)
                /*
                if (string.IsNullOrEmpty(lootInfo.Name) ||
                    item.Name.ToLower().Contains(lootInfo.Name.ToLower()) )
                {
                    validName = false;
                }
                */

                /*
                if (lootInfo.Properties.Count == 0)
                {
                    validProperties = true;
                }
                */
                /*
                if (lootInfo.Properties.Count > 0)
                {
                    bool found = false;
                    foreach ((string, string) property in lootInfo.Properties)
                    {
                        if (item.Properties.Any(prop => prop.ToString().Contains(property.Item1) && prop.ToString().Contains(property.Item2)))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }
                */

                if ((validID == true) || (validName == true) || (validProperties == true))
                {
                    return true;
                }
            }
            return false;
        }
    }
}