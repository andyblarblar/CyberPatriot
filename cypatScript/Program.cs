
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;
using Console = Colorful.Console;
using System.Windows.Forms;



/*
 * backlog:
 * add an option to delete to be reomved users (easy)
 * stylise the ACII (easy)
 * 
 *
 *
 * 
 */
    

namespace cypatScript
{
    internal class Program
    {
        private static readonly String UsrProfile = Environment.GetEnvironmentVariable("USERPROFILE"); 
        public static void Main(string[] args)
        {   Console.WriteAscii("Code Crusaders");
            Console.WriteLine($"By the Pope! It is currently {DateTime.Now}");
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
                        {Console.WriteLine("starting bad users search, this will take a while the first time...");
                            Thread.CurrentThread.IsBackground = true;

                            try
                            {
                                ShowMessageBoxWithUsers(
                                    CheckIfAccountOnMachine(FormatReadmeToLists(GetReadmeThings(GetReadme()))));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("\nThere were problems in the bad users thread! (most likely no readme on desktop, or new error)",Color.Fuchsia);
                                
                            }
                            
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
        
        private static List<string> GetMachineAdmins()
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
        
        
        private static List<string> GetMachineUsers()
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
            var doc = new HtmlAgilityPack.HtmlDocument();
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
        /// parses the pre element to find the users and admins marked by the HTML and enumerates them in a Dictionary
        /// </summary>
        /// <param name="readmeData">the string to parse</param>
       
        private static Dictionary<String,AccountType> FormatReadmeToLists(String readmeData)
        {
            var result = new Dictionary<String, AccountType>();
            var users = new List<string>();
            var admins = new List<string>();
            
            String[] split = readmeData.Split(new []{"Authorized Users:"},StringSplitOptions.RemoveEmptyEntries);

            users = split[1].Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var user in users)//add users to map
            {
                result.Add(user,AccountType.User);
            }

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

            foreach (var admin in admins)//add admins to map
            {
                result.Add(admin,AccountType.Admin);
                
            }

            return result;
        }

        /// <summary>
        /// Checks the readme accounts against the local accounts and marks what should be done.
        /// </summary>
        /// <param name="accounts">A Dictionary of accounts from the readme marked as the proffered type via the Value</param>
        /// <returns>A Dictionary formatted with the result of the check. User or Admin means the account is fine, the rest are self explanatory</returns>
        private static Dictionary<String,AccountType> CheckIfAccountOnMachine(Dictionary<String,AccountType> accounts)
        {
            var result = new Dictionary<String,AccountType>();
            var localUsers = GetMachineUsers();
            var localAdmins = GetMachineAdmins();
            
            foreach (var account in accounts)
            {
                if (account.Value == AccountType.User)//if marked as user in readme,
                {
                    if (localUsers.Contains(account.Key))//if existing on machine, add as User
                    {
                        result.Add(account.Key,account.Value);

                    }
                    else
                    {
                        if (localAdmins.Contains(account.Key))//if admin, but should be User
                        {
                            result.Add(account.Key,AccountType.ShouldBeUser);
                        }
                        else
                        {
                            result.Add(account.Key, AccountType.ShouldBeAddedAsUser);//they should be added
                        }
                    }
                    
                }//end users

                if (account.Value == AccountType.Admin)//if marked as admin
                {
                    if (localAdmins.Contains(account.Key))//if existing on machine
                    {
                        result.Add(account.Key,account.Value);

                    }
                    else
                    {
                        if (localUsers.Contains(account.Key))//if user, but should be admin
                        {
                            result.Add(account.Key,AccountType.ShouldBeAdmin);
                        }
                        else
                        {
                            result.Add(account.Key, AccountType.ShouldBeAddedAsAdmin);//they should be added
                        }
                        
                    }
                    
                }//end admins
                
            }

            foreach (var localUser in localUsers)//checking if local users should exist
            {
                if (!accounts.ContainsKey(localUser))
                {
                    try
                    {
                        result.Add(localUser,AccountType.ShouldBeRemoved);
                    }
                    catch (Exception e)//if they were already added above
                    {
                        result.Remove(localUser);
                        result.Add(localUser,AccountType.ShouldBeRemoved);
                    }
                }
                
                
            }
            
            foreach (var localAdmin in localAdmins)//checking if local admins should exist
            {
                if (!accounts.ContainsKey(localAdmin))
                {
                    try
                    {
                        result.Add(localAdmin,AccountType.ShouldBeRemoved);
                    }
                    catch (Exception e)//if they were already added above
                    {
                        result.Remove(localAdmin);
                        result.Add(localAdmin,AccountType.ShouldBeRemoved);
                    }
                    
                }
                
                
            }


            return result;
            
        }
        
        private static void ShowMessageBoxWithUsers(Dictionary<String, AccountType> accounts)
        {    
            Console.WriteLine("\n");

            foreach (var account in accounts)
            {
                switch (account.Value)
                {
                    case AccountType.Admin:
                        Console.WriteLineFormatted("{1} should {0} admin","remain",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.User:
                        Console.WriteLineFormatted("{1} should {0} user","remain",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.ShouldBeAdmin:
                        Console.WriteLineFormatted("{1} should {0}","be changed to admin",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.ShouldBeUser:
                        Console.WriteLineFormatted("{1} should {0}","be changed to user",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.ShouldBeRemoved:
                        Console.WriteLineFormatted("{1} should {0}","be removed",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.ShouldBeAddedAsAdmin:
                        Console.WriteLineFormatted("{1} should be {0}","added as admin",account.Key,Color.Red,Color.White);
                        break;
                    case AccountType.ShouldBeAddedAsUser:
                        Console.WriteLineFormatted("{1} should be {0}","added as user",account.Key,Color.Red,Color.White);
                        break;
                    
                }
                
            }
            Console.WriteLine("\n");
        }
        
        
        #endregion
        
        
        
        
        

        
        
    }
}