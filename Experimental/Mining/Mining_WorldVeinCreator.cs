//C#
// step 1: Mining_WorldVeinCreator.cs-- Create CUO map explored vein markers(update when need visual update)
// step 2: Mining_CUO_SurveyedVeinCreator.cs-- Create starting points list + show with CUO map markers (1x thing)
// step 3: Mining_Survey.cs-- Character mining script, now auto walks from a to next where possible

// The CSV format for World Map markers is quite simple. The format is
// x, y, mapindex, name of marker, iconname, color, zoom level

using System;
using System.IO;
using System.Collections.Generic;

namespace RazorEnhanced
{
    public class Mining_WorldVeinCreator
    {
        // Base path
    	private const string basePath = @"C:\UltimaOnline\ClassicUO\ClassicUO\Data\Client";

        // OreVein starting position list
        private readonly string filePath = Path.Combine(basePath, @"WorldOreVeinSurveyed.csv");

        private static void HeadLog(string message)
        {
            Player.HeadMessage(20, message);
        }

        private static void Log(object messageString)
        {
            Misc.SendMessage(messageString, 201);
        }

        public static long UnixTimeStamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public enum MapType
        {
            Felucca,
            Trammel,
            Ilshenar,
            Malas,
            Tokuno,
            TerMur
        }

        public enum NotorietyType
        {
            Black,
            Blue,
            Green,
            Greyish,
            Grey,
            Orange,
            Red,
            Yellow
        }

        public enum VeinOreType
        {
            Unknown,
            Unreachable,
            Iron,
            DullCopper,
            Shadow,
            Copper,
            Bronze,
            Gold,
            Agapite,
            Verite,
            Valorite
        }

        Mobile mobilePlayer = Mobiles.FindBySerial(Player.Serial);

        private class WorldMapVeinData
        {
            public int posX;
            public int posY;
            public int facet;
            public string description;
            public string cursor;
            public string colorMarker;
            public int zoomlevel;
        }


        WorldMapVeinData GetPositionInfo(int x, int y, int facet)
        {
            int landID1 = Statics.GetLandID(x, y, facet);
            int landID2 = Statics.GetLandID(x, y - 1, facet);
            int landID3 = Statics.GetLandID(x + 1, y - 1, facet);
            int landID4 = Statics.GetLandID(x + 1, y, facet);
            if (landID1 == 240
                && landID2 == 241
                && landID3 == 242
                && landID4 == 243)
            {
                int z = Statics.GetLandZ(x, y, facet);
                if (z == 0)
                {
                    WorldMapVeinData worldMapVeinDataObject = new WorldMapVeinData();
                    worldMapVeinDataObject.posX = x;
                    worldMapVeinDataObject.posY = y;
                    worldMapVeinDataObject.facet = facet;
                    worldMapVeinDataObject.description = "";
                    worldMapVeinDataObject.cursor = "";
                    worldMapVeinDataObject.colorMarker = "white";
                    worldMapVeinDataObject.zoomlevel = 9;

                    return worldMapVeinDataObject;
                }
            }
            return null;
        }


        private List<WorldMapVeinData> worldMapVeinDataList = new List<WorldMapVeinData>();

        public void Run()
        {
            Log("Script Started.....");

            Log("Generating world vein list.....");

            int facet = Player.Map;

            long np = 0;
            long prevn = -1;
            for (int x = 0; x < 6143; x++)
            {
                for (int y = 0; y < 4095; y++)
                {
                    long n = np++ * 100 / (6143 * 4095);

                    if ((n % 10 == 0) && (n != prevn))
                    {
                        prevn = n;
                        string str = "[";
                        for (int i = 0; i < n/10; i++)
                        {
                            str = str + "*";
                        }
                        for (int i = (int)(n/10); i < 10; i++)
                        {
                            str = str + "_";
                        }
                        HeadLog(str + "]");
                    }

                    WorldMapVeinData veinData = GetPositionInfo(x, y, (int)MapType.Felucca);
                    if (veinData != null)
                    {
                        // Trammel and Felucca are identical. Find one is enough, the other is the same
                        worldMapVeinDataList.Add(veinData); // Adding Felucca
                        veinData.facet = (int)MapType.Trammel; // Changing info to Trammel
                        worldMapVeinDataList.Add(veinData); // Adding Trammel
                    }

                    

                }
            }
            HeadLog("[**********]");
            HeadLog("Writing file...");

            // The CSV format for World Map markers is quite simple. The format is
            // x, y, mapindex, name of marker, iconname, color, zoom level

            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < worldMapVeinDataList.Count; i++)
            {
                WorldMapVeinData worldMapVeinDataObject = worldMapVeinDataList[i];

                string result = "";
                result += worldMapVeinDataObject.posX;
                result += ",";
                result += worldMapVeinDataObject.posY;
                result += ",";
                result += worldMapVeinDataObject.facet;
                result += ",";
                result += worldMapVeinDataObject.description;
                result += ",";
                result += worldMapVeinDataObject.cursor;
                result += ",";
                result += worldMapVeinDataObject.colorMarker;
                result += ",";
                result += worldMapVeinDataObject.zoomlevel;
                sw.WriteLine(result);
            }
            sw.Close();

            HeadLog("File Saved.");

        }
    }
}