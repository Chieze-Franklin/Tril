using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.App
{
    [Serializable]
    public class AppSettings
    {
        string _translatorsDir;

        public AppSettings() { }

        /// <summary>
        /// Gets or sets the "Translators" directory
        /// </summary>
        public string TranslatorsDirectory
        {
            get { return _translatorsDir; }
            set { _translatorsDir = value; }
        }
    }
}
