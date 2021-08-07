//C#
using Assistant;
using System;
using System.Collections.Generic;

//#import <Test_Include3.cs>

namespace RazorEnhanced
{
    // The class can have any valid class name
    public class TestInclude2
    {
        public static void Included()
        {
            Misc.SendMessage("TestInclude2:Included");
            TestInclude3.Included();
        }
    }
}
