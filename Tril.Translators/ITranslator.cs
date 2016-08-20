using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tril.Models;

namespace Tril.Translators
{
    /// <summary>
    /// Base interface for all translators
    /// </summary>
    public interface ITranslator
    {
        void TranslateBundle(Bundle bundle);
        void TranslateEvent(Event _event);
        void TranslateField(Field field);
        //InnerKind
        void TranslateKind(Kind kind);
        void TranslateMethod(Method method);
        void TranslatePackage(Package package);
        void TranslateProperty(Property property);
    }
}
