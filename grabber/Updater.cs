using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace grabber
{
    /// <summary>
    /// Updater provides services for checking and installing application updates.
    /// </summary>
    public class Updater
    {
        /// <summary>
        /// Checks for application updates in the specified update location.
        /// </summary>
        /// <returns></returns>
        public async Task CheckForUpdates()
        {
            ////string updateLocation = @"\\sbs\Users\Noahb\grabber\Releases";
            //string updateLocation = @"\\sbs\Users\Noahb\grabber\Releases";
            //if (Directory.Exists(updateLocation))
            //{
            //    using (var manager = new UpdateManager(updateLocation))
            //    {
            //        await manager.UpdateApp();
            //        //await manager.CheckForUpdate();
            //    }

            //}
            //else
            //{
            //    Console.WriteLine("Update location: " + updateLocation + " not found.  The application will continue to run, but updates will no longer be available until the missing location is resolved.");
            //}
        }

        /// <summary>
        /// Returns the current version number.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"v.{versionInfo.FileVersion}";
        }


    }
}
