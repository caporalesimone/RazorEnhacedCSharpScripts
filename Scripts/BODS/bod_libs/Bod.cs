//
// BODs Class
// 
// Developed by SimonSoft - 2021
//
// Note:
//          This is a library made for SSBodsFiller and was tailored on Demise Server
//          It could work on other server with small changes

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RazorEnhanced;
using Scripts.Libs;

namespace BODS
{
    class Bod
    {
        public enum BodSkillEnum
        {
            TAILORING,
            BLACKSMITHING,

            UNKNOWN,
        }
        public enum BodSizeEnum
        {
            SMALL,
            LARGE
        }

        private bool _isExceptional = false;
        private int _amountMax = 0;
        private int _serial = 0;
        private string _specialMaterial = "";
        private BodSkillEnum _bodType = BodSkillEnum.UNKNOWN;
        private BodSizeEnum _bodSize = BodSizeEnum.SMALL;

        // Item and quantity made
        private readonly List<(BodCraftable ItemToDo, int qty)> _craftableStatus = new List<(BodCraftable ItemToDo, int qty)>();

        public Bod(Item bodItem)
        {
            UpdateBodInfos(bodItem);
        }
        private void UpdateBodInfos(Item bodItem)
        {
            if (bodItem != null && bodItem.Properties.Count > 0 && bodItem.Properties[0].ToString().Contains("a bulk order deed"))
            {
                _serial = bodItem.Serial;

                switch (bodItem.Hue)
                {
                    case 0x0483:
                        _bodType = BodSkillEnum.TAILORING;
                        break;
                    case 0x044e:
                        _bodType = BodSkillEnum.BLACKSMITHING;
                        break;
                    default:
                        _bodType = BodSkillEnum.UNKNOWN;
                        break;
                }

                foreach (var prop in bodItem.Properties)
                {
                    string text = prop.ToString();

                    var skip = new[] { "a bulk order deed", "Blessed", "Weight" };
                    if (skip.Any(text.Contains))
                    {
                        continue;
                    }
                    else if (text.Contains("small bulk order"))
                    {
                        _bodSize = BodSizeEnum.SMALL;
                        continue;
                    }
                    else if (text.Contains("large bulk order"))
                    {
                        _bodSize = BodSizeEnum.LARGE;
                        continue;
                    }
                    else if (text.Contains("exceptional"))
                    {
                        _isExceptional = true;
                        continue;
                    }
                    else if (text.Contains("must be made with"))
                    {
                        // Extract the kind of material
                        string regex_rule = @"with\s(?<material>[\w\s]+)\s(ingots|leather).";
                        _specialMaterial = Regex.Match(text, regex_rule, RegexOptions.IgnoreCase).Groups["material"].Value;
                        continue;
                    }
                    else if (text.Contains("amount to make"))
                    {
                        _amountMax = Int32.Parse(text.Split(':')[1]);
                    }
                    else
                    {
                        try
                        {
                            string itemName = text.Split(':')[0];
                            int made = Int32.Parse(text.Split(':')[1]);

                            BodCraftable craft = BodCraftableDatabase.Instance.FindCraftable(itemName, _specialMaterial);
                            if (craft != null)
                            {
                                _craftableStatus.Add((craft, made));
                            }
                        }
                        catch
                        {
                            Logger.Log("Error parsing BOD with serial: " + bodItem.Serial.ToString());
                        }
                    }
                }
            }
            else
            {
                Logger.Log("This doesn't seems to be a BOD. Serial: " + bodItem.Serial.ToString());
            }
        }
        public override string ToString()
        {
            string text;
            switch (_bodType)
            {
                case BodSkillEnum.TAILORING:
                    text = "[T]";
                    break;
                case BodSkillEnum.BLACKSMITHING:
                    text = "[B]";
                    break;
                case BodSkillEnum.UNKNOWN:
                default:
                    text = "[?]";
                    break;
            }

            if (_bodSize == BodSizeEnum.SMALL)
            {
                if (_craftableStatus.Count > 0)
                    text = text + " " + _craftableStatus[0].ItemToDo.Name + " : " + _craftableStatus[0].qty + "/" + _amountMax + ((_isExceptional == true) ? " - Exceptional" : "");
                else
                    return "Bod not valid (Missing from database?)";
            }
            else
            {
                if (_craftableStatus.Count > 0)
                    text = text + "Large Bod with " + _craftableStatus.Count + " items to be made" + ((_isExceptional == true) ? " all Exceptional" : "");
                else
                    return "Large Bod not initalized";
            }

            if (_specialMaterial != "")
            {
                text = text + " - [ " + _specialMaterial + " ]";
            }

            return text;

        }
        public void UpdateBodInfos()
        {
            _craftableStatus.Clear();
            Item bod = Items.FindBySerial(_serial);
            Misc.Pause(200);
            UpdateBodInfos(bod);
        }
        public BodSizeEnum Size { get { return _bodSize; } }
        public bool IsExceptional { get { return _isExceptional; } }
        public int AmountMax { get { return _amountMax; } }
        public int Serial { get { return _serial; } }
        public BodSkillEnum Type { get { return _bodType; } }
        public bool IsFilled
        {
            get
            {
                bool filled = true;

                foreach (var itm in _craftableStatus)
                {
                    if (itm.qty != _amountMax)
                    {
                        filled = false;
                        break;
                    }
                }

                if (_craftableStatus.Count <= 0)
                {
                    filled = false;
                }

                return filled;
            }
        }
        public List<(BodCraftable ItemToDo, int qty)> Craftables { get { return _craftableStatus; } }
    }

