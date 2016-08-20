using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;

namespace Tril.App
{
    /// <summary>
    /// Represents a translation
    /// </summary>
    [Serializable]
    public class Translation
    {
        public Translation()
        {
            Interest = TranslationInterest.Errors;
            Optimize = true;
            TargetPlatforms = new string[] { "*" };
        }

        /// <summary>
        /// Gets or sets the output event type to monitor.
        /// </summary>
        public TranslationInterest Interest;
        /// <summary>
        /// Gets or sets the name of the translation.
        /// </summary>
        public string Name;
        /// <summary>
        /// Gets or sets the path of the namesake assembly.
        /// </summary>
        [OptionalField]
        public string NamesakeAssembly;
        /// <summary>
        /// Gets or sets a value to determine if the translated code should be optimized.
        /// </summary>
        public bool Optimize;
        /// <summary>
        /// Gets or sets the directory to which the translated files are to be output.
        /// </summary>
        public string OutputDirectory;
        /// <summary>
        /// Gets or sets a value to determine if the translated codes should still be
        /// output for methods that generared errors.
        /// </summary>
        public bool ReturnPartial;
        /// <summary>
        /// Gets or sets the path to the source assembly.
        /// </summary>
        public string SourceAssembly;
        /// <summary>
        /// Gets or sets an array of the target platforms for the translator.
        /// </summary>
        public string[] TargetPlatforms;
        /// <summary>
        /// Gets or sets an array of the target types for the translator.
        /// </summary>
        public string[] TargetTypes;
        /// <summary>
        /// Gets or sets the path to the .tp file.
        /// </summary>
        public string TranslatorPlugIn;
        /// <summary>
        /// Gets or sets a value to determine if custom Tril attributes are to be ignored.
        /// </summary>
        public bool UseDefaultOnly;
    }

    public enum TranslationInterest
    {
        None,
        All,
        Doing,
        Done,
        Errors
    }
}
