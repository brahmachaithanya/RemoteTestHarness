/////////////////////////////////////////////////////////////////////
// Messages.cs - defines communication messages                    //
// ver 1.1                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides helper code for building and parsing XML messages.
 *
 * Required files:
 * ---------------
 * - Messages.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.1 : 10 Nov 2016
 * - moved TestElement and TestRequest to MessageTests.cs
 * ver 1.0 : 16 Oct 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Utilities;

namespace CommChannelDemo
{
  [Serializable]
  public class Message
  {
    public string type { get; set; } = "default";
    public string to { get; set; }
    public string from { get; set; }
    public string author { get; set; } = "";
    public DateTime time { get; set; } = DateTime.Now;
    public string body { get; set; } = "none";

    public List<string> messageTypes { get; set; } = new List<string>();

    public Message()
    {
      messageTypes.Add("TestRequest");
      body = "";
    }
    public Message(string bodyStr)
    {
      messageTypes.Add("TestRequest");
      body = bodyStr;
    }
    public Message fromString(string msgStr)
    {
      Message msg = new Message();
      try
      {
        string[] parts = msgStr.Split(',');
        for (int i = 0; i < parts.Count(); ++i)
          parts[i] = parts[i].Trim();

        msg.type = parts[0].Substring(6);
        msg.to = parts[1].Substring(4);
        msg.from = parts[2].Substring(6);
        msg.author = parts[3].Substring(8);
        msg.time = DateTime.Parse(parts[4].Substring(6));
        msg.body = parts[5].Substring(6);
      }
      catch
      {
        Console.Write("\n  string parsing failed in Message.fromString(string)");
        return null;
      }
      //XDocument doc = XDocument.Parse(body);
      return msg;
    }
    public override string ToString()
    {
      string temp = "type: " + type;
      temp += ", to: " + to;
      temp += ", from: " + from;
      if(author != "")
        temp += ", author: " + author;
      temp += ", time: " + time;
      temp += ", body:\n" + body;
      return temp;
    }
    public Message copy()
    {
      Message temp = new Message();
      temp.type = type;
      temp.to = to;
      temp.from = from;
      temp.author = author;
      temp.time = DateTime.Now;
      temp.body = body;
      return temp;
    }
  }

  public static class extMethods
  {
    public static void showMsg(this Message msg)
    {
      Console.Write("\n  formatted message:");
      string[] lines = msg.ToString().Split(new char[] { ',' });
      foreach (string line in lines)
      {
        Console.Write("\n    {0}", line.Trim());
      }
      Console.WriteLine();
    }
    public static string showThis(this object msg)
    {
      string showStr = "\n  formatted message:";
      string[] lines = msg.ToString().Split('\n');
      foreach (string line in lines)
        showStr += "\n    " + line.Trim();
      showStr += "\n";
      return showStr;
    }
    public static string shift(this string str, int n = 2)
    {
      string insertString = new string(' ', n);
      string[] lines = str.Split('\n');
      for(int i=0; i<lines.Count(); ++i)
      {
        lines[i] = insertString + lines[i];
      }
      string temp = "";
      foreach (string line in lines)
        temp += line + "\n";
      return temp;
    }
    public static string formatXml(this string xml, int n = 2)
    {
      XDocument doc = XDocument.Parse(xml);
      return doc.ToString().shift(n);
    }
  }

  class TestMessages
  {
#if (TEST_MESSAGES)
    static void Main(string[] args)
    {
      Console.Write("\n  Testing Message Class");
      Console.Write("\n =======================\n");

      Message msg = new Message();
      msg.to = "http://localhost:8080/ICommunicator";
      msg.from = "http://localhost:8081/ICommunicator";
      msg.author = "Fawcett";
      msg.type = "TestRequest";
      msg.showMsg();
      Console.Write("\n\n");
    }
#endif
  }
}
