////////////////////////////////////////////////////////////////////////////
// CommService.cs - Communicator Service                                   //
//  ver 2.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Communicator  for CSE681 - Software Modeling & Analysis  //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defindes a Sender class and Receiver class that
 * manage all of the details to set up a WCF channel.
 * helps to upload and downlaod files using file streaming
 * Required Files:
 * ---------------
 * CommService.cs, ICommunicator, BlockingQueue.cs, Messages.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 06 Nov 2016
 * - first release
 * Ver 2.0 : 20 Nov 2016
 * - first release 
 *  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using SWTools;
using HRTimer;
using System.IO;

namespace CommChannelDemo
{
  ///////////////////////////////////////////////////////////////////
  // Receiver hosts Communication service used by other Peers

  public class Receiver<T> : ICommunicator
  {
    static BlockingQueue<Message> rcvBlockingQ = null;
    ServiceHost service = null;

        public  string name { get; set; }
        //private Thread rcvThread = null;
        string filename;
         string savePath { get; set; } = "..\\..\\Storage";
         string ToSendPath { get; set; } = "..\\..\\Storage";
        int BlockSize = 1024;
        byte[] block;
        HiResTimer hrt = null;

        public Receiver()
    {
      if (rcvBlockingQ == null)
        rcvBlockingQ = new BlockingQueue<Message>();
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
        }

    public Thread start(ThreadStart rcvThreadProc)
    {
      Thread rcvThread = new Thread(rcvThreadProc);
      rcvThread.Start();
      return rcvThread;
    }

    public void Close()
    {
      service.Close();
    }

    //  Create ServiceHost for Communication service

    public void CreateRecvChannel(string address)
    {
      WSHttpBinding binding = new WSHttpBinding();
      Uri baseAddress = new Uri(address);
      service = new ServiceHost(typeof(Receiver<T>), baseAddress);
      service.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
      service.Open();
      Console.Write("\n  Service is open listening on {0}", address);
    }

    // Implement service method to receive messages from other Peers

    public void PostMessage(Message msg)
    {
      //Console.Write("\n  service enQing message: \"{0}\"", msg.body);
      rcvBlockingQ.enQ(msg);
    }

    // Implement service method to extract messages from other Peers.
    // This will often block on empty queue, so user should provide
    // read thread.

    public Message GetMessage()
    {
      Message msg = rcvBlockingQ.deQ();
      //Console.Write("\n  {0} dequeuing message from {1}", name, msg.from);
      return msg;
    }
        public void upLoadFile(FileTransferMessage msg)
        {
            int totalBytes = 0;
            hrt.Start();
            filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt.ElapsedMicroseconds
            );
        }

        public Stream downLoadFile(string filename)
        {
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                throw new Exception("open failed for \"" + filename + "\"");
            hrt.Stop();
            Console.Write("\n  Sent \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
        }

      public  static ServiceHost CreateServiceChannel(string url)
        {
            // Can't configure SecurityMode other than none with streaming.
            // This is the default for BasicHttpBinding.
            //   BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            //   BasicHttpBinding binding = new BasicHttpBinding(securityMode);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(CommChannelDemo.Receiver<T>);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
            return host;
        }


    }
    ///////////////////////////////////////////////////////////////////
    // Sender is client of another Peer's Communication service

    public class Sender
  {
    public string name { get; set; }        
         string ToSendPath = "..\\..\\ToSend";
        string SavePath = "..\\..\\SavedFiles";
        int BlockSize = 1024;
        byte[] block;
        HRTimer.HiResTimer hrt = null;
       public ICommunicator channel;
    string lastError = "";
    BlockingQueue<Message> sndBlockingQ = null;
    Thread sndThrd = null;
    int tryCount = 0, MaxCount = 10;
    string currEndpoint = "";

        //----< processing for send thread >-----------------------------
        
    void ThreadProc()
    {
      tryCount = 0;
      while (true)
      {
        Message msg = sndBlockingQ.deQ();
        if(msg.to != currEndpoint)
        {
          currEndpoint = msg.to;
          CreateSendChannel(currEndpoint);
        }
        while (true)
        {
          try
          {
            channel.PostMessage(msg);
            Console.Write("\n  posted message from {0} to {1}", name, msg.to);
            tryCount = 0;
            break;
          }
          catch (Exception ex)
          {
            Console.Write("\n  connection failed",ex);
            if (++tryCount < MaxCount)
              Thread.Sleep(100);
            else
            {
              Console.Write("\n  {0}", "can't connect\n");
              currEndpoint = "";
              tryCount = 0;
              break;
            }
          }
        }
        if (msg.body == "quit")
          break;
      }
    }
      public  void changepath(string path)
        {
            this.SavePath = path;
        }
    //----< initialize Sender >--------------------------------------

