using System;
using System.Collections.Generic;
using System.Linq;

//#forcedebug
//#import <../Libs/stored_data.cs>

namespace RazorEnhanced
{
    // Crea una classe tree che contiene le informazioni dell'albero come posizione x,y,z e nome dell'alebro

    internal class TreeChopper
    {
        private class Tree
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public string Name { get; set; }
            public int StaticID { get; set; }
            public DateTime LastChop { get; set; }
        }

        private List<Tree> ignore_list;

        private const bool DROP_COMMON_LOGS = false; // Drop on ground commons logs
        private const int LOGS_ID = 0x1BDD;

        private const int PURGE_AFTER_MINS = 20; // Remove tree from the list after N minutes
        
        private const int MAX_WEIGHT_RANGE = 25;
        private const int MAX_WIGHT_FOR_RESTART_WORK = 300; // If weight is over this value, the script will pause

        private const int FIND_RADIUS = 10;
        private readonly List<int> HATCHET_IDs = new List<int> { 0x0F49, 0x1443, 0x48B2 }; // 0x48B2 Gargoyle
        private readonly List<string> INVALID_TREE_NAMES = new List<string> { "o'hii tree" };

        private readonly StoredData json_storedData = new StoredData();

        public void Run()
        {
            ignore_list = json_storedData.GetData<List<Tree>>("ChoppedTree",StoredData.StoreType.Character);
            if (ignore_list == null) ignore_list = new List<Tree>();

            while (true)
            {
                PurgeIgnoreList();

                if (CheckPlayerOverloaded() == true)
                {
                    while (Player.Weight >= MAX_WIGHT_FOR_RESTART_WORK)
                    {
                        Player.HeadMessage(33, "Waiting for less weight");
                        Misc.Pause(5000);
                    }
                }

                var trees = FindTrees();

                if (trees.Count == 0)
                {
                    Player.HeadMessage(33, "No trees found");
                    continue;
                }

                for (int i = 0; i < trees.Count; i++)
                {
                    Tree tree = trees[i];
                    MoveToTree(tree);
                    ChopTheTree(ref tree);

                    // Update the ingore list on disk
                    json_storedData.StoreData(ignore_list, "ChoppedTree", StoredData.StoreType.Character);

                    if (CheckPlayerOverloaded() == true)
                    {
                        break;
                    }
                }

                Misc.Pause(800);
            }
        }

        private void PurgeIgnoreList()
        {
            ignore_list.RemoveAll(item => item.LastChop < DateTime.Now.AddMinutes(-PURGE_AFTER_MINS));
        }

        private bool CheckPlayerOverloaded()
        {
            if (Player.Weight > Player.MaxWeight - MAX_WEIGHT_RANGE)
            {
                Player.HeadMessage(33, "Backpack is full");

                if (DROP_COMMON_LOGS == true)
                {
                    Player.HeadMessage(33, "Dropping logs");
                    var logs = Items.FindByID(LOGS_ID, 0, Player.Backpack.Serial);
                    if (logs != null)
                    {
                        Items.MoveOnGround(logs, logs.Amount, Player.Position.X + 1, Player.Position.Y, Player.Position.Z);
                        Misc.Pause(500);
                    }
                }

                return true;
            }
            return false;
        }

