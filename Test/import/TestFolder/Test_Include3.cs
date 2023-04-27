//C#
using Assistant;
using System;
using System.Collections.Generic;

// Recursive imports that must be ignored

//#import <Test_Include3.cs>
//#import <../TestFolder/Test_Include3.cs>
//#import <../TestFolder/Test_Include2.cs>
//#import <Test_Include2.cs>

namespace RazorEnhanced
{
    // The class can have any valid class name
    public class TestInclude3
    {
        public static void Included()
        {
            Misc.SendMessage("TestInclude3:Included");
        }
    }
}
