//C#
using Assistant;
using System;
using System.Collections.Generic;

//#import <Test_Include2.cs>
namespace RazorEnhanced
{
    public class TestInclude1
    {
        public static void Included()
        {
            Misc.SendMessage("TestInclude1:Included");
            TestInclude2.Included();
        }
    }
}
