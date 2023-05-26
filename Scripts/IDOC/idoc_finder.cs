//C#

//
// IDOC Finder
// This scripts scan houses around plater to see if one is in IDOC
// 
// Developed by SimonSoft on Demise Server - 2021
//

//-#forcedebug 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace RazorEnhanced
{
    public class IDOC_Finder
    {
        public IDOC_Finder()
        {

        }

        public void Run()
        {
            Misc.SendMessage("IDOC finder started!", 48);

            // Elenco di tutti i tipi di cartelli di casa. += 2 perchè interessa solo un orientamento
            List<int> signpostList = new List<int>();
            for (int i = 0x0BA4; i <= 0x0C0E; i += 2)
            {
                signpostList.Add(i);
            }
            signpostList.Add(0x0B96); // library 
            signpostList.Add(0x0C44); // Beekeeper

            Items.Filter signFilter = new Items.Filter
            {
                Graphics = signpostList,
                Enabled = true
            };

            // Seriale, Numero di volte che è comparso
            Dictionary<int, int> knownSerials = new Dictionary<int, int>();

            while (true)
            {
                List<Item> signs = Items.ApplyFilter(signFilter);
                if (signs != null || signs.Count > 0)
                {
                    foreach (Item sign in signs)
                    {
                        if (!knownSerials.ContainsKey(sign.Serial))
                        {
                            knownSerials[sign.Serial] = 1;
                        }

                        // Ripete n volte e poi smette di segnalare
                        if (knownSerials[sign.Serial] <= 10)
                        {
                            if (sign.Properties.Count < 4) { continue; }
                            string owner = sign.Properties[2].ToString().Replace("Owner:", "").Trim();
                            string houseType = sign.Properties[3].ToString().ToLower();

                            Items.Message(sign, 0, owner);
                            if (houseType == "this house is open to the public") Items.Message(sign, 0, "Public");

                            // Se ha più di 4 properties allora dovrebbe essere un IDOC (In Danger Of Collapse)
                            if (sign.Properties.Count > 4)
                            {
                                string house_status_old = sign.Properties[4].ToString();
                                string last_change = DateTime.Now.ToString();
                                while (true)
                                {
                                    Item sign_updated = Items.FindBySerial(sign.Serial);
                                    if (sign_updated == null)
                                    {
                                        break;
                                    }
                                    string house_status_updated = sign_updated.Properties[4].ToString();

                                    if (house_status_old != house_status_updated)
                                    {
                                        house_status_old = house_status_updated;
                                        last_change = DateTime.Now.ToString();
                                    }

                                    Items.Message(sign_updated, 33, ">>> SIGN <<<");
                                    Player.HeadMessage(33, "[ House Alert ]");
                                    Player.HeadMessage(34, "[ " + last_change + " ]");
                                    Player.HeadMessage(34, house_status_updated);
                                    Misc.Pause(1000);
                                }
                            }
                            knownSerials[sign.Serial] += 1;
                        }
                    }
                }
                Misc.Pause(1000);
            }
        }

    }
}
