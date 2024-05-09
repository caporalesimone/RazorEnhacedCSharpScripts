using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RazorEnhanced
{
    internal class ItemsComparer
    {
        private uint GUMP_ID = 0xABCD1234;

        public ItemsComparer()
        {

        }
        public void Run()
        {
            
            Misc.SendMessage("Gump inspector started", 33);

            /*
            Gumps.WaitForGump(1143181367, 10000);
            string raw = Gumps.LastGumpRawData();
            List<string> txt = Gumps.LastGumpRawText();
            List<string> linelist = Gumps.LastGumpGetLineList();
            List<int> tile = Gumps.LastGumpTile();
            var gumpData = Gumps.GetGumpData(0xcdeb1507);
            var a = 12;
            */



            DisplayMenu();

            /*
            string text = "{ nomove }{ noresize }{ page 0 }{ checkertrans 0 0 1024 786 }{ gumppictiled 0 786 1024 654 2624 }{ checkertrans 0 786 1024 654 }{ gumppictiled 1024 0 1024 786 2624 }{ checkertrans 1024 0 1024 786 }{ gumppictiled 1024 786 1024 654 2624 }{ checkertrans 1024 786 1024 654 }{ gumppictiled 2048 0 512 786 2624 }{ checkertrans 2048 0 512 786 }{ gumppictiled 2048 786 512 654 2624 }{ checkertrans 2048 786 512 654 }{ resizepic 250 200 40000 420 50 }{ gumppictiled 260 210 400 30 40004 }{ button 265 215 2008 2007 1 0 1 }{ tooltip 1015326 }{ button 640 220 10741 10742 1 0 2 }{ tooltip 3002085 }{ croppedtext 340 215 310 20 51 0 }{ gumppictiled 250 250 420 10 40004 }{ resizepic 250 255 40000 420 230 }{ gumppictiled 260 265 400 210 40004 }{ button 265 270 4006 4007 1 0 3 }{ croppedtext 300 272 340 28 85 1 }{ button 265 300 4006 4007 1 0 4 }{ croppedtext 300 302 340 28 85 2 }{ button 265 330 4006 4007 1 0 5 }{ croppedtext 300 332 340 28 85 3 }{ button 265 360 4006 4007 1 0 6 }{ croppedtext 300 362 340 28 85 4 }{ button 265 390 4006 4007 1 0 7 }{ croppedtext 300 392 340 28 85 5 }{ button 265 420 4006 4007 1 0 8 }{ croppedtext 300 422 340 28 85 6 }{ button 265 450 4006 4007 1 0 9 }{ croppedtext 300 452 340 28 85 7 }{ gumppic 260 246 2360 }{ gumppictiled 271 246 378 11 87 }{ gumppic 649 246 2360 }";


            var gump = Gumps.CreateGump(false, true, true, false);
            gump.gumpId = GUMP_ID;
            gump.serial = (uint)Player.Serial;
            gump.gumpDefinition = text;
            gump.gumpStrings = new() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" };
            Gumps.SendGump(gump.gumpId, gump.serial, 0, 0, gump.gumpDefinition, gump.gumpStrings);
            */



            /*
            List<uint> gumps = new();
            while (true)
            {
                foreach (uint i in Gumps.AllGumpIDs())
                {
                    if (!gumps.Contains(i))
                    {
                        gumps.Add(i);
                        Misc.SendMessage("Added Gump ID: " + i.ToString(), 33);
                    }
                }

                Misc.Pause(100);
            }
            */
        }

        public int DisplayMenu()
        {
            var item1 = Items.FindBySerial(0x415FF671);
            var item2 = Items.FindBySerial(0x41890DC2);
            




            var gump = Gumps.CreateGump(true, true, true, true);
            gump.gumpId = GUMP_ID;
            gump.serial = (uint)Player.Serial;

            // Help from https://docs.polserver.com/pol100/guides.php?guidefile=gumpcmdlist#button

            Gumps.AddPage(ref gump, 0);


            Gumps.AddBackground(ref gump, 218, 105, 500, 394, 9200);

            Gumps.AddImageTiled(ref gump, 282, 162, 80, 40, 2624); // Black Backgroun of the text
            Gumps.AddHtml(ref gump, 282, 162, 80, 40, $"<CENTER><BASEFONT COLOR=\"YELLOW\">{item1.Name}</BASEFONT></CENTER>", false, false);

            Gumps.AddImageTiled(ref gump, 500, 162, 80, 40, 2624); // Black Backgroun of the text
            Gumps.AddHtml(ref gump, 500, 162, 100, 40, $"<CENTER><BASEFONT COLOR=\"YELLOW\";>{item2.Name}</BASEFONT></CENTER>", false, false);

            //Gumps.AddAlphaRegion(ref gump, 271, 149, 248, 306);
            Gumps.AddImageTiledButton(ref gump, 282, 210, 2329, 2329, 1, 0, 10000, item1.ItemID, item1.Hue, 12, 17);
            gump.gumpDefinition += $"{{itemproperty {item1.Serial}}}";

            //Gumps.AddLabel(ref gump, 400, 162, 0, $"{item2.Name}"); // ITEM NAME
            Gumps.AddImageTiledButton(ref gump, 500, 210, 2329, 2329, 1, 0, 10000, item2.ItemID, item2.Hue, 12, 17);
            gump.gumpDefinition += $"{{itemproperty {item2.Serial}}}";


            //Gumps.AddButton(ref gump, 288, 351, 4005, 4007, 1, 1, 0);

            /*
            Gumps.AddBackground(ref gump, 0, 0, 230, 137, 5054);
            Gumps.AddAlphaRegion(ref gump, 10, 10, 210, 117);

            Gumps.AddImageTiled(ref gump, 10, 10, 210, 22, 2624); // Black Backgroun of the text
            Gumps.AddHtml(ref gump, 10, 12, 212, 20, "<CENTER><BASEFONT COLOR=\"YELLOW\">MINING CARTS AND STUMPS</BASEFONT></CENTER>", false, false);

            Gumps.AddButton(ref gump, 25, 60, 4005, 4007, 1, 1, 0); // Mining Carts
            Gumps.AddHtml(ref gump, 70, 62, 212, 20, "<BASEFONT COLOR=\"WHITE\">MINING CARTS</BASEFONT>", false, false);

            Gumps.AddButton(ref gump, 25, 85, 4005, 4007, 2, 1, 0); // Tree Stumps
            Gumps.AddHtml(ref gump, 70, 87, 212, 20, "<BASEFONT COLOR=\"WHITE\">TREE STUMPS</BASEFONT>", false, false);
            */

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
