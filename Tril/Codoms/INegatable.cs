using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Codoms
{
    /// <summary>
    /// To be implemented by a value statement that can be negated
    /// </summary>
    public interface INegatable
    {
        /// <summary>
        /// Negates the statement
        /// </summary>
        void Negate();
    }
}
