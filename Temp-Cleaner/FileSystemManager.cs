using System;
using System.IO;
using System.Linq;

namespace FileSystemManager
{
    /// <summary>
    /// A folder class I made to ease the interaction with files and folders.
    /// </summary>
    class Folder
    {
        private DirectoryInfo MainDir;

        #region "OBJECT PROPERTIES"
        public string Name { get { return this.MainDir.Name; } }
        public string FullName { get { return this.MainDir.FullName; } }
        public Folder Parent { get { return new Folder(this.MainDir.Parent.FullName); } }
        public DirectoryInfo Main { get { return this.MainDir; } }
        public DirectoryInfo[] SubDirectories
        { get { return this.MainDir.GetDirectories("*", SearchOption.AllDirectories); } }
        public FileInfo[] Files
        { get { return this.MainDir.GetFiles("*.*", SearchOption.AllDirectories); } }
        public long Size
        {
            get
            {
                FileInfo[] files = this.Files;
                long size_bytes = 0;

                for (int i = 0; i < files.Length; i++)
                    size_bytes += files[i].Length;

                return size_bytes;
            }
        }

        #endregion

        #region "STATIC METHODS"

        #region "SIZE FORMATTING"
        public static string AutoSizeToString(long value)
        {
            string[] sizes = { " B", " KB", " MB", " GB", " TB" };
            double return_val = value;

            for (int i = 0; i < sizes.Length; i++)
            {
                if (return_val <= 1024) return (Math.Round(return_val, 1).ToString() + sizes[i]);

                return_val = return_val / 1024;
            }

            return Math.Round(return_val, 1).ToString() + " TB";
        }
        #endregion

        #region "EXTRACTION"
        /// <summary>
        /// Extracts the parent of a directory from a correctly formatted path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns a string of the parent directory</returns>
        public static string ExtractParentName(string path)
        {
            if (!path.Contains('\\')) return path;

            return path.Replace("\\" + Folder.ExtractName(path), "");
        }

        /// <summary>
        /// Extracts the name of the file/folder from a correctly formatted path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns a string of the name of the path</returns>
        public static string ExtractName(string path)
        {
            if (!path.Contains('\\')) return path;

            return path.Split('\\').Last();
        }
        #endregion

        #region "LOGIC CHECKS"
        /// <summary>
        /// Checks if a directory path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Checks if a file path exists
        /// </summary>
        /// <param name="path"></param>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Checks if a path exists
        /// </summary>
        /// <param name="path"></param>
        public static bool Exists(string path)
        {
            return (Folder.FileExists(path) || Folder.FolderExists(path));
        }

        /// <summary>
        /// Checks if a directory is empty
        /// </summary>
        /// <param name="path"></param>
        public static bool IsEmpty(string path)
        {
            if (!Folder.FolderExists(path)) return false;

            Folder path_folder = new Folder(path);

            if ((path_folder.Files.Length == 0) || (path_folder.SubDirectories.Length == 0))
                return true;
            return false;
        }
        #endregion

        #region "FOLDER/FILE CREATION"
        public static DirectoryInfo CreateFolder(string path)
        {
            if (!Folder.FolderExists(path))
                Directory.CreateDirectory(path);

            return new DirectoryInfo(path);
        }

        public static FileInfo CreateFile(string path)
        {
            if (Folder.FileExists(path)) return new FileInfo(path);

            Folder.CreateFolder(ExtractParentName(path));
            File.Create(path);

            return new FileInfo(path);
        }

        public static void Delete(string to_delete, bool force_delete)
        {
            if (force_delete) Directory.Delete(to_delete, true);
        }
        #endregion

        #endregion

        #region "OBJECT CREATION"
        public Folder(string main_path)
        {
            this.MainDir = new DirectoryInfo(main_path);
        }
        #endregion

        #region "OBJECT METHODS"
        public void ChangeMain(string new_main)
            => this.MainDir = new DirectoryInfo(new_main);

        /// <summary>
        /// Moves the main directory and its contents of the main directory to another location.
        /// </summary>
        /// <param name="destination"></param>
        public void Move(string destination)
        {
            if (!Folder.FolderExists(destination)) Directory.CreateDirectory(destination);
            destination = destination + "\\" + this.MainDir.Name;

            this.MainDir.MoveTo(destination);
            this.ChangeMain(destination);
        }

        /// <summary>
        /// Copies the main directory and its contents to another directory.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>Returns a <b>Folder</b> object of the copied directory.</returns>
        public Folder Copy(string destination)
        {
            if (!Folder.FolderExists(destination)) Directory.CreateDirectory(destination);

            DirectoryInfo[] Sub_Directories = this.SubDirectories;
            FileInfo[] Files = this.Files;

            destination = destination + "\\" + this.MainDir.Name;

            Directory.CreateDirectory(destination);

            for (int i = 0; i < Sub_Directories.Length; i++)
            {
                Directory.CreateDirectory(
                    Sub_Directories[i].FullName.Replace(
                        this.MainDir.FullName,
                        destination
                        )
                    );
            }

            for (int i = 0; i < Files.Length; i++)
            {
                File.Copy(
                    Files[i].FullName,
                    Files[i].FullName.Replace(
                        this.MainDir.FullName,
                        destination
                        ),
                    true
                    );
            }

            return new Folder(destination);
        }

        /// <summary>
        /// Renames the main directory
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            this.MainDir.MoveTo(
                this.MainDir.FullName.Replace(
                    this.MainDir.Name,
                    name
                    )
                );
        }

        /// <summary>
        /// Deletes all sub-directories and files
        /// </summary>
        public void EmptyContent()
        {
            this.DeleteAllDirectories();
            Console.WriteLine();
            this.DeleteAllFiles();
            Console.WriteLine();
        }

        /// <summary>
        /// Delete all sub-directories and their content
        /// </summary>
        public void DeleteAllDirectories()
        {
            DirectoryInfo[] directory = this.SubDirectories;

            for (int i = 0; i < directory.Length; i++)
                try
                {
                    directory[i].Delete(true);
                }
                catch (IOException)
                {
                    Console.WriteLine("\"{0}\" was not deleted", directory[i].Name);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("\"{0}\" could not be accessed", directory[i].Name);
                }
        }

        /// <summary>
        /// Deletes all files within the main directory and sub-directories
        /// </summary>
        public void DeleteAllFiles()
        {
            FileInfo[] files = this.Files;

            for (int i = 0; i < files.Length; i++)
                try
                {
                    files[i].Delete();
                }
                catch (IOException)
                {
                    Console.WriteLine("\"{0}\": {1} Bytes", files[i].FullName.Replace(this.FullName + '\\', ""), files[i].Length);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("\"{0}\": {1} Bytes", files[i].FullName.Replace(this.FullName + '\\', ""), files[i].Length);
                }
        }
        #endregion
    }

}
