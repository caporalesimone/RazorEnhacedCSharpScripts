//C#
using System;
using Assistant;
using System.Collections.Generic;
namespace RazorEnhanced
{
    public class EmptyTemplate
    {
        public EmptyTemplate()
        {
            Misc.SendMessage("Constructor");
        }

        public void Run()
        {
            Misc.SendMessage("Run");
        }
    }
}