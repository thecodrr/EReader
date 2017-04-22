using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EReader.Epub;

namespace EReader.Common
{
    public class Progress<T> : Epub.IProgress<T>
    {
        Action<T, string> ProgressAction;
       
        void Epub.IProgress<T>.Report(T Progress, string status)
        {
            ProgressAction.Invoke(Progress, status);
        }

        public Progress(Action<T, string> action)
        {
            ProgressAction = action;
        }
    }
}
