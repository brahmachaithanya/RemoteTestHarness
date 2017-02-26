/////////////////////////////////////////////////////////////////////
// Server.cs - Demonstrate application use of channel                      //
//  ver 2.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  TestHarness for CSE681 - Software Modeling & Analysis    //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The TestHarness Server package defines one class, Server, that uses the Comm<Server>
 * class to receive messages from a remote endpoint.
 * Receives test requests from multiple clients
 * Runs tests in child threads
 * contact repository to donload the files
 * stores logs to repository
 * sends results back to clients

 * Required Files:
 * ---------------
 * - Server.cs
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
using TestHarness;
using System.Reflection;
using System.Security.Policy;    // defines evidence needed for AppDomain construction
using System.Runtime.Remoting;   // provides remote communication between AppDomains
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel;
using HRTimer;


namespace CommChannelDemo
{
  class Server
  {
    public Comm<Server> comm { get; set; } = new Comm<Server>();

    public string endPoint { get; } = Comm<Server>.makeEndPoint("http://localhost", 8080);

    private Thread rcvThread = null;
        public SWTools.BlockingQueue<Message> inQ_ { get; set; } = new SWTools.BlockingQueue<Message>();
        private ICallback cb_;
        private IClient client_;
        private string repoPath_ = "../../ToSend";
        private string filePath_;
        object sync_ = new object();
        List<Thread> threads_ = new List<Thread>();
        Dictionary<int, string> TLS = new Dictionary<int, string>();

        public Server()
    {

      comm.rcvr.CreateRecvChannel(endPoint);
      rcvThread = comm.rcvr.start(processMessages);
            Console.Write("\n  creating instance of TestHarness");
            
            repoPath_ = System.IO.Path.GetFullPath(repoPath_);
            cb_ = new Callback();
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
      return msg;
    }

        public void Child_thread_wait()
        {
            foreach (Thread t in threads_)
                t.Join();
        }
        void rcvThreadProc()
    {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;

                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();

                if (msg.body == "quit")
                {
                   
                    return;
                }
                else
                {
                   
                }

            }
        }


      public class Callback : MarshalByRefObject, ICallback
        {
            public void sendMessage(Message message)
            {
                Console.Write("\n  received msg from childDomain: \"" + message.body + "\"");
            }
        }
        ///////////////////////////////////////////////////////////////////
        // Test and RequestInfo are used to pass test request information
        // to child AppDomain
        //
        [Serializable]
        class Test : ITestInfo
        {
            public string testName { get; set; }
            public List<string> files { get; set; } = new List<string>();
        }
        [Serializable]
        class RequestInfo : IRequestInfo
        {
            public string tempDirName { get; set; }
            public List<ITestInfo> requestInfo { get; set; } = new List<ITestInfo>();
        }
        ///////////////////////////////////////////////////////////////////
        // class TestHarness

        
            
            public void setClient(IClient client)
            {
                client_ = client;
            }
            //----< called by clients >--------------------------------------

            public void sendTestRequest(Message testRequest)
            {
                Console.Write("\n  TestHarness received a testRequest - Req #2");
                inQ_.enQ(testRequest);
            }
            //----< not used for Project #2 >--------------------------------

            public Message sendMessage(Message msg)
            {
                return msg;
            }
            //----< make path name from author and time >--------------------

            string makeKey(string author)
            {
                DateTime now = DateTime.Now;
                string nowDateStr = now.Date.ToString("d");
                string[] dateParts = nowDateStr.Split('/');
                string key = "";
                foreach (string part in dateParts)
                    key += part.Trim() + '_';
                string nowTimeStr = now.TimeOfDay.ToString();
                string[] timeParts = nowTimeStr.Split(':');
                for (int i = 0; i < timeParts.Count() - 1; ++i)
                    key += timeParts[i].Trim() + '_';
                key += timeParts[timeParts.Count() - 1];
                key = author + "_" + key + "_" + "ThreadID" + Thread.CurrentThread.ManagedThreadId;
                return key;
            }
            //----< retrieve test information from testRequest >-------------

            List<ITestInfo> extractTests(Message testRequest)
            {
                Console.Write("\n  parsing test request");
                List<ITestInfo> tests = new List<ITestInfo>();
                XDocument doc = XDocument.Parse(testRequest.body);
                foreach (XElement testElem in doc.Descendants("TestElement"))
                {
                    Test test = new Test();
                    string testDriverName = testElem.Element("testDriver").Value;
                    test.testName = testElem.Element("testName").Value;
                    test.files.Add(testDriverName);
                    foreach (XElement lib in testElem.Elements("testCodes"))
                    {
                        test.files.Add(lib.Value);
                    }
                    tests.Add(test);
                }
                return tests;
            }
            //----< retrieve test code from testRequest >--------------------

