using System.Collections.Generic;
using System.Linq;
using System.Threading;

//#forcedebug

namespace RazorEnhanced
{
    internal class TestGump1
    {
        public TestGump1()
        {
        }

        public void Run ()
        {
            int same = 0;
            int different = 0;
            for (int i = 0; i < 20; i++)
            {
                int SERIAL_RECALLBOOK = 0x415C17F9;

                Items.UseItem(SERIAL_RECALLBOOK);
                Gumps.WaitForGump(0, 20000);

                uint gump = Gumps.CurrentGump();
                string gumpContent = Gumps.GetGumpRawData(gump);
                var gumpLines1 = Gumps.GetGumpRawText(gump);

                Gumps.SendAction(gump, 1150); // Next poage

                Gumps.WaitForGump(gump, 20000);
                gumpContent = Gumps.GetGumpRawData(gump);
                var gumpLines2 = Gumps.GetGumpRawText(gump);

                bool areEqual = gumpLines1.SequenceEqual(gumpLines2);
                if (areEqual)
                {
                    Player.HeadMessage(33, $"The same: {++same}");
                }
                else
                {
                    Player.HeadMessage(33, $"Different: {++different}");
                }
                Gumps.CloseGump(gump);
            }
        }
    }
}
