using Assistant;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.XPath;

//#forcedebug

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

        private List<Tree> ignore_list = new List<Tree>();

        private const bool DROP_COMMON_LOGS = false;
        private const int LOGS_ID = 0x1BDD;

        private const int MAX_WEIGHT_RANGE = 25;
        private const int MAX_WIGHT_FOR_RESTART_WORK = 255; // If weight is over this value, the script will pause

        private const int FIND_RADIUS = 10;
        private readonly List<int> HATCHET_IDs = new List<int> { 0x0F49 };
        

        public void Run()
        {
            while (true)
            {
                if (CheckPlayerOverloaded() == true)
                {
                    while (Player.Weight >= MAX_WIGHT_FOR_RESTART_WORK)
                    {
                        Player.HeadMessage(33, "Waiting for less weight");
                        Misc.Pause(5000);
                    }
                }

                var tree = FindTrees();

                if (tree.Count == 0)
                {
                    Player.HeadMessage(33, "No trees found");
                    continue;
                }

                foreach (var item in tree)
                {
                    MoveToTree(item);
                    ChopTheTree(item);
                    if (CheckPlayerOverloaded() == true)
                    {
                        break;
                    }
                }

                Misc.Pause(800);
            }
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
                        Items.MoveOnGround(logs, logs.Amount, Player.Position.X+1, Player.Position.Y, Player.Position.Z);
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
                    List<Statics.TileInfo> tile = Statics.GetStaticsTileInfo(x + i, y + j, Player.Map);
                    if (tile.Count == 0) continue;
                    List<Statics.TileInfo> found = tile.Where(item => Statics.GetTileName(item.ID).Contains("tree")).ToList();
                    if (found.Count == 0) continue;

                    // Check if the tree is in the ignore list
                    if (ignore_list.Any(item => item.X == x + i && item.Y == y + j)) continue;

                    foreach (var item in found)
                    {
                        Tree t = new Tree()
                        {
                            X = x + i,
                            Y = y + j,
                            Z = item.StaticZ,
                            Name = Statics.GetTileName(item.ID),
                            StaticID = item.ID,
                            LastChop = DateTime.Now,
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
                    Player.HeadMessage(33, $"Arrived at the tree - {200 - safeExit} iterations");
                    break;
                }
                    

                Misc.Pause(10);
            }

            Misc.Pause(50);
        }

        private void ChopTheTree(Tree tree)
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
