/* 
 * Copyright (C) 2011 A. Sofyan Wahyudin <Asfydien@gmail.com>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

namespace MobileDictMaker
{
    public partial class Form1 : Form
    {
        string appPath, tmpPath;
        string fileName, safeFileName;
        string[] src;

        public Form1()
        {
            InitializeComponent();         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            appPath = Application.StartupPath + "/";
            tmpPath = appPath + "/dist/";

            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            bool bAlert = false, bExist = false;

            openFileDialog1.ShowDialog(this);

            fileName = openFileDialog1.FileName;
            safeFileName = openFileDialog1.SafeFileName;

            if (File.Exists(fileName))
            {
                src = File.ReadAllLines(fileName);
                
                for (int i = 0; i < src.Length; i++)
                    if (!src[i].Contains(tbSeparator.Text))
                    {
                        MessageBox.Show(this, "Line " + (i + 1) + " without separator!", "Info",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        bAlert = true;
                        break;
                    }

                tbFileSrc.Text = fileName;
                
                if (listView1.Items.Count>0)
                    foreach (ListViewItem lsItem in listView1.Items)
                    {
                        int hash = safeFileName.GetHashCode();
                        if (hash < 0) hash *= -1;

                        if (lsItem.Tag.Equals(hash.ToString()))
                            bExist = true;
                    }

                if (fileName.Equals("") == false & bAlert == false & bExist == false)
                {
                    controlAdd(true);
                    btnAdd.Enabled = false;
                }
                    
            }  

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string sPart = "", sIndex = "", idxFrom = "", idxTo = "";
            string dicPath = tmpPath + "/dic/";

            string[] texts;

            int n = 0, nPath = 0;
            int hash = safeFileName.GetHashCode();

            if (hash < 0) hash *= -1;

            if (!Directory.Exists(dicPath))
                Directory.CreateDirectory(dicPath);

            

            btnAdd.Enabled = false;

            texts = src[0].Split(new string[] {tbSeparator.Text}, StringSplitOptions.None);
            idxFrom = texts[0];

            // dict number
            string dictNum = "0" + (listView1.Items.Count +1).ToString();

            for (int i = 0; i < src.Length; i++)
            {
                n++;

                texts = src[i].Split(new string[] { tbSeparator.Text }, StringSplitOptions.None);

                if (n > nudMaxLine.Value)
                {
                    nPath++;

                    File.WriteAllText(dicPath + dictNum + "x" + nPath, sPart);

                    sIndex += idxFrom + "#" + idxTo + "#" + nPath + "\n";

                    sPart = texts[0] + "#" + texts[1];
                    idxFrom = texts[0];

                    n = 0;
                }
                else
                {
                    sPart += texts[0] + "#" + texts[1] + "\n";
                    idxTo = texts[0];
                }

            }

            if (sPart != "")
            {
                nPath++;
                File.WriteAllText(dicPath + dictNum + "x" + nPath, sPart);
                sIndex += idxFrom + "#" + idxTo + "#" + nPath + "\n";  
            }

            File.WriteAllText(tmpPath + dictNum, sIndex);

            Application.DoEvents();
            btnAdd.Enabled = true;

            // tambahkan ke listview
            ListViewItem lsItem = listView1.Items.Add(tbDictName.Text);
            lsItem.Tag = hash.ToString();
            lsItem.SubItems.Add(tbDictInfo.Text);
            lsItem.SubItems.Add(dictNum);
            lsItem.SubItems.Add(src.Length.ToString());
            lsItem.SubItems.Add(nPath.ToString());

            // reset
            src = null;
            tbFileSrc.Text = "";
            tbDictName.Text = "";
            tbDictInfo.Text = "";

            controlAdd(false);
            controlBuild(true);

        }

        private void btnBuild_Click(object sender, EventArgs e)
        {

            if (tbMidletName.Text.Trim().Length == 0 & tbMidletName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Name or Description...?", "Build", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // # manifes
            string manifes = "Manifest-Version: 1.0\n" +
                            "Ant-Version: Apache Ant 1.8.2\n" +
                            "Created-By: 1.7.0 (Oracle Corporation)\n" +
                            "MIDlet-1: Kabayan,/img/logo.png,bin.logic.Kabayan\n" +
                            "MIDlet-Vendor: Sofyan\n" +
                            "MIDlet-Info-URL: http://code.google.com/p/kabayan\n" +
                            "MIDlet-Name: {0}\n" +
                            "MIDlet-Description: {1}\n" +
                            "MIDlet-Version: {2}\n" +
                            "MicroEdition-Configuration: CLDC-1.1\n" +
                            "MicroEdition-Profile: MIDP-2.0\n";

            string jadfile = "MIDlet-1: Kabayan,/img/logo.png,bin.logic.Kabayan\n" +
                            "MIDlet-Description: {0}\n" +
                            "MIDlet-Info-URL: http://code.google.com/p/kabayan\n" +
                            "MIDlet-Jar-Size: {1}\n" +
                            "MIDlet-Jar-URL: {2}\n" +
                            "MIDlet-Name: {3}\n" +
                            "MIDlet-Vendor: Sofyan\n" +
                            "MIDlet-Version: {4}\n" +
                            "MicroEdition-Configuration: CLDC-1.1\n" +
                            "MicroEdition-Profile: MIDP-2.0\n";

            string config = "";
            FastZip fz = new FastZip();

            // save manifest
            btnBuild.Enabled = false;

            manifes = string.Format(manifes, tbMidletName.Text, tbMidletDesc.Text, "0.1.6");

            if (!Directory.Exists(tmpPath + "/META-INF/"))
                Directory.CreateDirectory(tmpPath + "/META-INF/");

            File.WriteAllText(tmpPath + "/META-INF/MANIFEST.MF", manifes);

            // save config
            foreach (ListViewItem lsItem in listView1.Items)
                config += lsItem.SubItems[0].Text + "#" + lsItem.SubItems[2].Text + "|" + lsItem.SubItems[1].Text + "\n";

            File.WriteAllText(tmpPath + "config", config);

            // #
            Application.DoEvents();

            string targetName = tbMidletName.Text + ".jar";

            // delete old
            try
            {
                if (File.Exists(appPath + targetName))
                    File.Delete(appPath + targetName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Build", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnBuild.Enabled = true;
                return;
            }

            // mono
            Directory.SetCurrentDirectory(appPath);

            // extract resource
            switch (cboxTemplate.SelectedIndex){
                case 0: 
                    File.WriteAllBytes("dist_", MobileDictMaker.Properties.Resources.Karmix_0_1_6);
                    break;
                case 1:
                    File.WriteAllBytes("dist_", MobileDictMaker.Properties.Resources.Kabayan_0_1_7);
                    break;

            }

            fz.ExtractZip("dist_", "dist", "");

            // zip
            fz.CreateZip(targetName, "dist", true, "");

            // make jad and confirm
            try
            {
                FileInfo fi = new FileInfo(appPath + targetName);

                jadfile = string.Format(jadfile, tbMidletDesc.Text, fi.Length, targetName, tbMidletName.Text, "0.1.6");

                File.WriteAllText(appPath + tbMidletName.Text + ".jad", jadfile);

                Application.DoEvents();

                if (fi.Length > 0)
                    MessageBox.Show("Output : " + targetName, "Build",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }

            // clean
            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);

            if (File.Exists("dist_"))
                File.Delete("dist_");

            // reset
            listView1.Items.Clear();
            controlBuild(false);
            fileName = "";
            safeFileName = "";

        }

        private void tbDictName_TextChanged(object sender, EventArgs e)
        {
            if (tbDictName.Text.Length > 0 & tbDictInfo.Text.Length > 0)
                btnAdd.Enabled = true;
            else
                btnAdd.Enabled = false;
        }

        private void tbDictInfo_TextChanged(object sender, EventArgs e)
        {
            tbDictName_TextChanged(sender, e);
        }

        // 
        void controlAdd(bool b)
        {
            tbDictName.Enabled = b;
            tbDictInfo.Enabled = b;
            nudMaxLen.Enabled = b;
            nudMaxLine.Enabled = b;
            btnAdd.Enabled = b;
        }

        void controlBuild(bool b)
        {
            listView1.Enabled = b;
            tbMidletName.Enabled = b;
            tbMidletDesc.Enabled = b;
            cboxTemplate.Enabled = b;
            btnBuild.Enabled = b;
        }


    }
}
