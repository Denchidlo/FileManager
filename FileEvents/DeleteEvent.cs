using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class DeleteEvent : IFileChangeEvent
    {
        public ICollection<FileSystemInfo> RestoreCollection { get; set; }
        public void GetBack()
        {
            RecycleBin.Restore(RestoreCollection);
        }
        public void InvokeEvent()
        {
            RecycleBin.MoveToTrash(RestoreCollection);
        }
    }
}
