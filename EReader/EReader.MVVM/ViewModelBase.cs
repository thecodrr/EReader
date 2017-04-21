using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EReader.MVVM
{
    public class ViewModelBase : ObservableObject
    {
        protected Messenger ViewModelMessenger { get { return Messenger.Instance; } }
    }
}
