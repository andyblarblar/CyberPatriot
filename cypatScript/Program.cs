
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace cypatScript
{
    internal class Program
    {
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

        
        
        
        
        //-------------------------------USERS AND README--------------------------------------
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
        
        

        
        
    }
}