        private List<Tree> FindTrees()
        {
            Player.HeadMessage(33, "Searching for trees");

            int x = Player.Position.X;
            int y = Player.Position.Y;

            List<Tree> trees = new List<Tree>();

            for (int i = -FIND_RADIUS; i < FIND_RADIUS; i++)
            {
                for (int j = -FIND_RADIUS; j < FIND_RADIUS; j++)
                {
                    Misc.Pause(1);
                    List<Statics.TileInfo> tiles = Statics.GetStaticsTileInfo(x + i, y + j, Player.Map);
                    if (tiles.Count == 0) continue;

                    // Remove banned trees
                    tiles.RemoveAll(tile => INVALID_TREE_NAMES.Contains(Statics.GetTileName(tile.ID)));
                    // Remove not tree tiles
                    tiles.RemoveAll(tile => !Statics.GetTileName(tile.ID).Contains("tree"));

                    if (tiles.Count == 0) continue;
                    
                    // Check if the tree is in the ignore list
                    if (ignore_list.Any(item => item.X == x + i && item.Y == y + j)) continue;

                    foreach (var item in tiles)
                    {
                        Tree t = new Tree()
                        {
                            X = x + i,
                            Y = y + j,
                            Z = item.StaticZ,
                            Name = Statics.GetTileName(item.ID),
                            StaticID = item.ID,
                            LastChop = DateTime.Now.AddDays(1), // Set a future date because not yet chopped
                        };
                        trees.Add(t);
                    }
                }
            }

            // Sort by distance
            trees.Sort((item1, item2) =>
            {
                double distance1 = Misc.Distance(item1.X, item1.Y, Player.Position.X, Player.Position.Y);
                double distance2 = Misc.Distance(item2.X, item2.Y, Player.Position.X, Player.Position.Y);

                return distance1.CompareTo(distance2);
            });

            Player.HeadMessage(33, $"Found {trees.Count} trees");

            return trees;
        }

        private void MoveToTree(Tree tree)
        {
            if (tree == null) return;

            int safeExit = 200;

            while (true)
            {
                if (safeExit % 100 == 0)
                {
                    Player.PathFindTo(tree.X + 1, tree.Y - 1, tree.Z);
                    Misc.Pause(200);
                }

                if (safeExit-- <= 0)
                {
                    Player.HeadMessage(33, "Safe EXIT during pathfinding");
                    break;
                }

                double dist = Misc.Distance(Player.Position.X, Player.Position.Y, tree.X, tree.Y);
                if (dist <= 2)
                {
                    //Player.HeadMessage(33, $"Arrived at the tree - {200 - safeExit} iterations");
                    break;
                }
                    

                Misc.Pause(10);
            }

            Misc.Pause(50);
        }

        private void ChopTheTree(ref Tree tree)
        {
            if (tree == null) return;

            Journal journal = new Journal();
            journal.Clear();

            while (true)
            {
                Player.HeadMessage(33, "Chopping the tree");
                Item hatchet = SearchAndEquipHatchet();
                if (hatchet == null)
                {
                    Player.HeadMessage(33, "No hatchet found");
                    Misc.Pause(1000);
                    break;
                }

                Items.UseItem(hatchet);
                Target.WaitForTarget(5000);
                Target.TargetExecute(tree.X, tree.Y, tree.Z, tree.StaticID);
                Misc.Pause(1500);

                tree.LastChop = DateTime.Now; // Set the last chop time

                if (journal.Search("not enough wood") == true)
                {
                    ignore_list.Add(tree);
                    break;
                }

                if (journal.Search("too far away") == true)
                {
                    break;
                }

                if (CheckPlayerOverloaded() == true)
                {
                    break;
                }

                Misc.Pause(10);
            }
        }

        private Item SearchAndEquipHatchet()
        {
            Item rightHand = Player.GetItemOnLayer("RightHand");
            Item leftHand = Player.GetItemOnLayer("LeftHand");

            if (rightHand != null) { Player.UnEquipItemByLayer("RightHand"); Misc.Pause(700); }

            if (leftHand != null && !HATCHET_IDs.Contains(leftHand.ItemID)) { Player.UnEquipItemByLayer("LeftHand"); Misc.Pause(700); }
            if (leftHand != null && HATCHET_IDs.Contains(leftHand.ItemID)) return leftHand;

            Items.Filter itemFilter = new Items.Filter
            {
                Enabled = true,
                OnGround = 0,
            };

            foreach (var itemID in HATCHET_IDs) itemFilter.Graphics.Add(itemID);
            Item hatchet = Items.ApplyFilter(itemFilter)[0];

            if (hatchet != null)
            {
                Player.EquipItem(hatchet.Serial);
                Player.HeadMessage(33, "Equipping Hatchet");
                Misc.Pause(2500);
                return hatchet;
            }

            return null;
        }

    }
}
