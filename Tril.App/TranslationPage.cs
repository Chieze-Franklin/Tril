using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

using Mono.Cecil;

using Tril.Attributes;
using Tril.Models;
using Tril.Translators;
using System.Xml;

namespace Tril.App
{
    public partial class TranslationPage : TabPage
    {
        SplitContainer splitCont;
        PropertyPanel propPanel;
        BackgroundWorker worker;
        TranslateEventViewer eventViewer;
        string lastExMsg = "";

        const int ERROR = -1;
        const int BDL_TING = 0;
        const int BDL_TED = 1;
        const int PKG_TING = 2;
        const int PKG_TED = 3;
        const int KND_TING = 4;
        const int KND_TED = 5;
        const int FLD_TING = 6;
        const int FLD_TED = 7;
        const int MTD_TING = 8;
        const int MTD_TED = 9;
        const int PPT_TING = 10;
        const int PPT_TED = 11;
        const int EVT_TING = 12;
        const int EVT_TED = 13;

        //public TranslationPage()
        //    : this(null) { }
        public TranslationPage(string filePath = null)
        {
           InitializeComponent();
           
           CurrentTranslation = new Translation();
           FilePath = filePath;
           IsDirty = false;

           //propPanel
           propPanel = new PropertyPanel();
           propPanel.Dock = DockStyle.Fill;
           propPanel.TranslationName.TextChanged += TranslationName_TextChanged;
           propPanel.ListenForAll.CheckedChanged += ListenForAll_CheckedChanged;
           propPanel.ListenForDoing.CheckedChanged += ListenForDoing_CheckedChanged;
           propPanel.ListenForDone.CheckedChanged += ListenForDone_CheckedChanged;
           propPanel.ListenForErrors.CheckedChanged += ListenForErrors_CheckedChanged;
           propPanel.ListenForNone.CheckedChanged += ListenForNone_CheckedChanged;
           propPanel.OutputDirectory.TextChanged += OutputDirectory_TextChanged;
           propPanel.NamesakeAssembly.TextChanged += NamesakeAssembly_TextChanged;
           propPanel.TranslatorPlugIn.TextChanged += TranslatorPlugIn_TextChanged;
           propPanel.Optimize.CheckedChanged += Optimize_CheckedChanged;
           propPanel.ReturnPartial.CheckedChanged += ReturnPartial_CheckedChanged;
           propPanel.UseDefaultOnly.CheckedChanged += UseDefaultOnly_CheckedChanged;
           propPanel.TargetLanguages.ListChanged += TargetPlatforms_ListChanged;
           propPanel.SourceAssembly.TextChanged += SourceAssembly_TextChanged;
           propPanel.TargetTypes.ListChanged += TargetTypes_ListChanged;

           //eventViewer
           eventViewer = new TranslateEventViewer();
           eventViewer.Dock = DockStyle.Fill;

           //splitCont
           splitCont = new SplitContainer();
           splitCont.Dock = DockStyle.Fill;
           //splitCont.IsSplitterFixed = true;

           //splitCont.BackColor = Color.Black;
           //splitCont.Panel1.BackColor = splitCont.Panel2.BackColor = this.BackColor;
           splitCont.Panel1.Controls.Add(propPanel);
           splitCont.Panel2.Controls.Add(eventViewer);

           //this
           this.Controls.Add(splitCont);
           this.Text = "New Translation";
           this.Paint += TranslationPage_Paint;

           LoadTranslation();
        }