            List<string> extractCode(List<ITestInfo> testInfos)
            {
                Console.Write("\n  retrieving code files from testInfo data structure");
                List<string> codes = new List<string>();
                foreach (ITestInfo testInfo in testInfos)
                    codes.AddRange(testInfo.files);
                return codes;
            }
            //----< create local directory and load from Repository >--------

            RequestInfo processRequestAndLoadFiles(Message testRequest)
            {    HiResTimer hrt = new HiResTimer();
            string localDir_ = "";
                RequestInfo rqi = new RequestInfo();
                rqi.requestInfo = extractTests(testRequest);
                List<string> files = extractCode(rqi.requestInfo);
                localDir_ = makeKey(testRequest.author);
            Console.WriteLine(" \n Key :", localDir_);
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 8 ,unique key");
            // name of temporary dir to hold test files
            rqi.tempDirName = localDir_;
                lock (sync_)                {
                    filePath_ = System.IO.Path.GetFullPath(localDir_);  // LoadAndTest will use this path
                    TLS[Thread.CurrentThread.ManagedThreadId] = filePath_;
                }                Console.Write("\n  creating local test directory \"" + localDir_ + "\"");
                System.IO.Directory.CreateDirectory(localDir_);       //for file streaming to download files
            this.comm.sndr.channel = Sender.CreateServiceChannel("http://localhost:8082/StreamService");
              Console.Write("\n  loading code from Repository");
                foreach (string file in files)                {                hrt.Start();
                try                {
                    string name = System.IO.Path.GetFileName(file);
                    this.comm.sndr.changepath( System.IO.Path.GetFullPath(localDir_));
                    this.comm.sndr.download(file);                }
                catch (Exception message)                       {
                    Console.Write("Exception of file", message);
                    Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": could not retrieve file \"" + file + "\"");
                }            hrt.Stop();
                Console.Write(
                  "\n\n  Total elapsed time for downloading = {0}",
                  hrt.ElapsedMicroseconds );
                        Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": retrieved file \"" + file + "\"");
                 }                Console.WriteLine();
                return rqi;            }
            //----< save results and logs in Repository >--------------------
                
            bool saveResultsAndLogs(ITestResults testResults)
            {
                string logName = testResults.testKey + ".txt";
                System.IO.StreamWriter sr = null;
                try
                {
                    sr = new System.IO.StreamWriter(System.IO.Path.Combine(repoPath_, logName));
                    sr.WriteLine(logName);
                    foreach (ITestResult test in testResults.testResults)
                    {
                        sr.WriteLine("-----------------------------");
                        sr.WriteLine(test.testName);
                        sr.WriteLine(test.testResult);
                        sr.WriteLine(test.testLog);
                    }
                    sr.WriteLine("-----------------------------");
                }
                catch(Exception message)
                {
                Console.WriteLine("Error message", message);
                    sr.Close();
                    return false;
                }

                sr.Close();
            
            this.comm.sndr.channel = Sender.CreateServiceChannel("http://localhost:8082/StreamService");
            this.comm.sndr.uploadFile(logName); //sending log to repository
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 7 ,saving logs to Repository");

            return true;
            }
            //----< run tests >----------------------------------------------
            /*
             * In Project #4 this function becomes the thread proc for
             * each child AppDomain thread.
             */
            ITestResults runTests(Message testRequest)
            {
                AppDomain ad = null;
                ILoadAndTest ldandtst = null;
                RequestInfo rqi = null;
                ITestResults tr = null;

                try
                {
                    //lock (sync_)
                    {
                        rqi = processRequestAndLoadFiles(testRequest);
                        ad = createChildAppDomain();
                        ldandtst = installLoader(ad);
                    }
                    if (ldandtst != null)
                    {
                        tr = ldandtst.test(rqi);
                    }
                    // unloading ChildDomain, and so unloading the library

                    saveResultsAndLogs(tr);

                Console.WriteLine("\n**********************************************************************");
                Console.WriteLine("\nDemonstrating requirement 5");

                lock (sync_)
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": unloading: \"" + ad.FriendlyName + "\"\n");
                        AppDomain.Unload(ad);
                        try
                        {
                            System.IO.Directory.Delete(rqi.tempDirName, true);
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + rqi.tempDirName);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + rqi.tempDirName);
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                        }
                    }
                    return tr;
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n---- {0}\n\n", ex.Message);
                    return tr;
                }
            }
            //----< make TestResults Message >-------------------------------

            Message makeTestResultsMessage(ITestResults tr,string from ,string to ,string author)
            {
            
            Message trMsg = new Message();
                trMsg.author = author;
                trMsg.to = from;
                trMsg.from = to;
            trMsg.type = "TestResult";
                XDocument doc = new XDocument();
                XElement root = new XElement("testResultsMsg");
                doc.Add(root);
                XElement testKey = new XElement("testKey");
                testKey.Value = tr.testKey;
                root.Add(testKey);
                XElement timeStamp = new XElement("timeStamp");
                timeStamp.Value = tr.dateTime.ToString();
                root.Add(timeStamp);
                XElement testResults = new XElement("testResults");
                root.Add(testResults);
                foreach (ITestResult test in tr.testResults)
                {
                    XElement testResult = new XElement("testResult");
                    testResults.Add(testResult);
                    XElement testName = new XElement("testName");
                    testName.Value = test.testName;
                    testResult.Add(testName);
                    XElement result = new XElement("result");
                    result.Value = test.testResult;
                    testResult.Add(result);
                    XElement log = new XElement("log");
                    log.Value = test.testLog;
                    testResult.Add(log);
                }
                trMsg.body = doc.ToString();
                return trMsg;
            }
            //----< wait for all threads to finish >-------------------------

            
            //----< main activity of TestHarness >---------------------------

            public void processMessages()
            {
                AppDomain main = AppDomain.CurrentDomain;
                Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");

                ThreadStart doTests = () => {
                    while (true)
                    {
                        Message msg = comm.rcvr.GetMessage();
                        msg.time = DateTime.Now;

                        Console.Write("\n  {0} received message:", comm.name);
                        msg.showMsg();
                        Message testRequest =msg ;
                        
                        if (testRequest.body == "quit")
                        {
                            inQ_.enQ(testRequest);
                            break;
                        }
                        ITestResults testResults = runTests(testRequest);
                        lock (sync_)
                        {
                            // client_.sendResults(makeTestResultsMessage(testResults));
              Console.WriteLine("test result{0}\n",makeTestResultsMessage(testResults, testRequest.from, testRequest.to, testRequest.author));
                            comm.sndr.PostMessage(makeTestResultsMessage(testResults, testRequest.from, testRequest.to, testRequest.author));
                        }
                    }
                };

                int numThreads = 8;
            Console.WriteLine("\n**********************************************************************");
            Console.WriteLine("\nDemonstrating requirement 4 creating child threads");

            for (int i = 0; i < numThreads; ++i)
                {
                    Console.Write("\n  Creating AppDomain thread");
                    Thread t = new Thread(doTests);
                    threads_.Add(t);
                    t.Start();
                }
            }
            //----< was used for debugging >---------------------------------

            void showAssemblies(AppDomain ad)
            {
                Assembly[] arrayOfAssems = ad.GetAssemblies();
                foreach (Assembly assem in arrayOfAssems)
                    Console.Write("\n  " + assem.ToString());
            }
            //----< create child AppDomain >---------------------------------

            public AppDomain createChildAppDomain()
            {
                try
                {
                    Console.Write("\n  creating child AppDomain - Req #5");

                    AppDomainSetup domaininfo = new AppDomainSetup();
                    domaininfo.ApplicationBase
                      = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library

                    //Create evidence for the new AppDomain from evidence of current

                    Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                    // Create Child AppDomain

                    AppDomain ad
                      = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

                    Console.Write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
                    return ad;
                }
                catch (Exception except)
                {
                    Console.Write("\n  " + except.Message + "\n\n");
                }
                return null;
            }
            //----< Load and Test is responsible for testing >---------------

            ILoadAndTest installLoader(AppDomain ad)
            {
                ad.Load("LoadAndTest");
                //showAssemblies(ad);
                //Console.WriteLine();

                // create proxy for LoadAndTest object in child AppDomain

                ObjectHandle oh
                  = ad.CreateInstance("LoadAndTest", "TestHarness.LoadAndTest");
                object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain
                                            // Console.Write("\n  {0}", ob);

                // set reference to LoadAndTest object in child

                ILoadAndTest landt = (ILoadAndTest)ob;

                // create Callback object in parent domain and pass reference
                // to LoadAndTest object in child

                landt.setCallback(cb_);
                lock (sync_)
                {
                    filePath_ = TLS[Thread.CurrentThread.ManagedThreadId];
                    landt.loadPath(filePath_);  // send file path to LoadAndTest
                }
                return landt;
            }
#if (TEST_TESTHARNESS)
    static void Main(string[] args)
    {
    }
#endif
        






        static void Main(string[] args)
    {
            Console.Title = "TestHarness";
            Console.Write("\n  TestHarness Demo");
      Console.Write("\n =====================\n");

      Server Server = new Server();
      
            Message msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);

                  Console.Write("\n  press key to exit: ");
      Console.ReadKey();
            msg.to = Server.endPoint;
            msg.body = "quit";
            Server.comm.sndr.PostMessage(msg);
           
            Server.wait();
            Server.Child_thread_wait();
            ((IChannel)Server.comm.sndr).Close();
            Console.Write("\n\n");
    }
  }
}
