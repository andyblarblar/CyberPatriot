
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace cypatScript
{
    internal class Program
    {
        private static readonly String UsrProfile = Environment.GetEnvironmentVariable("USERPROFILE"); 
        public static void Main(string[] args)
        {Console.WriteLine("Greetings comrade. It is currently {0}",DateTime.Now);
        MainLoop();
        
        }

        private static void MainLoop()
        {
            var usrIn = string.Empty;
            
            while (!usrIn.Equals("Q"))
            {
                Console.WriteLine("\n please choose an option, or \"Q\" to quit.\n" +
                                  "1) show bad users from readme");
                
                usrIn = Console.ReadLine();
                
                switch (usrIn)
                {case "1":
                        new Thread(() => 
                        {Console.WriteLine("starting bad users thread...");
                            Thread.CurrentThread.IsBackground = true; 
                            //TODO compare machine stuff to readme. have a method that makes a new window containing the colored users and nest methods from there.
                            
                            formatReadmeToLists(GetReadmeThings(GetReadme()), out List<String> users, out List<String> admins);
                            
                            users.ForEach(Console.WriteLine);
                            admins.ForEach(Console.WriteLine);
                            
                        }).Start();
                        Thread.Sleep(100);
                    break;
                    
                
                case "Q":
                    Console.WriteLine("see ya!");
                    break;
                
                default:
                    Console.WriteLine("please enter a valid operation...");
                    break;
                    
                }
            
                
            }
        
            
            
        }





        #region Users and readme 
        
        private static IEnumerable<string> GetMachineAdmins()
        {    var returns = new List<string>();
            using (var machine = new DirectoryEntry("WinNT://" + Environment.MachineName))
            {
                //get local admin group
                using (var group = machine.Children.Find("Administrators", "Group"))
                {
                    //get all members of local admin group
                    var members = group.Invoke("Members", null);
                    returns.AddRange(from object member in (IEnumerable) members select new DirectoryEntry(member).Name);
                }
            }
            return returns;
        }
        
        
        private static IEnumerable<string> GetMachineUsers()
        {    var returns = new List<string>();
            using (var machine =  new DirectoryEntry("WinNT://" + Environment.MachineName))
            {
                //get local admin group
                using (var group = machine.Children.Find("Users", "Group"))
                {
                    //get all members of local admin group
                    var members = group.Invoke("Members", null);
                    returns.AddRange(from object member in (IEnumerable) members select new DirectoryEntry(member).Name);
                }
            }
            return returns;
        }
        
        /// <summary>
        /// Gets the path of a file with readme in the name 
        /// </summary>
        /// <returns>the FULL path to the file</returns>
        private static string GetReadme() { return Directory.GetFiles(UsrProfile + "\\Desktop", "*readme*")[0]; }
        
        /// <summary>
        /// returns the authorised users and admins from the readme file
        /// </summary>
        /// <param name="reamePath">path to the readme</param>
        /// <returns>raw users and admins (so not usable yet)</returns>
        private static String GetReadmeThings(String reamePath)
        { 
            var doc = new HtmlDocument();
            doc.Load(reamePath);
            foreach (var node in doc.DocumentNode.Descendants().ToList())
            {
                if (node.Name.Equals("pre"))
                {
                    return node.InnerText;
                }
                
            }

            return String.Empty;
        }

        
        /// <summary>
        /// parses the pre element to find the users and admins marked by the HTML and enumerates them in a list
        /// </summary>
        /// <param name="readmeData">the string to parse</param>
        /// <param name="users">the names of found users</param>
        /// <param name="admins">the names of found admins</param>
        private static void formatReadmeToLists(String readmeData,out List<String> users,out List<String> admins)
        {
            
            String[] split = readmeData.Split(new []{"Authorized Users:"},StringSplitOptions.RemoveEmptyEntries);

            users = split[1].Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.RemoveEmptyEntries).ToList();

            admins = split[0].Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            admins.Remove("Authorized Administrators:");
            
            for(var i = 0;i<admins.Count;i++)
            {
                var name = admins[i];
                if (name.Trim().Contains("password:"))//removes password lines
                {
                    admins.Remove(name);
                    continue;
                }

                admins[i] = name.Split(' ')[0];//removes (you)
            }
            
        }
        
        #endregion
        
        
        
        
        

        
        
    }
}