        void ListenForAll_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                if (propPanel.ListenForAll.Checked)
                    CurrentTranslation.Interest = TranslationInterest.All;
            }
            IsDirty = true;
        }
        void ListenForDoing_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                if (propPanel.ListenForDoing.Checked)
                    CurrentTranslation.Interest = TranslationInterest.Doing;
            }
            IsDirty = true;
        }
        void ListenForDone_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                if (propPanel.ListenForDone.Checked)
                    CurrentTranslation.Interest = TranslationInterest.Done;
            }
            IsDirty = true;
        }
        void ListenForErrors_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                if (propPanel.ListenForErrors.Checked)
                    CurrentTranslation.Interest = TranslationInterest.Errors;
            }
            IsDirty = true;
        }
        void ListenForNone_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                if (propPanel.ListenForNone.Checked)
                    CurrentTranslation.Interest = TranslationInterest.None;
            }
            IsDirty = true;
        }
        void Optimize_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.Optimize = propPanel.Optimize.Checked;
            }
            IsDirty = true;
        }
        void ReturnPartial_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.ReturnPartial = propPanel.ReturnPartial.Checked;
            }
            IsDirty = true;
        }
        void UseDefaultOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.UseDefaultOnly = propPanel.UseDefaultOnly.Checked;
            }
            IsDirty = true;
        }
        void OutputDirectory_TextChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.OutputDirectory = propPanel.OutputDirectory.Text.Trim();
            }
            IsDirty = true;
        }
        void NamesakeAssembly_TextChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.NamesakeAssembly = propPanel.NamesakeAssembly.Text.Trim();
            }
            IsDirty = true;
        }
        void SourceAssembly_TextChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.SourceAssembly = propPanel.SourceAssembly.Text.Trim();
            }
            IsDirty = true;
        }
        void TargetPlatforms_ListChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                string[] nonNullList = propPanel.TargetLanguages.GetList().Where(s => s != null).ToArray();
                CurrentTranslation.TargetPlatforms = nonNullList;
            }
            IsDirty = true;
        }
        void TargetTypes_ListChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                string[] nonNullList = propPanel.TargetTypes.GetList().Where(s => s != null).ToArray();
                CurrentTranslation.TargetTypes = nonNullList;
            }
            IsDirty = true;
        }
        void TranslationName_TextChanged(object sender, EventArgs e)
        {
            if (CurrentTranslation != null)
            {
                CurrentTranslation.Name = propPanel.TranslationName.Text.Trim();
            }
            IsDirty = true;
        }
        void TranslatorPlugIn_TextChanged(object sender, EventArgs e)
        {
            string transPluginPath = propPanel.TranslatorPlugIn.Text.Trim();
            if (CurrentTranslation != null)
            {
                CurrentTranslation.TranslatorPlugIn = transPluginPath;
            }
            try
            {
                string absTransPluginpath = FileSystemServices.GetAbsolutePath(transPluginPath, FilePath);
                TranslatorPlugIn plugin = (TranslatorPlugIn)FileSystemServices.DeserializeFromXml
                    (absTransPluginpath, typeof(TranslatorPlugIn), null);
                if (plugin.DisplayName != null && plugin.Description != null)
                    propPanel.PlugInBoard.DocumentText = "<h3>" + plugin.DisplayName + "</h3>" + plugin.Description;
                else
                    propPanel.PlugInBoard.DocumentText = "<h3>Translator plugin loaded!</h3>"
                        + Convert.ToChar(9786).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9787).ToString()
                        + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9788).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
                        + Convert.ToChar(9829).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9835).ToString()
                        + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9834).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
                        + Convert.ToChar(9827).ToString();
            }
            catch
            {
                propPanel.PlugInBoard.DocumentText = "<h3>Translator plugin could not be loaded!</h3>"
                        + Convert.ToChar(1160).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9618).ToString()
                        + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9608).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
                        + Convert.ToChar(709).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(685).ToString()
                        + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(182).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
                        + Convert.ToChar(9660).ToString();
            }
            IsDirty = true;
        }
        void translator_FileSystemEvent(Translator sender, Translators.FileSystemEventArgs eventArgs)
        {
            if (eventArgs.Path != null && eventArgs.Path.Trim() != "" && CurrentTranslation != null && CurrentTranslation.OutputDirectory != null)
            {
                try
                {
                    //output dir path is relative wrt translation file path
                    string absPath = FileSystemServices.GetAbsolutePath(CurrentTranslation.OutputDirectory, FilePath).TrimEnd('\\')
                        + "\\" + eventArgs.Path.TrimStart('\\');
                    if (eventArgs.EventType == FileSystemEventType.ClearDirectoryContent)
                    {
                        FileSystemServices.CreateDirectory(absPath, true);
                    }
                    else if (eventArgs.EventType == FileSystemEventType.ClearFileContent)
                    {
                        FileSystemServices.CreateFile(absPath, true, "");
                    }
                    else if (eventArgs.EventType == FileSystemEventType.CreateDirectory)
                    {
                        FileSystemServices.CreateDirectory(absPath, false);
                    }
                    else if (eventArgs.EventType == FileSystemEventType.CreateFile)
                    {
                        FileSystemServices.CreateFile(absPath, false, "");
                    }
                    else if (eventArgs.EventType == FileSystemEventType.DeleteDirectory)
                    {
                        if (Directory.Exists(absPath))
                        {
                            Directory.Delete(absPath);
                        }
                    }
                    else if (eventArgs.EventType == FileSystemEventType.DeleteFile)
                    {
                        if (File.Exists(absPath))
                        {
                            File.Delete(absPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessageBox(ex, ex.Message + "\nIt is possible the output file or folder is open or being used by another process.");
                }
            }
        }
        void translator_FileWriteEvent(Translator sender, FileWriteEventArgs eventArgs)
        {
            if (eventArgs.Content != null && eventArgs.Path != null && eventArgs.Path.Trim() != "" 
                && CurrentTranslation != null && CurrentTranslation.OutputDirectory != null)
            {
                try
                {
                    //output dir path is relative wrt translation file path
                    string absFilePath = FileSystemServices.GetAbsolutePath(CurrentTranslation.OutputDirectory, FilePath).TrimEnd('\\')
                        + "\\" + eventArgs.Path.TrimStart('\\');
                    if (eventArgs.WriteEventType == FileWriteEventType.AppendFileContentAsString)
                    {
                        FileSystemServices.AppendTextToFile(absFilePath, eventArgs.Content.ToString());
                    }
                    else if (eventArgs.WriteEventType == FileWriteEventType.WriteFileContentAsString)
                    {
                        FileSystemServices.WriteTextToFile(absFilePath, eventArgs.Content.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessageBox(ex, ex.Message + "\nIt is possible the output file is open or being used by another process.");
                }
            }
        }
        void translator_BundleTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(BDL_TING, array);
                }
                else
                {
                    worker.ReportProgress(BDL_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_BundleTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(BDL_TED, array);
                }
                else
                {
                    worker.ReportProgress(BDL_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_EventTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(EVT_TING, array);
                }
                else
                {
                    worker.ReportProgress(EVT_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_EventTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(EVT_TED, array);
                }
                else
                {
                    worker.ReportProgress(EVT_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_FieldTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(FLD_TING, array);
                }
                else
                {
                    worker.ReportProgress(FLD_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_FieldTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(FLD_TED, array);
                }
                else
                {
                    worker.ReportProgress(FLD_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_KindTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(KND_TING, array);
                }
                else
                {
                    worker.ReportProgress(KND_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_KindTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(KND_TED, array);
                }
                else
                {
                    worker.ReportProgress(KND_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_MethodTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(MTD_TING, array);
                }
                else
                {
                    worker.ReportProgress(MTD_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_MethodTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(MTD_TED, array);
                }
                else
                {
                    worker.ReportProgress(MTD_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_PackageTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(PKG_TING, array);
                }
                else
                {
                    worker.ReportProgress(PKG_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_PackageTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(PKG_TED, array);
                }
                else
                {
                    worker.ReportProgress(PKG_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_PropertyTranslating(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(PPT_TING, array);
                }
                else
                {
                    worker.ReportProgress(PPT_TING, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void translator_PropertyTranslated(Translator sender, TranslateObjectEventArgs eventArgs)
        {
            try
            {
                object[] array = new object[] { sender, eventArgs };
                if (CurrentTranslation.Interest == TranslationInterest.Errors)
                {
                    if (!eventArgs.Succeeded)
                        worker.ReportProgress(PPT_TED, array);
                }
                else
                {
                    worker.ReportProgress(PPT_TED, array);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex, ex.Message);
            }
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is object[])
            {
                object[] array = (e.UserState as object[]);
                if (array.Length == 2)
                {
                    if (array[0] is Translator && array[1] is TranslateObjectEventArgs)
                    {
                        string displayText = "";

                        if (e.ProgressPercentage == BDL_TING)
                            displayText = "Translating Bundle";
                        else if (e.ProgressPercentage == BDL_TED)
                            displayText = "Translated  Bundle";
                        else if (e.ProgressPercentage == PKG_TING)
                            displayText = "Translating Package";
                        else if (e.ProgressPercentage == PKG_TED)
                            displayText = "Translated  Package";
                        else if (e.ProgressPercentage == KND_TING)
                            displayText = "Translating Kind";
                        else if (e.ProgressPercentage == KND_TED)
                            displayText = "Translated  Kind";
                        else if (e.ProgressPercentage == FLD_TING)
                            displayText = "Translating Field";
                        else if (e.ProgressPercentage == FLD_TED)
                            displayText = "Translated  Field";
                        else if (e.ProgressPercentage == MTD_TING)
                            displayText = "Translating Method";
                        else if (e.ProgressPercentage == MTD_TED)
                            displayText = "Translated  Method";
                        else if (e.ProgressPercentage == PPT_TING)
                            displayText = "Translating Property";
                        else if (e.ProgressPercentage == PPT_TED)
                            displayText = "Translated  Property";
                        else if (e.ProgressPercentage == EVT_TING)
                            displayText = "Translating Event";
                        else if (e.ProgressPercentage == EVT_TED)
                            displayText = "Translated  Event";

                        eventViewer.AddEvent((array[0] as Translator), (array[1] as TranslateObjectEventArgs), displayText);
                    }
                }
            }
            else if (e.UserState is Exception)
            {
                ShowErrorMessageBox((e.UserState as Exception), (e.UserState as Exception).Message);
            }
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_runComp != null)
                _runComp(this, new EventArgs());
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Run();
        }
        void TranslationPage_Paint(object sender, EventArgs e)
        {
            splitCont.SplitterDistance = this.Width / 5;
        }

        void LoadTranslation()
        {
            if (FilePath != null)
            {
                try
                {
                    CurrentTranslation = (Translation)FileSystemServices.DeserializeFromXml(FilePath, typeof(Translation), null);
                }
                catch
                {
                    CurrentTranslation = new Translation();
                }
            }
            try
            {
                if (CurrentTranslation.Name != null)
                {
                    this.Text = CurrentTranslation.Name.Trim();
                    propPanel.TranslationName.Text = CurrentTranslation.Name.Trim();
                }
                if (CurrentTranslation.Interest == TranslationInterest.All)
                    propPanel.ListenForAll.Checked = true;
                else if (CurrentTranslation.Interest == TranslationInterest.Doing)
                    propPanel.ListenForDoing.Checked = true;
                else if (CurrentTranslation.Interest == TranslationInterest.Done)
                    propPanel.ListenForDone.Checked = true;
                else if (CurrentTranslation.Interest == TranslationInterest.Errors)
                    propPanel.ListenForErrors.Checked = true;
                else if (CurrentTranslation.Interest == TranslationInterest.None)
                    propPanel.ListenForNone.Checked = true;
                if (CurrentTranslation.OutputDirectory != null)
                {
                    propPanel.OutputDirectory.Text = CurrentTranslation.OutputDirectory.Trim();
                }
                if (CurrentTranslation.NamesakeAssembly != null)
                {
                    propPanel.NamesakeAssembly.Text = CurrentTranslation.NamesakeAssembly.Trim();
                }
                if (CurrentTranslation.TranslatorPlugIn != null)
                {
                    propPanel.TranslatorPlugIn.Text = CurrentTranslation.TranslatorPlugIn.Trim();
                }
                propPanel.Optimize.Checked = CurrentTranslation.Optimize;
                propPanel.ReturnPartial.Checked = CurrentTranslation.ReturnPartial;
                propPanel.UseDefaultOnly.Checked = CurrentTranslation.UseDefaultOnly;
                if (CurrentTranslation.TargetPlatforms != null)
                {
                    string[] nonNullTgtLangs = CurrentTranslation.TargetPlatforms.Where(s => s != null).ToArray();
                    propPanel.TargetLanguages.SetList(nonNullTgtLangs);
                }
                if (CurrentTranslation.SourceAssembly != null)
                {
                    propPanel.SourceAssembly.Text = CurrentTranslation.SourceAssembly.Trim();
                }
                if (CurrentTranslation.TargetTypes != null)
                {
                    string[] nonNullTgtTypes = CurrentTranslation.TargetTypes.Where(s => s != null).ToArray();
                    propPanel.TargetTypes.SetList(nonNullTgtTypes);
                }

                IsDirty = false;
            }
            catch
            {
            }
        }

        public bool CanSave()
        {
            return FilePath != null && CurrentTranslation != null;
        }
        public void Close()
        {
            if (this.Parent != null && this.Parent is TabControl)
            {
                (this.Parent as TabControl).Controls.Remove(this);
                this.Dispose(true);
            }
        }
        /// <summary>
        /// The actual translation is "invoked" here
        /// </summary>
        void Run()
        {
            /*
             * get the bundle from CurrentTranslation.SourcePath
             * get the translator
             * set appropriate options
             * hook up to appropriate events (determined by CurrentTranslation.Interest)
             * Start a new thread
             * In that thread, call translator.TranslateBundle(bundle)
             */

            try
            {
                if (CurrentTranslation != null)
                {
                    if (CurrentTranslation.SourceAssembly == null)
                        throw new NullReferenceException("Source code path is not specified!");
                    if (CurrentTranslation.TranslatorPlugIn == null)
                        throw new NullReferenceException("Translator pluggin path is not specified!");

                    //get the translator
                    //translator plugin path is relative wrt translation file path
                    string absTransPluginpath = FileSystemServices.GetAbsolutePath(CurrentTranslation.TranslatorPlugIn, FilePath);
                    TranslatorPlugIn plugin = (TranslatorPlugIn)FileSystemServices.DeserializeFromXml
                        (absTransPluginpath, typeof(TranslatorPlugIn), null);
                    if (plugin.AssemblyPath == null)
                        throw new NullReferenceException("The relative path of the translator containing assembly cannot be found!");
                    if (plugin.ClassName == null)
                        throw new NullReferenceException("The class name of the translator cannot be found!");
                    //assembly path is relative wrt translator plugin path
                    string asmPath = FileSystemServices.GetAbsolutePath(plugin.AssemblyPath, absTransPluginpath);

                    //*****************LOADING THE ASSEMBLY***************************************
                    AppDomain domain = AppDomain.CurrentDomain;//.CreateDomain("MyDomain");
                    Assembly assembly = domain.Load(AssemblyName.GetAssemblyName(asmPath));
                    //Assembly assembly = domain.GetAssemblies().First(a => a.CodeBase.Trim().ToLower() == fullPath.Trim().ToLower());

                    //Assembly assembly = Assembly.LoadFrom(asmPath);
                    //***************************************************************************

                    Type translatorType = assembly.GetType(plugin.ClassName, true);
                    Translator translator = (Translator)Activator.CreateInstance(translatorType);

                    //set the namesake assembly path
                    //namesake assembly path is relative wrt translation file path
                    if (CurrentTranslation.NamesakeAssembly != null && CurrentTranslation.NamesakeAssembly.Trim() != "")
                        Model.SetNamesakeAssemblyPath(FileSystemServices.GetAbsolutePath(CurrentTranslation.NamesakeAssembly, FilePath));
                    //get the bundle
                    //source assembly path is relative wrt translation file path
                    Bundle bundle = new Bundle(FileSystemServices.GetAbsolutePath(CurrentTranslation.SourceAssembly, FilePath));
                    #region //was supposed to help solve the "Failed to resolve assembly..." exception being thrown by TypeReference.Resolve()
                    ////load specified assembly refs
                    //var bundleRefsFile = FileSystemServices.GetAbsolutePath(@"a.r", bundle.Location);
                    //if (File.Exists(bundleRefsFile))
                    //{
                    //    string[] asmRefs = File.ReadAllLines(bundleRefsFile).Where(ar => !string.IsNullOrWhiteSpace(ar))
                    //        .Select(ar => FileSystemServices.GetAbsolutePath(ar, bundleRefsFile))
                    //        .Where(ar => ar != asmPath).ToArray();
                    //    foreach (string asmref in asmRefs)
                    //    {
                    //        domain.Load(AssemblyName.GetAssemblyName(asmref));
                    //    }
                    //}
                    #endregion

                    //set appropriate options
                    translator.Optimize = CurrentTranslation.Optimize;
                    translator.ReturnPartial = CurrentTranslation.ReturnPartial;
                    translator.TargetPlatforms = CurrentTranslation.TargetPlatforms;
                    translator.TargetTypes = CurrentTranslation.TargetTypes;
                    translator.UseDefaultOnly = CurrentTranslation.UseDefaultOnly;

                    //hook up to events
                    translator.FileSystemEvent += translator_FileSystemEvent;
                    translator.FileWriteEvent += translator_FileWriteEvent;
                    if (CurrentTranslation.Interest == TranslationInterest.All || CurrentTranslation.Interest == TranslationInterest.Errors)
                    {
                        translator.BundleTranslating += translator_BundleTranslating;
                        translator.BundleTranslated += translator_BundleTranslated;
                        translator.PackageTranslating += translator_PackageTranslating;
                        translator.PackageTranslated += translator_PackageTranslated;
                        translator.KindTranslating += translator_KindTranslating;
                        translator.KindTranslated += translator_KindTranslated;
                        translator.FieldTranslating += translator_FieldTranslating;
                        translator.FieldTranslated += translator_FieldTranslated;
                        translator.MethodTranslating += translator_MethodTranslating;
                        translator.MethodTranslated += translator_MethodTranslated;
                        translator.PropertyTranslating += translator_PropertyTranslating;
                        translator.PropertyTranslated += translator_PropertyTranslated;
                        translator.EventTranslating += translator_EventTranslating;
                        translator.EventTranslated += translator_EventTranslated;
                    }
                    else if (CurrentTranslation.Interest == TranslationInterest.Doing)
                    {
                        translator.BundleTranslating += translator_BundleTranslating;
                        translator.PackageTranslating += translator_PackageTranslating;
                        translator.KindTranslating += translator_KindTranslating;
                        translator.FieldTranslating += translator_FieldTranslating;
                        translator.MethodTranslating += translator_MethodTranslating;
                        translator.PropertyTranslating += translator_PropertyTranslating;
                        translator.EventTranslating += translator_EventTranslating;
                    }
                    else if (CurrentTranslation.Interest == TranslationInterest.Done)
                    {
                        translator.BundleTranslated += translator_BundleTranslated;
                        translator.PackageTranslated += translator_PackageTranslated;
                        translator.KindTranslated += translator_KindTranslated;
                        translator.FieldTranslated += translator_FieldTranslated;
                        translator.MethodTranslated += translator_MethodTranslated;
                        translator.PropertyTranslated += translator_PropertyTranslated;
                        translator.EventTranslated += translator_EventTranslated;
                    }

                    //translate
                    translator.StartingPoint = TranslationStartingPoints.FromBundle;
                    translator.TranslateBundle(bundle);
                    //translator.Dispose();
                }
            }
            catch (Exception e) { worker.ReportProgress(ERROR, e); }
        }
        public void RunTranslation()
        {
            #region Method 2
            try
            {
                eventViewer.ClearEvents();

                if (worker == null)
                {
                    worker = new BackgroundWorker();
                    worker.DoWork += worker_DoWork;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.WorkerReportsProgress = true;
                    worker.WorkerSupportsCancellation = true;
                }
                if (!worker.IsBusy)
                {
                    lastExMsg = "";
                    worker.RunWorkerAsync();
                }
            }
            catch { throw; }
            #endregion
        }
        public bool Save()
        {
            if (FilePath == null)
                throw new NullReferenceException("File path cannot be null!");

            try
            {
                if (CurrentTranslation != null)
                {
                    FileSystemServices.SerializeToXml(FilePath, CurrentTranslation, typeof(Translation), null);
                    this.Text = CurrentTranslation.Name;
                    IsDirty = false;
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Tril", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool SaveAs(string filePath)
        {
            FilePath = filePath;
            return Save();
        }
        public void StopTranslation() //*****************not working
        {
            #region Method 2
            try
            {
                if (worker != null)
                {
                    worker.CancelAsync();
                }
            }
            catch { throw; }
            #endregion
        }
        void ShowErrorMessageBox(Exception e, string displayMsg)
        {
            string exMsg = e.Message;
            if (exMsg != lastExMsg)
            {
                lastExMsg = exMsg;
                MessageBox.Show(displayMsg, "Tril", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        Translation CurrentTranslation;
        public string FilePath;
        bool _dirty;
        public bool IsDirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                if (_dirty)
                    this.Text = this.Text.TrimEnd('*') + "*";
                else
                    this.Text = this.Text.TrimEnd('*');
            }
        }

        void Page_RunCompleted(object sender, EventArgs e) { }
        event EventHandler _runComp;
        public event EventHandler RunCompleted
        {
            add
            {
                if (_runComp == null)
                    _runComp = new EventHandler(Page_RunCompleted);
                _runComp += value;
            }
            remove
            {
                if (_runComp == null)
                    _runComp = new EventHandler(Page_RunCompleted);
                _runComp -= value;
            }
        }
    }

    internal class TranslateEventViewer : Panel
    {
        ListView listView;
        RichTextBox rtbTranslated, rtbExpanded;
        ExceptionViewer exceptionViewer;
        ContextMenuStrip contextMenu, expandcontextMenu;
        Form formExpand;
        ToolStripContainer tscListViewCont, tscRtbTranslated;
        ToolStrip stripListViewToolBar, stripRtbMenu;
        ToolStripTextBox searchBox;
        ToolStripButton btnRtbCopy, btnSaveLog, btnRtbSelectAll, btnRtbExpand;
        SaveFileDialog saveDialog;
        XmlDocument logDocument;

        internal TranslateEventViewer()
        {
            //saveDialog
            saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = ".xml";
            saveDialog.Filter = "XML File|*.xml|All|*.*";

            //logDocument
            InitLogDocument();

            //contextMenu
            contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Copy", null, contextMenu_Copy);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Select All", null, contextMenu_SelectAll);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Expand", null, contextMenu_Expand);

            //expandcontextMenu
            expandcontextMenu = new System.Windows.Forms.ContextMenuStrip();
            expandcontextMenu.Items.Add("Copy", null, expandcontextMenu_Copy);
            expandcontextMenu.Items.Add(new ToolStripSeparator());
            expandcontextMenu.Items.Add("Select All", null, expandcontextMenu_SelectAll);

            //imgLst
            ImageList imgLst = new ImageList();
            imgLst.Images.Add("T_BDL", Tril.App.Properties.Resources.TransBundle);
            imgLst.Images.Add("T_BDL_D", Tril.App.Properties.Resources.TransBundleDone);
            imgLst.Images.Add("T_BDL_F", Tril.App.Properties.Resources.TransBundleFailed);
            imgLst.Images.Add("T_PKG", Tril.App.Properties.Resources.TransPackage);
            imgLst.Images.Add("T_PKG_D", Tril.App.Properties.Resources.TransPackageDone);
            imgLst.Images.Add("T_PKG_F", Tril.App.Properties.Resources.TransPackageFailed);
            imgLst.Images.Add("T_KND", Tril.App.Properties.Resources.TransKind);
            imgLst.Images.Add("T_KND_D", Tril.App.Properties.Resources.TransKindDone);
            imgLst.Images.Add("T_KND_F", Tril.App.Properties.Resources.TransKindFailed);
            imgLst.Images.Add("T_FLD", Tril.App.Properties.Resources.TransField);
            imgLst.Images.Add("T_FLD_D", Tril.App.Properties.Resources.TransFieldDone);
            imgLst.Images.Add("T_FLD_F", Tril.App.Properties.Resources.TransFieldFailed);
            imgLst.Images.Add("T_MTD", Tril.App.Properties.Resources.TransMethod);
            imgLst.Images.Add("T_MTD_D", Tril.App.Properties.Resources.TransMethodDone);
            imgLst.Images.Add("T_MTD_F", Tril.App.Properties.Resources.TransMethodFailed);
            imgLst.Images.Add("T_PPT", Tril.App.Properties.Resources.TransProperty);
            imgLst.Images.Add("T_PPT_D", Tril.App.Properties.Resources.TransPropertyDone);
            imgLst.Images.Add("T_PPT_F", Tril.App.Properties.Resources.TransPropertyFailed);
            imgLst.Images.Add("T_EVT", Tril.App.Properties.Resources.TransEvent);
            imgLst.Images.Add("T_EVT_D", Tril.App.Properties.Resources.TransEventDone);
            imgLst.Images.Add("T_EVT_F", Tril.App.Properties.Resources.TransEventFailed);

            //listView
            listView = new ListView();
            listView.AllowColumnReorder = false;
            listView.CheckBoxes = false;
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.LargeImageList = listView.SmallImageList = listView.StateImageList = imgLst;
            listView.MultiSelect = false;
            listView.ShowItemToolTips = true;
            listView.View = View.Details;
            listView.Columns.Add("", 40, HorizontalAlignment.Left);
            listView.Columns.Add("Event", 100, HorizontalAlignment.Left);
            listView.Columns.Add("Object", 100, HorizontalAlignment.Left);
            listView.KeyUp += listView_KeyUp;
            listView.MouseDoubleClick += listView_MouseDoubleClick;

            //searchBox
            searchBox = new ToolStripTextBox() { };
            searchBox.ToolTipText = "search";
            searchBox.KeyUp += searchBox_KeyUp;
            searchBox.TextChanged += searchBox_TextChanged;

            //btnSaveLog
            btnSaveLog = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.SaveTranslation,
                ToolTipText = "Save Log"
            };
            btnSaveLog.Click += btnSaveLog_Click;

            //stripListViewToolBar
            stripListViewToolBar = new ToolStrip();
            stripListViewToolBar.Dock = DockStyle.Fill;
            stripListViewToolBar.Items.Add(searchBox);
            stripListViewToolBar.Items.Add(new ToolStripSeparator());
            stripListViewToolBar.Items.Add(btnSaveLog);

            //tscListView
            tscListViewCont = new ToolStripContainer();
            tscListViewCont.Dock = DockStyle.Left;
            tscListViewCont.TopToolStripPanel.Controls.Add(stripListViewToolBar);
            tscListViewCont.ContentPanel.Controls.Add(listView);

            //rtbTranslated
            rtbTranslated = new RichTextBox();
            rtbTranslated.AutoWordSelection = true;
            rtbTranslated.BackColor = Color.White;
            rtbTranslated.ContextMenuStrip = contextMenu;
            rtbTranslated.Dock = DockStyle.Fill;
            rtbTranslated.Font = new System.Drawing.Font("Comic Sans MS", 9.0F, FontStyle.Regular);
            rtbTranslated.ReadOnly = true;
            rtbTranslated.WordWrap = false;

            //btnRtbCopy
            btnRtbCopy = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.Copy,
                ToolTipText = "Copy"
            };
            btnRtbCopy.Click += contextMenu_Copy;

            //btnRtbSelectAll
            btnRtbSelectAll = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.SelectAll,
                ToolTipText = "Select all"
            };
            btnRtbSelectAll.Click += contextMenu_SelectAll;

            //btnRtbExpand
            btnRtbExpand = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.Expand,
                ToolTipText = "Fullscreen"
            };
            btnRtbExpand.Click += contextMenu_Expand;

            //stripRtbMenu
            stripRtbMenu = new ToolStrip();
            stripRtbMenu.Dock = DockStyle.Fill;
            stripRtbMenu.Items.Add(btnRtbCopy);
            stripRtbMenu.Items.Add(new ToolStripSeparator());
            stripRtbMenu.Items.Add(btnRtbSelectAll);
            stripRtbMenu.Items.Add(new ToolStripSeparator());
            stripRtbMenu.Items.Add(btnRtbExpand);

            //tscRtbTranslated
            tscRtbTranslated = new ToolStripContainer();
            tscRtbTranslated.Dock = DockStyle.Top;
            tscRtbTranslated.TopToolStripPanel.Controls.Add(stripRtbMenu);
            tscRtbTranslated.ContentPanel.Controls.Add(rtbTranslated);

            //rtbExpanded
            rtbExpanded = new RichTextBox();
            rtbExpanded.AutoWordSelection = true;
            rtbExpanded.BackColor = Color.White;
            rtbExpanded.ContextMenuStrip = expandcontextMenu;
            rtbExpanded.Dock = DockStyle.Fill;
            rtbExpanded.Font = new System.Drawing.Font("Comic Sans MS", 9.0F, FontStyle.Regular);
            rtbExpanded.ReadOnly = true;
            rtbExpanded.WordWrap = false;

            //formExpand
            formExpand = new Form();
            formExpand.Icon = Properties.Resources.Icon;
            formExpand.MinimizeBox = false;
            formExpand.WindowState = FormWindowState.Maximized;
            formExpand.ShowInTaskbar = false;
            formExpand.Controls.Add(rtbExpanded);

            //exceptionViewer
            exceptionViewer = new ExceptionViewer();
            exceptionViewer.Dock = DockStyle.Bottom;

            //this
            this.Controls.Add(exceptionViewer);
            this.Controls.Add(tscRtbTranslated);
            this.Controls.Add(tscListViewCont);
            this.SizeChanged += TranslateEventViewer_Paint;
        }

        void btnSaveLog_Click(object sender, EventArgs e)
        {
            if (logDocument.LastChild.HasChildNodes)
            {
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    logDocument.Save(saveDialog.FileName);
                }
            }
        }
        void contextMenu_Copy(object sender, EventArgs e)
        {
            rtbTranslated.Copy();
        }
        void contextMenu_SelectAll(object sender, EventArgs e)
        {
            rtbTranslated.Focus();
            rtbTranslated.SelectAll();
        }
        void contextMenu_Expand(object sender, EventArgs e)
        {
            rtbExpanded.Text = rtbTranslated.Text;

            formExpand.ShowDialog();
        }
        void expandcontextMenu_Copy(object sender, EventArgs e)
        {
            rtbExpanded.Copy();
        }
        void expandcontextMenu_SelectAll(object sender, EventArgs e)
        {
            rtbExpanded.Focus();
            rtbExpanded.SelectAll();
        }
        void listView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ViewEvent();
            }
        }
        void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ViewEvent();
        }
        void searchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                HighlightEvents(searchBox.Text);
            }
        }
        void searchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text == string.Empty)
            {
                //was supposed to draw a search string and icon
            }
        }
        void TranslateEventViewer_Paint(object sender, EventArgs e)
        {
            //listView
            listView.Columns[0].Width = (listView.Width * 5) / 20;
            listView.Columns[1].Width = (listView.Width * 5) / 20;
            listView.Columns[2].Width = (listView.Width * 10) / 20;

            //tscListView
            tscListViewCont.Width = this.Width / 2;

            //tscRtbTranslated
            tscRtbTranslated.Width = this.Width / 2;
            tscRtbTranslated.Height = (this.Height * 6) / 10;

            //exceptionViewer
            exceptionViewer.Height = this.Height - tscRtbTranslated.Height;
        }

        int CurrentIndentation = 0;
        XmlNode CurrentParentNode = null;
        bool hasEnteredBundleOddNumOfTimes = false;
        bool hasEnteredPackageOddNumOfTimes = false;
        internal void AddEvent(Translator translator, TranslateObjectEventArgs eventArgs, string displayText)
        {
            if (eventArgs != null)
            {
                XmlElement currentNode = null;

                if (CurrentParentNode == null)
                    CurrentParentNode = logDocument.LastChild;

                ListViewItem item = new ListViewItem();

                ListViewItem.ListViewSubItem eventItem = new ListViewItem.ListViewSubItem();
                eventItem.Text = displayText;

                ListViewItem.ListViewSubItem objItem = new ListViewItem.ListViewSubItem();
                if (eventArgs.SourceObject != null)
                {
                    if (eventArgs.SourceObject is Bundle)
                    {
                        currentNode = logDocument.CreateElement("Bundle");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            //if (eventArgs.TranslatedObject != null) //currently this is ALWAYS false
                            //{
                            //    //item.ForeColor = Color.Blue;
                            //    item.ImageKey = "T_BDL_D";
                            //    //item.IndentCount = --CurrentIndentation;
                            //    CurrentParentNode = currentNode.ParentNode.ParentNode;
                            //}
                            //else
                            {
                                item.ImageKey = "T_BDL";
                                hasEnteredBundleOddNumOfTimes = !hasEnteredBundleOddNumOfTimes;
                                if (hasEnteredBundleOddNumOfTimes)
                                {
                                    item.IndentCount = CurrentIndentation++;
                                    CurrentParentNode = currentNode;
                                }
                                else
                                {
                                    item.IndentCount = --CurrentIndentation;
                                    CurrentParentNode = CurrentParentNode.ParentNode;
                                }
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_BDL_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        AssemblyDefinition underlyingAss = (eventArgs.SourceObject as Bundle).UnderlyingAssembly;
                        if (underlyingAss != null && underlyingAss.FullName != null)
                        {
                            string text = underlyingAss.FullName;
                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Package)
                    {
                        currentNode = logDocument.CreateElement("Package");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            //if (eventArgs.TranslatedObject != null) //currently this is ALWAYS false
                            //{
                            //    //item.ForeColor = Color.Blue;
                            //    item.ImageKey = "T_PKG_D";
                            //    //item.IndentCount = --CurrentIndentation;
                            //    CurrentParentNode = currentNode.ParentNode.ParentNode;
                            //}
                            //else
                            {
                                item.ImageKey = "T_PKG";
                                hasEnteredPackageOddNumOfTimes = !hasEnteredPackageOddNumOfTimes;
                                if (hasEnteredPackageOddNumOfTimes)
                                {
                                    item.IndentCount = CurrentIndentation++;
                                    CurrentParentNode = currentNode;
                                }
                                else
                                {
                                    item.IndentCount = --CurrentIndentation;
                                    CurrentParentNode = CurrentParentNode.ParentNode;
                                }
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_PKG_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Package package = (eventArgs.SourceObject as Package);
                        if (package != null && package.Namespace != null)
                        {
                            string text = package.Namespace;
                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Kind)
                    {
                        currentNode = logDocument.CreateElement("Kind");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            if (eventArgs.TranslatedObject != null)
                            {
                                //item.ForeColor = Color.Blue;
                                item.ImageKey = "T_KND_D";
                                item.IndentCount = --CurrentIndentation;
                                CurrentParentNode = CurrentParentNode.ParentNode;
                            }
                            else
                            {
                                item.ImageKey = "T_KND";
                                item.IndentCount = CurrentIndentation++;
                                CurrentParentNode = currentNode;
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_KND_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Kind kind = (eventArgs.SourceObject as Kind);
                        if (kind != null)
                        {
                            string text = null;

                            if (translator != null)
                                text = kind.GetLongName(translator.UseDefaultOnly, translator.TargetPlatforms);
                            else
                                text = kind.GetLongName();

                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Field)
                    {
                        currentNode = logDocument.CreateElement("Field");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            if (eventArgs.TranslatedObject != null)
                            {
                                //item.ForeColor = Color.Blue;
                                item.ImageKey = "T_FLD_D";
                                item.IndentCount = --CurrentIndentation;
                                CurrentParentNode = CurrentParentNode.ParentNode;
                            }
                            else
                            {
                                item.ImageKey = "T_FLD";
                                item.IndentCount = CurrentIndentation++;
                                CurrentParentNode = currentNode;
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_FLD_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Field field = (eventArgs.SourceObject as Field);
                        if (field != null)
                        {
                            string text = null;

                            if (translator != null)
                                text = field.GetLongName(translator.UseDefaultOnly, translator.TargetPlatforms);
                            else
                                text = field.GetLongName();

                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Method)
                    {
                        currentNode = logDocument.CreateElement("Method");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            if (eventArgs.TranslatedObject != null)
                            {
                                //item.ForeColor = Color.Blue;
                                item.ImageKey = "T_MTD_D";
                                item.IndentCount = --CurrentIndentation;
                                CurrentParentNode = CurrentParentNode.ParentNode;
                            }
                            else
                            {
                                item.ImageKey = "T_MTD";
                                item.IndentCount = CurrentIndentation++;
                                CurrentParentNode = currentNode;
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_MTD_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Method method = (eventArgs.SourceObject as Method);
                        if (method != null)
                        {
                            string text = null;

                            if (translator != null)
                                text = method.GetSignature_CsStyle(translator.UseDefaultOnly, translator.TargetPlatforms);
                            else
                                text = method.GetSignature_CsStyle();

                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Property)
                    {
                        currentNode = logDocument.CreateElement("Property");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            if (eventArgs.TranslatedObject != null)
                            {
                                //item.ForeColor = Color.Blue;
                                item.ImageKey = "T_PPT_D";
                                item.IndentCount = --CurrentIndentation;
                                CurrentParentNode = CurrentParentNode.ParentNode;
                            }
                            else
                            {
                                item.ImageKey = "T_PPT";
                                item.IndentCount = CurrentIndentation++;
                                CurrentParentNode = currentNode;
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_PPT_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message))
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Property property = (eventArgs.SourceObject as Property);
                        if (property != null)
                        {
                            string text = null;

                            if (translator != null)
                                text = property.GetLongName(translator.UseDefaultOnly, translator.TargetPlatforms);
                            else
                                text = property.GetLongName();

                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                    else if (eventArgs.SourceObject is Event)
                    {
                        currentNode = logDocument.CreateElement("Event");
                        currentNode.SetAttribute("Action", displayText);
                        CurrentParentNode.AppendChild(currentNode);

                        if (eventArgs.Succeeded)
                        {
                            if (eventArgs.TranslatedObject != null)
                            {
                                //item.ForeColor = Color.Blue;
                                item.ImageKey = "T_EVT_D";
                                item.IndentCount = --CurrentIndentation;
                                CurrentParentNode = CurrentParentNode.ParentNode;
                            }
                            else
                            {
                                item.ImageKey = "T_EVT";
                                item.IndentCount = CurrentIndentation++;
                                CurrentParentNode = currentNode;
                            }
                        }
                        else
                        {
                            item.ForeColor = Color.Red;
                            item.ImageKey = "T_EVT_F";
                            item.IndentCount = --CurrentIndentation;
                            currentNode.SetAttribute("Result", "Failed");
                            if (eventArgs.FailureCause != null && !string.IsNullOrEmpty(eventArgs.FailureCause.Message)) 
                            {
                                currentNode.InnerText = eventArgs.FailureCause.Message;
                            }
                            CurrentParentNode = CurrentParentNode.ParentNode;
                        }
                        Event _event = (eventArgs.SourceObject as Event);
                        if (_event != null)
                        {
                            string text = null;

                            if (translator != null)
                                text = _event.GetLongName(translator.UseDefaultOnly, translator.TargetPlatforms);
                            else
                                text = _event.GetLongName();

                            objItem.Text = text;
                            currentNode.SetAttribute("Item", text);
                        }
                    }
                }

                if (currentNode != null)
                    currentNode.SetAttribute("Time", System.DateTime.Now.ToString());
                //if (eventArgs.TranslatedObject != null)
                //{
                //    currentNode.InnerText = eventArgs.TranslatedObject.ToString();
                //}

                item.SubItems.Add(eventItem);
                item.SubItems.Add(objItem);

                item.Tag = eventArgs;
                listView.Items.Add(item);
            }
        }
        internal void ClearEvents()
        {
            InitLogDocument();
            listView.Items.Clear();
            rtbTranslated.Clear();
            exceptionViewer.ClearAll();
        }
        internal void HighlightEvents(string searchTerm) 
        {
            foreach (ListViewItem item in listView.Items) 
            {
                item.BackColor = Color.White;
                if (searchTerm != null && searchTerm.Trim() != string.Empty)
                {
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        if (subItem.Text.ToLower().Contains(searchTerm.ToLower()))
                        {
                            item.BackColor = Color.Yellow;
                            break;
                        }
                    }
                }
            }
        }
        void ViewEvent()
        {
            if (listView.SelectedItems != null && listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.SelectedItems[0];
                if (item.Tag != null && item.Tag is TranslateObjectEventArgs)
                {
                    TranslateObjectEventArgs eventArgs = item.Tag as TranslateObjectEventArgs;

                    bool removedBar = false;
                    if (eventArgs.SourceObject != null && eventArgs.SourceObject is Event)
                    {
                        exceptionViewer.AddBar(eventArgs.SourceObject as Event);
                    }
                    else if (eventArgs.SourceObject != null && eventArgs.SourceObject is Method)
                    {
                        exceptionViewer.AddBar(eventArgs.SourceObject as Method);
                    }
                    else if (eventArgs.SourceObject != null && eventArgs.SourceObject is Property)
                    {
                        exceptionViewer.AddBar(eventArgs.SourceObject as Property);
                    }
                    else
                    {
                        removedBar = true;
                        exceptionViewer.RemoveBar();
                    }

                    if (eventArgs.TranslatedObject != null)
                    {
                        rtbTranslated.Text = eventArgs.TranslatedObject.ToString();
                    }
                    else
                    {
                        rtbTranslated.Clear();
                    }

                    if (!eventArgs.Succeeded && eventArgs.FailureCause != null)
                    {
                        exceptionViewer.ViewException(eventArgs.FailureCause);
                    }
                    else
                    {
                        exceptionViewer.ClearException();
                        if (removedBar)
                            exceptionViewer.ClearText();
                    }
                }
            }
        }
        void InitLogDocument()
        {
            logDocument = new XmlDocument();
            var root = logDocument.CreateElement("Log");
            logDocument.AppendChild(root);
            CurrentIndentation = 0;
            CurrentParentNode = null;
            hasEnteredBundleOddNumOfTimes = false;
            hasEnteredPackageOddNumOfTimes = false;
        }
    }

    internal class PropertyPanel : Panel
    {
        int indent = 20;
        int deltaY = 3;
        GroupBox grpTranslation, grpTranslator, grpSource;
        Label lblName, lblListenFor, lblOutputDir, lblTranslator, lblOptions, lblTgtLangs, lblSourceAsm, lblNamesakeAsm, lblTgtTypes;
        internal TextBox TranslationName, OutputDirectory, TranslatorPlugIn, SourceAssembly, NamesakeAssembly;
        internal RadioButton ListenForNone, ListenForAll, ListenForDoing, ListenForDone, ListenForErrors;
        internal CheckBox Optimize, ReturnPartial, UseDefaultOnly;
        Button btnChooseOutDir, btnChoosePlugIn, btnChooseSrcAsm, btnChooseNmskAsm;
        internal WebBrowser PlugInBoard;
        internal ListPanel TargetLanguages, TargetTypes;
        FolderBrowserDialog folderDialog;
        OpenFileDialog fileDialog;

        internal PropertyPanel()
        {
            //folderDialog
            folderDialog = new FolderBrowserDialog();

            //fileDialog
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;

            //lblName
            lblName = new Label()
            {
                AutoSize = false,
                Location = new Point(2, 15),
                Height = 15,
                Text = "Name:",
                TextAlign = ContentAlignment.BottomLeft
            };
            //TranslationName
            TranslationName = new TextBox()
            {
                Location = new Point(lblName.Location.X + indent,
                    lblName.Location.Y + lblName.Height)
            };

            //lblTransName
            lblListenFor = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    TranslationName.Location.Y + TranslationName.Height + deltaY),
                Height = lblName.Height,
                Text = "Listen For:",
                TextAlign = lblName.TextAlign
            };
            //ListenForNone
            ListenForNone = new RadioButton()
            {
                AutoSize = false,
                //Checked = true,
                Font = new System.Drawing.Font(Font.FontFamily, 6.5F, Font.Style),
                Height = 15,
                Location = new Point(lblListenFor.Location.X + indent,
                    lblListenFor.Location.Y + lblListenFor.Height),
                Text = "None",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 50
            };
            //ListenForAll
            ListenForAll = new RadioButton()
            {
                AutoSize = false,
                Font = ListenForNone.Font,
                Height = 15,
                Location = new Point(ListenForNone.Location.X + ListenForNone.Width,
                    ListenForNone.Location.Y),
                Text = "All",
                TextAlign = ListenForNone.TextAlign,
                Width = 40
            };
            //ListenForDoing
            ListenForDoing = new RadioButton()
            {
                AutoSize = false,
                Font = ListenForNone.Font,
                Height = 15,
                Location = new Point(ListenForAll.Location.X + ListenForAll.Width,
                    ListenForNone.Location.Y),
                Text = "Doing",
                TextAlign = ListenForNone.TextAlign,
                Width = 55
            };
            //ListenForDone
            ListenForDone = new RadioButton()
            {
                AutoSize = false,
                Font = ListenForNone.Font,
                Height = 15,
                Location = new Point(ListenForDoing.Location.X + ListenForDoing.Width,
                    ListenForNone.Location.Y),
                Text = "Done",
                TextAlign = ListenForNone.TextAlign,
                Width = 50
            };
            //ListenForErrors
            ListenForErrors = new RadioButton()
            {
                AutoSize = false,
                Font = ListenForNone.Font,
                Height = 15,
                Location = new Point(ListenForDone.Location.X + ListenForDone.Width,
                    ListenForNone.Location.Y),
                Text = "Errors",
                TextAlign = ListenForNone.TextAlign,
                Width = 50
            };

            //lblOutputDir
            lblOutputDir = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    ListenForErrors.Location.Y + ListenForErrors.Height + deltaY),
                Height = lblName.Height,
                Text = "Output Directory:",
                TextAlign = lblName.TextAlign
            };
            //OutputDirectory
            OutputDirectory = new TextBox()
            {
                Location = new Point(lblOutputDir.Location.X + indent,
                    lblOutputDir.Location.Y + lblOutputDir.Height)
            };
            //btnChooseOutDir
            btnChooseOutDir = new Button()
            {
                Font = new System.Drawing.Font("Times New Roman", 7.0F, FontStyle.Bold),
                Height = OutputDirectory.Height,
                //Text = Convert.ToChar(8230).ToString(),
                Text = Convert.ToChar(8226).ToString() + Convert.ToChar(8226).ToString() + Convert.ToChar(8226).ToString(),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 30
            };
            btnChooseOutDir.Click += btnChooseOutDir_Click;

            //grpTranslation
            grpTranslation = new GroupBox()
            {
                Height = 130,
                Text = "Translation"
            };
            grpTranslation.Controls.Add(lblName);
            grpTranslation.Controls.Add(TranslationName);
            grpTranslation.Controls.Add(lblListenFor);
            grpTranslation.Controls.Add(ListenForNone);
            grpTranslation.Controls.Add(ListenForAll);
            grpTranslation.Controls.Add(ListenForDoing);
            grpTranslation.Controls.Add(ListenForDone);
            grpTranslation.Controls.Add(ListenForErrors);
            grpTranslation.Controls.Add(lblOutputDir);
            grpTranslation.Controls.Add(OutputDirectory);
            grpTranslation.Controls.Add(btnChooseOutDir);

            //lblTranslator
            lblTranslator = new Label()
            {
                AutoSize = false,
                Location = new Point(2, 15),
                Height = 15,
                Text = "Translator Plug-In:",
                TextAlign = ContentAlignment.BottomLeft,
                Width = 200
            };
            //TranslatorPlugIn
            TranslatorPlugIn = new TextBox()
            {
                BackColor = Color.White,
                Location = new Point(lblTranslator.Location.X + indent,
                    lblTranslator.Location.Y + lblTranslator.Height),
                ReadOnly = true
            };
            //TranslatorPlugIn.TextChanged += TranslatorPlugIn_TextChanged;
            //btnChoosePlugIn
            btnChoosePlugIn = new Button()
            {
                Font = btnChooseOutDir.Font,
                Height = TranslatorPlugIn.Height,
                Text = btnChooseOutDir.Text,
                TextAlign = btnChooseOutDir.TextAlign,
                Width = btnChooseOutDir.Width
            };
            btnChoosePlugIn.Click += btnChoosePlugIn_Click;
            //PlugInBoard
            PlugInBoard = new WebBrowser()
            {
                Height = 65,
                Location = new Point(TranslatorPlugIn.Location.X,
                       TranslatorPlugIn.Location.Y + TranslatorPlugIn.Height + 1)
            };
            //lblOptions
            lblOptions = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    PlugInBoard.Location.Y + PlugInBoard.Height + deltaY),
                Height = lblName.Height,
                Text = "Options:",
                TextAlign = lblName.TextAlign
            };
            //Optimize
            Optimize = new CheckBox()
            {
                AutoSize = false,
                AutoCheck = true,
                //Checked = true,
                Height = 18,
                Location = new Point(lblOptions.Location.X + indent,
                    lblOptions.Location.Y + lblOptions.Height),
                Text = "Optimize",
                Width = 200
            };
            //ReturnPartial
            ReturnPartial = new CheckBox()
            {
                AutoSize = false,
                AutoCheck = true,
                Height = Optimize.Height,
                Location = new Point(Optimize.Location.X,
                    Optimize.Location.Y + Optimize.Height),
                Text = "On Error Return Partial",
                Width = Optimize.Width
            };
            //UseDefaultOnly
            UseDefaultOnly = new CheckBox()
            {
                AutoSize = false,
                AutoCheck = true,
                Height = Optimize.Height,
                Location = new Point(ReturnPartial.Location.X,
                    ReturnPartial.Location.Y + ReturnPartial.Height),
                Text = "Use Default Values Only",
                Width = Optimize.Width
            };

            //lblTgtLangs
            lblTgtLangs = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    UseDefaultOnly.Location.Y + UseDefaultOnly.Height + deltaY),
                Height = lblName.Height,
                Text = "Target Platforms:",
                TextAlign = lblName.TextAlign
            };
            //TargetLanguages
            TargetLanguages = new ListPanel()
            {
                Height = 100,
                Location = new Point(lblTgtLangs.Location.X + indent,
                    lblTgtLangs.Location.Y + lblTgtLangs.Height)
            };

            //grpTranslator
            grpTranslator = new GroupBox()
            {
                Height = 310,
                Location = new Point(grpTranslation.Location.X, grpTranslation.Location.Y + grpTranslation.Height + 2),
                Text = "Translator"
            };
            grpTranslator.Controls.Add(lblTranslator);
            grpTranslator.Controls.Add(TranslatorPlugIn);
            grpTranslator.Controls.Add(btnChoosePlugIn);
            grpTranslator.Controls.Add(PlugInBoard);
            grpTranslator.Controls.Add(lblOptions);
            grpTranslator.Controls.Add(Optimize);
            grpTranslator.Controls.Add(ReturnPartial);
            grpTranslator.Controls.Add(UseDefaultOnly);
            grpTranslator.Controls.Add(lblTgtLangs);
            grpTranslator.Controls.Add(TargetLanguages);

            //lblSourceAsm
            lblSourceAsm = new Label()
            {
                AutoSize = false,
                Location = new Point(2, 15),
                Height = 15,
                Text = "Source Assembly:",
                TextAlign = ContentAlignment.BottomLeft
            };
            //SourceAssembly
            SourceAssembly = new TextBox()
            {
                BackColor = Color.White,
                Location = new Point(lblSourceAsm.Location.X + indent,
                    lblSourceAsm.Location.Y + lblSourceAsm.Height)
            };
            //btnChooseSrcAsm
            btnChooseSrcAsm = new Button()
            {
                Font = btnChooseOutDir.Font,
                Height = TranslatorPlugIn.Height,
                Text = btnChooseOutDir.Text,
                TextAlign = btnChooseOutDir.TextAlign,
                Width = btnChooseOutDir.Width
            };
            btnChooseSrcAsm.Click += btnChooseSrcAsm_Click;

            //lblNamesakeAsm
            lblNamesakeAsm = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    SourceAssembly.Location.Y + SourceAssembly.Height + deltaY),
                Height = lblName.Height,
                Width = 200,
                Text = "Namesake Assembly:",
                TextAlign = lblName.TextAlign
            };
            //NamesakeAssembly
            NamesakeAssembly = new TextBox()
            {
                Location = new Point(lblNamesakeAsm.Location.X + indent,
                    lblNamesakeAsm.Location.Y + lblNamesakeAsm.Height)
            };
            //btnChooseNmskAsm
            btnChooseNmskAsm = new Button()
            {
                Font = new System.Drawing.Font("Times New Roman", 7.0F, FontStyle.Bold),
                Height = OutputDirectory.Height,
                //Text = Convert.ToChar(8230).ToString(),
                Text = Convert.ToChar(8226).ToString() + Convert.ToChar(8226).ToString() + Convert.ToChar(8226).ToString(),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 30
            };
            btnChooseNmskAsm.Click += btnChooseNmskAsm_Click;

            //lblTgtTypes
            lblTgtTypes = new Label()
            {
                AutoSize = false,
                Location = new Point(lblName.Location.X,
                    NamesakeAssembly.Location.Y + NamesakeAssembly.Height + deltaY),
                Height = lblName.Height,
                Text = "Target Types:",
                TextAlign = lblName.TextAlign
            };
            //TargetTypes
            TargetTypes = new ListPanel()
            {
                Height = 100,
                Location = new Point(lblTgtTypes.Location.X + indent,
                    lblTgtTypes.Location.Y + lblTgtTypes.Height)
            };

            //grpSource
            grpSource = new GroupBox()
            {
                Height = 205,
                Location = new Point(grpTranslator.Location.X, grpTranslator.Location.Y + grpTranslator.Height + 2),
                Text = "Source Code"
            };
            grpSource.Controls.Add(lblSourceAsm);
            grpSource.Controls.Add(SourceAssembly);
            grpSource.Controls.Add(btnChooseSrcAsm);
            grpSource.Controls.Add(lblNamesakeAsm);
            grpSource.Controls.Add(NamesakeAssembly);
            grpSource.Controls.Add(btnChooseNmskAsm);
            grpSource.Controls.Add(lblTgtTypes);
            grpSource.Controls.Add(TargetTypes);

            //this
            this.Controls.Add(grpTranslation);
            this.Controls.Add(grpTranslator);
            this.Controls.Add(grpSource);
            this.SizeChanged += PropertyPanel_Paint;
        }

        void btnChoosePlugIn_Click(object sender, EventArgs e)
        {
            fileDialog.Filter = "Translator Plugin|*.tp";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TranslatorPlugIn.Text = fileDialog.FileName;
            }
        }
        //void TranslatorPlugIn_TextChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        TranslatorPlugin plugin = (TranslatorPlugin)FileSystemServices.DeserializeFromXml
        //            (TranslatorPluginPath.Text.Trim(), typeof(TranslatorPlugin), null);
        //        if (plugin.DisplayName != null && plugin.Description != null)
        //            PluginBoard.DocumentText = "<h3>" + plugin.DisplayName + "</h3>" + plugin.Description;
        //        else
        //            PluginBoard.DocumentText = "<h3>Translator plugin loaded!</h3>"
        //                + Convert.ToChar(9786).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9787).ToString()
        //                + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9788).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
        //                + Convert.ToChar(9829).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9835).ToString()
        //                + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9834).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
        //                + Convert.ToChar(9827).ToString();
        //    }
        //    catch
        //    {
        //        PluginBoard.DocumentText = "<h3>Translator plugin could not be loaded!</h3>"
        //                + Convert.ToChar(1160).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9618).ToString()
        //                + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(9608).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
        //                + Convert.ToChar(709).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(685).ToString()
        //                + "&nbsp;&nbsp;&nbsp;&nbsp;" + Convert.ToChar(182).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;"
        //                + Convert.ToChar(9660).ToString();
        //    }
        //}
        void btnChooseOutDir_Click(object sender, EventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                OutputDirectory.Text = folderDialog.SelectedPath;
            }
        }
        void btnChooseSrcAsm_Click(object sender, EventArgs e)
        {
            fileDialog.Filter = "Library|*.dll|Executable|*.exe|All|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                SourceAssembly.Text = fileDialog.FileName;
            }
        }
        void btnChooseNmskAsm_Click(object sender, EventArgs e)
        {
            fileDialog.Filter = "Library|*.dll|Executable|*.exe|All|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                NamesakeAssembly.Text = fileDialog.FileName;
            }
        }

        void PropertyPanel_Paint(object sender, EventArgs e)
        {
            grpTranslation.Width = this.Width - 2;
            TranslationName.Width = grpTranslation.Width - (TranslationName.Location.X + 2);
            OutputDirectory.Width = grpTranslation.Width - (OutputDirectory.Location.X + btnChooseOutDir.Width + 2);
            btnChooseOutDir.Location = new Point(OutputDirectory.Location.X + OutputDirectory.Width, OutputDirectory.Location.Y);

            grpTranslator.Width = grpTranslation.Width;
            TranslatorPlugIn.Width = grpTranslator.Width - (TranslatorPlugIn.Location.X + btnChoosePlugIn.Width + 2);
            btnChoosePlugIn.Location = new Point(TranslatorPlugIn.Location.X + TranslatorPlugIn.Width,
                TranslatorPlugIn.Location.Y);
            PlugInBoard.Width = grpTranslator.Width - (PlugInBoard.Location.X + 2);
            TargetLanguages.Width = grpTranslator.Width - (TargetLanguages.Location.X + 2);

            grpSource.Width = grpTranslator.Width;
            SourceAssembly.Width = grpSource.Width - (SourceAssembly.Location.X + btnChooseSrcAsm.Width + 2);
            btnChooseSrcAsm.Location = new Point(SourceAssembly.Location.X + SourceAssembly.Width,
                SourceAssembly.Location.Y);
            NamesakeAssembly.Width = grpTranslation.Width - (NamesakeAssembly.Location.X + btnChooseNmskAsm.Width + 2);
            btnChooseNmskAsm.Location = new Point(NamesakeAssembly.Location.X + NamesakeAssembly.Width, NamesakeAssembly.Location.Y);
            TargetTypes.Width = grpSource.Width - (TargetTypes.Location.X + 2);
        }
    }

    internal class ListPanel : Panel
    {
        ListBox listBox;
        Button btnAdd, btnRem, btnUp, btnDown;
        InputBox inputBox;

        internal ListPanel()
        {
            //inputBox
            inputBox = new InputBox();

            //listBox
            listBox = new ListBox()
            {
                ItemHeight = 20,
                Location = new Point(1, 1)
            };

            //btnAdd
            btnAdd = new Button()
            {
                Font = new System.Drawing.Font("Times New Roman", 9.0F, FontStyle.Bold),
                Size = new System.Drawing.Size(19, 22),
                Text = "+",
                TextAlign = ContentAlignment.TopCenter
            };
            btnAdd.Click += btnAdd_Click;

            //btnRem
            btnRem = new Button()
            {
                Font = btnAdd.Font,
                Size = btnAdd.Size,
                Text = Convert.ToChar(215).ToString(),// "x",
                TextAlign = btnAdd.TextAlign
            };
            btnRem.Click += btnRem_Click;

            //btnUp
            btnUp = new Button()
            {
                Font = btnAdd.Font,
                Size = btnAdd.Size,
                Text = Convert.ToChar(8593).ToString(),
                TextAlign = btnAdd.TextAlign
            };
            btnUp.Click += btnUp_Click;

            //btnDown
            btnDown = new Button()
            {
                Font = btnAdd.Font,
                Size = btnAdd.Size,
                Text = Convert.ToChar(8595).ToString(),
                TextAlign = btnAdd.TextAlign
            };
            btnDown.Click += btnDown_Click;

            //this
            this.Size = new Size(100, 100);
            this.MinimumSize = new Size(100, 100);
            this.Controls.Add(listBox);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnRem);
            this.Controls.Add(btnUp);
            this.Controls.Add(btnDown);
            this.SizeChanged += ListPanel_Paint;
        }

        void btnAdd_Click(object sender, EventArgs e)
        {
            if (inputBox.ShowDialog("Add a new item to the list", "Tril") == DialogResult.OK)
            {
                listBox.Items.Add(inputBox.Input);
                listBox.SelectedIndex = listBox.Items.Count - 1; //select the newly added item
                if (_listChanged == null)
                    _listChanged = new EventHandler(ListPanel_ListChanged);
                _listChanged(this, new EventArgs());
            }
        }
        void btnDown_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                int index = listBox.SelectedIndex;
                if (index < listBox.Items.Count - 1) //if it is not the last item
                {
                    object selected = listBox.SelectedItem;
                    listBox.Items.Remove(listBox.SelectedItem);
                    if (index == listBox.Items.Count - 1) //if index now points to the last item, simply add selected
                    {
                        listBox.Items.Add(selected);
                    }
                    else
                    {
                        listBox.Items.Insert(index + 1, selected);
                    }
                    listBox.SelectedIndex = index + 1; //select the newly inserted object
                    if (_listChanged == null)
                        _listChanged = new EventHandler(ListPanel_ListChanged);
                    _listChanged(this, new EventArgs());
                }
            }
        }
        void btnRem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                listBox.Items.Remove(listBox.SelectedItem);
                if (_listChanged == null)
                    _listChanged = new EventHandler(ListPanel_ListChanged);
                _listChanged(this, new EventArgs());
            }
        }
        void btnUp_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                int index = listBox.SelectedIndex;
                if (index > 0) //if it is not the first item
                {
                    object selected = listBox.SelectedItem;
                    listBox.Items.Remove(listBox.SelectedItem);
                    listBox.Items.Insert(index - 1, selected);
                    listBox.SelectedIndex = index - 1; //select the newly inserted object
                    if (_listChanged == null)
                        _listChanged = new EventHandler(ListPanel_ListChanged);
                    _listChanged(this, new EventArgs());
                }
            }
        }
        void ListPanel_Paint(object sender, EventArgs e)
        {
            listBox.Height = this.Height - 2;
            listBox.Width = this.Width - 23;
            btnAdd.Location = new Point(listBox.Location.X + listBox.Width + 1, 1);
            btnRem.Location = new Point(btnAdd.Location.X, btnAdd.Location.Y + btnAdd.Height + 1);
            btnUp.Location = new Point(btnRem.Location.X, btnRem.Location.Y + btnRem.Height + 1);
            btnDown.Location = new Point(btnUp.Location.X, btnUp.Location.Y + btnUp.Height + 1);
        }

        internal string[] GetList()
        {
            List<string> list = new List<string>();
            foreach (object item in listBox.Items)
            {
                list.Add(item.ToString());
            }
            return list.ToArray();
        }
        internal void SetList(string[] list)
        {
            listBox.Items.Clear();
            foreach (string listItem in list)
            {
                listBox.Items.Add(listItem);
            }
        }

        event EventHandler _listChanged;
        void ListPanel_ListChanged(object sender, EventArgs e) { }
        /// <summary>
        /// Fired when the list changes due to an item added, removed, or reordered.
        /// </summary>
        public event EventHandler ListChanged
        {
            add
            {
                if (_listChanged == null)
                    _listChanged = new EventHandler(ListPanel_ListChanged);
                _listChanged += value;
            }
            remove
            {
                if (_listChanged == null)
                    _listChanged = new EventHandler(ListPanel_ListChanged);
                _listChanged -= value;
            }
        }
    }

    internal class InputBox : Form
    {
        DialogResult _result = DialogResult.Cancel;
        Label lblMsg;
        TextBox txtInput;
        Button btnOk, btnCancel;

        internal InputBox()
        {
            //lblMsg
            lblMsg = new Label()
            {
                AutoEllipsis = true,
                AutoSize = false,
                Font = new System.Drawing.Font("Times New Roman", 13.0F, FontStyle.Regular),
                Height = 40,
                Location = new Point(2, 2)
            };

            //txtInput
            txtInput = new TextBox() { };

            //btnOk
            btnOk = new Button()
            {
                Size = new System.Drawing.Size(100, 30),
                Text = "OK"
            };
            btnOk.Click += btnOk_Click;

            //btnCancel
            btnCancel = new Button()
            {
                Size = btnOk.Size,
                Text = "Cancel"
            };
            btnCancel.Click += btnCancel_Click;

            //this
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimizeBox = this.MaximizeBox = false;
            this.Size = new Size(400, 140);
            this.ShowInTaskbar = false;
            this.Controls.Add(lblMsg);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.Paint += InputBox_SizeChanged;

            txtInput.Focus();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            _result = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
        void btnOk_Click(object sender, EventArgs e)
        {
            _result = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        void InputBox_SizeChanged(object sender, EventArgs e)
        {
            lblMsg.Width = this.Width - 10;
            txtInput.Location = new Point(lblMsg.Location.X, lblMsg.Location.Y + lblMsg.Height);
            txtInput.Width = lblMsg.Width;
            btnOk.Location = new Point(txtInput.Location.X, txtInput.Location.Y + txtInput.Height + 1);
            btnCancel.Location = new Point(btnOk.Location.X + btnOk.Width + 2, btnOk.Location.Y);
        }

        public DialogResult ShowDialog(string message, string title)
        {
            if (message == null || title == null)
                throw new NullReferenceException();

            //_result
            _result = System.Windows.Forms.DialogResult.Cancel;

            //lblMsg
            lblMsg.Text = message;

            //txtInput
            txtInput.Text = "";

            //this
            this.Text = title;

            base.ShowDialog();
            return _result;
        }

        public string Input
        {
            get { return txtInput.Text; }
        }
    }

    internal class ExceptionViewer : Panel
    {
        ContextMenuStrip contextMenu;

        RichTextBox rtbException;
        ToolStripContainer toolsContainer;
        ToolStrip toolStrip;
        ToolStripButton btnOuter, btnInner, btnMsg, btnSrc, btnSrcMthd, btnTrace, btnExtra;
        Stack<Exception> exStack;
        Exception currentException;

        ToolStrip headerBar;
        ToolStripButton btnLocals, btnOutput, btnStack, btnProcIL, btnCurrIL;
        ToolStripComboBox btnEvent, btnProperty;
        ToolStripSplitButton btnRep;
        Method currentMethod;
        Property currentProperty;
        Event currentEvent;
        ToolStripSeparator standBySeparator = new ToolStripSeparator();

        const int REP_ALL = 0;
        const int REP_ERR = 1;
        const int REP_INFO = 2;
        const int REP_COMP = 3;
        const int REP_PROG = 4;
        const int REP_WAR = 5;

        internal ExceptionViewer()
        {
            //exStack
            exStack = new Stack<Exception>();

            //contextMenu
            contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Copy", null, contextMenu_Copy);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Select All", null, contextMenu_SelectAll);

            //btnEvent
            btnEvent = new ToolStripComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            btnEvent.Items.Add("Add Method");
            btnEvent.Items.Add("Remove Method");
            btnEvent.SelectedIndexChanged += btnEvent_SelectedIndexChanged;
            //btnProperty
            btnProperty = new ToolStripComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            btnProperty.Items.Add("Get Method");
            btnProperty.Items.Add("Set Method");
            btnProperty.SelectedIndexChanged += btnProperty_SelectedIndexChanged;
            //btnLocals
            btnLocals = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.MethodLocals,
                ToolTipText = "Local Variables\nShows the local variables discovered thus far."
            };
            btnLocals.Click += btnLocals_Click;
            //btnOutput
            btnOutput = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.MethodTransCodes,
                ToolTipText = "Output Codes\nShows the output of the translation thus far."
            };
            btnOutput.Click += btnOutput_Click;
            //btnStack
            btnStack = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.MethodStack,
                ToolTipText = "Current Stack\nShows the current statement stack."
            };
            btnStack.Click += btnStack_Click;
            btnRep = new ToolStripSplitButton()
            {
                Image = Tril.App.Properties.Resources.MethodReports,
                ToolTipText = "Method Reports\nShows the reports thus far."
            };
            btnRep.DropDownItems.Add("All");
            btnRep.DropDownItems.Add("Error");
            btnRep.DropDownItems.Add("Information");
            btnRep.DropDownItems.Add("Operation Completed");
            btnRep.DropDownItems.Add("Operation In Progress");
            btnRep.DropDownItems.Add("Warning");
            btnRep.ButtonClick += btnRep_Click;
            btnRep.DropDownItemClicked += btnRep_DropDownItemClicked;

            //btnProcIL
            btnProcIL = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.MethodProcIL,
                ToolTipText = "Processed IL Elements\nShows the IL elements that have been processed thus far."
            };
            btnProcIL.Click += btnProcIL_Click;
            //btnCurrIL
            btnCurrIL = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.MethodCurrIL,
                ToolTipText = "Current IL Element\nShows the IL element that was about to be processed."
            };
            btnCurrIL.Click += btnCurrIL_Click;

            //headerBar
            headerBar = new ToolStrip();
            headerBar.Dock = DockStyle.Fill;

            //rtbException
            rtbException = new RichTextBox();
            rtbException.AutoWordSelection = true;
            rtbException.BackColor = Color.White;
            rtbException.ContextMenuStrip = contextMenu;
            rtbException.Dock = DockStyle.Fill;
            rtbException.Font = new System.Drawing.Font("Comic Sans MS", 11.0F, FontStyle.Regular);
            rtbException.ReadOnly = true;
            rtbException.WordWrap = false;

            //btnOuter
            btnOuter = new ToolStripButton()
            {
                AutoToolTip = false,
                Image = Tril.App.Properties.Resources.Back,
                Text = "Outer Exception"
            };
            btnOuter.Click += btnOuter_Click;
            //btnInner
            btnInner = new ToolStripButton()
            {
                AutoToolTip = false,
                Image = Tril.App.Properties.Resources.Forward,
                Text = "Inner Exception"
            };
            btnInner.Click += btnInner_Click;
            //btnMsg
            btnMsg = new ToolStripButton()
            {
                AutoToolTip = false,
                Text = "Message"
            };
            btnMsg.Click += btnMsg_Click;
            //btnSrc
            btnSrc = new ToolStripButton()
            {
                AutoToolTip = false,
                Text = "Source"
            };
            btnSrc.Click += btnSrc_Click;
            //btnSrcMthd
            btnSrcMthd = new ToolStripButton()
            {
                AutoToolTip = false,
                Text = "Source Method"
            };
            btnSrcMthd.Click += btnSrcMthd_Click;
            //btnTrace
            btnTrace = new ToolStripButton()
            {
                AutoToolTip = false,
                Text = "Stack Trace"
            };
            btnTrace.Click += btnTrace_Click;
            //btnExtra
            btnExtra = new ToolStripButton()
            {
                AutoToolTip = false,
                Text = "Extra"
            };
            btnExtra.Click += btnExtra_Click;

            //toolStrip
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Fill;
            toolStrip.Items.Add(btnOuter);
            toolStrip.Items.Add(btnInner);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnMsg);
            toolStrip.Items.Add(btnSrc);
            toolStrip.Items.Add(btnSrcMthd);
            toolStrip.Items.Add(btnTrace);
            toolStrip.Items.Add(btnExtra);

            //toolsContainer
            toolsContainer = new ToolStripContainer();
            toolsContainer.Dock = DockStyle.Fill;
            toolsContainer.BottomToolStripPanel.Controls.Add(toolStrip);
            toolsContainer.TopToolStripPanel.Controls.Add(headerBar);
            toolsContainer.ContentPanel.Controls.Add(rtbException);

            //this
            this.Controls.Add(toolsContainer);

            RemoveBar();
        }

        void contextMenu_Copy(object sender, EventArgs e)
        {
            rtbException.Copy();
        }
        void contextMenu_SelectAll(object sender, EventArgs e)
        {
            rtbException.Focus();
            rtbException.SelectAll();
        }

        public void ClearAll()
        {
            ClearText();
            ClearException();
            RemoveBar();
        }
        public void ClearText()
        {
            rtbException.Clear();
        }

        void btnInner_Click(object sender, EventArgs e)
        {
            if (currentException != null && currentException.InnerException != null)
            {
                exStack.Push(currentException);
                currentException = currentException.InnerException;
                ViewExceptionMessage(currentException);
            }
        }
        void btnMsg_Click(object sender, EventArgs e)
        {
            ViewExceptionMessage(currentException);
        }
        void btnOuter_Click(object sender, EventArgs e)
        {
            if (exStack.Count > 0)
            {
                currentException = exStack.Pop();
                ViewExceptionMessage(currentException);
            }
        }
        void btnSrc_Click(object sender, EventArgs e)
        {
            ViewExceptionSource(currentException);
        }
        void btnSrcMthd_Click(object sender, EventArgs e)
        {
            ViewExceptionSourceMethod(currentException);
        }
        void btnTrace_Click(object sender, EventArgs e)
        {
            ViewExceptionTrace(currentException);
        }
        void btnExtra_Click(object sender, EventArgs e)
        {
            ViewExceptionExtra(currentException);
        }

        public void ClearException()
        {
            exStack.Clear();
            currentException = null;
        }
        public void ViewException(Exception exception)
        {
            if (exception == null)
                throw new NullReferenceException("Exception to view cannot be null!");

            currentException = exception;

            ViewExceptionMessage(currentException);
        }
        void ViewExceptionMessage(Exception exception)
        {
            rtbException.Text = "";
            if (exception != null && exception.Message != null)
            {
                rtbException.Text = exception.Message;
            }
        }
        void ViewExceptionSource(Exception exception)
        {
            rtbException.Text = "";
            if (exception != null && exception.Source != null)
            {
                rtbException.Text = exception.Source;
            }
        }
        void ViewExceptionSourceMethod(Exception exception)
        {
            rtbException.Text = "";
            if (exception != null && exception.TargetSite != null)
            {
                string displayText = "";

                try
                {
                    Method method = Method.GetCachedMethod(exception.TargetSite.ToMethodDefinition(true));
                    displayText += method.GetSignature_CsStyle(true);
                }
                catch
                {
                    if (exception.TargetSite.DeclaringType != null)
                        displayText += exception.TargetSite.DeclaringType.FullName + "::" + exception.TargetSite.Name;
                    else
                        displayText += exception.TargetSite.Name;
                }

                rtbException.Text = displayText;
            }
        }
        void ViewExceptionTrace(Exception exception)
        {
            rtbException.Text = "";
            if (exception != null && exception.StackTrace != null)
            {
                rtbException.Text = exception.StackTrace;
            }
        }
        void ViewExceptionExtra(Exception exception)
        {
            rtbException.Text = "";
            if (exception != null)
            {
                rtbException.Text = "Help Link: ";
                if (exception.HelpLink != null)
                {
                    rtbException.Text += exception.HelpLink;
                }

                rtbException.Text += "\n\nData:";
                if (exception.Data != null)
                {
                    foreach (KeyValuePair<object, object> pair in exception.Data)
                    {
                        rtbException.Text += "\n\t" + pair.Key.ToString() + ": " + pair.Value.ToString();
                    }
                }
            }
        }

        void btnEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                if (btnEvent.SelectedIndex == 0)
                    currentMethod = currentEvent.AddMethod;
                else if (btnEvent.SelectedIndex == 1)
                    currentMethod = currentEvent.RemoveMethod;
            }
            else
                currentMethod = null;
        }
        void btnProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentProperty != null)
            {
                if (btnProperty.SelectedIndex == 0)
                    currentMethod = currentProperty.GetMethod;
                else if (btnProperty.SelectedIndex == 1)
                    currentMethod = currentProperty.SetMethod;
            }
            else
                currentMethod = null;
        }
        void btnCurrIL_Click(object sender, EventArgs e)
        {
            ViewMethodCurrentIlElement(currentMethod);
        }
        void btnLocals_Click(object sender, EventArgs e)
        {
            ViewMethodLocals(currentMethod);
        }
        void btnOutput_Click(object sender, EventArgs e)
        {
            ViewMethodOutput(currentMethod);
        }
        void btnProcIL_Click(object sender, EventArgs e)
        {
            ViewMethodProcessedIlElements(currentMethod);
        }
        void btnRep_Click(object sender, EventArgs e)
        {
            ViewMethodReports(currentMethod, REP_ALL);
        }
        void btnRep_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ViewMethodReports(currentMethod, btnRep.DropDownItems.IndexOf(e.ClickedItem));
        }
        void btnStack_Click(object sender, EventArgs e)
        {
            ViewMethodStack(currentMethod);
        }

        public void AddBar(Event _event)
        {
            if (_event == null)
                throw new NullReferenceException("Event to view cannot be null!");

            currentEvent = _event;
            currentMethod = _event.AddMethod;

            headerBar.Items.Clear();
            btnEvent.SelectedIndex = 0;
            headerBar.Items.Add(btnEvent);
            headerBar.Items.Add(standBySeparator);
            headerBar.Items.Add(btnLocals);
            headerBar.Items.Add(btnOutput);
            headerBar.Items.Add(btnStack);
            headerBar.Items.Add(btnRep);
            headerBar.Items.Add(btnProcIL);
            headerBar.Items.Add(btnCurrIL);

            headerBar.Show();
        }
        public void AddBar(Method method)
        {
            if (method == null)
                throw new NullReferenceException("Method to view cannot be null!");

            currentMethod = method;

            headerBar.Items.Clear();
            headerBar.Items.Add(btnLocals);
            headerBar.Items.Add(btnOutput);
            headerBar.Items.Add(btnStack);
            headerBar.Items.Add(btnRep);
            headerBar.Items.Add(btnProcIL);
            headerBar.Items.Add(btnCurrIL);

            headerBar.Show();
        }
        public void AddBar(Property property)
        {
            if (property == null)
                throw new NullReferenceException("Property to view cannot be null!");

            currentProperty = property;
            currentMethod = property.GetMethod;

            headerBar.Items.Clear();
            btnProperty.SelectedIndex = 0;
            headerBar.Items.Add(btnProperty);
            headerBar.Items.Add(standBySeparator);
            headerBar.Items.Add(btnLocals);
            headerBar.Items.Add(btnOutput);
            headerBar.Items.Add(btnStack);
            headerBar.Items.Add(btnRep);
            headerBar.Items.Add(btnProcIL);
            headerBar.Items.Add(btnCurrIL);

            headerBar.Show();
        }
        public void RemoveBar()
        {
            headerBar.Hide();
            currentMethod = null;
            currentProperty = null;
            currentEvent = null;
        }
        void ViewMethodCurrentIlElement(Method method)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var element = method.CurrentIlElement;
                if (element != null)
                {
                    rtbException.Text += element.ToString();
                }
            }
        }
        void ViewMethodLocals(Method method)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var locals = method.Locals;
                if (locals != null)
                {
                    foreach (var local in locals)
                    {
                        if (local != null)
                        {
                            rtbException.Text += local.VariableKind.GetLongName() + " " + local.ToString() + "\n";
                        }
                    }
                }
            }
        }
        void ViewMethodOutput(Method method)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var codes = method.TranslatedCodes;
                if (codes != null)
                {
                    foreach (var code in codes)
                    {
                        if (code != null)
                        {
                            rtbException.Text += code.ToString() + "\n";
                        }
                    }
                }
            }
        }
        void ViewMethodProcessedIlElements(Method method)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var elements = method.ProcessedIlElements;
                if (elements != null)
                {
                    foreach (var element in elements)
                    {
                        if (element != null)
                        {
                            rtbException.Text += element.ToString() + "\n";
                        }
                    }
                }
            }
        }
        void ViewMethodReports(Method method, int index)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var reports = method.Reports;
                if (reports != null)
                {
                    foreach (var report in reports)
                    {
                        if (report != null)
                        {
                            if (index == REP_ALL)
                                rtbException.Text += report.ToString() + "\n";
                            else if (index == REP_COMP && report.Type == Method.ReportType.OperationCompleted)
                                rtbException.Text += report.ToString() + "\n";
                            else if (index == REP_ERR && report.Type == Method.ReportType.Error)
                                rtbException.Text += report.ToString() + "\n";
                            else if (index == REP_INFO && report.Type == Method.ReportType.Information)
                                rtbException.Text += report.ToString() + "\n";
                            else if (index == REP_PROG && report.Type == Method.ReportType.OperationInProgress)
                                rtbException.Text += report.ToString() + "\n";
                            else if (index == REP_WAR && report.Type == Method.ReportType.Warning)
                                rtbException.Text += report.ToString() + "\n";
                        }
                    }
                }
            }
        }
        void ViewMethodStack(Method method)
        {
            rtbException.Text = "";
            if (method != null)
            {
                var codes = method.Stack;
                if (codes != null)
                {
                    foreach (var code in codes)
                    {
                        if (code != null)
                        {
                            rtbException.Text += code.ToString() + "\n";
                        }
                    }
                }
            }
        }
    }
}
