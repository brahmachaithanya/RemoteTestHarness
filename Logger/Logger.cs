/////////////////////////////////////////////////////////////////////
// Logger.cs - logs test information                               //
// ver 1.1                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Logger provides facilities for logging strings to an arbitrary 
 * number of streams, simultaneously.
 * 
 * The Package provides classes:
 * - Logger that provides all of the core logger functionality
 * - StaticLogger<LogType> to support sharing of streams across
 *   all instances of the class.
 * - classes RLog, DLog, and DbLog provide categories of sharing
 *   for program results, demonstration output, and debugging
 *   output.
 * 
 * Required Files:
 * ---------------
 * - Logger.cs, BlockingQueue.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 13 Nov 2016
 * - locked creation of streams
 * ver 1.0 : 20 Oct 2016
 * - first release
 * 
 * Planned Changes:
 * ----------------
 * - Logger has problems in multi-threaded environment.
 *   Need to look carefully at MT operations.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace TestHarness
{
  ///////////////////////////////////////////////////////////////////
  // Logger class provides queued logging
  // - uses SWTools.BlockingQueue<string> to provide fast writes
  // - writes to one or more attached streams are handled by a 
  //   child thread
  // - can be started, stopped, and paused
  //
  public class Logger
  {
    private SWTools.BlockingQueue<string> Q_ = new SWTools.BlockingQueue<string>();
    private List<StreamWriter> streams_ = new List<StreamWriter>();
    private bool threadRunning_ = false;
    private Thread thrd_ = null;
    private bool paused_ = false;
    private bool showTimeStamp_ = false;
    private object sync_ = new object();

    //----< add a destination stream >-------------------------------

    public void attach(StreamWriter stream)
    {
      lock (sync_)
      {
        streams_.Add(stream);
      }
    }
    //----< factory for Console Streams >----------------------------

    public StreamWriter makeConsoleStream()
    {
      lock (sync_)
      {
        StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
        sw.AutoFlush = true;
        return sw;
      }
    }
    //----< factory for File Streams >-------------------------------

    public StreamWriter makeFileStream(string file)
    {
      lock (sync_)
      {
        try
        {
          StreamWriter sw = new StreamWriter(file);
          sw.AutoFlush = true;
          return sw;
        }
        catch
        {
          return null;
        }
      }
    }
    //----< enqueue string for writing >-----------------------------

    public void write(string msg)
    {
      if (threadRunning_)
        Q_.enQ(msg);

    }
    //----< process all strings in queue before returning >----------

    public void flush()
    {
      //Console.Write("\n  in flush() queue size = {0}", Q_.size());
      while (Q_.size() > 0)
        ;
      foreach (StreamWriter stream in streams_)
        stream.Flush();
      //Console.Write("\n  in flush() queue size = {0}", Q_.size());
    }
    //----< child thread processing for writing to streams >---------

    public void threadProc()
    {
      while(true)
      {
        string msg = Q_.deQ();
        if (showTimeStamp_)
          msg += " - " + DateTime.Now.ToString();
        if (msg == "quit")
        {
          threadRunning_ = false;
          break;
        }
        foreach (StreamWriter stream in streams_)
        {
          if(stream != null)
            stream.Write(msg);
        }
        if (paused_)
        {
          lock (sync_)
          {
            Monitor.Wait(sync_);
            foreach (StreamWriter stream in streams_)
            {
              stream.Write(msg);
            }
          }
        }
      }
    }
    //----< start up child thread >----------------------------------

    public void start()
    {
      if (threadRunning_)
        return;
      threadRunning_ = true;
      lock (sync_)
      {
        thrd_ = new Thread(threadProc);
        thrd_.Start();
      }
    }
    //----< has this logger been started ? >-------------------------

    public bool running()
    {
      return threadRunning_;
    }
    //----< block the child thread until unpaused >------------------
    /* 
     * this function doesn't block, it causes the thread to block
     * in threadProc()
     */
    public void pause(bool doPause)
    {
      if (doPause)
      {
        paused_ = true;
      }
      else
      {
        paused_ = false;
        lock (sync_)
        {
           Monitor.PulseAll(sync_);
        }
      }
    }
    //----< is this logger paused ? >--------------------------------

    public bool paused()
    {
      return paused_;
    }
    //----< append time stamp to msg if doShow is true >-------------

    public void showTimeStamp(bool doShow)
    {
      if (doShow)
        showTimeStamp_ = true;
      else
        showTimeStamp_ = false;
    }
    //----< flush the log and stop processing >----------------------

    public void stop(string msg = "")
    {
      if(threadRunning_)
      {
        if (msg != "")
          write(msg);
        write("quit");
        thrd_.Join();
      }
      threadRunning_ = false;
    }
    //----< current queue size >-------------------------------------

    public int size()
    {
      return Q_.size();
    }
    //----< write a log title >--------------------------------------

    public void title(string msg, bool isMajor = false)
    {
      char underline = '-';
      if (isMajor)
        underline = '=';
      string logTitle = "\n  " + msg;
      logTitle += "\n " + new string(underline, msg.Count() + 2);
      write(logTitle);
    }
    //----< add newline to log >-------------------------------------

    public void putLine()
    {
      write("\n");
    }
    //----< prompt and wait for keystroke >--------------------------

    public void waitForKey()
    {
      Console.Write("\n  press key to continue: ");
      Console.ReadKey();
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  // StaticLogger<LogType> class provides queued logging to a shared stream
  // - uses Logger for implementing all functionality
  // - provides dummy classes to provide categories of sharing, e.g.,
  // - All users of StaticLogger<LogType> share the same stream, but don't
  //   share with users of StaticLogger<anotherLogType>
  // - RLog, DLog, and DbLog are aliases for the classes
  //   StaticLogger<ResultsLog>, StaticLogger<DemoLog>, and StaticLogger<DebugLog>
  //
  public class ResultsLog { }
  public class DemoLog { }
  public class DebugLog { }

  public class StaticLogger<LogType>
  {
    public static void attach(StreamWriter stream) { logger_.attach(stream); }
    public static StreamWriter makeConsoleStream() { return logger_.makeConsoleStream(); }
    public static StreamWriter makeFileStream(string file) { return logger_.makeFileStream(file); }
    public static void write(string msg) { logger_.write(msg); }
    public static void flush() { logger_.flush(); }
    public static void showTimeStamp(bool doShow) { logger_.showTimeStamp(doShow); }
    public static void start() { logger_.start(); }
    public static bool running() { return logger_.running(); }
    public static void pause(bool doPause) { logger_.pause(doPause); }
    public static bool paused() { return logger_.paused(); }
    public static void stop() { logger_.stop(); }
    public static int size() { return logger_.size(); }
    public static void title(string msg, bool isMajor=false) { logger_.title(msg, isMajor); }
    public static void putLine() { logger_.putLine(); }

    private static Logger logger_ = new Logger();
  }

  ///////////////////////////////////////////////////////////////////
  // classes below are a hack to provide type aliases
  // - teststub below shows how to use them
  //
  public class RLog : StaticLogger<ResultsLog> { }
  public class DLog : StaticLogger<DemoLog> { }
  public class DbLog : StaticLogger<DebugLog> { }

  public class LoggerTestDriver : ITest
  {
    bool pred1, pred2, pred3;
    public bool test()
    {
      Logger logger = new Logger();
      logger.attach(logger.makeFileStream("test.txt"));
      logger.start();
      logger.pause(true);
      pred1 = logger.paused();
      logger.write("\n  first");
      logger.write("\n  second");
      pred2 = logger.size() == 2;
      logger.pause(false);
      logger.flush();
      pred3 = logger.size() == 0;
      logger.stop();
      return pred1 && pred2 && pred3;
    }
    public string getLog()
    {
      string log = "partial test of logger class - ";
      log += "did pause : " + pred1.ToString() + " - ";
      log += "didn't write : " + pred2.ToString() + " - ";
      log += "write after unpause : " + pred3.ToString();
      return log;
    }
  }
  class LoggerTest
  {
  }
}
