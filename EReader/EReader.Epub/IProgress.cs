using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EReader.Epub
{
    public interface IProgress<T>
    {
        void Report(T Progress, string status);
    }
}
