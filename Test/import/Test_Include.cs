//C#
using Assistant;
using System;
using System.Collections.Generic;

//#import <TestFolder\Test_Include1.cs>

namespace RazorEnhanced
{
    public class TestInclude
    {
        public void Run()
        {
            TestInclude1.Included();
            Misc.SendMessage("Run");
        }
    }
}
