//
// Common functions library.
// 
// Developed by SimonSoft - 2021
// Tested on Demise Server
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RazorEnhanced;

namespace Scripts.Libs
{
    class Common
    {
        private const int DELAY_USE_ITEM_MS = 600;

        /// <summary>
        /// This pause is better for windowed script bacause it allow execution of events in winform
        /// </summary>
        /// <param name="milliseconds"></param>
        static public void Pause(uint milliseconds)
        {
            for (int i = 0; i < milliseconds; i += 2)
            {
                Application.DoEvents();
                Thread.Sleep(2);
            }
        }

        /// <summary>
        /// Returns a list of all items from a list of the itemIDs. It can look recursiverly inside a container
        /// </summary>
        /// <param name="itemIDs"> List of itemsID</param>
        /// <param name="container"> Container where look into</param>
        /// <param name="exceptionalOnly"> Finds only exceptional items</param>
        /// <param name="recursive"> Look recursively into containers inside the main container</param>
        /// <param name="stopAtFirst"> Returns only the first found. Optimization if you need only one item</param>
        /// <returns>List of found Items</returns>
        public static List<Item> FindItems(List<int> itemIDs, Item container, bool exceptionalOnly = false, bool recursive = true, bool stopAtFirst = false)
        {
            List<Item> itemList = new List<Item>();

            foreach (Item item in container.Contains)
            {
                if (itemIDs.Contains(item.ItemID))
                {
                    //Items.WaitForProps(item.Serial, delayWaitForProprs);
                    if (exceptionalOnly == false)
                    {
                        itemList.Add(item);
                        if (stopAtFirst) { return itemList; }
                    }
                    else
                    {
                        foreach (Property prop in item.Properties)
                        {
                            if (prop.ToString().ToLower().Contains("exceptional"))
                            {
                                itemList.Add(item);
                                if (stopAtFirst) { return itemList; }
                            }
                        }
                    }
                }
            }

            if (recursive)
            {
                List<Item> subcontainers = container.Contains.Select(sublist => sublist).Where(item => item.IsContainer).ToList();

                foreach (Item bag in subcontainers)
                {
                    // If is a Book of BOD skip
                    if (bag.ItemID == 0x2259) { continue; }
                    Items.UseItem(bag);
                    Pause(DELAY_USE_ITEM_MS);
                    List<Item> itemInSubContainer = FindItems(itemIDs, bag, exceptionalOnly, true, stopAtFirst);
                    itemList.AddRange(itemInSubContainer);
                    if (stopAtFirst) { return itemList; }
                }
            }

            return itemList;
        }

        /// <summary>
        /// Returns a list of all items with the provided itemID. It can look recursiverly inside a container
        /// </summary>
        /// <param name="itemID">ItemID of the item you want look for</param>
        /// <param name="container"> Container where look into</param>
        /// <param name="exceptionalOnly"> Finds only exceptional items</param>
        /// <param name="recursive"> Look recursively into containers inside the main container</param>
        /// <param name="stopAtFirst"> Returns only the first found. Optimization if you need only one item</param>
        /// <returns>List of found Items</returns>
        public static List<Item> FindItems(int itemID, Item container, bool exceptionalOnly = false, bool recursive = true, bool stopAtFirst = false)
        {
            return FindItems(new List<int>() { itemID }, container, exceptionalOnly, recursive, stopAtFirst);
        }

        /// <summary>
        /// Finds a string in the last opened gump
        /// </summary>
        /// <param name="text">Message to be found</param>
        /// <returns>true if is found</returns>
        public static bool FindTextInLastGump(string text)
        {
            foreach (string line in Gumps.LastGumpGetLineList())
            {
                if (line.ToLower().Contains(text))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Looks for all not exceptional items with itemID in player backpack. It's not a recursive find.
        /// </summary>
        /// <param name="itemID">ItemID of the items to be found</param>
        /// <returns>List of all not exceptional items</returns>
        public static List<Item> FindItemsNotExceptionalInBackpack(int itemID)
        {
            List<Item> itemList = new List<Item>();

            foreach (Item item in Player.Backpack.Contains)
            {
                if (item.ItemID == itemID)
                {
                    //Items.WaitForProps(item.Serial, delayWaitForProprs);
                    bool isExceptional = false;
                    foreach (Property prop in item.Properties)
                    {
                        if (prop.ToString().ToLower().Contains("exceptional"))
                        {
                            isExceptional = true;
                            break;
                        }
                    }

                    if (!isExceptional) { itemList.Add(item); }
                }
            }

            return itemList;
        }


        public static bool UseItemWithGump(int itemSerial, int waitForGumpMs, int maxRetry)
        {
            Items.UseItem(itemSerial);

            int time_counter = 0;
            int safe_counter = maxRetry;
            while (!Gumps.HasGump())
            {
                Common.Pause(1);
                if (time_counter++ > waitForGumpMs)
                {
                    // If wait is elapsed I try again to use the item
                    Items.UseItem(itemSerial);
                    time_counter = 0;

                    // Untill the counter has reached 0
                    if (safe_counter-- <= 0) return false;
                }
            }

            return true;
        }


    }
}
