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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Ionic.Zip;

namespace MobileDictMaker
{
    public partial class Form1 : Form
    {
        string appPath, tmpPath;
        string fileName;
        string[] src;

        public Form1()
        {
            InitializeComponent();         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            appPath = Application.StartupPath + @"\";
            tmpPath = appPath + @"\dist\";

            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            bool bAlert = false, bExist = false;

            openFileDialog1.ShowDialog(this);

            fileName = openFileDialog1.FileName;

            if (File.Exists(fileName))
            {
                src = File.ReadAllLines(fileName);
                
                for (int i = 0; i < src.Length; i++)
                    if (!src[i].Contains(tbSeparator.Text))
                    {
                        MessageBox.Show(this, "baris " + (i + 1) + " tanpa separator!", "Info",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        bAlert = true;
                        break;
                    }

                tbFileSrc.Text = fileName;
                
                if (listView1.Items.Count>0)
                    foreach (ListViewItem lsItem in listView1.Items)
                    {
                        int hash = fileName.GetHashCode();
                        if (hash < 0) hash *= -1;

                        if (lsItem.SubItems[1].Text.Equals(hash.ToString()))
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
            int n = 0, nPath=0;
            int hash = fileName.GetHashCode();
            string[] texts;
            string sPart = "", sIndex = "";
            string sFirst = "", sEnd = "";
            string dicPath = tmpPath + @"\dic\";

            if (!Directory.Exists(dicPath))
                Directory.CreateDirectory(dicPath);

            if (hash < 0) hash *= -1;

            btnAdd.Enabled = false;

            texts = src[0].Split(new string[] {tbSeparator.Text}, StringSplitOptions.None);
            sFirst = texts[0];

            for (int i = 0; i < src.Length; i++)
            {
                n++;

                texts = src[i].Split(new string[] { tbSeparator.Text }, StringSplitOptions.None);

                if (n > nudMaxLine.Value)
                {
                    nPath++;

                    File.WriteAllText(dicPath + hash.ToString() + "x" + nPath, sPart);

                    sIndex += sFirst + "#" + sEnd + "#" + nPath + "\n";

                    sPart = texts[0] + "#" + texts[1];
                    sFirst = texts[0];

                    n = 0;
                }
                else
                {
                    sPart += texts[0] + "#" + texts[1] + "\n";
                    sEnd = texts[0];
                }

            }

            if (sPart != "")
            {
                nPath++;
                File.WriteAllText(dicPath + hash.ToString() + "x" + nPath, sPart);
                sIndex += sFirst + "#" + sEnd + "#" + nPath + "\n";  
            }

            File.WriteAllText(tmpPath + hash.ToString(), sIndex);

            Application.DoEvents();
            btnAdd.Enabled = true;

            // tambahkan ke listview
            ListViewItem lsItem = listView1.Items.Add(tbDictName.Text);
            lsItem.SubItems.Add(tbDictInfo.Text);
            lsItem.SubItems.Add(hash.ToString());
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
                MessageBox.Show("Name or Description...?", "Build", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // # manifes
            string manifes = "Manifest-Version: 1.0\n" +
                            "Ant-Version: Apache Ant 1.8.2\n" +
                            "Created-By: 1.7.0_02-b13 (Oracle Corporation)\n" +
                            "MIDlet-1: Kabayan,/img/logo.png,bin.logic.Kabayan\n" +
                            "MIDlet-Vendor: Sofyan\n" +
                            "MIDlet-Info-URL: http://code.google.com/p/kabayan\n" +
                            "MIDlet-Name: {0}\n" +
                            "MIDlet-Description: {1}\n" +
                            "MIDlet-Version: 0.1.7\n" +
                            "MicroEdition-Configuration: CLDC-1.1\n" +
                            "MicroEdition-Profile: MIDP-2.0\n";

            string jadfile = "MIDlet-1: Kabayan,/img/logo.png,bin.logic.Kabayan\n" +
                            "MIDlet-Description: {0}\n" +
                            "MIDlet-Info-URL: http://code.google.com/p/kabayan\n" +
                            "MIDlet-Jar-Size: {1}\n" +
                            "MIDlet-Jar-URL: {2}\n" +
                            "MIDlet-Name: {3}\n" +
                            "MIDlet-Vendor: Sofyan\n" +
                            "MIDlet-Version: 0.1.7\n" +
                            "MicroEdition-Configuration: CLDC-1.1\n" +
                            "MicroEdition-Profile: MIDP-2.0\n";

            // save manifest
            btnBuild.Enabled = false;

            manifes = string.Format(manifes, tbMidletName.Text, tbMidletDesc.Text);

            if (!Directory.Exists(tmpPath + @"\META-INF\"))
                Directory.CreateDirectory(tmpPath + @"\META-INF\");

            File.WriteAllText(tmpPath + @"\META-INF\MANIFEST.MF", manifes);

            // save config
            string config = "";

            foreach (ListViewItem lsItem in listView1.Items)
                // name#hash|info
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

            // extract
            using (ZipFile zip = ZipFile.Read("dist.bin"))
            {
                foreach (ZipEntry z in zip)
                    z.Extract("dist");  // overwrite == true  
            }

            // zip
            using (ZipFile zip = new ZipFile(targetName))
            {
                zip.AddDirectory("dist", "");
                zip.Save();
            }  

            // make jad
            FileInfo fi = new FileInfo(appPath + targetName);

            jadfile = string.Format(jadfile, tbMidletDesc.Text, fi.Length, targetName, tbMidletName.Text);

            File.WriteAllText(appPath + tbMidletName.Text + ".jad", jadfile);

            // clean
            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);

            // finish
            Application.DoEvents();
            MessageBox.Show("Output : " + targetName, "Build", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

            // reset
            listView1.Items.Clear();
            controlBuild(false);

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
            btnBuild.Enabled = b;
        }

        

    }
}
