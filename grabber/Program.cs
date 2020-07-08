using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection.Metadata;
using Jumble.ExternalCacheManager.Services;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Jumble.ExternalCacheManager;
using grabber.Commands;
using grabber.Enums;
using Jumble.ExternalCacheManager.Managers;

namespace grabber
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //ShadowCommand _shadowCommand; 
            //Detect and assign the desired user function. 
            string _userFunction = AssignUserFunction(args);

            //List<string> fakeList = new List<string>();
            //fakeList.Add("ChunkyMonkey");
            //RunPathCommand(fakeList);





            // ****************************************************
            List<string> falseFlags = new List<string>()
            {
                "/n",
                "/c",
                "/d"
            };

            //Catch blank/no entry
            if (_userFunction == null)
            {
                Console.WriteLine("Please specify a function or type 'help' for help");
            }
            //Catch help  function 
            else if (_userFunction == "help")
            {
                Console.WriteLine("This is the help manual");
            }
            //Catch flags at improper position
            else if (_userFunction.Substring(0,1) == "/")
            {
                Console.WriteLine("A flag was provided as the first argument, which won't work.  Please provide arguments using the following syntax:\n \n    {function name}, {flags}, {input parameters} \n \nFor more information, consult the help manual by calling the 'help' function. ");
            }
            else
            {
                //Process the provided arguments/flags
                List<string> raisedFlags = new List<string>();
                List<string> userArguments = new List<string>();
                int flagBoundary; 

                for (int i=1; i<args.Length; i++)
                {
                    string arg = Convert.ToString(args[i]);
                    if (arg.Substring(0,1) == "/")
                    {
                        raisedFlags.Add(arg); 
                    }
                    else
                    {
                        //Mark the position of the first non-flag argument in sequence.  Flags beyond this point will be ignored.
                        flagBoundary = i;
                        //Populate the arguments list
                        for (int j=flagBoundary; j<args.Length; j++)
                        {
                            string param = Convert.ToString(args[j]);
                            if (param.Substring(0, 1) == "/")
                            {
                                //Ignore
                            }
                            else 
                            { 
                                userArguments.Add(param); 
                            }
                        }
                        break;
                    }
                }

                //-------------------------------------------------------------

              



                //-------------------------------------------------------------

                //Execute the desired function 
                switch (_userFunction)
                {
                    case "path":
                        //Console.WriteLine("calling 'pathdirect' with flags:");
                        //foreach(string f in raisedFlags)
                        //{
                        //    Console.WriteLine(f);
                        //}
                        RunPathCommand(userArguments);
                        break;
                    case "cache":
                        //_shadowCommand = new ShadowCommand();
                        //_shadowCommand.Execute(raisedFlags);
                        RunCacheCommand(raisedFlags);
                        break;
                    case "register":
                        RunRegisterCommand(userArguments);
                        break;
                    case "rename":
                        RunRenameCommand();
                        break;
                    case "restore":
                        Console.WriteLine("All current information will be erased.  Folders and files will be restored to the default starting condition.\nThis is a non-reversible operation.");
                        Console.WriteLine("Continue? (Y or N): ");
                        string response = Console.ReadLine();
                        if(response.ToUpper() == "Y")
                        {
                            RunRestoreCommand();
                        }
                        else
                        { break;}
                        break;
                    case "echo":
                        Console.WriteLine("User called the '" + _userFunction + "' function with the following flags: ");
                        foreach(string s in raisedFlags)
                        {
                            Console.WriteLine(s);
                        }
                        Console.WriteLine("The following arguments were provided: ");
                        foreach(string a in userArguments)
                        {
                            Console.WriteLine(a);
                        }
                        break;
                    default:
                        break; 
                }

            }
            

        }

        /// <summary>
        /// Reads first argument provided by user and interprets it; if it is valid function, the function will be called.  Otherwise, 
        /// an error message will be returned to the user.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string AssignUserFunction(string[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            else
            {
                return Convert.ToString(args[0]);
            }
        }


        /// <summary>
        /// Runs the cache function using the provided raised flags.
        /// </summary>
        /// <param name="raisedFlags"></param>
        private static void RunCacheCommand(List<string> raisedFlags)
        {
            //CachePathManager pathManager = new CachePathManager();
            ////pathManager.UpdatePathMapFile();
            //pathManager.GetPathMap();



        }

        /// <summary>
        /// Runs the path command with provided flags.
        /// </summary>
        private static void RunPathCommand(List<string> userParameters)
        {
            string sampleString = userParameters[0];
            Console.WriteLine("Original string: " + sampleString);
       
            CryptoService cryptor = new CryptoService();
            string encryptedString = cryptor.Encrypt(sampleString);
            Console.WriteLine("Encrypted string: " + encryptedString);

            string decryptedString = cryptor.Decrypt(encryptedString);
            Console.WriteLine("Decrypted string: " + decryptedString);


        }

        /// <summary>
        /// Register a new file name with the ECM. 
        /// </summary>
        private static bool RunRegisterCommand(List<string> userParameters)
        {
            //Parse arguments
            Purview purview = GetPurviewType(userParameters[0]);
            string fileName = userParameters[1];
            string somType = userParameters[2];

            //validate
            if (purview == Purview.Nonspecific)
            {
                Console.WriteLine("The purview : " + userParameters[0] + " is unrecognized.");
                return false;
            }

            RegisterCommand registerCommand = new RegisterCommand();
            RegistrationResult result = registerCommand.Register(purview, fileName, somType);
            if (result == RegistrationResult.Success)
            {
                Console.WriteLine("'" + fileName + "' was registered to the cache for: " + purview);
                return true;
            }
            else
            {
                Console.WriteLine("Registration Result: " + result.ToString());
                return false;
            }
        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        private static void RunRenameCommand()
        {

        }

        /// <summary>
        /// Restores all file and purview information to the original default settings; all 'rename' events will be deleted, so only
        /// applications that name-map to the original file names will continue to function.  All other applications must be reset to 
        /// search for the default file names used by the ECM.
        /// </summary>
        private static void RunRestoreCommand()
        {
            RestoreCommand restoreCommand = new RestoreCommand();
            restoreCommand.RestoreDefaults(@"\\sbs\Users\Noahb\EstimatingAddIn\cache\");
        }

        private static Purview GetPurviewType(string val)
        {
            switch (val)
            {
                case "Budget":
                    return Purview.Budget;
                case "Document":
                    return Purview.Document;
                case "Aire":
                    return Purview.Aire;
                case "Silo":
                    return Purview.Silo;
                default:
                    return Purview.Nonspecific;
            }
        }



    }
}
