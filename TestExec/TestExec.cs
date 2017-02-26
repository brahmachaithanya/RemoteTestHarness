////////////////////////////////////////////////////////////////////////////
//  testExec.cs -To demonstarte all the functionalities of Project 4      //
//  ver 4.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:RemoteTestHarness for CSE681 Software Modeling & Analysis    //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
/////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *  To demonstarte all the requirements of teh project

 * 

 * 
 *   Public Interface
 *   ----------------
 *   TestExec test=new TestExec();
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Client.cs,TestExec.cs,Server.cs
 *   - Compiler command: csc Client.cs,TestExec.cs,Server.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : Novermber 20 2016
 *     - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExec
{
    class TestExec
    {
        static void Main(string[] args)
        {
            Console.Title = "TestExec";
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 1 :");
            Console.WriteLine("\n TestHarness is implemented  in C# using the facilities of the .Net Framework Class Library and Visual Studio 2015");
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 11 ,WPF is implemented in client2 package");
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 13 TestExec");
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 14 ");
            Console.WriteLine("\n Difference between the OCD and the implementation is discussed in file OCD_Differences.doc ");


        }
    }
}
