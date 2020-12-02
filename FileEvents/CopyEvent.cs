using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.FileEvents
{
    class CopyEvent : IFileChangeEvent
    {
        public List<FileSystemInfo> CopyCollection;

        public DirectoryInfo newPath;

        public void GetBack()
        {

        }

        public void InvokeEvent()
        {
            foreach (var fi in CopyCollection)
            {
                if (fi is DirectoryInfo)
                {
                    CopyEvent.CopyDir(fi.FullName, newPath + "\\" + fi.Name);
                }
            }
        }

        static void CopyDir(string FromDir, string ToDir)
        {
            Directory.CreateDirectory(ToDir);
            foreach (string s1 in Directory.GetFiles(FromDir))
            {
                string s2 = ToDir + "\\" + Path.GetFileName(s1);
                File.Copy(s1, s2);
            }
            foreach (string s in Directory.GetDirectories(FromDir))
            {
                CopyDir(s, ToDir + "\\" + Path.GetFileName(s));
            }
        }
    }
}
