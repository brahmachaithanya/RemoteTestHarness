////////////////////////////////////////////////////////////////////////////
// ICommunicator.cs - Peer-To-Peer Communicator Service Contract           //
//  ver 3.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Icommunicator  for CSE681 - Software Modeling & Analysis  //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
// Source :Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
////////////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
  * ver 3.0 : 20 Nov 2016
 * Added file streaming contracts
 *   
 * ver 2.0 : 10 Oct 11
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 * ver 1.0 : 14 Jul 07
 * - first release
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace CommChannelDemo
{
  [ServiceContract]
  public interface ICommunicator
  {
    [OperationContract(IsOneWay = true)]
    void PostMessage(Message msg);

    // used only locally so not exposed as service method

    Message GetMessage();
        [OperationContract(IsOneWay = true)]
        void upLoadFile(FileTransferMessage msg);
        [OperationContract]
        Stream downLoadFile(string filename);
    }
    //public interface IStreamService
    //{
      
    //}

    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
    // The class Message is defined in CommChannelDemo.Messages as [Serializable]
    // and that appears to be equivalent to defining a similar [DataContract]

}
