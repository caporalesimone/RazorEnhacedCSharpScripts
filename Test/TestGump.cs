using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace RazorEnhanced
{
    class TestGump
    {
        private const int gumpId = 84765431;

        public void Run()
        {
            var time = UnixTime();
            Misc.SendMessage("> " + time, 201);

            this.CreateMenuInstance();

            while (this.EventListener())
            {
                Misc.Pause(100);
            }
        }

        public static long UnixTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public void CreateMenuInstance()
        {
            var gump = Gumps.CreateGump(true, true, true, true);
            gump.gumpId = gumpId;
            gump.serial = (uint)Player.Serial;

            // Help from https://docs.polserver.com/pol100/guides.php?guidefile=gumpcmdlist#button


            Gumps.AddPage(ref gump, 0);
            Gumps.AddBackground(ref gump, 0, 0, 530, 437, 5054);
            Gumps.AddImageTiled(ref gump, 10, 10, 510, 22, 2624);
            Gumps.AddImageTiled(ref gump, 10, 292, 150, 45, 2624);
            Gumps.AddImageTiled(ref gump, 165, 292, 355, 45, 2624);
            Gumps.AddImageTiled(ref gump, 10, 342, 510, 85, 2624);
            Gumps.AddImageTiled(ref gump, 10, 37, 200, 250, 2624);
            Gumps.AddImageTiled(ref gump, 215, 37, 305, 250, 2624);
            
            Gumps.AddAlphaRegion(ref gump, 10, 10, 510, 417);

            //Gumps.AddHtml(ref gump, 10, 12, 512, 20, "<CENTER><BASEFONT COLOR=\"YELLOW\" SIZE=\"7\">COLORED TITLE</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 12, 512, 20, "<CENTER><BASEFONT COLOR=\"YELLOW\">CARPENTRY MENU</BASEFONT></CENTER>", false, false);


            //Gumps.AddHtmlLocalized(ref gump, 10, 12, 510, 20, 1044004, 32767, false, false); // CARPENTRY MENU
            Gumps.AddHtmlLocalized(ref gump, 10, 37, 200, 22, 1044010, 32767, false, false); // CATEGORIES
            Gumps.AddHtmlLocalized(ref gump, 215, 37, 305, 22, 1044011, 32767, false, false); // SECTIONS
            Gumps.AddHtmlLocalized(ref gump, 10, 302, 150, 25, 1044012, 32767, false, false); // NOTICES

            Gumps.AddButton(ref gump, 15, 402, 4017, 4019, 0, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 50, 405, 150, 18, 1011441, 32767, false, false); // EXIT

            Gumps.AddButton(ref gump, 270, 402, 4005, 4007, 21, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 305, 405, 150, 18, 1044013, 32767, false, false); // MAKE LAST

            Gumps.AddButton(ref gump, 270, 362, 4005, 4007, 49, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 305, 365, 150, 18, 1044017, 32767, false, false); // MARK ITEM

            Gumps.AddButton(ref gump, 270, 342, 4005, 4007, 42, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 305, 345, 150, 18, 1044260, 32767, false, false); // REPAIR ITEM

            Gumps.AddButton(ref gump, 270, 382, 4005, 4007, 63, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 305, 385, 150, 18, 1061001, 32767, false, false); // ENHANCE ITEM

            Gumps.AddButton(ref gump, 15, 362, 4005, 4007, 7, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 50, 365, 150, 18, 1072643, "0", 32767, false, false); // WOOD (0) (parametro)

            Gumps.AddButton(ref gump, 15, 60, 4005, 4007, 28, 1, 0);
            Gumps.AddHtmlLocalized(ref gump, 50, 63, 150, 18, 1044014, 32767, false, false); // LAST TEN

            
            Gumps.SendGump(gump.gumpId, gump.serial, 0, 0, gump.gumpDefinition, gump.gumpStrings);
        }

        public bool EventListener()
        {
            var gumpData = Gumps.GetGumpData(gumpId);
            Misc.SendMessage(gumpData.gumpId.ToString());

            if (gumpData.gumpId == gumpId && gumpData.buttonid == 0)
            {
                Misc.SendMessage("Closing", 201);
                return false;
            }


            if (gumpData.gumpId == gumpId && gumpData.buttonid == 22)
            {
                Gumps.CloseGump(gumpId);
                this.CreateMenuInstance();
                Misc.SendMessage("> OnButton()", 201);
            }
            return true;

        }
    }
}
