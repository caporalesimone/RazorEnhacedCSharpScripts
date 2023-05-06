using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
            var gump = Gumps.CreateGump(true, true, true, false);
            gump.gumpId = gumpId;
            gump.serial = (uint)Player.Serial;
            Gumps.AddPage(ref gump, 0);
            Gumps.AddBackground(ref gump, 0, 0, 220, 100, 9200);
            Gumps.AddLabel(ref gump, 10, 10, 0, Player.Name);
            Gumps.AddButton(ref gump, 10, 50, 2242, 2242, 22, 1, 0);
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
