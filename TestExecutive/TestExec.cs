/////////////////////////////////////////////////////////////////////
// TestExec.cs - Demonstrate TestHarness, Client, and Repository   //
// ver 1.1                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestExec package orchestrates TestHarness, Client, and Repository
 * operations to show that all requirements for Project #2 have been
 * satisfied. 
 *
 * Required files:
 * ---------------
 * - TestExec.cs
 * - ITest.cs
 * - Client.cs, Repository.cs, TestHarness.cs
 * - LoadAndTest, Logger, Messages
 * 
 * Maintanence History:
 * --------------------
 * ver 1.1 : 13 Nov 2016
 * - removed logger statements
 *   Lot's of logging messages were confusing in multi-threaded environment
 * - now posts quit message
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
  class TestExec
  {
    public TestHarness testHarness { get; set; }
    public Client client { get; set; }
    public Repository repository { get; set; }
    TestExec()
    {
      Console.Write("\n  creating Test Executive - Req #9");
      testHarness = new TestHarness(repository);
      client = new Client(testHarness as ITestHarness);
      repository = new Repository();
      testHarness.setClient(client);
      client.setRepository(repository);
    }
    void sendTestRequest(Message testRequest)
    {
      client.sendTestRequest(testRequest);
    }
    Message buildTestMessage()
    {
      Message msg = new Message();
      msg.to = "TH";
      msg.from = "CL";
      msg.author = "Fawcett";

      testElement te1 = new testElement("test1");
      te1.addDriver("testdriver.dll");
      te1.addCode("testedcode.dll");
      testElement te2 = new testElement("test2");
      te2.addDriver("td1.dll");
      te2.addCode("tc1.dll");
      testElement te3 = new testElement("test3");
      te3.addDriver("anothertestdriver.dll");
      te3.addCode("anothertestedcode.dll");
      testElement tlg = new testElement("loggerTest");
      //tlg.addDriver("logger.dll");
      testRequest tr = new testRequest();
      tr.author = "Jim Fawcett";
      tr.tests.Add(te1);
      tr.tests.Add(te2);
      tr.tests.Add(te3);
      tr.tests.Add(tlg);
      msg.body = tr.ToString();
      return msg;
    }
    static void Main(string[] args)
    {
      try
      {
        Console.Write("\n  Demonstrating TestHarness - Project #2 with Threading");
        Console.Write("\n =======================================================");

        TestExec te = new TestExec();
        Message msg = te.buildTestMessage();
        te.sendTestRequest(msg);
        te.sendTestRequest(msg);
        msg = msg.copy();
        msg.body = "quit";
        te.sendTestRequest(msg);
        te.testHarness.processMessages();
        te.testHarness.wait();
        te.client.makeQuery("test1");
        Console.Write("\n\n");
      }
      catch (Exception ex)
      {
        Console.Write("\n\n  {0}\n\n", ex.Message);
      }
    }
  }
}
