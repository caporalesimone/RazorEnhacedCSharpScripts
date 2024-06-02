using System.Collections.Generic;
using System.Linq;
using System.Threading;

//#forcedebug

namespace RazorEnhanced
{
    internal class PlantsPicker
    {
        enum RecallType
        {
            NoRecall,
            Magery,
            SacredJourney
        }

        enum RuneBookType
        {
            UseRuneBook,
            UseAtlas
        }

        private const RecallType RECALL_TYPE = RecallType.SacredJourney;
        private const RuneBookType RECALL_BOOK_TYPE = RuneBookType.UseAtlas;

        private const int HOME_RUNE = 0;
        //private const int MAX_RUNE = 0;
        private const int START_FROM_RUNE_NUMBER = 1; // 0 is Home
        private const int FIND_RADIUS = 35;

        private int SERIAL_RECALLBOOK = 0x415C17F9;
        private int SERIAL_CONTAINER = 0x418B701F;

        private List<int> PLANTS = new()
        { 
            0x0C51, 0x0C52, 0x0C53, 0x0C54, // Cotton
            0x1A99, 0x1A9A, 0x1A9B,         // Flax
         // 0x0C5E, 0x0C5F, 0x0C60, // Vines but this plant does not change graphics so this script remains blocked
            
            0x0C61, 0x0C62, 0x0C63, // Turnip
            0x0C6F, // Onions
            0x0C76, // Carrots
         
         // 0x0C55, 0x0C56, 0x0C57, 0x0C58, // Wheat
         };

        private readonly List<int> RESOURCES = new()
        {
                    0x0DF9, // Bale of cotton
            0x1A9C, 0x1A9D, // Flax bundle
            
            0x0D39, 0x0D3A, // Turnip
            0x0C6B,         // Pumpkin from vines
            0x0C6D, 0x0C6E, // Onion
            0x0C77, 0x0C78, // Carrots
            
            0x1EBD, // Wheat
        };

        public PlantsPicker()
        {
        }

