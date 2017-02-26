////////////////////////////////////////////////////////////////////////////
// Client2.cs - First Client with Console for the Remote TestHarness       //
//  ver 2.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Client console for CSE681 - Software Modeling & Analysis      //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Client package defines  one class, Client, that uses the Comm<Client>
 * class send and accept to messages from a remote endpoints.
 * Takes input from UI and forms test request message
 * uploads file to repository
 * Display test results
 * Retrieve logs from repository
 * Display log contents

 * Required Files:
 * ---------------
 * - Client.cs
 * - ICommunicator.cs, CommServices.cs
 * - Messages.cs, MessageTest, Serialization
 *
 * Maintenance History
 * --------------------
 * Ver 1.0 : 10 Nov 2016
 * - first release 
 * Ver 2.0 : 20 Nov 2016
 * - first release 
 *  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Utilities;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.IO;
using HRTimer;

namespace CommChannelDemo
{
  class Client
  {
    public Comm<Client> comm { get; set; } = new Comm<Client>();

    public string endPoint { get; } = Comm<Client>.makeEndPoint("http://localhost", 8081);

    private Thread rcvThread = null;
       




        //----< initialize receiver >------------------------------------

        public Client()
    {
        comm.rcvr.CreateRecvChannel(endPoint);
      rcvThread = comm.rcvr.start(rcvThreadProc);
    }
    //----< join receive thread >------------------------------------

    public void wait()
    {
      rcvThread.Join();
    }
    //----< construct a basic message >------------------------------

    public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
    {
      Message msg = new Message();
      msg.author = author;
      msg.from = fromEndPoint;
      msg.to = toEndPoint;
      return msg;
    }

      
        //----< use private service method to receive a message >--------

        void rcvThreadProc()
    {
      while (true)
      {
        Message msg = comm.rcvr.GetMessage();
        msg.time = DateTime.Now;
                if (msg.body == "quit")
                    break;
                if (msg.type == "testlog")
                {
                    Console.WriteLine("\n**********************************************************************");
                    Console.WriteLine("\nDemonstrating requirement 6 ,log from repository");

                    Console.Write("\n  {0} received log:", msg.body);
                    
                }
                else
                {
                    
                    Console.Write("\n  {0} received message:", comm.name);
                    Console.WriteLine("\n**********************************************************************");
                    Console.WriteLine("\nDemonstrating requirement 7 ,logs from repository");
                    msg.showMsg();
                }
                
        if (msg.body == "quit")
          break;
      }
    }
    //----< run client demo >----------------------------------------

    static void Main(string[] args)
    {
            Console.Title = "Client 1 Console";
            Console.Write("\n  Testing Client Demo");
      Console.Write("\n =====================\n");
      Client clnt = new Client();
            clnt.comm.sndr.channel = Sender.CreateServiceChannel("http://localhost:8082/StreamService");
            HiResTimer hrt = new HiResTimer();
            hrt.Start();
            clnt.comm.sndr.uploadFile("TestDriver.dll");
            clnt.comm.sndr.uploadFile("TestedCode.dll");
            hrt.Stop();
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 12 latency");
            Console.Write(
              "\n\n  total elapsed time for uploading = {0} microsec.\n",
              hrt.ElapsedMicroseconds
            );
            Message msg = clnt.makeMessage("Brahma", clnt.endPoint, clnt.endPoint);
            msg.type = "TestRequest";
            msg = clnt.makeMessage("Brahma", clnt.endPoint, clnt.endPoint);
            msg.body = MessageTest.makeTestRequest();
          Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\n Demonstrating requirement 2 : XML messages");
            msg.showMsg();
            string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8080);
            msg = msg.copy();
            msg.to = remoteEndPoint;
            clnt.comm.sndr.PostMessage(msg);
            string repository = Comm<Client>.makeEndPoint("http://localhost", 8082);
            msg = clnt.makeMessage("Brahma", clnt.endPoint, repository);
            msg.type = "testlogs";
            msg.body = "brahma";
            clnt.comm.sndr.PostMessage(msg);
            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
            msg.body = "quit";
            clnt.comm.sndr.PostMessage(msg);
            msg.showMsg();
            Console.Write("\n\n  Press key to terminate client");
            Console.ReadKey();
            Console.Write("\n\n");
            clnt.wait();
            ((IChannel)clnt.comm.sndr).Close();
            Console.Write("\n\n");
    }
  }
}
