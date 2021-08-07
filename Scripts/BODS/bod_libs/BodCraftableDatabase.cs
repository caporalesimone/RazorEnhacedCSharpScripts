//
// BOD Craftable Database
// 
// Developed by SimonSoft - 2021
//
// Note:
//          This is a library made for SSBodsFiller and was tailored on Demise Server
//          It could work on other server with small changes on craftableItemsDatabase

using System.Collections.Generic;
using System.Linq;
using Scripts.Libs;

//#import <../../Libs/logger.cs>

namespace BODS
{
    class BodCraftableDatabase
    {
        public enum Skill
        {
            TAILORING = 0,
            BLACKSMITHY = 1,

            UNKNOWN = 255
        }

        private static BodCraftableDatabase m_instance = null;
        public static BodCraftableDatabase Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new BodCraftableDatabase();
                }
                return m_instance;
            }
        }
        private BodCraftableDatabase() { }
        public BodCraftable FindCraftable(string craftName, string specialMaterial)
        {
            foreach (var (Name, graphicID, gumpCategoryID, gumpSelectionID, materialList) in craftableItemsDatabase)
            {
                if (Name == craftName)
                {
                    // If the special material is defined then I will replace the base material with the special
                    if (specialMaterial != "")
                    {
                        var(_, spGID, _) = FindMaterial(specialMaterial);  // Looking for the grapthic ID of the special material

                        // For each material in list I try to see if it has the same graphic id of the special material
                        // in this case i just replace the name
                        for (int i = 0; i < materialList.Count; i++)
                        {
                            var (_, baseGID, _) = FindMaterial(materialList[i].mat);
                            if (spGID == baseGID)
                            {
                                materialList[i] = (specialMaterial, materialList[i].qty);
                                break;
                            }
                        }
                    }
                    return new BodCraftable(Name, graphicID, gumpCategoryID, gumpSelectionID, materialList);
                }
            }

            Logger.Log("Unable to find " + craftName + " in internal database", Logger.COLORS.RED);
            return null;
        }
        public (string material, int graphicID, int color) FindMaterial(string materialName)
        {
            foreach (var (material, graphicID, color) in materialDatabase)
            {
                if (material == materialName)
                {
                    return (material, graphicID, color);
                }
            }

            return ("", 0, 0); // Invalid material
        }

        public int GumpButtonForMaterial(string material)
        {
            foreach (var i in gumpButtonForMaterial)
            {
                if (i.material == material)
                {
                    return i.gumpButton;
                }
            }
            return 0;
        }

        public static Skill GetSkillFromMaterial(string material)
        {
            switch (material)
            {
                case "cloth":
                case "leather":
                case "barbed":
                case "horned":
                case "spined":
                case "bones":
                    return Skill.TAILORING;

                case "iron":
                case "dull copper":
                case "shadow iron":
                case "copper":
                case "bronze":
                case "golden":
                case "agapite":
                case "verite":
                case "valorite":
                    return Skill.BLACKSMITHY;

                default:
                    return Skill.UNKNOWN;
            }
        }

        public static List<int> DestroyToolFromMaterial(string material)
        {
            List<int> itemIDs = new List<int>();
            // The first material tells the tool needed
            switch (material)
            {
                case "cloth":
                case "leather":
                case "barbed":
                case "horned":
                case "spined":
                    itemIDs.Add(0x0F9F); // Scissors
                    break;

                case "iron":
                case "dull copper":
                case "shadow iron":
                case "copper":
                case "bronze":
                case "golden":
                case "agapite":
                case "verite":
                case "valorite":
                    itemIDs.Add(0x0FBB); // Tongs
                    itemIDs.Add(0x13E3); // Smith's Hammer
                    break;
            }
            return itemIDs;
        }

        public static uint DelayNeededForDestroyMaterial(string material)
        {
            uint delay;
            switch (material)
            {
                case "cloth":
                case "leather":
                case "barbed":
                case "horned":
                case "spined":
                    delay = 700;
                    break;

                case "iron":
                    delay = 600;
                    break;

                default:
                    delay = 1000;  // Default 1 second
                    break;
            }
            return delay;
        }

        public static List<int> ToolNeededForMaterial(string material)
        {
            List<int> tools = new List<int>();

            switch (material)
            {
                case "cloth":
                case "leather":
                case "barbed":
                case "horned":
                case "spined":
                    tools.Add(0x0F9D); // Sewing Kit
                    break;
                case "iron":
                case "dull copper":
                case "shadow iron":
                case "copper":
                case "bronze":
                case "golden":
                case "agapite":
                case "verite":
                case "valorite":
                    tools.Add(0x0FBB); // Tongs
                    tools.Add(0x13E3); // Smith's Hammer
                    break;
            }
            return tools;
        }

        public readonly List<(string material, int gumpButton)> gumpButtonForMaterial = new[]
        {
            ("leather", 6),
            ("spined",  13),
            ("horned",  20),
            ("barbed",  27),

            ("iron",        6),
            ("dull copper", 13),
            ("shadow iron", 20),
            ("copper",      27),
            ("bronze",      34),
            ("golden",      41),
            ("agapite",     48),
            ("verite",      55),
            ("valorite",    62),

        }.ToList();

        public class MatLst : List<(string mat, int qty)> { }

        public readonly List<(string material, int graphicID, int color)> materialDatabase = new[] {
            ("cloth",        0x1766, 0),

            ("leather",      0x1081, 0),
            ("barbed",       0x1081, 0x0851),
            ("horned",       0x1081, 0x0845),
            ("spined",       0x1081, 0x08AC),

            ("bones",        0x0F7E, 0),

            ("iron",         0x1BF2, 0),
            ("dull copper",  0x1BF2, 0x0973),
            ("shadow iron",  0x1BF2, 0x0966),
            ("copper",       0x1BF2, 0x096d),
            ("bronze",       0x1BF2, 0x0972),
            ("golden",       0x1BF2, 0x08a5),
            ("agapite",      0x1BF2, 0x0979),
            ("verite",       0x1BF2, 0x089f),
            ("valorite",     0x1BF2, 0x089f),
        }.ToList();

        private readonly List<(string Name, int graphicID, int gumpCategoryID, int gumpSelectionID, MatLst materialList)> craftableItemsDatabase = new[] {
            //
            // ALL INFOS FROM THIS LIST ARE BASED ON DEMISE SERVER
            //

            //***********************
            //** T A I L O R I N G **
            //***********************

            // HATS
            ("skullcap",        0x1544, 1, 2,   new MatLst { ("cloth",  2) } ),
            ("bandana",         0x1540, 1, 9,   new MatLst { ("cloth",  2) } ),
            ("floppy hat",      0x1713, 1, 16,  new MatLst { ("cloth", 11) } ),
            ("cap",             0x1715, 1, 23,  new MatLst { ("cloth", 11) } ),
            ("wide-brim hat",   0x1714, 1, 30,  new MatLst { ("cloth", 12) } ),
            ("straw hat",       0x1717, 1, 37,  new MatLst { ("cloth", 10) } ),
            ("tall straw hat",  0x1716, 1, 44,  new MatLst { ("cloth", 13) } ),
            ("wizard's hat",    0x1718, 1, 51,  new MatLst { ("cloth", 15) } ),
            ("bonnet",          0x1719, 1, 58,  new MatLst { ("cloth", 11) } ),
            ("feathered hat",   0x171A, 1, 65,  new MatLst { ("cloth", 12) } ),
            ("tricorne hat",    0x171B, 1, 72,  new MatLst { ("cloth", 12) } ),
            ("jester hat",      0x171C, 1, 79,  new MatLst { ("cloth", 15) } ),

            // SHIRTS - CAEGORIES ID 8
            ("doublet",         0x1F7B, 8, 2,   new MatLst { ("cloth", 8)  } ),
            ("shirt",           0x1517, 8, 9,   new MatLst { ("cloth", 8)  } ),
            ("fancy shirt",     0x1EFD, 8, 16,  new MatLst { ("cloth", 8)  } ),
            ("tunic",           0x1FA1, 8, 23,  new MatLst { ("cloth", 12)  } ),
            ("surcoat",         0x1FFD, 8, 30,  new MatLst { ("cloth", 14) } ),
            ("plain dress",     0x1F01, 8, 37,  new MatLst { ("cloth", 10) } ),
            ("fancy dress",     0x1F00, 8, 44,  new MatLst { ("cloth", 12) } ),
            ("cloak",           0x1515, 8, 51,  new MatLst { ("cloth", 14) } ),
            ("robe",            0x1F03, 8, 58,  new MatLst { ("cloth", 16) } ),
            ("jester suit",     0x1F9F, 8, 65,  new MatLst { ("cloth", 24) } ),

            // PANTS - CAEGORIES ID 15
            ("short pants",     0x152E, 15, 2,  new MatLst { ("cloth", 6)  } ),
            ("long pants",      0x1539, 15, 9,  new MatLst { ("cloth", 8)  } ),
            ("kilt",            0x1537, 15, 16, new MatLst { ("cloth", 8)  } ),
            ("skirt",           0x1516, 15, 23, new MatLst { ("cloth", 10) } ),

            // MISCELLANEOUS - CATEGORIES 22
            ("body sash",       0x1541, 22, 2, new MatLst { ("cloth", 4) } ),
            ("half apron",      0x153B, 22, 9, new MatLst { ("cloth", 6) } ),
            ("full apron",      0x153D, 22, 16, new MatLst { ("cloth", 10) } ),

            // FOOTWEAR - CAEGORIES ID 29
            ("sandals",         0x170D, 29, 30, new MatLst { ("leather", 4) } ),
            ("shoes",           0x170F, 29, 37, new MatLst { ("leather", 6) } ),
            ("boots",           0x170B, 29, 44, new MatLst { ("leather", 8) } ),
            ("thigh boots",     0x1711, 29, 51, new MatLst { ("leather", 10) } ),

            // LEATHER ARMOR - CAEGORIES ID 36
            ("leather gorget",  0x13C7, 36, 23, new MatLst { ("leather", 4) } ),
            ("leather cap",     0x1DB9, 36, 30, new MatLst { ("leather", 2) } ),
            ("leather gloves",  0x13C6, 36, 37, new MatLst { ("leather", 3) } ),
            ("leather sleeves", 0x13CD, 36, 44, new MatLst { ("leather", 4) } ),
            ("leather leggings",0x13CB, 36, 51, new MatLst { ("leather", 10) } ),
            ("leather tunic",   0x13CC, 36, 58, new MatLst { ("leather", 12) } ),

            // STUDDED ARMOR - CATEGORIES ID 43
            ("studded gorget",  0x13D6, 43, 2, new MatLst { ("leather", 6) } ),
            ("studded gloves",  0x13D5, 43, 9, new MatLst { ("leather", 8) } ),
            ("studded sleeves", 0x13D4, 43, 16, new MatLst { ("leather", 10) } ),
            ("studded leggings",0x13DA, 43, 23, new MatLst { ("leather", 12) } ),
            ("studded tunic",   0x13DB, 43, 30, new MatLst { ("leather", 14) } ),

            // FEMALE ARMOR - CATEGORIES ID 50
            ("leather shorts",  0x1C00, 50, 2,  new MatLst { ("leather", 8 ) } ),
            ("leather skirt",   0x1C08, 50, 9,  new MatLst { ("leather", 6 ) } ),
            ("leather bustier", 0x1C0A, 50, 16, new MatLst { ("leather", 6 ) } ),
            ("studded bustier", 0x1C0C, 50, 23, new MatLst { ("leather", 8) } ),
       ("female leather armor", 0x1C06, 50, 30, new MatLst { ("leather", 8) } ),
            ("studded armor",   0x1C02, 50, 37, new MatLst { ("leather", 10) } ),

            // BONES - CATEGORIES ID 57
            ("bone helmet",     0x1456, 57, 2,  new MatLst { ("leather", 4 ), ("bones", 2) } ),
            ("bone gloves",     0x1455, 57, 9,  new MatLst { ("leather", 6 ), ("bones", 2) } ),
            ("bone arms",       0x1453, 57, 16, new MatLst { ("leather", 8 ), ("bones", 4) } ),
            ("bone leggings",   0x1452, 57, 23, new MatLst { ("leather", 10), ("bones", 6) } ),
            ("bone armor",      0x144F, 57, 30, new MatLst { ("leather", 12), ("bones", 10) } ),


            //***************************
            //** B L A C K S M I T H Y **
            //***************************

            // RINGMAIL - CATEGORIES 1
            ("ringmail leggings",   0x13F0, 1, 9,   new MatLst { ("iron", 16 ) } ),
            ("ringmail sleeves",    0x13EE, 1, 16,  new MatLst { ("iron", 14 ) } ),
            //("ringmail gloves",     0x13EB, 1, 16,  new MatLst { ("iron", 14 ) } ),
            //("ringmail tunic",      0x13EC, 1, 16,  new MatLst { ("iron", 14 ) } ),

            // CHAINMAIL - CATEGORIES 8
            ("chainmail tunic",     0x13BF, 8, 16,  new MatLst { ("iron", 20 ) } ),
//["chainmail coif", 0x13BB, 5, 'tongs'],
//["chainmail leggings", 0x13BE, 6, 'tongs'],
//["chainmail tunic", 0x13BF, 7, 'tongs'],

            // PLATEMAIL - CATEGORIES 15
            ("female plate",        0x1C04, 15, 37, new MatLst { ("iron", 20 ) } ),
//["platemail arms", 0x1410, 8, 'tongs'],
//["platemail gloves", 0x1414, 9, 'tongs'],
//["platemail gorget", 0x1413, 10, 'tongs'],
//["platemail legs", 0x1411, 11, 'tongs'],
//["platemail tunic", 0x1415, 12, 'tongs'],
//["plate helm", 0x1412, 24, 'tongs'],

            // HELMETS - CATEGORIES 22
            ("bascinet",            0x140C, 22, 2,  new MatLst { ("iron", 15 ) } ),
            ("close helmet",        0x1408, 22, 9,  new MatLst { ("iron", 15 ) } ),
            ("norse helm",          0x140E, 22, 23, new MatLst { ("iron", 15 ) } ),
//["helmet", 0x140A, 22, 'tongs'],

            // SHIELDS - CATEGORIES ID 29
            //("buckler",             0x1B73, 29, 2, new MatLst { ("iron", 10 ) } ),
            //("bronze shield",       0x1B72, 29, 9, new MatLst { ("iron", 12 ) } ),
            ("heater shield",       0x1B76, 29, 16, new MatLst { ("iron", 18 ) } ),
            ("metal shield",        0x1B7B, 29, 23, new MatLst { ("iron", 14 ) } ),
            ("metal kite shield",   0x1B74, 29, 30, new MatLst { ("iron", 16 ) } ),
            ("tear kite shield",    0x1B79, 29, 37, new MatLst { ("iron", 8 ) } ),

            // BLADED - CATEGORIES 36
            //("broadsword",          0x0F5E, 36, 23, new MatLst { ("iron", 10 ) } ),
            ("cutlass",             0x1441, 36, 23, new MatLst { ("iron", 8 ) } ),
            ("dagger",              0x0F51, 36, 30, new MatLst { ("iron", 3 ) } ),
            ("katana",              0x13FF, 36, 37, new MatLst { ("iron", 8 ) } ),
            ("kryss",               0x1401, 36, 44, new MatLst { ("iron", 8 ) } ),
            //("longsword",           0x0F61, 36, 51, new MatLst { ("iron", 12 ) } ),
            ("scimitar",            0x13B6, 36, 58, new MatLst { ("iron", 10 ) } ),
            //("viking sword"",       0x13B9, 36, 65, new MatLst { ("iron", 14 ) } ),

            // AXES - CATEGORIES ID 43
            ("axe",                 0x0F49, 43, 2,  new MatLst { ("iron", 14 ) } ),
            ("battle axe",          0x0F47, 43, 9,  new MatLst { ("iron", 14 ) } ),
            ("double axe",          0x0F4B, 43, 16, new MatLst { ("iron", 12 ) } ),
            ("large battle axe",    0x13FB, 43, 30, new MatLst { ("iron", 12 ) } ),
            ("two handed axe",      0x1443, 43, 37, new MatLst { ("iron", 16 ) } ),
            ("war axe",             0x13B0, 43, 44, new MatLst { ("iron", 16 ) } ),
//["executioner's axe", 0x0F45, 62, 'tongs'], #'

            // POLEARMS - CATEGORIES 50
            ("halberd",             0x143E, 50, 23,  new MatLst { ("iron", 20 ) } ),
            ("short spear",         0x1403, 50, 44,  new MatLst { ("iron", 6 )  } ),
//["bardiche", 0x0F4D, 66, 'tongs'],
//['bladed staff', 0x26BD, 67, 'tongs'],

//["spear", 0x0F62, 74, 'tongs'],
//["war fork", 0x1405, 75, 'tongs'],

            // BASHING - CATEGORIES ID 57
            //("hammer pick",         0x143D, 57, 2,  new MatLst { ("iron", 16 ) } ),
            ("mace",                0x0F5C, 57, 9,  new MatLst { ("iron", 6 ) } ),
            ("maul",                0x143B, 57, 16, new MatLst { ("iron", 10 ) } ),
            ("war mace",            0x1407, 57, 30, new MatLst { ("iron", 14 ) } ),
            ("war hammer",          0x1439, 57, 37, new MatLst { ("iron", 16 ) } ),
         }.ToList();
    }
}
