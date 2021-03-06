using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Management;

namespace AutomatedDiskCleaner
{  
    public partial class FrmMain : Form
    {
        #region Local Variables
        int maxbytes = 0;
        int totalsizegain = 0;
        int totalfiles = 0;
        int Read = 0;
        #endregion

        public FrmMain()
        {
            InitializeComponent();
            this.listview();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {            
            this.Read_Xml();
            this.ListOfPrograms();
            vbtnSelectOpt.ButtonText = "Select All";
            listViewDrives.View = View.Details;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {                
                    ListViewItem lviDrives = new ListViewItem(
                            new string[] { drive.Name, drive.DriveFormat, (drive.TotalSize / 1024 / 1024 / 1024).ToString() + " GB", (drive.TotalFreeSpace / 1024 / 1024 / 1024).ToString() + " GB" });
                    listViewDrives.Items.Add(lviDrives);
                    listViewDrives.Items[0].Checked = true;
                }
                else
                {
                    return;
                }
            }  
        }
        
        #region Analyzer Function
        private void toolStripButtonScan_Click(object sender, EventArgs e)
        {          
            grbListView.Visible = true;
            grbInfo.Visible = true;
            grbIncFolders.Visible = false;  
        }

        private void vbtnScan_Click(object sender, EventArgs e)
        {
            lsvFile.Items.Clear();
            ClearAll();
            vbtnScan.ForeColor = Color.Gray;
            vbtnScan.Enabled = false;
            lbltotalsizegain.Visible = true;
            lblTotalFiles.Visible = true;
            for (int item = 0; item < listViewDrives.Items.Count; item++)
            {
                if (listViewDrives.Items[item].Checked)
                {
                    for (int folder = 0; folder < lsvIncFolders.Items.Count; folder++)
                    {
                        ScanSubFoldersAndFiles(lsvIncFolders.Items[folder].Text);
                    }
                }
            }
            vbtnClear.Visible = true;
            toolStripBtnClean.Enabled = true;
            lbltotalsizegain.Text = "Total Size Gain: " + (totalsizegain / 1024).ToString() + " Kilobytes";
            lblTotalFiles.Text = "Total Number of Files: " + totalfiles.ToString() + " Found";
            lblFilename.Text = "Scanning Completed...";
        }

        public void ScanSubFoldersAndFiles(string path)
        {
            DirectoryInfo DirPath = new DirectoryInfo(path);
            //FrmScan frm_scan = new FrmScan(DirPath);
            //frm_scan.ShowDialog();
            if (!DirPath.Exists)
            {
                return;
            }
            foreach (FileInfo file in DirPath.GetFiles())
            {
                maxbytes = 0;
                lblFilename.Text = "Scan File(s) " + file.Name;
                lblFilename.Refresh();
                maxbytes += (int)file.Length;
                prgFileRead.Maximum = maxbytes;
                Read += (int)file.Length;
                Read /= 1024;
                prgFileRead.Step = Read;
                prgFileRead.PerformStep();
                totalfiles += 1;
                totalsizegain += maxbytes;
                ListViewItem lvi = new ListViewItem(
                    new string[] { file.Name, maxbytes.ToString() + " Bytes", path });
                lsvFile.Items.Add(lvi);

            }
            foreach (DirectoryInfo subFolder in DirPath.GetDirectories())
            {
                ScanSubFoldersAndFiles(Path.Combine(path, subFolder.Name));
            }

        }
        #endregion

        //Clear function
        private void ClearAll()
        {
            lblFN.Visible = false;
            lblFName.Text = "";
            lblLoc.Visible = false;
            lblLocation.Text = "";
            lblSiz.Visible = false;
            lblSize.Text = "";
            lsvFile.Clear();
            listview();  
            vbtnClear.Visible = false;
            pcbIcon.Visible = false;
            lbltotalsizegain.Visible = false;
            lblTotalFiles.Visible = false;
            vbtnScan.Enabled = true;
            vbtnScan.Focus();
            vbtnScan.ForeColor = Color.White;
            toolStripBtnClean.Enabled = false;
            totalsizegain = 0;
            totalfiles = 0;
        }
        //Display listview as initial component
        private void listview()
        {
            lsvFile.View = View.Details;
            lsvFile.Columns.Add("Files", 200, HorizontalAlignment.Left);
            lsvFile.Columns.Add("Size", 100, HorizontalAlignment.Left);  
            lsvFile.Columns.Add("Location", 150, HorizontalAlignment.Left);     
        }

