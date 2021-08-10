// Conversions Library
//  This library contains manu useful function for convert stuffs (eg from sextant coordinates to xy coordinates)
// 
// Developed by SimonSoft - 2021

using System;

namespace Scripts.Libs
{
    class Conversions
    {
        static public (int x, int y) SextantToXY(int degNS, int minNS, char dirNS, int degWE, int minWE, char dirWE)
        {
            double off_x = 1323;
            double off_y = 1624;

            double max_x = 5120;
            double max_y = 4096;

            double tile_deg_x = max_x / 360;
            double tile_deg_y = max_y / 360;

            double tile_min_x = tile_deg_x / 60;
            double tile_min_y = tile_deg_y / 60;

            // coords relative to throne
            double x = (degWE * tile_deg_x) + (minWE * tile_min_x);
            double y = (degNS * tile_deg_y) + (minNS * tile_min_y);

            //# coords relative to world  
            x = off_x + ((dirWE == 'E') ? x : -x);
            y = off_y + ((dirNS == 'S') ? y : -y);

            if (x < 0) x = max_x - x;
            if (y < 0) x = max_y - y;

            //# correct for "round world"
            x %= max_x;
            y %= max_y;

            return ((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
