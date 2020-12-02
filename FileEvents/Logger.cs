using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class Logger
    {
        public Stack<IFileChangeEvent> fileChanges;
        public Logger()
        {
            fileChanges = new Stack<IFileChangeEvent>();
        }
        public void CancelLastChange()
        {
            if (fileChanges.Count != 0) 
            {
                IFileChangeEvent fileChangeEvent = fileChanges.Pop();
                fileChangeEvent.GetBack();
            }
        }
        public void CancelLastChange(int count)
        {
            if (count > 0)
            {
                while (count != 0 && fileChanges.Count != 0)
                {
                    fileChanges.Pop().GetBack();
                    --count;
                }
            }
        }
    }
}
