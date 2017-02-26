////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Second Client with UI for the Remote TestHarness  //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Client UI for CSE681 - Software Modeling & Analysis      //
// Author:      Brahmachaitahnya Sadashiva, Syracuse University(CE)        //
//              bsadashi@syr.edu                                           //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package contains all the methods required for Ui to accept the input 
 * and display the results
 * The Client2 package defines  one class, Client2, that uses the Comm<Client2>
 * class send and accept to messages from a remote endpoints.
 * Takes input from UI and forms test request message
 * uploads file to repository
 * Display test results
 * Retrieve logs from repository
 * Display log contents

 * Required Files:
 * ---------------
 *-MainWindow.xaml.cs
 * - Client2.cs
 * - ICommunicator.cs, CommServices.cs
 * - Messages.cs, MessageTest, Serialization
 *
 * Maintenance History
 * --------------------
 * Ver 1.0 : 20 Nov 2016
 * - first release 

 *  
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;

namespace CommChannelDemo
{
   
    public partial class MainWindow : Window
    {
        Client2 cc = new Client2();
        private List<String> fileList = new List<string>();
        private List<String> testcode = new List<string>();
        private string testdriver;
        private string testname;
        private string author;
        public MainWindow()
        {
            InitializeComponent();

        }

        // to select the testdriver file
        private void testdriver_button(object sender, RoutedEventArgs e)
        {
            

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            dlg.InitialDirectory = System.IO.Path.GetFullPath("..\\..\\TestDrivers");
            dlg.DefaultExt = ".dll";
            dlg.Filter = "DLL Files(*.dll)|*.dll";
           
            try
            {
                Nullable<bool> result = dlg.ShowDialog();
                if (null != dlg.FileName)
                {
                    
                        if (!fileList.Contains(dlg.FileName))
                            fileList.Add(dlg.FileName);
                    string testdrive = System.IO.Path.GetFileName(dlg.FileName);
                        testdriver = testdrive;
                           Refresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error", ex);
            }


        }
        //to select the testcode file
        private void testcode_button(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetFullPath("..\\..\\TestCode");
            dlg.DefaultExt = ".dll";
            dlg.Filter = "DLL Files(*.dll)|*.dll";

            try
            {
                Nullable<bool> result = dlg.ShowDialog();
                if (null != dlg.FileName)
                {
                    
                        if (!fileList.Contains(dlg.FileName))
                            fileList.Add(dlg.FileName);
                        testcode.Add(System.IO.Path.GetFileName(dlg.FileName));                
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error", ex);
            }
            
        }
        //refrsh the list box
        private void Refresh()
        {
            fileListBox.ItemsSource = null;
            fileListBox.ItemsSource = fileList;
        }

        //close the list box
        private void remove_button_1(object sender, RoutedEventArgs e)
        {
            int location = fileListBox.SelectedIndex;
            string selectedItem = fileListBox.SelectedItem as string;
            fileList.Remove(selectedItem);
            Refresh();
        }

        // forming the test message and uploading files and sending test request
        private void start_test(object sender, RoutedEventArgs e)
        {
            if (testname != "" && fileList.Count > 0 && author != "")
            {
                cc.comm.sndr.channel = Sender.CreateServiceChannel("http://localhost:8082/StreamService");
                foreach (string file in fileList)
                {
                    //uploading files
                    cc.comm.sndr.uploadFile(System.IO.Path.GetFileName(file));
                }
                string to = Comm<Client2>.makeEndPoint("http://localhost", 8080);
                string from = Comm<Client2>.makeEndPoint("http://localhost", 8084);
                Message msg =  cc.makeMessage(author, from, to);
                //forming test request
                msg.body = MessageTest.makeTestRequestUI(testname,testcode,testdriver);
               //sending request
                cc.comm.sndr.PostMessage(msg);
                



            }
            else
            {
                MessageBox.Show("enter author and testname");
                return;
            }
            
        }

       


        private void show_log(object sender, RoutedEventArgs e)
        {
            if (test_author.Text == "")
            {
                MessageBox.Show("Enter author Name");
                return;
            }
            string to = Comm<Client2>.makeEndPoint("http://localhost", 8082);
            string from = Comm<Client2>.makeEndPoint("http://localhost", 8084);
            Message msg = cc.makeMessage(test_author.Text, from, to);
            msg.type = "testlogs";
            msg.body = test_author.Text;
            cc.comm.sndr.PostMessage(msg);
            //sending request for logs
            RichTextBox rctxtBx = new RichTextBox();
            rctxtBx.Height = 250;
            rctxtBx.Width = 400;
            rctxtBx.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Paragraph p = rctxtBx.Document.Blocks.FirstBlock as Paragraph;
            p.Margin = new Thickness(0);
            p.LineHeight = 10;

            try
            {
                
                fetchResultGrid.Children.Add(rctxtBx);
                rctxtBx.AppendText(cc.getlog());
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Error",Ex);
            }      
        }

        private void prjname_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            testname = box.Text;
        }
        //to display results from testharness
        private void show_results_button(object sender, RoutedEventArgs e)
        {
            
            RichTextBox rctxtBx = new RichTextBox();
            rctxtBx.Height = 250;
            rctxtBx.Width = 400;
            rctxtBx.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Paragraph p = rctxtBx.Document.Blocks.FirstBlock as Paragraph;
            p.Margin = new Thickness(0);
            p.LineHeight = 10;
            fetchResultGrid.Children.Add(rctxtBx);
            rctxtBx.AppendText(cc.getmessage());

        }

       
        private void test_author_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void username_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            author = box.Text;
        }
    }
}
