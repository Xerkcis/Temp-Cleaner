using System;
using System.IO;
using System.Threading;

using Microsoft.Toolkit.Uwp.Notifications;

using FileSystemManager;

namespace Temp_Cleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ToastContentBuilder cleaner_status = new ToastContentBuilder();

            DriveInfo C_Drive = new DriveInfo("C:");
            Folder temp_folder = new Folder(Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile) + "\\AppData\\Local\\Temp"
                );

            long initial_size = temp_folder.Size;

            #region "Toast Initial Content"
            cleaner_status.AddText("Cleaned Temp folder");
            #endregion

            temp_folder.EmptyContent();

            #region "Final Toast Content"
            cleaner_status.AddText($"{Folder.AutoSizeToString(initial_size - temp_folder.Size)} have been cleaned");
            cleaner_status.AddText($"{Folder.AutoSizeToString(C_Drive.AvailableFreeSpace)} are available in C:");
            #endregion

            cleaner_status.Show();

            Thread.Sleep(20);
        }
    }
}