        public void Run ()
        {
            int max_rune_cnt = CountRunesInBook();
            int rune_cnt = START_FROM_RUNE_NUMBER;

            while(true)
            {
                Recall(rune_cnt);

                bool harvestResult = HarvestTheField();
                
                if (!harvestResult) GoBackHome();
                
                if (++rune_cnt >= max_rune_cnt)
                {
                    GoBackHome();
                    rune_cnt = START_FROM_RUNE_NUMBER; // Restart from the begin
                }

                if (Player.Weight > Player.MaxWeight - 10)
                {
                    Player.HeadMessage(33, "Overloaded");
                    if (RECALL_TYPE != RecallType.NoRecall)
                    {
                        GoBackHome();
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        private List<Item> FindItemOnGround(int range, List<int> graphics)
        {
            var findFilter = new Items.Filter()
            {
                Enabled = true,
                RangeMax = range,
                Graphics = graphics
            };

            var foundItems = Items.ApplyFilter(findFilter);
            if (foundItems != null || foundItems.Count > 0)
            {
                // Sort by distance
                foundItems.Sort((item1, item2) =>
                {
                    double distance1 = Misc.Distance(item1.Position.X, item1.Position.Y, Player.Position.X, Player.Position.Y);
                    double distance2 = Misc.Distance(item2.Position.X, item2.Position.Y, Player.Position.X, Player.Position.Y);

                    return distance1.CompareTo(distance2);
                });

                return foundItems;
            }

            return new List<Item>();
        }

        private void MoveToPlant(Item plant)
        {
            if (plant == null) return;

            if (Player.Position.Equals(plant.Position)) return; // This happen if 2 plants are in the same tile (Server bug?)

            Player.PathFindTo(plant.Position);

            int safeExit = 200;

            while (true)
            {
                if (safeExit-- <= 0)
                {
                    Player.HeadMessage(33, "Safe EXIT during pathfinding");
                    break;
                }

                double dist = Misc.Distance(Player.Position.X, Player.Position.Y, plant.Position.X, plant.Position.Y);
                if (dist <= 1) break;

                Misc.Pause(10);
            }
        }

        private void HarvestPlant(Item plant)
        {
            Items.Message(plant.Serial, 33, ">.<");

            double dist = Misc.Distance(Player.Position.X, Player.Position.Y, plant.Position.X, plant.Position.Y);
            if (dist > 1)
            {
                Player.HeadMessage(33, "Plant is too far");
                Misc.Pause(100);
                return;
            }

            Items.UseItem(plant);
            Misc.Pause(700);
            List<Item> harvest = FindItemOnGround(2, RESOURCES); // Sometimes happen that 2 plants are in the same tile
            foreach (Item harvestItem in harvest)
            {
                Items.Move(harvestItem, Player.Backpack, -1);
                Misc.Pause(700);
            }
        }

        private bool HarvestTheField()
        {
            List<Item> groundPlants = FindItemOnGround(FIND_RADIUS, PLANTS);
            
            while (groundPlants.Count != 0)
            {
                Player.HeadMessage(25, $"Harvesting");

                Item plant = groundPlants.ElementAt(0);
                MoveToPlant(plant);
                HarvestPlant(plant);

                if (Player.Weight > Player.MaxWeight - 10) return false;

                // Search Again
                groundPlants = FindItemOnGround(FIND_RADIUS, PLANTS);
            }
            Player.HeadMessage(25, "All plants harvested");
            return true;
        }

        private void UnloadResources()
        {
            Player.HeadMessage(33, "Moving all the harvest");
            foreach (var resource in RESOURCES)
            {
                Item item = Items.FindByID(resource, 0, Player.Backpack.Serial);
                if (item != null) 
                {
                    Items.UseItem(SERIAL_CONTAINER);
                    Misc.Pause(700);
                    Items.Move(item, SERIAL_CONTAINER, item.Amount);
                    Misc.Pause(700);
                }
            }
            Misc.Pause(1000);
            Gumps.CloseGump(Gumps.CurrentGump());
        }

        private void GoBackHome()
        {
            if (RECALL_TYPE == RecallType.NoRecall) {
                Misc.Pause(500);
                return;
            }

            Recall(HOME_RUNE);

            UnloadResources();

            while(Player.Mana < Player.ManaMax)
            {
                Player.HeadMessage(33, $"Resting - Mana: {Player.Mana}/{Player.ManaMax}");
                Misc.Pause(2000);
            }
        }

        private void Recall(int runePosition)
        {
            while (Player.Mana < 20)
            {
                Player.HeadMessage(33, $"Waiting for al least 20 Mana pints: {Player.Mana}/{Player.ManaMax}");
                Misc.Pause(2000);
            }

            if (RECALL_TYPE == RecallType.NoRecall || SERIAL_RECALLBOOK == 0)
            {
                Misc.Pause(500);
                return;
            }

            if (RECALL_BOOK_TYPE == RuneBookType.UseRuneBook)
            {
                Items.UseItem(SERIAL_RECALLBOOK);
                Gumps.WaitForGump(0, 20000);
                uint gump = Gumps.CurrentGump();
                Misc.Pause(50);
                Gumps.SendAction(gump, 5 + runePosition * 6);
            }
            else if (RECALL_BOOK_TYPE == RuneBookType.UseAtlas)
            {
                Recall_Atlas(runePosition);
            }
        }

        private void Recall_Atlas(int runePosition)
        {
            Items.UseItem(SERIAL_RECALLBOOK);
            Gumps.WaitForGump(0, 20000);
            uint gump = Gumps.CurrentGump();

            int pageNum = 0;
            for (int i = 0; i < runePosition / 16; i++)
            {
                Gumps.SendAction(gump, 1150); // Next Page
                Gumps.WaitForGump(0, 20000);
                gump = Gumps.CurrentGump();
                pageNum++;
            }

            // Select the rune
            Gumps.SendAction(gump, 100 + runePosition);
            Gumps.WaitForGump(0, 20000);
            gump = Gumps.CurrentGump();

            Misc.Pause(1000);

            List<string> RuneListInPage = Gumps.GetLineList(gump);
            int runeIndexInPage = runePosition + (3 * pageNum) + 1; // First lise of the List contains the carhes number of the book "0 / 80"
            string runeText = RuneListInPage[runeIndexInPage];
            Player.HeadMessage(33, "Recalling to " + runeText);

            if (RECALL_TYPE == RecallType.Magery)
            {
                Gumps.SendAction(gump, 4); // Recall
            }
            else if (RECALL_TYPE == RecallType.SacredJourney)
            {
                Gumps.SendAction(gump, 7); // Sacred Journey
            }

            Misc.Pause(5000);
        }

        private int CountRunesInBook()
        {
            if (RECALL_TYPE == RecallType.NoRecall) return 0;
            if (SERIAL_RECALLBOOK == 0) return 0;

            Items.UseItem(SERIAL_RECALLBOOK);

            int runesCount = 0;

            // I don't know if there are only 3 pages everywhere. So I consider 10 pages and then exit if I don't find the button to go to the next page.
            for (int i = 0; i < 10; i++)  
            {
                Gumps.WaitForGump(0, 20000);
                uint gump = Gumps.CurrentGump();
                string gumpContent = Gumps.GetGumpRawData(gump);

                gumpContent = gumpContent.ToLower();
                gumpContent = gumpContent.Replace("@move up@", "");
                gumpContent = gumpContent.Replace("@move down@", "");
                gumpContent = gumpContent.Replace("@empty@", "");

                if (RECALL_BOOK_TYPE == RuneBookType.UseAtlas)
                {
                    // Tooltips are between @ so it count twice. Eg: { tooltip 1042971 @Britain 1 Trammel@ }
                    runesCount += gumpContent.Count(c => c == '@') / 2; 
                }
                else if (RECALL_BOOK_TYPE == RuneBookType.UseRuneBook)
                {
                    // Tooltips are between @ so it count twice. Eg: { tooltip 1042971 @Britain 1 Trammel@ }
                    // In runebook there tooltips are repeated twice 
                    runesCount += gumpContent.Count(c => c == '@') / 4; 
                }

                if (gumpContent.Contains("{ button 374 3 2206 2206 1 0 1150 }"))
                {
                    Gumps.SendAction(gump, 1150); // Next poage
                    Misc.Pause(100);
                }
                else 
                {
                    // Next page button missing, so this is the last page
                    Gumps.CloseGump(gump);
                    Misc.Pause(100);
                    break;
                }
            }
            return runesCount;
        }

    }
}
