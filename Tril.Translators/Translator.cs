using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tril.Models;

namespace Tril.Translators
{
    public abstract class Translator : ITranslator, IDisposable
    {
        /*
         * raise an event for when translator settings change
         * raise an event for method events/progress
         * raise event for translation progress
         */
        string[] _tgtPlat4ms;
        string[] _tgtTypes;
        string _indent = "";
        bool _optimize = true;
        bool _returnPartial = false;
        bool _useDefaultOnly = false;
        TranslationStartingPoints _startingPt;

        public Translator()
        {
            _tgtPlat4ms = new string[1] { "*" };
            _startingPt = TranslationStartingPoints.FromBundle;
        }

        /// <summary>
        /// A tab for indentation
        /// </summary>
        const string TAB = "\t";

        /// <summary>
        /// Makes a request for a content to be appended as string to a file in the assigned output directory of this translator.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        protected void AppendFileContentAsString(string filePath, string content)
        {
            FileWriteEventArgs eventArgs = new FileWriteEventArgs
                (filePath, content, FileWriteEventType.AppendFileContentAsString);
            if (_fileWriteEvent != null)
                _fileWriteEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for the content of a directory in the assigned output directory of this translator to be cleared.
        /// </summary>
        /// <param name="directoryPath"></param>
        protected void ClearDirectoryContent(string directoryPath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (directoryPath, FileSystemEventType.ClearDirectoryContent);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for the content of a file in the assigned output directory of this translator to be cleared.
        /// </summary>
        /// <param name="filePath"></param>
        protected void ClearFileContent(string filePath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (filePath, FileSystemEventType.ClearFileContent);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for a directory to be created in the assigned output directory of this translator.
        /// </summary>
        /// <param name="directoryPath"></param>
        protected void CreateDirectory(string directoryPath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (directoryPath, FileSystemEventType.CreateDirectory);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for a file to be created in the assigned output directory of this translator.
        /// </summary>
        /// <param name="filePath"></param>
        protected void CreateFile(string filePath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (filePath, FileSystemEventType.CreateFile);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for a directory to be deleted in the assigned output directory of this translator.
        /// </summary>
        /// <param name="directoryPath"></param>
        protected void DeleteDirectory(string directoryPath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (directoryPath, FileSystemEventType.DeleteDirectory);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Makes a request for a file to be deleted in the assigned output directory of this translator.
        /// </summary>
        /// <param name="filePath"></param>
        protected void DeleteFile(string filePath)
        {
            FileSystemEventArgs eventArgs = new FileSystemEventArgs
                (filePath, FileSystemEventType.DeleteFile);
            if (_fileSysEvent != null)
                _fileSysEvent(this, eventArgs);
        }
        /// <summary>
        /// Clean up
        /// </summary>
        public void Dispose()
        {
            _bndlTransed = null;
            _bndlTransing = null;
            _fileSysEvent = null;
            _fileWriteEvent = null;
            _fldTransed = null;
            _fldTransing = null;
            _kindTransed = null;
            _kindTransing = null;
            _mthdTransed = null;
            _mthdTransing = null;
        }
        /// <summary>
        /// Adjusts the indentation to signify that a code block has been entered
        /// </summary>
        protected void EnterBlock()
        {
            //add a tab
            _indent += TAB;
        }
        /// <summary>
        /// Adjusts the indentation to signify that a code block has been exited
        /// </summary>
        protected void ExitBlock()
        {
            //remove a tab
            if (_indent.Length >= TAB.Length)
                _indent = _indent.Substring(0, _indent.Length - TAB.Length);
            else
                _indent = "";
        }
        /// <summary>
        /// Raises the BundleTranslated event
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnBundleTranslated(Bundle bundle, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_bndlTransed != null)
                _bndlTransed(this, new TranslateObjectEventArgs(bundle, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the BundleTranslating event
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnBundleTranslating(Bundle bundle, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_bndlTransing != null)
                _bndlTransing(this, new TranslateObjectEventArgs(bundle, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the EventTranslated event
        /// </summary>
        /// <param name="_event"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnEventTranslated(Event _event, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_evntTransed != null)
                _evntTransed(this, new TranslateObjectEventArgs(_event, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the EventTranslating event
        /// </summary>
        /// <param name="_event"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnEventTranslating(Event _event, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_evntTransing != null)
                _evntTransing(this, new TranslateObjectEventArgs(_event, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the FieldTranslated event
        /// </summary>
        /// <param name="field"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnFieldTranslated(Field field, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_fldTransed != null)
                _fldTransed(this, new TranslateObjectEventArgs(field, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the FieldTranslating event
        /// </summary>
        /// <param name="field"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnFieldTranslating(Field field, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_fldTransing != null)
                _fldTransing(this, new TranslateObjectEventArgs(field, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the KindTranslated event
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnKindTranslated(Kind kind, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_kindTransed != null)
                _kindTransed(this, new TranslateObjectEventArgs(kind, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the KindTranslating event
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnKindTranslating(Kind kind, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_kindTransing != null)
                _kindTransing(this, new TranslateObjectEventArgs(kind, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the MethodTranslated event
        /// </summary>
        /// <param name="method"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnMethodTranslated(Method method, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_mthdTransed != null)
                _mthdTransed(this, new TranslateObjectEventArgs(method, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the MethodTranslatingevent
        /// </summary>
        /// <param name="method"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnMethodTranslating(Method method, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_mthdTransing != null)
                _mthdTransing(this, new TranslateObjectEventArgs(method, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the PackageTranslated event
        /// </summary>
        /// <param name="package"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnPackageTranslated(Package package, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_pckgTransed != null)
                _pckgTransed(this, new TranslateObjectEventArgs(package, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the PackageTranslating event
        /// </summary>
        /// <param name="package"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnPackageTranslating(Package package, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_pckgTransing != null)
                _pckgTransing(this, new TranslateObjectEventArgs(package, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the PropertyTranslated event
        /// </summary>
        /// <param name="property"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnPropertyTranslated(Property property, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_propTransed != null)
                _propTransed(this, new TranslateObjectEventArgs(property, translatedObject, succeeded, cause));
        }
        /// <summary>
        /// Raises the PropertyTranslating event
        /// </summary>
        /// <param name="property"></param>
        /// <param name="translatedObject"></param>
        /// <param name="succeeded"></param>
        /// <param name="cause"></param>
        protected void OnPropertyTranslating(Property property, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (_propTransing != null)
                _propTransing(this, new TranslateObjectEventArgs(property, translatedObject, succeeded, cause));
        }
        ///// <summary>
        ///// Makes a request for a content to be read as string from a file in the assigned output directory of this translator.
        ///// </summary>
        ///// <param name="filePath"></param>
        //protected string ReadFileContentAsString(string filePath)
        //{
        //    FileReadEventArgs eventArgs = new FileReadEventArgs(filePath, FileReadEventType.ReadFileContentAsString);
        //    if (_fileReadEvent == null)
        //        _fileReadEvent = new FileReadEventHandler(Translator_FileReadEvent);
        //    return (string)_fileReadEvent(this, eventArgs);
        //}
        /// <summary>
        /// Resets indentation to its initial state
        /// </summary>
        protected void ResetIndentation()
        {
            _indent = "";
        }
        /// <summary>
        /// Translates a Bundle
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public abstract void TranslateBundle(Bundle bundle);
        /// <summary>
        /// Translates an Event
        /// </summary>
        /// <param name="_event"></param>
        /// <returns></returns>
        public abstract void TranslateEvent(Event _event);
        /// <summary>
        /// Translates a field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public abstract void TranslateField(Field field);
        /// <summary>
        /// Translates a Kind
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public abstract void TranslateKind(Kind kind);
        /// <summary>
        /// Translates a Method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public abstract void TranslateMethod(Method method);
        /// <summary>
        /// Translates a Package
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public abstract void TranslatePackage(Package package);
        /// <summary>
        /// Translates a Property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public abstract void TranslateProperty(Property property);
        /// <summary>
        /// Makes a request for a content to be written as string to a file in the assigned output directory of this translator.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        protected void WriteFileContentAsString(string filePath, string content)
        {
            FileWriteEventArgs eventArgs = new FileWriteEventArgs
                (filePath, content, FileWriteEventType.WriteFileContentAsString);
            if (_fileWriteEvent != null)
                _fileWriteEvent(this, eventArgs);
        }

        /// <summary>
        /// Gets the current indentation
        /// </summary>
        protected string Indentation
        {
            get { return _indent; }
        }
        /// <summary>
        /// Gets or sets a value to determine if the translation should be optimize
        /// </summary>
        public virtual bool Optimize
        {
            get { return _optimize; }
            set { _optimize = value; }
        }
        /// <summary>
        /// Gets or sets a value to determine if the translated codes
        /// of a method are to be returned even when an exception is thrown
        /// in the process of translating the method body.
        /// </summary>
        public virtual bool ReturnPartial
        {
            get { return _returnPartial; }
            set { _returnPartial = value; }
        }
        /// <summary>
        /// Gets or sets the starting point of the translation
        /// </summary>
        public TranslationStartingPoints StartingPoint
        {
            get { return _startingPt; }
            set { _startingPt = value; }
        }
        /// <summary>
        /// Gets or sets the target platforms for this translator.
        /// </summary>
        public virtual string[] TargetPlatforms
        {
            get { return _tgtPlat4ms; }
            set { _tgtPlat4ms = value; }
        }
        /// <summary>
        /// Gets or sets the target types for this translator.
        /// If TargetTypes is not null or empty, only the System.Type objects
        /// that have names matching those in the TargetTypes
        /// will be translated.
        /// If TargetTypes is null or empty, all System.Type objects eligible
        /// for translation (e.g. those that are NOT marked with the 
        /// [HideAttribute] attribute) will be translated
        /// </summary>
        public virtual string[] TargetTypes
        {
            get { return _tgtTypes; }
            set { _tgtTypes = value; }
        }
        /// <summary>
        /// Gets or sets a value to determine if custom Tril attributes are to be ignored.
        /// </summary>
        public virtual bool UseDefaultOnly
        {
            get { return _useDefaultOnly; }
            set { _useDefaultOnly = value; }
        }

        /*events----------------------------------------------*/
        //object Translator_FileReadEvent(Translator sender, FileReadEventArgs eventArgs) { return new object(); }
        //event FileReadEventHandler _fileReadEvent;
        ///// <summary>
        ///// Fired when a request for a file to be read is made.
        ///// </summary>
        //public event FileReadEventHandler FileReadEvent
        //{
        //    add
        //    {
        //        if (_fileReadEvent == null)
        //            _fileReadEvent = new FileReadEventHandler(Translator_FileReadEvent);
        //        _fileReadEvent += value;
        //    }
        //    remove
        //    {
        //        if (_fileReadEvent == null)
        //            _fileReadEvent = new FileReadEventHandler(Translator_FileReadEvent);
        //        _fileReadEvent -= value;
        //    }
        //}

        void Translator_FileSystemEvent(Translator sender, FileSystemEventArgs eventArgs) { }
        event FileSystemEventHandler _fileSysEvent;
        /// <summary>
        /// Fired when a request for a file or directory to be created or deleted is made.
        /// </summary>
        public event FileSystemEventHandler FileSystemEvent
        {
            add
            {
                if (_fileSysEvent == null)
                    _fileSysEvent = new FileSystemEventHandler(Translator_FileSystemEvent);
                _fileSysEvent += value;
            }
            remove
            {
                if (_fileSysEvent == null)
                    _fileSysEvent = new FileSystemEventHandler(Translator_FileSystemEvent);
                _fileSysEvent -= value;
            }
        }

        void Translator_FileWriteEvent(Translator sender, FileWriteEventArgs eventArgs) { }
        event FileWriteEventHandler _fileWriteEvent;
        /// <summary>
        /// Fired when a request to write the content of a file is made.
        /// </summary>
        public event FileWriteEventHandler FileWriteEvent
        {
            add
            {
                if (_fileWriteEvent == null)
                    _fileWriteEvent = new FileWriteEventHandler(Translator_FileWriteEvent);
                _fileWriteEvent += value;
            }
            remove
            {
                if (_fileWriteEvent == null)
                    _fileWriteEvent = new FileWriteEventHandler(Translator_FileWriteEvent);
                _fileWriteEvent -= value;
            }
        }

        //------------------------------------------------------

        void Translator_BundleTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _bndlTransed;
        public event TranslateObjectEventHandler BundleTranslated
        {
            add
            {
                if (_bndlTransed == null)
                    _bndlTransed = new TranslateObjectEventHandler(Translator_BundleTrans);
                _bndlTransed += value;
            }
            remove
            {
                if (_bndlTransed == null)
                    _bndlTransed = new TranslateObjectEventHandler(Translator_BundleTrans);
                _bndlTransed -= value;
            }
        }
        event TranslateObjectEventHandler _bndlTransing;
        public event TranslateObjectEventHandler BundleTranslating
        {
            add
            {
                if (_bndlTransing == null)
                    _bndlTransing = new TranslateObjectEventHandler(Translator_BundleTrans);
                _bndlTransing += value;
            }
            remove
            {
                if (_bndlTransing == null)
                    _bndlTransing = new TranslateObjectEventHandler(Translator_BundleTrans);
                _bndlTransing -= value;
            }
        }

        void Translator_PackageTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _pckgTransed;
        public event TranslateObjectEventHandler PackageTranslated
        {
            add
            {
                if (_pckgTransed == null)
                    _pckgTransed = new TranslateObjectEventHandler(Translator_PackageTrans);
                _pckgTransed += value;
            }
            remove
            {
                if (_pckgTransed == null)
                    _pckgTransed = new TranslateObjectEventHandler(Translator_PackageTrans);
                _pckgTransed -= value;
            }
        }
        event TranslateObjectEventHandler _pckgTransing;
        public event TranslateObjectEventHandler PackageTranslating
        {
            add
            {
                if (_pckgTransing == null)
                    _pckgTransing = new TranslateObjectEventHandler(Translator_PackageTrans);
                _pckgTransing += value;
            }
            remove
            {
                if (_pckgTransing == null)
                    _pckgTransing = new TranslateObjectEventHandler(Translator_PackageTrans);
                _pckgTransing -= value;
            }
        }

        void Translator_KindTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _kindTransed;
        public event TranslateObjectEventHandler KindTranslated
        {
            add
            {
                if (_kindTransed == null)
                    _kindTransed = new TranslateObjectEventHandler(Translator_KindTrans);
                _kindTransed += value;
            }
            remove
            {
                if (_kindTransed == null)
                    _kindTransed = new TranslateObjectEventHandler(Translator_KindTrans);
                _kindTransed -= value;
            }
        }
        event TranslateObjectEventHandler _kindTransing;
        public event TranslateObjectEventHandler KindTranslating
        {
            add
            {
                if (_kindTransing == null)
                    _kindTransing = new TranslateObjectEventHandler(Translator_KindTrans);
                _kindTransing += value;
            }
            remove
            {
                if (_kindTransing == null)
                    _kindTransing = new TranslateObjectEventHandler(Translator_KindTrans);
                _kindTransing -= value;
            }
        }

        void Translator_MethodTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _mthdTransed;
        public event TranslateObjectEventHandler MethodTranslated
        {
            add
            {
                if (_mthdTransed == null)
                    _mthdTransed = new TranslateObjectEventHandler(Translator_MethodTrans);
                _mthdTransed += value;
            }
            remove
            {
                if (_mthdTransed == null)
                    _mthdTransed = new TranslateObjectEventHandler(Translator_MethodTrans);
                _mthdTransed -= value;
            }
        }
        event TranslateObjectEventHandler _mthdTransing;
        public event TranslateObjectEventHandler MethodTranslating
        {
            add
            {
                if (_mthdTransing == null)
                    _mthdTransing = new TranslateObjectEventHandler(Translator_MethodTrans);
                _mthdTransing += value;
            }
            remove
            {
                if (_mthdTransing == null)
                    _mthdTransing = new TranslateObjectEventHandler(Translator_MethodTrans);
                _mthdTransing -= value;
            }
        }

        void Translator_FieldTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _fldTransed;
        public event TranslateObjectEventHandler FieldTranslated
        {
            add
            {
                if (_fldTransed == null)
                    _fldTransed = new TranslateObjectEventHandler(Translator_FieldTrans);
                _fldTransed += value;
            }
            remove
            {
                if (_fldTransed == null)
                    _fldTransed = new TranslateObjectEventHandler(Translator_FieldTrans);
                _fldTransed -= value;
            }
        }
        event TranslateObjectEventHandler _fldTransing;
        public event TranslateObjectEventHandler FieldTranslating
        {
            add
            {
                if (_fldTransing == null)
                    _fldTransing = new TranslateObjectEventHandler(Translator_FieldTrans);
                _fldTransing += value;
            }
            remove
            {
                if (_fldTransing == null)
                    _fldTransing = new TranslateObjectEventHandler(Translator_FieldTrans);
                _fldTransing -= value;
            }
        }

        void Translator_PropTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _propTransed;
        public event TranslateObjectEventHandler PropertyTranslated
        {
            add
            {
                if (_propTransed == null)
                    _propTransed = new TranslateObjectEventHandler(Translator_PropTrans);
                _propTransed += value;
            }
            remove
            {
                if (_propTransed == null)
                    _propTransed = new TranslateObjectEventHandler(Translator_PropTrans);
                _propTransed -= value;
            }
        }
        event TranslateObjectEventHandler _propTransing;
        public event TranslateObjectEventHandler PropertyTranslating
        {
            add
            {
                if (_propTransing == null)
                    _propTransing = new TranslateObjectEventHandler(Translator_PropTrans);
                _propTransing += value;
            }
            remove
            {
                if (_propTransing == null)
                    _propTransing = new TranslateObjectEventHandler(Translator_PropTrans);
                _propTransing -= value;
            }
        }

        void Translator_EventTrans(Translator sender, TranslateObjectEventArgs eventArgs) { }
        event TranslateObjectEventHandler _evntTransed;
        public event TranslateObjectEventHandler EventTranslated
        {
            add
            {
                if (_evntTransed == null)
                    _evntTransed = new TranslateObjectEventHandler(Translator_EventTrans);
                _evntTransed += value;
            }
            remove
            {
                if (_evntTransed == null)
                    _evntTransed = new TranslateObjectEventHandler(Translator_EventTrans);
                _evntTransed -= value;
            }
        }
        event TranslateObjectEventHandler _evntTransing;
        public event TranslateObjectEventHandler EventTranslating
        {
            add
            {
                if (_evntTransing == null)
                    _evntTransing = new TranslateObjectEventHandler(Translator_EventTrans);
                _evntTransing += value;
            }
            remove
            {
                if (_evntTransing == null)
                    _evntTransing = new TranslateObjectEventHandler(Translator_EventTrans);
                _evntTransing -= value;
            }
        }
    }

    /// <summary>
    /// Lists the possible starting points of the translation
    /// </summary>
    public enum TranslationStartingPoints
    {
        /// <summary>
        /// from Bundle
        /// </summary>
        FromBundle,
        /// <summary>
        /// from event
        /// </summary>
        FromEvent,
        /// <summary>
        /// from Field
        /// </summary>
        FromField,
        /// <summary>
        /// from Kind
        /// </summary>
        FromKind,
        /// <summary>
        /// from Method
        /// </summary>
        FromMethod,
        /// <summary>
        /// from Package
        /// </summary>
        FromPackage,
        /// <summary>
        /// from Property
        /// </summary>
        FromProperty
    }

    /// <summary>
    /// A delegate to a method that handles translator events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public delegate void TranslatorEventHandler(Translator sender, TranslatorEventArgs eventArgs);
    /// <summary>
    /// Contains translator event data.
    /// </summary>
    public class TranslatorEventArgs : EventArgs { }

    //*********************************************************************************************************************************

    /// <summary>
    /// A delegate to a method that handles translate object events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public delegate void TranslateObjectEventHandler(Translator sender, TranslateObjectEventArgs eventArgs);
    /// <summary>
    /// Contains translate object event data.
    /// </summary>
    public class TranslateObjectEventArgs : TranslatorEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceObject">the object to translate</param>
        /// <param name="translatedObject">the output of the translation (null if no output was reported)</param>
        /// <param name="succeeded">true if the translation succeeded with no errors</param>
        /// <param name="cause">the cause of the failure (null if no cause or if translation succeeded)</param>
        public TranslateObjectEventArgs(object sourceObject, object translatedObject = null, bool succeeded = true, Exception cause = null)
        {
            if (sourceObject == null)
                throw new NullReferenceException("Source object cannot be null!");

            SourceObject = sourceObject;
            Succeeded = succeeded;
            FailureCause = cause;
            TranslatedObject = translatedObject;
        }

        public object SourceObject { get; private set; }
        public bool Succeeded { get; private set; }
        public object TranslatedObject { get; private set; }
        public Exception FailureCause { get; private set; }
    }

    //*********************************************************************************************************************************

    /// <summary>
    /// Contains translator file event data.
    /// </summary>
    public class FileEventArgs : TranslatorEventArgs
    {
        /// <summary>
        /// Creates a new instance of Tril.FileEventArgs
        /// </summary>
        /// <param name="path">the name of the file or directory to act upon</param>
        public FileEventArgs(string path)
        {
            if (path == null /*|| path.Trim() == ""*/)
                throw new NullReferenceException(this.GetType().FullName + ": Name of file/directory cannot be null!");// or empty!");
            Path = path;
        }

        /// <summary>
        /// Gets the name of the file or directory to act upon
        /// </summary>
        public string Path { get; private set; }
    }

    ///// <summary>
    ///// A delegate to a method that handles translator file read events
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="eventArgs"></param>
    //public delegate object FileReadEventHandler(Translator sender, FileReadEventArgs eventArgs);
    ///// <summary>
    ///// Contains translator file read event data.
    ///// </summary>
    //public class FileReadEventArgs : FileEventArgs
    //{
    //    /// <summary>
    //    /// Creates a new instance of Tril.FileReadEventArgs
    //    /// </summary>
    //    /// <param name="path">the name of the file or directory to act upon</param>
    //    /// <param name="eventType">the task/action to be taken on the specified file or directory</param>
    //    public FileReadEventArgs(string path, FileReadEventType eventType)
    //        : base(path)
    //    {
    //        ReadEventType = eventType;
    //    }

    //    /// <summary>
    //    /// Gets the event type (the task to perform)
    //    /// </summary>
    //    public FileReadEventType ReadEventType { get; private set; }
    //}
    ///// <summary>
    ///// List of file read event types
    ///// </summary>
    //public enum FileReadEventType
    //{
    //    /// <summary>
    //    /// Read file content as string
    //    /// </summary>
    //    ReadFileContentAsString
    //}

    /// <summary>
    /// A delegate to a method that handles translator file system events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public delegate void FileSystemEventHandler(Translator sender, FileSystemEventArgs eventArgs);
    /// <summary>
    /// Contains translator file system event data.
    /// </summary>
    public class FileSystemEventArgs : FileEventArgs
    {
        /// <summary>
        /// Creates a new instance of Tril.TranslatorFileSystemEventArgs
        /// </summary>
        /// <param name="path">the name of the file or directory to act upon</param>
        /// <param name="eventType">the task/action to be taken on the specified file or directory</param>
        public FileSystemEventArgs(string path, FileSystemEventType eventType)
            : base(path)
        {
            EventType = eventType;
        }

        /// <summary>
        /// Gets the event type (the task to perform)
        /// </summary>
        public FileSystemEventType EventType { get; private set; }
    }
    /// <summary>
    /// List of file system event types
    /// </summary>
    public enum FileSystemEventType
    {
        /// <summary>
        /// Clear directory content
        /// </summary>
        ClearDirectoryContent,
        /// <summary>
        /// Clear file content
        /// </summary>
        ClearFileContent,
        /// <summary>
        /// Create directory
        /// </summary>
        CreateDirectory,
        /// <summary>
        /// Create file
        /// </summary>
        CreateFile,
        /// <summary>
        /// Delete directory
        /// </summary>
        DeleteDirectory,
        /// <summary>
        /// Delete file
        /// </summary>
        DeleteFile
    }

    /// <summary>
    /// A delegate to a method that handles translator file write events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public delegate void FileWriteEventHandler(Translator sender, FileWriteEventArgs eventArgs);
    /// <summary>
    /// Contains translator file write event data.
    /// </summary>
    public class FileWriteEventArgs : FileEventArgs
    {
        /// <summary>
        /// Creates a new instance of Tril.FileWriteEventArgs
        /// </summary>
        /// <param name="path">the name of the file or directory to act upon</param>
        /// <param name="content">the content of the file to write or append</param>
        /// <param name="eventType">the task/action to be taken on the specified file or directory</param>
        public FileWriteEventArgs(string path, object content, FileWriteEventType eventType)
            : base(path)
        {
            if (content == null)
                throw new NullReferenceException(this.GetType().FullName + ": Content of file cannot be null!");
            Content = content;
            WriteEventType = eventType;
        }

        /// <summary>
        /// Gets the content of the file to write or append
        /// </summary>
        public object Content { get; private set; }
        /// <summary>
        /// Gets the event type (the task to perform)
        /// </summary>
        public FileWriteEventType WriteEventType { get; private set; }
    }
    /// <summary>
    /// List of file write event types
    /// </summary>
    public enum FileWriteEventType
    {
        /// <summary>
        /// Append file content as string
        /// </summary>
        AppendFileContentAsString,
        /// <summary>
        /// Write file content as string
        /// </summary>
        WriteFileContentAsString
    }
}
