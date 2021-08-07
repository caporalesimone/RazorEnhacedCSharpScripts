//C#
// step 1: Mining_WorldVeinCreator.cs-- Create CUO map explored vein markers(update when need visual update)
// step 2: Mining_CUO_SurveyedVeinCreator.cs-- Create starting points list + show with CUO map markers (1x thing)
// step 3: Mining_Survey.cs-- Character mining script, now auto walks from a to next where possible
using System;
using System.IO;
using System.Collections.Generic;
//using System.Windows.Forms;
//using System.Drawing;

namespace RazorEnhanced
{
    public class Mining_Survey
    {
        // Base path
        private const string basePath = @"C:\UltimaOnline\ClassicUO\ClassicUO\Data\Client";

        // OreVein starting position list
        private readonly string filePath_worldOreVeinPositions = Path.Combine(basePath, @"WorldOreVeinSurveyed.csv");

        // Directory surveyed OreVeins
        private readonly string folderPath_OreVeinsSurveyed = Path.Combine(basePath, @"OreVeinsSurveyed\");

        // Directory file summary surveyed OreVeins
        private readonly string filePath_OreVeinsSurveyList = Path.Combine(basePath, @"OreVeinsSurveyList.txt");


        private static void Log(object messageString)
        {
            Misc.SendMessage(messageString, 201);
        }

        public static long UnixTimeStamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /*
        public enum MapType
        {
            Felucca,
            Trammel,
            Ilshenar,
            Malas,
            Tokuno,
            TerMur
        }
        */

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

        public int DistanceToTile(int tilePosX, int tilePosY)
        {
            double powX = Math.Pow(Player.Position.X - tilePosX, 2);
            double powY = Math.Pow(Player.Position.Y - tilePosY, 2);
            int result = (int)Math.Round(Math.Pow(powX + powY, 0.5f));

            return result;
        }

        public struct WorldVeinPositionData
        {
            public int posX;
            public int posY;
        }

        private int[] itemID_oreList = new int[] { 0x19BA, 0x19B7, 0x19B8, 0x19B9 };

        private Mobile mobilePlayer = Mobiles.FindBySerial(Player.Serial);

        private List<WorldVeinPositionData> worldVeinPositionDataList;

        //private int itemID_rune = 0x1F14;
        //private int serialID_recallRuneHome = 0x452EB15F;
        public void Run()
        {
            Log("Script Started.....");
            Log(Player.Map);
            
            worldVeinPositionDataList = new List<WorldVeinPositionData>();
            ReadWorldOreVeins();

            while (true)
            {
                // If folder does not exists i create it
                if (!Directory.Exists(folderPath_OreVeinsSurveyed))
                {
                    Log("Creating directory " + folderPath_OreVeinsSurveyed);
                    Directory.CreateDirectory(folderPath_OreVeinsSurveyed);
                }

                RemoveSurveyedVeins();

                WorldVeinPositionData nearestWorldVeinPosition = GetNearestVein();

                MoveToDestination(nearestWorldVeinPosition);
            }
        }



        private void ReadWorldOreVeins()
        {
            string[] stringArray = System.IO.File.ReadAllLines(filePath_worldOreVeinPositions);

            foreach (string stringLine in stringArray)
            {
                WorldVeinPositionData worldVeinDataObject = new WorldVeinPositionData();

                int spi_1 = 0; // String Position Index
                int spi_2 = 0; // String Position Index

                spi_2 = stringLine.IndexOf(",", spi_1);
                int result_1 = 0;
                Int32.TryParse(stringLine.Substring(0, spi_2 - spi_1), out result_1);
                worldVeinDataObject.posX = result_1;

                spi_1 = spi_2 + 1;
                spi_2 = stringLine.IndexOf(",", spi_1);
                int result_2 = 0;
                Int32.TryParse(stringLine.Substring(spi_1, spi_2 - spi_1), out result_2);
                worldVeinDataObject.posY = result_2;

                //spi_1 = spi_2 + 1;
                //spi_2 = stringLine.IndexOf(",", spi_1);
                //int result_3 = 0;
                //Int32.TryParse(stringLine.Substring(spi_1, spi_2 - spi_1), out result_3);
                //worldVeinDataObject.posZ = result_3;

                //spi_1 = spi_2 + 1;
                //spi_2 = stringLine.IndexOf(",", spi_1);
                //worldVeinDataObject.description = stringLine.Substring(spi_1, spi_2 - spi_1);

                //spi_1 = spi_2 + 1;
                //spi_2 = stringLine.IndexOf(",", spi_1);
                //worldVeinDataObject.cursor = stringLine.Substring(spi_1, spi_2 - spi_1);

                //spi_1 = spi_2 + 1;
                //spi_2 = stringLine.IndexOf(",", spi_1);
                //worldVeinDataObject.colorMarker = stringLine.Substring(spi_1);

                worldVeinPositionDataList.Add(worldVeinDataObject);
            }
        }

        private void RemoveSurveyedVeins()
        {
            System.Diagnostics.Process.Start("cmd.exe", "/C " + @"dir.exe " + folderPath_OreVeinsSurveyed + " /l /d /b /a-d >" + filePath_OreVeinsSurveyList);
            Misc.Pause(500);


            string[] stringArray = System.IO.File.ReadAllLines(filePath_OreVeinsSurveyList);

            foreach (string stringLine in stringArray)
            {

                int posX = 0;
                Int32.TryParse(stringLine.Substring(0, 4), out posX);
                int posY = 0;
                Int32.TryParse(stringLine.Substring(4, 4), out posY);

                for (int i = 0; i < worldVeinPositionDataList.Count; i++)
                {
                    WorldVeinPositionData worldVeinPosition = worldVeinPositionDataList[i];

                    if (worldVeinPosition.posX == posX && worldVeinPosition.posY == posY)
                    {
                        worldVeinPositionDataList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private WorldVeinPositionData GetNearestVein()
        {
            WorldVeinPositionData worldVeinPositionData = new WorldVeinPositionData();

            int nearestDistance = 99999;
            for (int i = 0; i < worldVeinPositionDataList.Count; i++)
            {
                WorldVeinPositionData worldVeinPosition = worldVeinPositionDataList[i];

                int distanceToTile = DistanceToTile(worldVeinPosition.posX, worldVeinPosition.posY);
                if (distanceToTile < nearestDistance)
                {
                    nearestDistance = distanceToTile;
                    worldVeinPositionData = worldVeinPositionDataList[i];
                }
            }

            return worldVeinPositionData;
        }

        private bool MoveToDestination(WorldVeinPositionData worldVeinPosition)
        {
            bool isVeinTileHouseBlocked = Statics.CheckDeedHouse(worldVeinPosition.posX, worldVeinPosition.posY);
            if (isVeinTileHouseBlocked == true)
            {
                Log("WorldVeinPosition " + worldVeinPosition + " blocked by house...");

                WriteVeinFile(worldVeinPosition, VeinOreType.Unreachable);
                return false;
            }

            List<Tile> tileList = PathFinding.GetPath(worldVeinPosition.posX, worldVeinPosition.posY, false);
            if (tileList == null)
            { return false; }

            int targetPositionZ = Statics.GetLandZ(tileList[0].X, tileList[0].Y, Player.Map);
            Player.PathFindTo(worldVeinPosition.posX, worldVeinPosition.posY, targetPositionZ);

            while (true)
            {
                Misc.Pause(250);
                int distance = DistanceToTile(worldVeinPosition.posX, worldVeinPosition.posY);
                if (distance <= 1)
                {
                    WorldVeinPositionData wPos = worldVeinPosition;

                    wPos.posX += 0;
                    wPos.posY += 0;
                    SurveyVein(wPos, targetPositionZ);

                    wPos.posX += 0;
                    wPos.posY -= 1;
                    SurveyVein(wPos, targetPositionZ);

                    wPos.posX += 1;
                    wPos.posY -= 0;
                    SurveyVein(wPos, targetPositionZ);

                    wPos.posX -= 0;
                    wPos.posY += 1;
                    SurveyVein(wPos, targetPositionZ);

                    break;
                }
            }

            return true;
        }

        private void SurveyVein(WorldVeinPositionData worldVeinPosition, int posZ)
        {
            string stringJournal = "";
            int heighestOreType = 0;

            while (true)
            {
                while (Player.Weight > Player.MaxWeight - 50)
                {
                    foreach (int itemID_ore in itemID_oreList)
                    {
                        Item itemOre = Items.FindByID(itemID_ore, -1, Player.Backpack.Serial);
                        if (itemOre != null)
                        {
                            Log("Dropping ore");

                            int z = Statics.GetLandZ(Player.Position.X + 1, Player.Position.Y, Player.Map);
                            Items.MoveOnGround(itemOre, 0, Player.Position.X + 1, Player.Position.Y, z);
                            Items.DropItemGroundSelf(itemOre, 0);

                            Misc.Pause(1000);
                        }
                    }
                }

                Item shovel = Items.FindByID(0x0F39, -1, Player.Backpack.Serial);
                if (shovel == null)
                {
                    Log("No shovel found...");
                    break;
                }

                Items.UseItem(shovel.Serial);
                Misc.Pause(250);
                Target.TargetExecute(worldVeinPosition.posX, worldVeinPosition.posY, posZ);
                Misc.Pause(1500);

                string[] oreStringType = new string[] { "Empty_0", "Empty_1"
                                                      , "iron"
                                                      , "dull copper"
                                                      , "shadow iron"
                                                      , "copper"
                                                      , "bronze"
                                                      , "golden"
                                                      , "agapite"
                                                      , "verite"
                                                      , "valorite" };

                for (int i = 0; i < oreStringType.Length; i++)
                {
                    stringJournal = "You dig some " + oreStringType[i] + " ore and put it in your backpack.";
                    if (Journal.Search(stringJournal) == true)
                    {
                        Log(stringJournal);
                        Journal.Clear();

                        if (i > heighestOreType)
                        {
                            heighestOreType = i;
                        }
                    }
                }


                //if (heighestOreType > 2)
                //{
                //    WriteVeinFile(worldVeinPosition, (VeinOreType)heighestOreType);
                //}



                stringJournal = "There is no metal here to mine.";
                if (Journal.Search(stringJournal) == true)
                {
                    Log(stringJournal);
                    Journal.Clear();


                    if (heighestOreType > 0)
                    {
                        WriteVeinFile(worldVeinPosition, (VeinOreType)heighestOreType);
                    }

                    if (heighestOreType == 9)
                    {
                        Player.HeadMessage(201, "VERITE");
                        Player.HeadMessage(201, "VERITE");
                        Player.HeadMessage(201, "VERITE");
                    }
                    else if (heighestOreType == 10)
                    {
                        Player.HeadMessage(201, "VALORITE");
                        Player.HeadMessage(201, "VALORITE");
                        Player.HeadMessage(201, "VALORITE");
                    }

                    for (int i = 0; i < worldVeinPositionDataList.Count; i++)
                    {
                        if (worldVeinPositionDataList[i].posX == worldVeinPosition.posX && worldVeinPositionDataList[i].posY == worldVeinPosition.posY)
                        {
                            worldVeinPositionDataList.RemoveAt(i);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private void WriteVeinFile(WorldVeinPositionData worldVeinPosition, VeinOreType veinOreTypeResult)
        {
            Player.HeadMessage(201, worldVeinPosition.posX + "," + worldVeinPosition.posY + " >> " + veinOreTypeResult.ToString());

            string posX = worldVeinPosition.posX.ToString("0000.##");
            string posY = worldVeinPosition.posY.ToString("0000.##");

            StreamWriter sw = new StreamWriter(folderPath_OreVeinsSurveyed + posX + posY, true);

            string result = ((int)veinOreTypeResult).ToString() + "," + Player.Map.ToString();
            sw.WriteLine(result.ToString());
            sw.Close();
        }
    }
}