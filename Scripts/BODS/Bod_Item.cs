//#CS
using RazorEnhanced;
using System;
using System.Linq;
using System.Text.RegularExpressions;

//#forcedebug

namespace BOD
{
    public class Bod
    {
        private Skill m_requiredSkill = Skill.SKILL_UNKNOWN;
        private Size m_size = Size.SIZE_UNKNOWN;
        private Item m_bodItem;
        private short m_amount = 0;
        private bool m_exceptional = false;
        private string m_specialMaterial = string.Empty;

        public enum Skill { TAILORING, BLACKSMITHING, SKILL_UNKNOWN = 0xFF }
        public enum Size { SMALL_BOD, LARGE_BOD, SIZE_UNKNOWN = 0xFF }

        /// <summary>
        /// Server serial of the BOD Item
        /// </summary>
        public int Serial => m_bodItem?.Serial ?? 0; // If _bodItem is null returns 0;

        /// <summary>
        /// Amount of item required to full this BOD
        /// </summary>
        public short Amount { get => m_amount; }
        
        /// <summary>
        /// Requires Exceptional crefted items
        /// </summary>
        public bool Exceptional { get => m_exceptional; }

        /// <summary>
        /// Required skill to work this BOD
        /// </summary>
        public Skill RequiredSkill { get => m_requiredSkill; }

        /// <summary>
        /// Size of the BOD. Could be Small or Large
        /// </summary>
        public Size BodSize { get => m_size; }

        /// <summary>
        /// Contains the name of the special material needed, if required
        /// </summary>
        public string SpecialMaterialRequired { get => m_specialMaterial; }

        /// <summary>
        /// This is the Factory method needed to create a new BOD Item from a serial
        /// </summary>
        /// <param name="serial">Razor Enhanced item serial that is expected to be a BOD</param>
        /// <returns>null if items is not a BOD, a Bod item otherwise</returns>
        public static Bod CreateNewBod(int serial)
        {
            Item bod = Items.FindBySerial(serial);

            // Checks if the serial is a real BOD
            if (bod == null) return null;
            if (bod.Properties.Count <= 0) return null;
            if (bod.ItemID != 0x2258) return null; // Graphic ID of a BOD
            if (!bod.Properties[0].ToString().Contains("a bulk order deed")) return null; 

            // Creating a new BOD
            Bod newBod = new Bod();
            newBod.BuildTheBodObject(bod);

            return newBod;
        }

        /// <summary>
        /// Private constructor because this is a factory class
        /// </summary>
        private Bod() {  }

        /// <summary>
        /// This function scans the item and collects all the info in order to populate all properties of the BOD Class 
        /// </summary>
        /// <param name="bod">Razor Item of the Bod</param>
        private Bod BuildTheBodObject(Item bod)
        {
            // BOD Item
            m_bodItem = bod;

            // BOD Required Skill
            switch (bod.Hue)
            {
                case 0x0483:
                    m_requiredSkill = Skill.TAILORING;
                    break;
                case 0x044e:
                    m_requiredSkill = Skill.BLACKSMITHING;
                    break;
                default:
                    m_requiredSkill = Skill.SKILL_UNKNOWN;
                    break;
            }

            foreach (var prop in bod.Properties)
            {
                string text = prop.ToString();

                var skip = new[] { "a bulk order deed", "Blessed", "Weight" };
                if (skip.Any(text.Contains))
                {
                    continue;
                }
                else if (text.Contains("small bulk order"))
                {
                    m_size = Size.SMALL_BOD;
                    continue;
                }
                else if (text.Contains("large bulk order"))
                {
                    m_size = Size.LARGE_BOD;
                    continue;
                }
                else if (text.Contains("exceptional"))
                {
                    m_exceptional = true;
                    continue;
                }
                else if (text.Contains("must be made with"))
                {
                    // Extract the kind of material
                    string regex_rule = @"with\s(?<material>[\w\s]+)\s(ingots|leather).";
                    m_specialMaterial = Regex.Match(text, regex_rule, RegexOptions.IgnoreCase).Groups["material"].Value;
                    continue;
                }
                else if (text.Contains("amount to make"))
                {
                    m_amount = System.Int16.Parse(text.Split(':')[1]);
                }
/*
                else
                {
                    try
                    {
                        string itemName = text.Split(':')[0];
                        short made = System.Int16.Parse(text.Split(':')[1]);

                        BodCraftable craft = BodCraftableDatabase.Instance.FindCraftable(itemName, m_specialMaterial);
                        if (craft != null)
                        {
                            _craftableStatus.Add((craft, made));
                        }
                    }
                    catch
                    {
                        RazorEnhanced.Logger.Log("Error parsing BOD with serial: " + bodItem.Serial.ToString());
                    }
                }
*/
            }
            return this;
        }
    }

    public class Test
    {
        public void Run ()
        {
            Target t = new Target();
            int serial = t.PromptTarget();

            Bod bod = Bod.CreateNewBod(serial);
            Misc.SendMessage(bod.Serial);
        }
    }

}
