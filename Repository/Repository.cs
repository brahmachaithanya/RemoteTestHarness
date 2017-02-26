/////////////////////////////////////////////////////////////////////
// Repository.cs - Remote Repository for the TestHarness                   //
//  ver 2.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Repository for CSE681 - Software Modeling & Analysis    //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Repository package defines  one class, Repository, that uses the Comm<repository>
 * class to messages from a remote endpoint.
 * Receives files uploaded from clients before test
 * Sends required files to Testharness
 * Stores log files
 * Accepts request from clients to retrieve logs
 * sends logs back to clients

 * Required Files:
 * ---------------
 * - Repository.cs
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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CommChannelDemo
{

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    class Repository 
    {
        public Comm<Repository> comm { get; set; } = new Comm<Repository>();

        public string endPoint { get; } = Comm<Repository>.makeEndPoint("http://localhost", 8082);

        private Thread rcvThread = null;


        public Repository()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
            //start the lisner

        }
       
        public void wait()
        {
            rcvThread.Join();
        }
        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg; //message with enpoints
        }

        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                
                if (msg.body == "quit")
                    break;
                if (msg.type == "testlogs")
                {
                    
                    Console.WriteLine("\n**********************************************************************");
                    Console.WriteLine("\nDemonstrating requirement 9 ,request for logs at repository");
                    msg.showMsg();
                    sendlogs(msg.from, msg.author); //for log requests from clients
                }
                else
                {
                    Console.Write("\n  {0} received message:", comm.name);
                    msg.showMsg();
                }
            }
        }
        void sendlogs(string from, string author)
        {

            try
            {
                //retrieving log file
                var directory = new DirectoryInfo("../../../Repository/Storage/");
                FileInfo myFile = (from f in directory.GetFiles("*.txt*")
                                   orderby f.LastWriteTime descending
                                   select f).First();
                 string filePath = System.IO.Path.GetFullPath(directory + myFile.ToString());
                string log = File.ReadAllText(filePath);
                Message msg = new Message();
                msg.to = from;
                msg.type = "log";
                msg.from = endPoint;
                msg.body = log;
                comm.sndr.PostMessage(msg);
            }
            catch (Exception ex)
            {
                Console.Write("exception", ex);
            }
           }
        static void Main(string[] args)
        {
            Console.Title = "Repository";
            Console.Write("\n  Repository Demo");
            Console.Write("\n =====================\n");

            Repository Repository = new Repository();
           //reciever for file transfer 
            ServiceHost host = Receiver<Repository>.CreateServiceChannel("http://localhost:8082/StreamService");
            
            host.Open();
            Repository.wait();
            
            Console.Write("\n\n");
        }
    }
}