    class BodCraftable
    {
        private readonly string _name = "";     // Name of the item to be crafted
        private int _graphicID = 0;             // Grapthic ID of the item to be crafted
        private int _gumpCategory = 0;          // Gump Category Button
        private int _gumpSelection = 0;         // Gump Selection Button
        List<Resource> _resourcesNeeded = new List<Resource>(); // Resources needed for craft this item
        public struct Resource
        {
            public readonly string material;
            public readonly int graphicID;
            public readonly int color;
            public readonly int quantity;

            public Resource(string material, int graphicID, int color, int quantity)
            {
                this.material = material;
                this.graphicID = graphicID;
                this.color = color;
                this.quantity = quantity;
            }
        }

        public BodCraftable(string name, int graphicID, int gumpCategory, int gumpSelection, BodCraftableDatabase.MatLst materials)
        {
            _name = name;
            _graphicID = graphicID;
            _gumpCategory = gumpCategory;
            _gumpSelection = gumpSelection;

            foreach (var i in materials)
            {
                var (mat, gID, col) = BodCraftableDatabase.Instance.FindMaterial(i.mat);
                _resourcesNeeded.Add(new Resource(mat, gID, col, i.qty));
            }
        }
        public string Name { get { return _name; } }
        public int GraphicID { get { return _graphicID; } }
        public (int category, int selection) GumpButtons { get { return (_gumpCategory, _gumpSelection); } }
        public List<Resource> ResourceList { get { return _resourcesNeeded; } }

        public BodCraftableDatabase.Skill RequiredSkill
        {
            get
            {
                return BodCraftableDatabase.GetSkillFromMaterial(_resourcesNeeded[0].material);
            }
        }
        public List<int> ToolsNeeded
        {
            get
            {
                return BodCraftableDatabase.ToolNeededForMaterial(_resourcesNeeded[0].material);
            }
        }
        public List<int> ToolNeededForDestroy
        {
            get
            {
                return BodCraftableDatabase.DestroyToolFromMaterial(_resourcesNeeded[0].material);
            }
        }
        public uint DelayNeededForDestroy
        {
            get
            {
                return BodCraftableDatabase.DelayNeededForDestroyMaterial(_resourcesNeeded[0].material);
            }
        }
    }
}
