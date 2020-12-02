using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.FileEvents
{
    class RenameEvent : IFileChangeEvent
    {
        FileSystemInfo target;
        string newFileName;
        string oldFileName;
        public RenameEvent(FileSystemInfo target, string newFileName)
        {
            this.target = target;
            this.newFileName = newFileName;
            oldFileName = target.Name;
        }
        public void GetBack()
        {
            string temp = oldFileName;
            oldFileName = newFileName;
            newFileName = temp;
            try
            {
                if (target == null)
                    throw new Exception("Null target");

                if (target is DirectoryInfo)
                {
                    string path = ((DirectoryInfo)target).Parent.FullName + '\\' + newFileName;
                    if (!Directory.Exists(path))
                        ((DirectoryInfo)target).MoveTo(path);
                    else
                        throw new Exception("Directory already exists");
                }
                else if (target is FileInfo)
                {
                    string path = ((FileInfo)target).DirectoryName + '\\' + newFileName;
                    if (!File.Exists(path))
                        ((FileInfo)target).MoveTo(path);
                    else
                        throw new Exception("File already exists");
                }
                else
                    throw new Exception("Unable to convert from FileSystemInfo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke RenameEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void InvokeEvent()
        {
            try
            {
                if (target == null)
                    throw new Exception("Null target");

                if (target is DirectoryInfo)
                {
                    string path = ((DirectoryInfo)target).Parent.FullName + '\\' + newFileName;
                    if (!Directory.Exists(path))
                        ((DirectoryInfo)target).MoveTo(path);
                    else
                        throw new Exception("Directory already exists");
                }
                else if (target is FileInfo)
                {
                    string path = ((FileInfo)target).DirectoryName + '\\' + newFileName + ((FileInfo)target).Extension;
                    if (!File.Exists(path))
                    {
                        try
                        {
                            ((FileInfo)target).MoveTo(path);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                        throw new Exception("File already exists");
                }
                else
                    throw new Exception("Unable to convert from FileSystemInfo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message , "Unable to invoke RenameEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
