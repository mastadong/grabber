using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace grabber.Commands
{
    public class PrintManualCommand
    {
        public void PrintManualToConsole()
        {
            string helpFile = @"AsBuilt.txt";
            string textContents; 

            if (File.Exists(helpFile))
            {
                using(var readText = new StreamReader(helpFile))
                {
                    textContents = readText.ReadToEnd();
                    Console.WriteLine(textContents);
                }
            }
            else
            {
                Console.WriteLine("The File '" + helpFile + "' could not be found in the application folder.  This is the help file for 'grabber', so until a new copy is placed in the application folder, the help manual cannot be displayed.");
            }
        }
    }
}
