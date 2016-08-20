using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tril.Codoms;

namespace Tril.Delegates
{
    /// <summary>
    /// Represents a method that translates a Codom
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public delegate string CodomTranslator(Codom code);
}
