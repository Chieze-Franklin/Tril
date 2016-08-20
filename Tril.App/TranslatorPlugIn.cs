using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;

namespace Tril.App
{
    [Serializable]
    public class TranslatorPlugIn
    {
        public TranslatorPlugIn()
        {
            Version = "1.0.0.0";
        }

        /// <summary>
        /// Gets or sets the class name of the target translator
        /// </summary>
        public string ClassName;
        /// <summary>
        /// Gets or sets the company
        /// </summary>
        [OptionalField]
        public Company Company;
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description;
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// Gets or sets the path of the containing dll file, relative to this translator pointer
        /// </summary>
        public string AssemblyPath;
        /// <summary>
        /// Gets or sets the version of the translator
        /// </summary>
        public string TranslatorVersion;
        /// <summary>
        /// Gets or sets the version of this object
        /// </summary>
        [XmlAttribute]
        public string Version;
    }

    [Serializable]
    public class Company
    {
        public Company() { }

        /// <summary>
        /// Gets or sets the copyright
        /// </summary>
        [OptionalField]
        public string Copyright;
        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        public string Name;
    }
}
