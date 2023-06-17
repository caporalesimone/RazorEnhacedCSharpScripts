//
// Simple logger library. To be extended
// 
// Developed by SimonSoft on Demise Server - 2021
//

using System;
using System.Windows.Forms;

namespace RazorEnhanced
{
    class Logger
    {
        public enum COLORS
        {
            GREY = 0,
            BLACK = 1,
            BLUE = 2,
            LIGHT_BLUE = 5,
            VIOLET = 9,
            LIGHT_VIOLET = 10,
            PURPLE = 14,
            LIGHT_PURPLE = 15,
            PINK = 23,
            LIGHT_PINK = 24,
            RED = 33,
            LIGHT_RED = 34,
            ORANGE = 43,
            LIGHT_ORANGE = 44,
            YELLOW = 48,
            LIGHT_YELLOW = 49,
            GREEN = 63,
            LIGHT_GREEEN = 66,
            AZURE = 93,
            LIGHT_AZURE = 96,
        }

        public enum MESSAGEBOX_TYPE
        {
            ERROR = MessageBoxIcon.Error,
            WARNING = MessageBoxIcon.Warning,
            EXCLAMATION = MessageBoxIcon.Exclamation,
            INFORMATION = MessageBoxIcon.Information,
            QUESTION = MessageBoxIcon.Question,
        }

        public static void Log(object message, COLORS color = COLORS.GREY)
        {
            Misc.SendMessage(message, (int)color);
        }

        public static void LogHead(object message, COLORS color = COLORS.GREY)
        {
            Player.HeadMessage((int)color, (string)message);
        }

        public static void LogWithTime(object message, COLORS color = COLORS.GREY)
        {
            Misc.SendMessage(DateTime.Now.TimeOfDay + message.ToString(), (int)color);
        }

        public static void MessageBox(string message, MESSAGEBOX_TYPE type, bool blocking = true)
        {
            if(blocking)
            {                
                // Displays the MessageBox.
                System.Windows.Forms.MessageBox.Show(message, "Razor Script", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Threading.Tasks.Task.Run(() => 
                {
                    // Displays the MessageBox.
                    System.Windows.Forms.MessageBox.Show(message, "Razor Script", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
                
        }
    }
}
