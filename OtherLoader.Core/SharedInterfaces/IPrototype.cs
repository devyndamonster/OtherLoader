using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.SharedInterfaces
{
    public interface IPrototype<T>
    {
        public T Clone();
    }
}
