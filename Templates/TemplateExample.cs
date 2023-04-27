//C#
using System;
using Assistant;
using System.Collections.Generic;
namespace RazorEnhanced
{
    public class TemplateExample
    {
        public void Run()
        {
            //System.Diagnostics.Debugger.Break();
            DateTime start;
            DateTime stop;
            
            Misc.SendMessage("Hello, my name is " + Player.Name);
            Items.UseItem(0x42854964);
            Player.HeadMessage(33,"Hello RE guys");
            Items.Filter filter = new Items.Filter();
            filter.Enabled = false;
            List<Item> itemList = Items.ApplyFilter(filter);
            
            /*
            foreach (Item itm in itemList) {
                Misc.SendMessage(itm.Name);
            }
            */
            
            start = DateTime.Now;
            int i = 0;
            while (i < 1000000) 
            {
            i++;
            }
            stop = DateTime.Now;
            double diff = (stop - start).TotalMilliseconds;
            Misc.SendMessage("Script 1M Loop time: " + diff.ToString() + " ms");

            /*            
            i = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                Misc.SendMessage(DateTime.Now.ToString("h:mm:ss tt"));
            }
            */
        }
    }
}