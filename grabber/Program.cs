using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Services;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager;
using System;
using System.IO;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices.ComTypes;
using grabber.Commands;
using Squirrel;
using grabber.CacheManager;

namespace grabber
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //Detect user-specified function.
            string _userFunction = AssignUserFunction(args);

            //Catch blank/no entry
            if (_userFunction == null)
            {
                Console.WriteLine("Please specify a function or type 'help' for help");
            }
            //Catch help  function 
            else if (_userFunction == "help")
            {
                PrintManualCommand printManualCommand = new PrintManualCommand();
                printManualCommand.PrintManualToConsole();
                Console.WriteLine("Press any key to continue...");
                Console.Read();
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

                //Execute the desired function 
                switch (_userFunction)
                {
                    case "path":
                        RunPathCommand(raisedFlags, userArguments);
                        break;
                    case "cache":
                        RunCacheCommand(raisedFlags);
                        break;
                    case "init":
                        RunRestoreCommand(raisedFlags, userArguments);
                        break;
                    case "info":
                        DisplaySystemInformation();
                        break;
                    case "register":
                        RunRegisterCommand(userArguments);
                        break;
                    case "rename":
                        RunRenameCommand(userArguments);
                        break;
                    case "restore":
                        Console.WriteLine("All current information will be erased.  Folders and files will be restored to the default starting condition.\nThis is a non-reversible operation.");
                        Console.WriteLine("Continue? (Y or N): ");
                        string response = Console.ReadLine();
                        if(response.ToUpper() == "Y")
                        {
                            RunRestoreCommand(raisedFlags, userArguments);
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
            if (raisedFlags.Count < 1)
            {
                Console.WriteLine("The 'cache' command requires at least one flag.");
            }
            else if (raisedFlags[0] == "/r")
            {
                BackupManager backupManager = new BackupManager();
                backupManager.RunBackup();
            }
            else
            {
                Console.WriteLine("No valid flags were provided.  'cache' operation was cancelled.");
            }
           
        }

        /// <summary>
        /// Runs the path command with provided flags.
        /// </summary>
        private static void RunPathCommand(List<string> raisedFlags, List<string> userParameters = null)
        {
            if (raisedFlags.Count < 1)
            {
                Console.WriteLine("The 'path' command requires at least one flag.  \nUse '/r' to display the directory structure or \nuse '/u {newdirectoryPath}' to change the existing cache root directory.");
            }
            else if (raisedFlags[0] == "/r")
            {
                PathCommand pathCommand = new PathCommand();
                pathCommand.DisplayDirectoryStructure();
            }
            else if (raisedFlags[0] == "/c")
            {
                PathCommand pathCommand = new PathCommand();
                pathCommand.ViewRootCacheDirectory();
            }
            else if (raisedFlags[0] == "/u")
            {
                if (userParameters.Count >= 1)
                {

                    try
                    {
                        string newCacheRootDirectory = userParameters[0];
                        Directory.CreateDirectory(newCacheRootDirectory);

                        if (Directory.Exists(newCacheRootDirectory))
                        {
                            Console.WriteLine("This process will copy all existing files to the new root directory and erase all the\nfolders and files in the current root directory.\nProceed? (Y or N): ");
                            string response = Console.ReadLine();
                            if (response.ToUpper() == "Y")
                            {
                                PathCommand pathCommand = new PathCommand();
                                if (pathCommand.ChangeCacheRootDirectory(newCacheRootDirectory))
                                {
                                    Console.WriteLine("The root cache directory has been successfully updated to: " + newCacheRootDirectory);
                                }
                            }
                            else
                            {
                                //Abort and return to user.
                                Console.WriteLine("User function was cancelled. ");
                            }
                        }
                        else
                        {
                            Console.WriteLine("The directory: " + newCacheRootDirectory + " cannot be found.\nPlease check that the directory exists and try again.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a directory path");
                }
            }

        }

        /// <summary>
        /// Register a new file name with the ECM. 
        /// </summary>
        private static bool RunRegisterCommand(List<string> userParameters)
        {

            if (userParameters.Count >= 3)
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
            else
            {
                Console.WriteLine("'register' requires the following arguments: {purview} {filename} {somType}.");
                return false;
            }
        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        private static void RunRenameCommand(List<string> userParameters)
        {
            if (userParameters.Count >= 2)
            {
                string oldName = userParameters[0];
                string newName = userParameters[1];
                RenameCommand renameCommand = new RenameCommand();
                if (renameCommand.Rename(oldName, newName) == true)
                {
                    Console.WriteLine("The file: '" + oldName + "' was successfully renamed to: '" + newName + "'.");
                }
                else
                {
                    //The rename command will print the error message.
                }
            }
            else
            {
                Console.WriteLine("'rename' requires two arguments: {oldname} {newname}.");
            }
        }

        /// <summary>
        /// Restores all file and purview information to the original default settings; all 'rename' events will be deleted, so only
        /// applications that name-map to the original file names will continue to function.  All other applications must be reset to 
        /// search for the default file names used by the ECM.
        /// </summary>
        private static void RunRestoreCommand(List<string> raisedFlags, List<string> userArguments)
        {
            if (raisedFlags.Count == 0)
            {
                RestoreCommand restoreCommand = new RestoreCommand();
                restoreCommand.RestoreDefaults(@"\\sbs\Users\Noahb\EstimatingAddIn\cache\");
            }
            else if (raisedFlags[0] == "/o" && userArguments.Count != 0)
            {
                if (!Directory.Exists(userArguments[0]))
                {
                    try
                    {
                        //Attempt to create the directory, since it does not currently exist.
                        Directory.CreateDirectory(userArguments[0]);
                        //If the attempt is successful, run the restore command on the new directory path.
                        RestoreCommand restoreCommand = new RestoreCommand();
                        restoreCommand.RestoreDefaults(userArguments[0]);
                    }
                    catch (Exception e)
                    {
                        //The attempt to create the directory failed, since the provided path was invalid.
                        Console.WriteLine("Error attempting create directory: " + userArguments[0] + ": " + e.Message.ToString());
                    }
                }
                else
                {
                    //The provided directory exists, so use it to run the restore command.
                    RestoreCommand restoreCommand = new RestoreCommand();
                    restoreCommand.RestoreDefaults(userArguments[0]);
                }
            }
            else
            {
                //Determine whether the flag or the provided argument is the source of the error.
                if(raisedFlags[0] != "/o")
                {
                    Console.WriteLine("Please provide a valid flag for the 'restore' command.");
                }
                if(userArguments.Count == 0)
                {
                    Console.WriteLine("Please provide a valid path as an argument for the 'restore' command.");
                }
            }
        }

        /// <summary>
        /// Displays system information about 'grabber'
        /// </summary>
        private static void DisplaySystemInformation()
        {
            Updater updater = new Updater();
            Console.WriteLine("'grabber' ");
            Console.WriteLine (updater.GetCurrentVersionNumber());
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