    public Sender()
    {
      sndBlockingQ = new BlockingQueue<Message>();
      sndThrd = new Thread(ThreadProc);
      sndThrd.IsBackground = true;
      sndThrd.Start();
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
        }

    //----< Create proxy to another Peer's Communicator >------------

    public void CreateSendChannel(string address)
    {
      EndpointAddress baseAddress = new EndpointAddress(address);
      WSHttpBinding binding = new WSHttpBinding();
      ChannelFactory<ICommunicator> factory
        = new ChannelFactory<ICommunicator>(binding, address);
      channel = factory.CreateChannel();
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 10 ");
            Console.Write("\n  service proxy created for {0}", address);
    }

    //----< posts message to another Peer's queue >------------------
    /*
     *  This is a non-service method that passes message to
     *  send thread for posting to service.
     */
    public void PostMessage(Message msg)
    {
      sndBlockingQ.enQ(msg);
    }

    public string GetLastError()
    {
      string temp = lastError;
      lastError = "";
      return temp;
    }

    //----< closes the send channel >--------------------------------

    public void Close()
    {
      ChannelFactory<ICommunicator> temp = (ChannelFactory<ICommunicator>)channel;
      temp.Close();
    }

     public  static  ICommunicator CreateServiceChannel(string url)
        {
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;

            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);

            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            return factory.CreateChannel();
        }

      public  void uploadFile(string filename)
        {
            string fqname = Path.Combine(ToSendPath, filename);
            try
            {
                hrt.Start();
                using (var inputStream = new FileStream(fqname, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    channel.upLoadFile(msg);
                }
                hrt.Stop();
                Console.WriteLine("\n**********************************************************************");
                Console.WriteLine("\nDemonstrating requirement 2 :");

                Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            }
            catch(Exception message)
            {
                Console.Write("\n  can't find \"{0}\"", message);
            }
        }

      public  void download(string filename)
        {
            int totalBytes = 0;
            try
            {
                hrt.Start();
                Stream strm = channel.downLoadFile(filename);
                string rfilename = Path.Combine(SavePath, filename);
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                hrt.Stop();
                ulong time = hrt.ElapsedMicroseconds;
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
                Console.WriteLine("\n**********************************************************************");
                Console.WriteLine("\nDemonstrating requirement 6 ");
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
        }


    }
    ///////////////////////////////////////////////////////////////////
    // Comm class simply aggregates a Sender and a Receiver
    //
    public class Comm<T>
  {
    public string name { get; set; } = typeof(T).Name;

    public Receiver<T> rcvr { get; set; } = new Receiver<T>();

    public Sender sndr { get; set; } = new Sender();

    public Comm()
    {
      rcvr.name = name;
      sndr.name = name;
    }
    public  static string makeEndPoint(string url, int port)
    {
      string endPoint = url + ":" + port.ToString() + "/ICommunicator";
      return endPoint;
    }
    //----< this thrdProc() used only for testing, below >-----------

    public void thrdProc()
    {
      while (true)
      {
        Message msg = rcvr.GetMessage();
        msg.showMsg();
        if (msg.body == "quit")
        {
          break;
        }
      }
    }
  }
#if(TEST_COMMSERVICE)

  class Cat { }
  class TestComm
  {
    [STAThread]
    static void Main(string[] args)
    {
      Comm<Cat> comm = new Comm<Cat>();
      string endPoint = Comm<Cat>.makeEndPoint("http://localhost", 8080);
      comm.rcvr.CreateRecvChannel(endPoint);
      comm.rcvr.start(comm.thrdProc);
      comm.sndr = new Sender();
      comm.sndr.CreateSendChannel(endPoint);
      Message msg1 = new Message();
      msg1.body = "Message #1";
      comm.sndr.PostMessage(msg1);
      Message msg2 = new Message();
      msg2.body = "quit";
      comm.sndr.PostMessage(msg2);
      Console.Write("\n  Comm Service Running:");
      Console.Write("\n  Press key to quit");
      Console.ReadKey();
      Console.Write("\n\n");
    }
#endif
  }
}
