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

        enum GumpReturn
        {
            Close,
            Error,
            OK
        }

        private RecallType RECALL_TYPE = RecallType.SacredJourney;
        private RuneBookType RECALL_BOOK_TYPE = RuneBookType.UseAtlas;

        private const int HOME_RUNE = 0;
        private const int START_FROM_RUNE_NUMBER = 1; // 0 is Home
        private const int FIND_RADIUS = 35;

        private int SERIAL_RECALLBOOK = 0;
        private int SERIAL_CONTAINER = 0;

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
            SERIAL_RECALLBOOK = (int)Misc.ReadSharedValue("PlantsPicker.RuneBookSerial");
            SERIAL_CONTAINER = (int)Misc.ReadSharedValue("PlantsPicker.ContainerSerial");

            GumpReturn gumpReturn = DisplayMenu();
            if (gumpReturn == GumpReturn.Error) return;
            if (gumpReturn == GumpReturn.Close) return;
            
            int max_rune_cnt = CountRunesInBook();
            int rune_cnt = START_FROM_RUNE_NUMBER;

            while (true)
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

                if (RECALL_TYPE == RecallType.NoRecall)
                {
                    break;
                }
            }
            Player.HeadMessage(33, "End of the run");
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
            if (SERIAL_CONTAINER == 0) return;

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
            Gumps.ResetGump();
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

        private GumpReturn DisplayMenu()
        {
            uint gumpID = 123456;

            var gump = Gumps.CreateGump(true, true, true, true);
            gump.gumpId = gumpID;
            gump.serial = (uint)Player.Serial;

            // Help from https://docs.polserver.com/pol100/guides.php?guidefile=gumpcmdlist#button

            Gumps.AddPage(ref gump, 0);
            Gumps.AddBackground(ref gump, 0, 0, 250, 330, 5054);
            Gumps.AddAlphaRegion(ref gump, 10, 10, 250-20, 330-20);

            Gumps.AddImageTiled(ref gump, 10, 10, 250-20, 22, 2624); // Black Backgroun of the text
            Gumps.AddHtml(ref gump, 10, 12, 220, 20, "<CENTER><BASEFONT COLOR=\"YELLOW\">= PLANTS PICKER =</BASEFONT></CENTER>", false, false);

            // Single Harverst or Harverst and Recall
            gump.gumpDefinition += "{ Group {0} }";
            Gumps.AddRadio(ref gump, 20, 40, 5836, 5828, false, 1);
            Gumps.AddHtml(ref gump, 55, 45, 150, 25, "<BASEFONT COLOR=#DCDEAB>Harvest only one field</BASEFONT>", false, false);

            Gumps.AddRadio(ref gump, 20, 75, 5836, 5828, true, 2);
            Gumps.AddHtml(ref gump, 55, 80, 150, 25, "<BASEFONT COLOR=#DCDEAB>Harvest and recall</BASEFONT>", false, false);
            gump.gumpDefinition += "{ EndGroup {0} }";

            // Book Type, ID and Container ID

            string bookName = (SERIAL_RECALLBOOK == 0 ? "None" : Items.FindBySerial(SERIAL_RECALLBOOK).Name);
            bookName = "<BASEFONT COLOR=#FFFFFF>" + bookName + "</BASEFONT>";
            string bookID = "<BASEFONT COLOR=#FFFFFF>" + "0x" + SERIAL_RECALLBOOK.ToString("X8") + "</BASEFONT>"; 

            Gumps.AddHtml(ref gump, 20, 120, 200, 25, $"<BASEFONT COLOR=\"YELLOW\">Book Name:</BASEFONT> {bookName}", false, false);
            Gumps.AddHtml(ref gump, 20, 140, 200, 25, $"<BASEFONT COLOR=\"YELLOW\">Book ID:</BASEFONT> {bookID}", false, false);
            Gumps.AddButton(ref gump, 200, 138, 4029, 4031, 10, 1, 0); // ID 10 - ID Book
            Gumps.AddTooltip(ref gump, "Select a Runebook or a Runic Atlas");

            
            string chestName = (SERIAL_CONTAINER == 0 ? "None" : Items.FindBySerial(SERIAL_CONTAINER).Name);
            chestName = "<BASEFONT COLOR=#FFFFFF>" + chestName + "</BASEFONT>";
            string chestID = "<BASEFONT COLOR=#FFFFFF>" + "0x" + SERIAL_CONTAINER.ToString("X8") + "</BASEFONT>";

            Gumps.AddHtml(ref gump, 20, 160, 210, 25, $"<BASEFONT COLOR=\"YELLOW\">Chest Name:</BASEFONT> {chestName}", false, false);
            Gumps.AddHtml(ref gump, 20, 180, 210, 25, $"<BASEFONT COLOR=\"YELLOW\">Chest ID:</BASEFONT> {chestID}", false, false);
            
            Gumps.AddButton(ref gump, 200, 178, 4029, 4031, 11, 1, 0); // ID 11 - ID Container
            Gumps.AddTooltip(ref gump, "Select a container where will drop items");

            // Recall Type: Magery or Sacred Journey
            gump.gumpDefinition += "{ Group {1} }";
            Gumps.AddRadio(ref gump, 20, 210, 5836, 5828, false, 1);
            Gumps.AddHtml(ref gump, 55, 210+5, 150, 25, "<BASEFONT COLOR=#DCDEAB>Use Magery</BASEFONT>", false, false);

            Gumps.AddRadio(ref gump, 20, 210+35, 5836, 5828, true, 2);
            Gumps.AddHtml(ref gump, 55, 210+35+5, 150, 25, "<BASEFONT COLOR=#DCDEAB>Use Sacred Journey</BASEFONT>", false, false);
            gump.gumpDefinition += "{ EndGroup {1} }";

            // Close and Start buttons
            Gumps.AddHtml(ref gump, 55, 290, 150, 25, "<BASEFONT COLOR=#FFFFFF>Close</BASEFONT>", false, false);
            Gumps.AddButton(ref gump, 20, 290, 30535, 30533, 20, 1, 0); // ID 20 - Start
            Gumps.AddTooltip(ref gump, "Close menu");

            Gumps.AddHtml(ref gump, 160, 290, 150, 25, "<BASEFONT COLOR=#FFFFFF>Start</BASEFONT>", false, false);
            Gumps.AddButton(ref gump, 200, 290, 30534, 30533, 21, 1, 0); // ID 21 - Close
            Gumps.AddTooltip(ref gump, "Start harversting");

            Gumps.SendGump(gump.gumpId, gump.serial, 0, 0, gump.gumpDefinition, gump.gumpStrings);

            bool bret = Gumps.WaitForGump(gumpID, 15000);
            if (!bret) return GumpReturn.Error; // Exit

            int button = -1;

            Target target = new();

            while (button == -1)
            {
                var gumpData = Gumps.GetGumpData(gumpID);
                if (gumpData.gumpId == gumpID)
                {
                    button = gumpData.buttonid;

                    if (button == 0) return GumpReturn.Close; // Exit

                    // Select Book
                    if (button == 10)
                    {
                        Misc.SendMessage("Select a Runebook or a Runic Atlas");
                        Item new_book = Items.FindBySerial(target.PromptTarget());
                        if ((new_book.ItemID == 0x22C5) || (new_book.ItemID == 0x9C16)) // Runebook or Atlas
                        {
                            SERIAL_RECALLBOOK = new_book.Serial;
                            Misc.SetSharedValue("PlantsPicker.RuneBookSerial", SERIAL_RECALLBOOK);
                        }
                        else
                        {
                            Player.HeadMessage(33, "Invalid target! Select a Runebook or a Runic Atlas");
                        }
                        Gumps.CloseGump(gumpID);
                        DisplayMenu();
                    }
                    // Select Container
                    else if (button == 11)
                    {
                        Misc.SendMessage("Select a container");
                        Item new_container = Items.FindBySerial(target.PromptTarget());
                        if (new_container.IsContainer)
                        {
                            SERIAL_CONTAINER = new_container.Serial;
                            Misc.SetSharedValue("PlantsPicker.ContainerSerial", SERIAL_CONTAINER);
                        }
                        else
                        {
                            Player.HeadMessage(33, "Invalid target! Select a valid container");
                        }

                        Gumps.CloseGump(gumpID);
                        DisplayMenu();
                    }
                    // Close Button
                    else if (button == 20)
                    {
                        Gumps.CloseGump(gumpID);
                        return GumpReturn.Close;
                    }
                    // Start Button
                    else if (button == 21)
                    {
                        int harvest_type = gumpData.switches[0];
                        int recall_type = gumpData.switches[1];

                        if (harvest_type == 1)
                        {
                            RECALL_TYPE = RecallType.NoRecall;
                        }
                        else
                        {
                            RECALL_TYPE = recall_type == 1 ? RecallType.Magery : RecallType.SacredJourney;
                        }
                        Gumps.CloseGump(gumpID);
                        

                        if (RECALL_TYPE != RecallType.NoRecall && SERIAL_RECALLBOOK == 0)
                        {
                            Player.HeadMessage(33, "Select a Runebook or a Runic Atlas before start using recall");
                            DisplayMenu();
                        }

                        if (RECALL_TYPE != RecallType.NoRecall && SERIAL_CONTAINER == 0)
                        {
                            Player.HeadMessage(33, "Select a container before start  harvesting using recall");
                            DisplayMenu();
                        }

                        return GumpReturn.OK;
                    }
                }
                Misc.Pause(100);
            }
            
            return GumpReturn.Close;
        }
    }
}
