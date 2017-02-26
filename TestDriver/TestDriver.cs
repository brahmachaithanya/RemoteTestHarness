/////////////////////////////////////////////////////////////////////
// TestDriver.cs - defines testing process                         //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace TestHarness
{
 public  class TestDriver : ITest
  {
    public bool test()
    {
      TestHarness.Tested tested = new TestHarness.Tested();
      return tested.myWackyFunction();
    }
    public string getLog()
    {
      return "demo test that always passes";
    }
#if (TEST_TESTDRIVER)
    static void Main(string[] args)
    {
    }
#endif
  }
}
