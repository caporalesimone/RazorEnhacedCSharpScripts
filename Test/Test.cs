//C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Libs;

//-#forcerelease
//#import <..\Scripts\Libs\conversions.cs>

namespace RazorEnhanced
{
    public class Test
    {
        public void Run()
        {
            double cnt = 0;

            var start = DateTime.Now;
            for (int i = 0; i < 30000000; i++)
            {
                cnt++;
                if (cnt / 2 == 0) cnt = 2;
            }
            Misc.SendMessage(DateTime.Now - start);

            // Testing Conversion Function
            (int x, int y) = Conversions.SextantToXY(84, 6, 'S', 152, 0, 'E');
            Misc.SendMessage(x.ToString() + "," + y.ToString());
        }
    }
}
