////////////////////////////////////////////////////////////////////////////
// Client2.cs - Second Client with UI for the Remote TestHarness           //
//  ver 2.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Client UI for CSE681 - Software Modeling & Analysis      //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Client2 package defines  one class, Client2, that uses the Comm<Client2>
 * class send and accept to messages from a remote endpoints.
 * Takes input from UI and forms test request message
 * uploads file to repository
 * Display test results
 * Retrieve logs from repository
 * Display log contents

 * Required Files:
 * ---------------
 * - Client2.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace CommChannelDemo
{
    public class Client2
    {
        public Comm<Client2> comm { get; set; } = new Comm<Client2>();

        public string endPoint { get; } = Comm<Client2>.makeEndPoint("http://localhost", 8084);

        private Thread rcvThread = null;
        private string recvmessage="TestResult";
        private string recvlog = "Retrieving logs from the repository ,please press GetLogs again";

        public Client2()
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
        public string getmessage()
        {
            return recvmessage;
        }
        public string getlog()
        {
            return recvlog;
        }
        //----< use private service method to receive a message >--------

        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                
                if (msg.body == "quit")
                    break;
                
                if (msg.type.ToString() == "log")
                {
                    recvlog = msg.body;
                }
                else
                {
                    recvmessage = msg.ToString();
                }
            }
        }
    }
}
