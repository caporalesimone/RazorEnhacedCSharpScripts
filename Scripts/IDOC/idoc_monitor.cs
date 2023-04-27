//C#

//
// IDOC Monitor
//  This scripts target a house sign and monitor it's IDOC status.
//  When it changes sends an email
// 
// Developed by SimonSoft on Demise Server - 2021
//

using System;
using Scripts.Libs;
using RazorEnhanced;

//#import <../libs/send_mail.cs>

namespace Scripts
{
    public class IDOC_Monitor
    {
        public IDOC_Monitor()
        {
        }

        public void Run()
        {
            Misc.SendMessage("IDOC Monitor started!", 48);

            Email mail = new Email("IDOC Monitor", "IDOC Status update");

            Item signpost = Items.FindBySerial(new Target().PromptTarget("Target house sign to monitor", 48));
            if (signpost == null)
            {
                Misc.SendMessage("Target not valid");
                return;
            }

            int id = signpost.ItemID;
            if (id == 0x0B96 || id == 0x0C44 || (id >= 0x0BA4 && id <= 0x0C0E))
            {

            }
            else
            {
                Misc.SendMessage("Target not valid");
                return;
            }

            if (signpost.Properties.Count <= 4)
            {
                Misc.SendMessage("Expected > 4 properties", 33);
                return;
            }

            string house_status_old = signpost.Properties[4].ToString();
            string last_change = DateTime.Now.ToString();
            
            while (true)
            {
                while (Player.IsGhost)
                {
                    Misc.SendMessage("Sending email alert");
                    mail.SendEmail(Player.Name + " is dead", "IDOC Monitor - Dead player");
                    Misc.SendMessage("Email Sent");
                    return;
                }


                Item sign_updated = Items.FindBySerial(signpost.Serial);
                if (sign_updated == null)
                {
                    Misc.SendMessage("Sending email alert");
                    mail.SendEmail("House collapsed at " + DateTime.Now.ToString());
                    Misc.SendMessage("Email Sent");
                    return;
                }

                string house_status_updated = sign_updated.Properties[4].ToString();

                if (house_status_old != house_status_updated)
                {
                    last_change = DateTime.Now.ToString();

                    string message = "";
                    message += "IDOC House status changed<br>";
                    message += "from <i>" + house_status_old.Replace("Condition: ", "") + "</i><br>\n";
                    message += "to <b>" + house_status_updated.Replace("Condition: ", "") + "</b><br>\n";
                    message += last_change;

                    Misc.SendMessage("Sending email alert");
                    mail.SendEmail(message);
                    Misc.SendMessage("Email Sent");

                    house_status_old = house_status_updated;
                }

                Player.HeadMessage(33, "[ House Alert ]");
                Player.HeadMessage(34, "[ " + last_change + " ]");
                Player.HeadMessage(34, house_status_updated);
                
                for(int i = 0; i < 20; i++)
                {
                    if (Player.Visible == true)
                    {
                        Player.UseSkill("Hiding");
                    } 
                    Misc.Pause(500);
                }
            }
        }
    }
}
