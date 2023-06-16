using System.Collections.Generic;

//#forcedebug

namespace RazorEnhanced
{
    internal class MiningCarts_Stumps
    {
        private const int BEETLE_SERIAL = 0x0007DF23;

        private const int HATCHET_ID = 0x0F43;
        private const int LOGS_ID = 0x1BDD;
        private const int BOARDS_ID = 0x1BD7;

        private const int GUMP_ID = 84765431;

        private Item rightHand = null;
        private Item leftHand = null;

        public MiningCarts_Stumps()
        {
            Misc.SendMessage("Started MiningCarts And Stump Script");
        }

        public void Run()
        {
            int button = -1;

            while (button != 0)
            {
                button = DisplayMenu();

                if (button == 1)
                {
                    ManageMiningCarts();
                }
                else if (button == 2)
                {
                    ManageStumps();
                }
            }
        }

        private void ManageMiningCarts()
        {
            List<Item> carts = FindCartsOrStump(true);
            foreach (var cart in carts)
            {
                if (Player.DistanceTo(cart) < 3)
                {
                    EmptyCartOrStump(cart);
                }
                else
                {
                    Player.Walk("South");
                    Player.Walk("South");
                    EmptyCartOrStump(cart);
                }
            }
        }

        private void ManageStumps()
        {
            List<Item> stumps = FindCartsOrStump(false);
            foreach (var stump in stumps)
            {
                if (Player.DistanceTo(stump) < 3)
                {
                    EmptyCartOrStump(stump);
                    MoveBoardInBeetle();
                }
                else
                {
                    Player.Walk("South");
                    Player.Walk("South");
                    EmptyCartOrStump(stump);
                }
            }

            Player.UnEquipItemByLayer("RightHand");
            Misc.Pause(700);
            Player.UnEquipItemByLayer("LeftHand");
            Misc.Pause(700);

            if (rightHand != null) { Player.EquipItem(rightHand); Misc.Pause(700); }
            if (leftHand != null) { Player.EquipItem(leftHand); Misc.Pause(700); }
        }

        private void MoveBoardInBeetle()
        {
            Item boards = Items.FindByID(BOARDS_ID, -1, Player.Backpack.Serial);
            if (boards == null) return;

            Mobiles.UseMobile(Player.Serial);
            Misc.Pause(700);

            while (true)
            {
                boards = Items.FindByID(BOARDS_ID, -1, Player.Backpack.Serial);
                if (boards == null) break;

                Items.Move(boards.Serial, BEETLE_SERIAL, -1);
                Misc.Pause(700);
            }

            Mobiles.UseMobile(BEETLE_SERIAL);
            Misc.Pause(300); // Not really needed. Just to be safe
        }

        private Item EquipHatchet()
        {
            rightHand = Player.GetItemOnLayer("RightHand");
            leftHand = Player.GetItemOnLayer("LeftHand");

            if (rightHand != null) { Player.UnEquipItemByLayer("RightHand"); Misc.Pause(700); }
            if (leftHand != null && leftHand.ItemID != HATCHET_ID) { Player.UnEquipItemByLayer("LeftHand"); Misc.Pause(700); }
            if (leftHand != null && leftHand.ItemID == HATCHET_ID) return leftHand;

            Item hatchet = Items.FindByID(HATCHET_ID, -1, Player.Backpack.Serial);
            if (hatchet != null) Player.EquipItem(hatchet);
            return hatchet;
        }

        private void EmptyCartOrStump(Item target)
        {
            Journal journal = new Journal();
            journal.Clear();

            int i = 0;
            while (!journal.Search("There are no more"))
            {
                if (journal.Search("Your backpack is full")) break;
                if (journal.Search("You are overloaded")) break;

                Items.Message(target, 33, $"{i++}");
                Items.UseItem(target);
                Misc.Pause(800);

                if (Player.Weight - 20 > Player.MaxWeight) break;
            }

            // Check if logs are in backpack
            while (true)
            {
                var logs = Items.FindByID(LOGS_ID, -1, Player.Backpack.Serial);
                if (logs == null) { break; }

                Item hatchet = EquipHatchet();
                if (hatchet != null)
                {
                    Items.UseItem(hatchet);
                    Target.WaitForTarget(15000);
                    Target.TargetExecute(logs);
                    Misc.Pause(800);
                }
            }
        }

