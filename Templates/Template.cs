//C#
using Assistant;
using System;
using System.Collections.Generic;

namespace RazorEnhanced
{
    // The class can have any valid class name
    public class Script
    {
        public Script()
        {
            Misc.SendMessage("Constructor");
        }

        // This method is the entrypoint and is mandatory
        public void Run()
        {
            //System.Diagnostics.Debugger.Break();
            Misc.SendMessage("Run");
            Misc.SendMessage(".NET version: " + Environment.Version.ToString());
        }
    }
}
