using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager
{
    static class RecycleBin
    {
        public static void MoveToTrash(ICollection<FileSystemInfo> collection)
        {
            try
            {
                if (collection.Count == 0 && collection == null)
                    throw new Exception("Attempted to delete the same file  twice");

                foreach (var fi in collection)
                    if (collection.Count(e => e == fi) != 1)
                        throw new Exception("Attempted to delete the same file  twice");

                foreach (var fi in collection)
                    if (ShellUtilities.SendToRecycle(fi.FullName, ShellUtilities.FileOperationFlags.FOF_WANTNUKEWARNING) == false)
                        throw new Exception("Unhandled error");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Unhandled error")
                    RecycleBin.Restore(collection);
            }
        }
        public static void Restore(ICollection<FileSystemInfo> collection)
        {
            try
            {
                if (collection.Count == 0 && collection == null)
                    throw new Exception("Attempted to delete the same file  twice");

                foreach (var fi in collection)
                    if (collection.Count(e => e == fi) != 1)
                        throw new Exception("Attempted to delete the same file  twice");

                if (ShellUtilities.RestoreFromRecycle(collection) == false)
                {
                    using (FileStream restoreFiles = new FileStream("restore_err.txt", FileMode.OpenOrCreate))
                    {
                        byte[] errorHeader = System.Text.Encoding.Default.GetBytes($"{DateTime.Now.ToString()}| Recycle restore " +
                                                                        $"exception\nTry to restore the following file on your own\n->");
                        restoreFiles.Write(errorHeader, 0, errorHeader.Length);
                        foreach (var fi in collection)
                        {
                            byte[] note = System.Text.Encoding.Default.GetBytes($"{fi.FullName}\n");
                            restoreFiles.Write(note, 0, note.Length);
                        }
                      
                        restoreFiles.Close();
                        throw new Exception($"Restore exception try\nSee info in {restoreFiles.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RESTORE ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}