        private List<Item> FindCartsOrStump(bool lookingForCart)
        {

            List<int> graphicsIDs = new List<int>();

            if (lookingForCart)
            {
                // Gem Cart and Mining Cart has the same graphics in Demise Server
                graphicsIDs.Add(0x1A83); // South 
                graphicsIDs.Add(0x1A88); // East
            }
            else
            {
                graphicsIDs.Add(0x0E56); // Stump with axe
                graphicsIDs.Add(0x0E57); // Stump
                graphicsIDs.Add(0x0E58); // Stump with axe
                graphicsIDs.Add(0x0E59); // Stump
            }

            Items.Filter filter = new Items.Filter
            {
                Graphics = graphicsIDs,
                Enabled = true,
                RangeMax = 10,
            };

            List<Item> carts = Items.ApplyFilter(filter);
            if (carts != null || carts.Count > 0)
            {
                Misc.SendMessage($"Found {carts.Count} carts");

                carts.Sort((item1, item2) =>
                    {
                        double distance1 = Misc.Distance(item1.Position.X, item1.Position.Y, Player.Position.X, Player.Position.Y);
                        double distance2 = Misc.Distance(item2.Position.X, item2.Position.Y, Player.Position.X, Player.Position.Y);

                        return distance1.CompareTo(distance2);
                    });


                int i = 0;
                foreach (var cart in carts)
                {
                    Items.Message(cart.Serial, 6, $"{++i}");
                }

            }

            if (carts == null) carts = new List<Item>(); // Avoid null errors

            return carts;
        }

        public int DisplayMenu()
        {
            var gump = Gumps.CreateGump(true, true, true, true);
            gump.gumpId = GUMP_ID;
            gump.serial = (uint)Player.Serial;

            // Help from https://docs.polserver.com/pol100/guides.php?guidefile=gumpcmdlist#button

            Gumps.AddPage(ref gump, 0);
            Gumps.AddBackground(ref gump, 0, 0, 230, 137, 5054);
            Gumps.AddAlphaRegion(ref gump, 10, 10, 210, 117);

            Gumps.AddImageTiled(ref gump, 10, 10, 210, 22, 2624); // Black Backgroun of the text
            Gumps.AddHtml(ref gump, 10, 12, 212, 20, "<CENTER><BASEFONT COLOR=\"YELLOW\">MINING CARTS AND STUMPS</BASEFONT></CENTER>", false, false);

            Gumps.AddButton(ref gump, 25, 60, 4005, 4007, 1, 1, 0); // Mining Carts
            Gumps.AddHtml(ref gump, 70, 62, 212, 20, "<BASEFONT COLOR=\"WHITE\">MINING CARTS</BASEFONT>", false, false);

            Gumps.AddButton(ref gump, 25, 85, 4005, 4007, 2, 1, 0); // Tree Stumps
            Gumps.AddHtml(ref gump, 70, 87, 212, 20, "<BASEFONT COLOR=\"WHITE\">TREE STUMPS</BASEFONT>", false, false);

            Gumps.SendGump(gump.gumpId, gump.serial, 0, 0, gump.gumpDefinition, gump.gumpStrings);

            bool bret = Gumps.WaitForGump(GUMP_ID, 15000);
            if (!bret) return 0; // Exit

            int button = -1;
            while (button == -1)
            {
                var gumpData = Gumps.GetGumpData(GUMP_ID);
                if (gumpData.gumpId == GUMP_ID)
                {
                    button = gumpData.buttonid;
                }
                Misc.Pause(100);
            }

            return button;
        }

    }
}
