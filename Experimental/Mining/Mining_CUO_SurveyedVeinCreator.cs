//C#
// step 1: Mining_WorldVeinCreator.cs-- Create CUO map explored vein markers(update when need visual update)
// step 2: Mining_CUO_SurveyedVeinCreator.cs-- Create starting points list + show with CUO map markers (1x thing)
// step 3: Mining_Survey.cs-- Character mining script, now auto walks from a to next where possible
using System;
using System.IO;
using System.Collections.Generic;

namespace RazorEnhanced
{
    public class Mining_CUO_SurveyedVeinCreator
    {
        // Base Path
        private const string basePath = @"C:\UltimaOnline\ClassicUO\ClassicUO\Data\Client";

        // Directory surveyed OreVeins
        private readonly string folderPath_OreVeinsSurveyed = Path.Combine(basePath, @"OreVeinsSurveyed\");

        // Directory file summary surveyed OreVeins
        private readonly string filePath_OreVeinsSurveyList = Path.Combine(basePath, @"OreVeinsSurveyList.txt");

        // File surveyed OreVeins CUO map
        private readonly string filePath_OreVeinsSurveyedList = Path.Combine(basePath, @"WorldOreVeinPositions.csv");

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

        public struct WorldMapVeinData
        {
            public string posX;
            public string posY;
            public string facet;
            public string description;
            public string cursor;
            public string colorMarker;
        }

        private List<WorldMapVeinData> worldMapVeinDataList = new List<WorldMapVeinData>();

        public void Run()
        {
            Log("Script Started.....");

            System.Diagnostics.Process.Start("cmd.exe", "/C " + @"dir.exe "+ folderPath_OreVeinsSurveyed + " /l /d /b /a-d >"+ filePath_OreVeinsSurveyList);
            Misc.Pause(500);
            string[] stringArray = System.IO.File.ReadAllLines(filePath_OreVeinsSurveyList);

            foreach (string stringLine in stringArray)
            {

                string posX = stringLine.Substring(0, 4);
                string posY = stringLine.Substring(4, 4);

                StreamReader sr = new StreamReader(folderPath_OreVeinsSurveyed + posX.ToString() + posY.ToString());

                int veinOreTypeValue = 0;
                string facet = "-1";
                
                string line = sr.ReadLine();
                Int32.TryParse(line.Split(',')[0], out veinOreTypeValue);
                facet = line.Split(',')[1];

                //Int32.TryParse(sr.ReadLine(), out veinOreTypeValue);
                sr.Close();

                string[] oreStringType = new string[] { "Empty_0", "Empty_1"
                                                            , "iron"          // Modified Empty_2
                                                            , "dullcopper"    // Modified space
                                                            , "shadow"
                                                            , "copper"
                                                            , "bronze"
                                                            , "golden"
                                                            , "agapite"
                                                            , "verite"
                                                            , "valorite" };


                WorldMapVeinData worldMapVeinDataObject = new WorldMapVeinData(); ;
                worldMapVeinDataObject.posX = posX;
                worldMapVeinDataObject.posY = posY;
                worldMapVeinDataObject.facet = facet;
                worldMapVeinDataObject.description = oreStringType[veinOreTypeValue];
                worldMapVeinDataObject.cursor = "orevein_" + oreStringType[veinOreTypeValue];
                worldMapVeinDataObject.colorMarker = "white";

                worldMapVeinDataList.Add(worldMapVeinDataObject);
            }


            string result = "";
            for (int i = 0; i < worldMapVeinDataList.Count; i++)
            {
                WorldMapVeinData worldMapVeinDataObject = worldMapVeinDataList[i];

                int posX = 0;
                Int32.TryParse(worldMapVeinDataObject.posX, out posX);
                result += posX.ToString();
                result += ",";
                int posY = 0;
                Int32.TryParse(worldMapVeinDataObject.posY, out posY);
                result += posY.ToString();
                result += ",";
                result += worldMapVeinDataObject.facet;
                result += ",";
                result += worldMapVeinDataObject.description;
                result += ",";
                result += worldMapVeinDataObject.cursor;
                result += ",";
                result += worldMapVeinDataObject.colorMarker;

                if (i < worldMapVeinDataList.Count - 1)
                {
                    result += "\n";
                }
            }

            File.WriteAllText(filePath_OreVeinsSurveyedList, result);

            Log("Script Finished.....");

        }
    }
}