        private void vtnClear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to \n'Clean' the selected documents?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ClearAll();
            }
        }

        private void lsvFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbltotalsizegain.Visible = false;
            lblTotalFiles.Visible = false;
            if (this.lsvFile.SelectedItems.Count > 0)
            {
                lblFN.Visible = true;
                lblLoc.Visible = true;
                lblSiz.Visible = true;
                pcbIcon.Visible = true;
                lblFName.Text = lsvFile.SelectedItems[0].Text;
                lblLocation.Text = lsvFile.SelectedItems[0].SubItems[2].Text;
                lblSize.Text = lsvFile.SelectedItems[0].SubItems[1].Text;
                string FileLocation = lsvFile.SelectedItems[0].SubItems[2].Text + @"\" + lblFName.Text;
                Icon fileIcon = Icon.ExtractAssociatedIcon(FileLocation);
                pcbIcon.Image = Bitmap.FromHicon(new Icon(fileIcon, new Size(64, 64)).Handle);
                lblFName.Refresh();
            }
        }

        private void vbtnSelectOpt_Click(object sender, EventArgs e)
        {
            if (lsvFile.Items.Count != 0)
            {
                if (vbtnSelectOpt.ButtonText == "Select All")
                {
                    foreach (ListViewItem item in lsvFile.Items)
                    {
                        item.Checked = true;
                    }
                    vbtnSelectOpt.ButtonText = "UnSelect All";
                }
                else
                {
                    foreach (ListViewItem item in lsvFile.Items)
                    {
                        item.Checked = false;
                    }
                    vbtnSelectOpt.ButtonText = "Select All";
                }
            }
        }

        private void toolStripBtnSheduler_Click(object sender, EventArgs e)
        {
            FrmScheduler frm_sched = new FrmScheduler();
            frm_sched.Visible = true;
        }

        private void toolStripBtnClean_Click(object sender, EventArgs e)
        {
            vbtnScan.Enabled = true;
            if (this.lsvFile.Items.Count > 0)
            {
                foreach (ListViewItem lvi in this.lsvFile.Items)
                {
                    if (!lvi.Checked)
                        continue;

                    try
                    {
                        if (rbtnDelete.Checked == true)
                        {
                            lblFilename.Text = "Deleted File(s) " + lvi.Text;
                            lblFilename.Refresh();
                            File.Delete(Path.Combine(lvi.SubItems[2].Text,lvi.Text));
                            continue;
                        }

                        if (rbtnMoveToFolder.Checked == true)
                        {
                            ListView.CheckedIndexCollection checkedItems = lsvFile.CheckedIndices;
                            lblFilename.Text = "Moving File(s) " + lvi.Text;
                            lblFilename.Refresh();
                            lsvFile.Items.RemoveAt(checkedItems[0]);
                            if (!File.Exists(Path.Combine(txtboxMoveFolder.Text, lvi.Text)))
                            File.Move(Path.Combine(lvi.SubItems[2].Text, lvi.Text), Path.Combine(txtboxMoveFolder.Text,lvi.Text));
                        }
                    }catch(Exception ex){MessageBox.Show(ex.Message);}
                }
            }
            ClearAll();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FrmAbout frm_about = new FrmAbout();
            frm_about.Show();
        }

        private void toolStripButtonOptions_Click(object sender, EventArgs e)
        {
            grbIncFolders.Visible = true;
        }
       
        #region Included Folder

        private void vbtnIncFolders_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string folderpath = folderBrowserDialog1.SelectedPath;
                DirectoryInfo foldername = new DirectoryInfo(folderpath);
                lsvIncFolders.Items.Add(folderpath).SubItems.Add(foldername.Name);
                Generate_Xml(foldername.Name, folderpath);

            }
        }

        private void Generate_Xml(string Name, string Path) 
        {
            if (!File.Exists(@"Inc_Folder.xml"))
            {
            XmlTextWriter writer = new XmlTextWriter("Inc_Folder.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteComment("Lists of Included Folders.");
            writer.WriteStartElement("Folders");
            writer.WriteStartElement("Folder");
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Path", Path);
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
            }
            else
            {
                XmlDocument document = new XmlDocument();
                document.Load(@"Inc_Folder.xml");
                XmlNode Folder = document.CreateElement("Folder");
                XmlNode FolderName = document.CreateElement("Name");
                FolderName.InnerText = Name;
                Folder.AppendChild(FolderName);
                XmlNode FolderPath = document.CreateElement("Path");
                FolderPath.InnerText = Path;
                Folder.AppendChild(FolderPath);
                document.DocumentElement.AppendChild(Folder);
                document.Save(@"Inc_Folder.xml");
            }
        }

        private void Read_Xml()
        {
            if (File.Exists(@"Inc_Folder.xml"))
            {
                XmlDocument document = new XmlDocument();
                document.Load(@"Inc_Folder.xml");
                XmlElement root = document.DocumentElement;
                XmlNodeList FolderPath = root.SelectNodes("/Folders/Folder/Path");
                XmlNodeList FolderName = root.SelectNodes("/Folders/Folder/Name");
                for (int item = 0; item < FolderPath.Count; item++)
                {
                    string folderpath = FolderPath.Item(item).InnerText;
                    string foldername = FolderName.Item(item).InnerText;
                    lsvIncFolders.Items.Add(folderpath).SubItems.Add(foldername);
                }
            }
            else { return; }
        }
        #endregion

        #region Programs and Features
        private void vbtnUninstallProgs_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Uninstall " + lsvListOfProgs.SelectedItems[0].Text + " from your Computer?", "Uninstall Program", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                UninstallProgram(lsvListOfProgs.SelectedItems[0].Text);
            }
        }

        private void ListOfPrograms()
        {
            
                ManagementObjectSearcher mos =
                  new ManagementObjectSearcher("SELECT * FROM Win32_Product");
                foreach (ManagementObject mo in mos.Get())
                {
                    try
                    {
                        DirectoryInfo DirPath = new DirectoryInfo(mo["InstallLocation"].ToString());                  
                        ListViewItem lvi = new ListViewItem(
                                 new string[] { mo["Name"].ToString(),(GetDirSize(DirPath) / 1024 / 1024).ToString() + " MB", mo["Vendor"].ToString(), mo["Version"].ToString() });
                        lsvListOfProgs.Items.Add(lvi);
                    }
                    catch (Exception ex)
                    {
                        //this program may not have a name property
                    }
                }      
        }

        private static long GetDirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += GetDirSize(di);
            }
            return (Size);
        }

        private bool UninstallProgram(string ProgramName)
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher(
                  "SELECT * FROM Win32_Product WHERE Name = '" + ProgramName + "'");
                foreach (ManagementObject mo in mos.Get())
                {
                    try
                    {
                        if (mo["Name"].ToString() == ProgramName)
                        {
                            object hr = mo.InvokeMethod("Uninstall", null);
                            return (bool)hr;
                        }
                    }
                    catch (Exception ex)
                    {
                        //this program may not have a name property, so an exception will be thrown
                    }
                }

                //was not found...
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void lsvListOfProgs_SelectedIndexChanged(object sender, EventArgs e)
        {
            vbtnUninstallProgs.Visible = true;
        }
        #endregion

        private void vbtnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtboxMoveFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void vbtnDelIncFolders_Click(object sender, EventArgs e)
        {
            
        }

    }
}
