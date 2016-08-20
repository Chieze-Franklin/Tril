using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Tril.App
{
    public partial class WfaMainWin : Form
    {
        ToolStripContainer tsContainer;
        ToolStrip toolStrip;
        ToolStripButton btnOpen, btnNew, btnSave, btnSaveAs, btnClose, btnRunStop;//, btnTransPlugins, btnSettings;
        TabControl tabControl;
        TranslationPage CurrentPage;
        OpenFileDialog openDialog;
        SaveFileDialog saveDialog;
        //bool isRunning = false;

        public WfaMainWin()
        {
            InitializeComponent();

            //openDialog
            openDialog = new OpenFileDialog();
            openDialog.AddExtension = true;
            openDialog.DefaultExt = ".tr";
            openDialog.Filter = "Translation|*.tr|All|*.*";
            openDialog.Multiselect = true;

            //saveDialog
            saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = ".tr";
            saveDialog.Filter = "Translation|*.tr|All|*.*";

            //btnOpen
            btnOpen = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.OpenTranslation,
                ToolTipText = "Open Translation"
            };
            btnOpen.Click += btnOpen_Click;
            //btnNew
            btnNew = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.NewTranslation,
                ToolTipText = "New Translation"
            };
            btnNew.Click += btnNew_Click;
            //btnSave
            btnSave = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.SaveTranslation,
                ToolTipText = "Save Translation"
            };
            btnSave.Click += btnSave_Click;
            //btnSaveAs
            btnSaveAs = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.SaveTranslationAs,
                ToolTipText = "Save Translation As..."
            };
            btnSaveAs.Click += btnSaveAs_Click;
            //btnClose
            btnClose = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.CloseTranslation,
                ToolTipText = "Close Translation"
            };
            btnClose.Click += btnClose_Click;
            //btnRunStop
            btnRunStop = new ToolStripButton()
            {
                Image = Tril.App.Properties.Resources.RunTranslation,
                ToolTipText = "Run Translation"
            };
            btnRunStop.Click += btnRunStop_Click;
            ////btnTransPlugins
            //btnTransPlugins = new ToolStripButton()
            //{
            //    Image = Tril.App.Properties.Resources.TranslatorPlugins,
            //    ToolTipText = "Manage Tranlator Plug-ins..."
            //};
            //btnTransPlugins.Click += (object sender, EventArgs e) => 
            //{
            //    MessageBox.Show("Not yet implemented.\nIntended to load a dialog form with which we can install, manage, delete translators.", 
            //        "Tril", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //};
            ////btnSettings
            //btnSettings = new ToolStripButton()
            //{
            //    Image = Tril.App.Properties.Resources.Settings,
            //    ToolTipText = "Settings..."
            //};
            //btnSettings.Click += (object sender, EventArgs e) =>
            //{
            //    MessageBox.Show("Not yet implemented.\nIntended to load a dialog form with which we can manage app settings.",
            //        "Tril", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //};

            //toolStrip
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Fill;
            toolStrip.Items.Add(btnOpen);
            toolStrip.Items.Add(btnNew);
            toolStrip.Items.Add(btnSave);
            toolStrip.Items.Add(btnSaveAs);
            toolStrip.Items.Add(btnClose);
            toolStrip.Items.Add(btnRunStop);
            //toolStrip.Items.Add(new ToolStripSeparator());
            //toolStrip.Items.Add(btnTransPlugins);
            //toolStrip.Items.Add(new ToolStripSeparator());
            //toolStrip.Items.Add(btnSettings);

            //tabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;

            //tsContainer
            tsContainer = new ToolStripContainer();
            tsContainer.Dock = DockStyle.Fill;
            tsContainer.TopToolStripPanel.Controls.Add(toolStrip);
            tsContainer.ContentPanel.Controls.Add(tabControl);

            //this
            this.AllowDrop = true;
            this.Icon = Properties.Resources.Icon;
            this.Size = new Size(500, 500);
            this.Text = "Tril";
            this.WindowState = FormWindowState.Maximized;
            this.Controls.Add(tsContainer);

            this.DragDrop += WfaMainWin_DragDrop;
            this.DragEnter += WfaMainWin_DragEnter;
            this.FormClosing += WfaMainWin_FormClosing;
            this.Load += WfaMainWin_Load;
        }

        bool GetFileNameFromDragEvent(out string[] fileNames, DragEventArgs e) 
        {
            bool ret = false;
            fileNames = new string[0];

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) 
            {
                Array data = ((IDataObject)e.Data).GetData(DataFormats.FileDrop) as Array;
                if (data != null && data is string[]) 
                {
                    fileNames = ((string[])data);
                    ret = true;
                }
            }

            return ret;
        }
        void OpenFile(string fileToOpen)
        {
            if (Path.GetExtension(fileToOpen).ToLower() == ".tr")
            {
                TranslationPage newPage = new TranslationPage(fileToOpen);
                tabControl.TabPages.Add(newPage);
                tabControl.SelectedTab = newPage;
                CurrentPage = newPage; //this is needed bcuz the first page added does not fire the tabControl.SelectedIndexChanged event;
                CurrentPage.RunCompleted += CurrentPage_RunCompleted;
            }
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.IsDirty)
                {
                    DialogResult result =
                        MessageBox.Show("'" + CurrentPage.Text + "' has not been saved!\nDo you want to save it?", "Tril",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (CurrentPage.FilePath == null)
                        {
                            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                CurrentPage.FilePath = saveDialog.FileName;
                            }
                        }
                        if (CurrentPage.CanSave())
                        {
                            if (CurrentPage.Save())
                                CurrentPage.Close();
                        }
                    }
                    else if (result == System.Windows.Forms.DialogResult.No)
                    {
                        CurrentPage.Close();
                    }
                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    CurrentPage.Close();
                }
            }
        }
        void btnNew_Click(object sender, EventArgs e)
        {
            TranslationPage newPage = new TranslationPage();
            tabControl.TabPages.Add(newPage);
            tabControl.SelectedTab = newPage;
            CurrentPage = newPage; //this is needed bcuz the first page added does not fire the tabControl.SelectedIndexChanged event
            CurrentPage.RunCompleted += CurrentPage_RunCompleted;
        }
        void btnOpen_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in openDialog.FileNames)
                {
                    OpenFile(fileName);
                }
            }
        }
        void btnRunStop_Click(object sender, EventArgs e)
        {
            try
            {
                //if (isRunning)
                //{
                //    if (CurrentPage != null)
                //    {
                //        CurrentPage.StopTranslation();
                //        btnRunStop.Image = Tril.App.Properties.Resources.RunTranslation;
                //        btnRunStop.ToolTipText = "Run Translation";
                //        isRunning = false;
                //    }
                //}
                //else
                {
                    if (CurrentPage != null)
                    {
                        //CurrentPage.RunCompleted += CurrentPage_RunCompleted;
                        btnRunStop.Enabled = false;
                        CurrentPage.RunTranslation();
                        //btnRunStop.Image = Tril.App.Properties.Resources.StopTranslation;
                        //btnRunStop.ToolTipText = "Stop Translation";
                        //isRunning = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tril", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void CurrentPage_RunCompleted(object sender, EventArgs e)
        {
            //btnRunStop.Image = Tril.App.Properties.Resources.RunTranslation;
            //btnRunStop.ToolTipText = "Run Translation";
            //isRunning = false;

            btnRunStop.Enabled = true;
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.FilePath == null)
                {
                    if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        CurrentPage.FilePath = saveDialog.FileName;
                    }
                }
                if (CurrentPage.CanSave())
                {
                    CurrentPage.Save();
                }
            }
        }
        void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CurrentPage.FilePath = saveDialog.FileName;
                }
                if (CurrentPage.CanSave())
                {
                    CurrentPage.Save();
                }
            }
        }
        void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
                CurrentPage = tabControl.SelectedTab as TranslationPage;
            else
                CurrentPage = null;
        }
        void WfaMainWin_DragDrop(object sender, DragEventArgs e)
        {
            string[] filesToOpen;
            bool validData = GetFileNameFromDragEvent(out filesToOpen, e);

            if (validData)
            {
                foreach (string fileToOpen in filesToOpen)
                {
                    if (fileToOpen != null && File.Exists(fileToOpen))
                    {
                        OpenFile(fileToOpen);
                    }
                }
            }
        }
        void WfaMainWin_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;

            string[] filesToOpen;
            bool validData = GetFileNameFromDragEvent(out filesToOpen, e);

            if (!validData)
                e.Effect = DragDropEffects.None;
        }
        void WfaMainWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            while (CurrentPage != null)//(tabControl.TabCount > 0)
            {
                //if (CurrentPage != null)
                {
                    if (CurrentPage.IsDirty)
                    {
                        DialogResult result =
                            MessageBox.Show("'" + CurrentPage.Text + "' has not been saved!\nDo you want to save it?", "Tril",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (CurrentPage.FilePath == null)
                            {
                                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    CurrentPage.FilePath = saveDialog.FileName;
                                }
                            }
                            if (CurrentPage.CanSave())
                            {
                                if (CurrentPage.Save())
                                    CurrentPage.Close();
                            }
                        }
                        else if (result == System.Windows.Forms.DialogResult.No)
                        {
                            CurrentPage.Close();
                        }
                        else //if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            e.Cancel = true;
                            break;
                        }
                    }
                    else
                    {
                        CurrentPage.Close();
                    }
                }
            }
        }
        void WfaMainWin_Load(object sender, EventArgs e)
        {
            try
            {
                //check for \Data\.settings

                string dataDirPath = Application.StartupPath + "\\Data";
                FileSystemServices.CreateDirectory(dataDirPath, false);
                string settingsFilePath = dataDirPath + "\\.settings";
                FileSystemServices.CreateFile(settingsFilePath, false, "");

                //if the app was started by dragging a file unto its icon
                string[] filesToOpen = Tril.App.Program.FilesToOpen;
                if (filesToOpen != null && filesToOpen.Length > 0)
                {
                    foreach (string fileToOpen in filesToOpen)
                    {
                        if (fileToOpen != null && File.Exists(fileToOpen))
                        {
                            OpenFile(fileToOpen);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tril", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
