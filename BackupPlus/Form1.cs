using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using WinSCP;


namespace BackupPlus
{
    public partial class Form1 : Form
    {
        int btn1Togg = 1;
        int btn2Togg = 0;
        int timeleft = 0;
        int intervalT = 0;
        Dictionary<string, string> confList;
        List<Server> serversL;
        string logs = "";
        string prvLog = "";
        string installPath = "";

        List<String> FileNameList;
       
        public Form1()
        {
            InitializeComponent();
            if (btn1Togg ==1)
            {
                toolStripButton2.Checked = true;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            
            tabControl1.SelectedIndex = 0;
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            treeView1.ImageIndex = 0;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(AppContext.BaseDirectory + "\\config.xml");
                confList = new Dictionary<string, string>();
                XmlNodeList setings = doc.GetElementsByTagName("appSettings");
                XmlNodeList servers = doc.GetElementsByTagName("Servers");

                foreach (XmlNode node in setings[0].ChildNodes)
                {

                    if (node.NodeType == XmlNodeType.Element)
                    {
                        confList.Add(node.Attributes[0].Value, node.Attributes[1].Value);
                    }


                }

                if (confList.Count > 0)
                {
                    foreach (var item in confList)
                    {
                        switch (item.Key)
                        {
                            case "Server":
                                textBox1.Text = item.Value;
                                textBox1.Enabled = false;
                                break;
                            case "Username":
                                textBox2.Text = item.Value;
                                textBox2.Enabled = false;
                                break;
                            case "Password":
                                textBox3.Text = item.Value;
                                textBox3.Enabled = false;
                                break;
                            case "ClientF":
                                textBox4.Text = item.Value;
                                break;
                            case "Fpath":
                                textBox9.Text = item.Value;
                                textBox9.Enabled = false;
                                break;
                            case "Service":
                                if (item.Value == "0")
                                {
                                    checkBox2.Checked = false;
                                }
                                else
                                {
                                    checkBox2.Checked = true;
                                }
                                break;
                            case "IntH":
                                textBox5.Text = item.Value;
                                break;
                            case "IntM":
                                textBox6.Text = item.Value;
                                break;
                            case "IntS":
                                textBox7.Text = item.Value;
                                break;
                            case "Cleanup":
                                textBox10.Text = item.Value;
                                break;
                            case "Keep":
                                textBox11.Text = item.Value;
                                break;
                            case "ClName":
                                textBox13.Text = item.Value;
                                break;
                            case "Level":
                                if (item.Value == "0")
                                {
                                    radioButton3.Checked = true;
                                }
                                else
                                {
                                    radioButton3.Checked = false;
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    logs = "All params were loaded...";

                }
                else
                {
                    logs = "Error loading parametes";
                }

                //===Loading Servers information
                serversL = new List<Server>();

                foreach (XmlNode node in servers[0].ChildNodes)
                {

                    if (node.NodeType == XmlNodeType.Element)
                    {

                        StringReader strReader = null;
                        XmlSerializer serializer = null;
                        XmlTextReader xmlReader = null;
                        Object obj = null;
                        try
                        {
                            strReader = new StringReader(node.OuterXml);
                            serializer = new XmlSerializer(typeof(Server));
                            xmlReader = new XmlTextReader(strReader);
                            obj = serializer.Deserialize(xmlReader);
                            serversL.Add((Server)obj);
                        }
                        catch (Exception exp)
                        {
                            //Handle Exception Code
                        }
                        finally
                        {
                            if (xmlReader != null)
                            {
                                xmlReader.Close();
                            }
                            if (strReader != null)
                            {
                                strReader.Close();
                            }
                        }


                    }


                }

                //===============================

                textBox8.Text += logs + Environment.NewLine;
                saveLog(logs);
                logs = "";

                timer3.Interval = Convert.ToInt32(confList["Cleanup"]) * 60000;
                updateTreeView();
            }
            catch (Exception ex)
            {

                saveLog(ex.ToString());
            }

            

            //====Loading NIC cards information

            ArrayList nics = NetworkManagement.GetNICNames();

            foreach (var item in nics)
            {
                comboBox2.Items.Add(item);
            }


        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            toolStripButton1.Checked = true;
            toolStripButton2.Checked = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox9.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox9.Enabled = false;
            }
            
        }

        

        private void button1_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < confList.Count; i++)
            {
                switch (confList.ElementAt(i).Key)
                {
                    case "Server":
                        confList["Server"] = textBox1.Text;
                        break;
                    case "Username":
                        confList["Username"] = textBox2.Text;
                        break;
                    case "Password":
                        confList["Password"] = textBox3.Text;
                        break;
                    case "ClientF":
                        confList["ClientF"] = textBox4.Text;
                        break;
                    case "Service":
                        if (checkBox2.Checked)
                        {
                            confList["Service"] = "1";
                        }
                        else
                        {
                            confList["Service"] = "0";
                        }
                        break;
                    case "IntH":
                        confList["IntH"] = textBox5.Text;
                        break;
                    case "Fpath":
                        confList["Fpath"] = textBox9.Text;
                        break;
                    case "IntM":
                        confList["IntM"] = textBox6.Text;
                        break;
                    case "IntS":
                        confList["IntS"] = textBox7.Text;
                        break;
                    case "Cleanup":
                        confList["Cleanup"] = textBox10.Text;
                        break;
                    case "Keep":
                        confList["Keep"] = textBox11.Text;
                        break;
                    case "ClName":
                        confList["ClName"] = textBox13.Text;
                        break;
                    case "Level":
                        if (radioButton3.Checked)
                        {
                            confList["Level"] = "0";
                        }
                        else
                        {
                            confList["Level"] = "1";
                        }
                        break;
                    default:
                        break;
                }
            }
           


            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(AppContext.BaseDirectory + "\\config.xml");
            XmlNodeList setings = doc.GetElementsByTagName("appSettings");

            if (setings.Count > 0)
            {

                doc.GetElementsByTagName("appSettings").Item(0).RemoveAll();

            }
            foreach (var elem in confList)
            {

                    XmlNode newnode = doc.CreateNode(XmlNodeType.Element, "add", null);
                    //Create a new attribute
                    XmlAttribute attr1 = doc.CreateAttribute("key");
                    attr1.Value = elem.Key;

                    XmlAttribute attr2 = doc.CreateAttribute("value");
                    attr2.Value = elem.Value;

                    //Add the attribute to the node     
                    newnode.Attributes.SetNamedItem(attr1);
                    newnode.Attributes.SetNamedItem(attr2);
                    XmlWhitespace ws = doc.CreateWhitespace("\r\n\t");
                    doc.GetElementsByTagName("appSettings").Item(0).AppendChild(ws);
                    doc.GetElementsByTagName("appSettings").Item(0).AppendChild(newnode);

            }
            XmlWhitespace wse = doc.CreateWhitespace("\r\n");
            doc.GetElementsByTagName("appSettings").Item(0).AppendChild(wse);
            doc.PreserveWhitespace = true;
            doc.Save(AppContext.BaseDirectory + "\\config.xml");

            logs = "All params were saved...";
            textBox8.Text += logs + Environment.NewLine;
            saveLog(logs);
            logs = "";

            button4.Enabled = true;

            timer3.Interval = Convert.ToInt32(confList["Cleanup"]) * 60000;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            var uri = new Uri("ftp://" + confList["Server"]);
            FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(uri);
            requestDir.Method = WebRequestMethods.Ftp.ListDirectory;
            requestDir.Credentials = new NetworkCredential(confList["Username"], confList["Password"]);
            try
            {
                WebResponse response = requestDir.GetResponse();
                logs = "Connection Established: " + response.ToString();
            }
            catch (WebException ex)
            {
                logs = "Error in connection : " + ex.ToString(); 
            }

            
            textBox8.Text += logs + Environment.NewLine;
            saveLog(logs);
            textBox8.Text += logs;
            logs = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                confList["ClientF"] = folderBrowserDialog1.SelectedPath;
                textBox4.Text = confList["ClientF"];
                button4.Enabled = false;
                MessageBox.Show("Please Save Changes before Start", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            int interval = (Convert.ToInt32(textBox5.Text)*3600000) + (Convert.ToInt32(textBox6.Text) * 60000 ) + Convert.ToInt32(textBox7.Text)*1000;
            intervalT = interval / 1000;
            timeleft = intervalT;
            if (interval ==0)
            {
                interval = 10000;
            }

            if (timer1.Enabled)
            {
                button4.Text = "Start";
                button4.BackColor = Color.LimeGreen;
                timer1.Enabled = false;
                
               
            }
           else
            {
                timer1.Interval = interval;
                button4.Text = "Stop";
                button4.BackColor = Color.Red;
                timer1.Enabled = true;
                

            }

        }

        private void timedSending(object Sender, EventArgs e)
        {
            logs += "Sending Files.......";
            label10.Visible = true;
            textBox8.Text += logs + Environment.NewLine;
            saveLog(logs);
            logs = "";
            var uri = new Uri("ftp://" + confList["Server"]);
            /* Create Object Instance */
            ftp ftpClient = new ftp(uri.ToString(), confList["Username"], confList["Password"]);
            string newFolder = confList["Fpath"] + "/" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

            if (radioButton3.Checked)
            {
                newFolder = newFolder +"_"+textBox13.Text+ "_UPPER";
            }
            else
            {
                newFolder = newFolder + "_"+textBox13.Text + "_LOWER";
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            ftpClient.createDirectory(newFolder);
            recursiveDirectory(textBox4.Text, newFolder, ftpClient);

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            updateTreeView();
            label10.Visible = false;
        }

        private void recursiveDirectory(string dirPath, string uploadPath, ftp ftpClient)
        {
            string[] files = Directory.GetFiles(dirPath, "*.*");
            string[] subDirs = Directory.GetDirectories(dirPath);

            foreach (string file in files)
            {
                string dateCreat = ftpClient.getFileCreatedDateTime(uploadPath + "/" + Path.GetFileName(file));
                if (dateCreat != File.GetCreationTime(file).ToString())
                {
                    ftpClient.delete(uploadPath + "/" + Path.GetFileName(file));
                    logs = ftpClient.upload(uploadPath + "/" + Path.GetFileName(file), file);
                }
                else
                {
                    //logs = ftpClient.upload(uploadPath + "/" + Path.GetFileName(file), file);
                }
                
                textBox8.Text += logs + Environment.NewLine;
                saveLog(logs);
                logs = "";
            }

            foreach (string subDir in subDirs)
            {
                ftpClient.createDirectory(uploadPath + "/" + Path.GetFileName(subDir));
                recursiveDirectory(subDir, uploadPath + "/" + Path.GetFileName(subDir),ftpClient);
            }
        }

       

        private void timer2_Tick(object sender, EventArgs e)
        {
           
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }


        public void saveLog(string log)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(AppContext.BaseDirectory + "\\logs.txt", true))
            {
                file.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "----" + log);
            }

        }

        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveLog("System Closed");
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            List<DateClass> dateList = new List<DateClass>(); 
            var uri = new Uri("ftp://" + confList["Server"]);
            /* Create Object Instance */
            ftp ftpClient = new ftp(uri.ToString(), confList["Username"], confList["Password"]);

            var dirStr = ftpClient.directoryListDetailed(confList["Fpath"]);

            if (dirStr.Count() > 3)
            {
                foreach (var item in dirStr)
                {
                    if (item!="")
                    {
                        var cdate = creationDate(item);
                        dateList.Add(cdate);
                    }
                   
                }

                dateList.Sort(delegate (DateClass x, DateClass y)
                {
                    return y.FullDate.CompareTo(x.FullDate);
                });

                //Using WinSCP for deleting directories
                string bareServer = confList["Server"].Split(':')[0];

                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = bareServer,
                    UserName = confList["Username"],
                    Password = confList["Password"],
                };
                sessionOptions.AddRawSettings("ProxyPort", "0");
                int sizeKeep = 1;
                sizeKeep = Convert.ToInt32(confList["Keep"]);

                for (int i = sizeKeep; i < dateList.Count; i++)
                {
                    
                    using (Session session = new Session())
                    {
                        // Connect
                        session.Open(sessionOptions);

                        // Delete folder
                        session.RemoveFiles(confList["Fpath"] + "/" + dateList[i].Name).Check();
                    }
                    logs += "Directory:--" + dateList[i].Name + "-- was removed"+ Environment.NewLine;
                    saveLog(logs);
                    textBox8.Text += logs;
                    logs = "";

                }

                updateTreeView();
            }

            
           
        }

        public DateClass creationDate(string info)
        {
            var spl = info.Split(' ');

            DateClass retVal = new DateClass();
            List<string> datest = new List<string>();  

            foreach (var item in spl)
            {
                if (item !="")
                {
                    datest.Add(item);
                }
            }
            retVal.Month = datest[5];
            retVal.Day = datest[6];
            retVal.Hour = datest[7];
            retVal.Name = datest[8];
            string month = "01";

            switch (retVal.Month)
            {
                case "Jan":
                    month = "01";
                    break;
                case "Feb":
                    month = "02";
                    break;
                case "Mar":
                    month = "03";
                    break;
                case "Apr":
                    month = "04";
                    break;
                case "May":
                    month = "05";
                    break;
                case "Jun":
                    month = "06";
                    break;
                case "Jul":
                    month = "07";
                    break;
                case "Aug":
                    month = "08";
                    break;
                case "Sep":
                    month = "09";
                    break;
                case "Oct":
                    month = "10";
                    break;
                case "Nov":
                    month = "11";
                    break;
                case "Dec":
                    month = "12";
                    break;

                default:
                    break;
            }

            string dateInput = "2019" + "-"  + month + "-" + retVal.Day + "T" + retVal.Hour+":"+"00";
            DateTime parsedDate = DateTime.Parse(dateInput);
            retVal.FullDate = parsedDate;
            return retVal;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox8.Text = "";
        }


        // tab2=========================================================================================================


        private void button6_Click(object sender, EventArgs e)
        {
            string bareServer = confList["Server"].Split(':')[0];
            ArrayList filesShow = ListFiles(bareServer, confList["Username"], confList["Password"], confList["Fpath"]);

            if (filesShow.Count >0)
            {
                int i = 0;
                foreach (var item in filesShow)
                {
                    listBox1.Items.Add(item);
                    i++;
                }
            }

        }

        private ArrayList ListFiles(String HostName, String UserName, String Password, String remotePath)
        {
            //int result = 0;
            Session session = null;
            ArrayList fileNameList = new ArrayList();
            try
            {
                // Setup session options               
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = HostName,
                    UserName = UserName,
                    Password = Password,
                    //HostName =  "119.59.73.48",
                    //UserName = "ftp@yourzine.com.cn",
                    //Password =  "989898qw",
                    //  SshHostKeyFingerprint = "ssh-rsa 1024 xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx"
                };

                using (session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);



                    RemoteDirectoryInfo dirInfo = session.ListDirectory(remotePath);

                    foreach (RemoteFileInfo folder in dirInfo.Files)
                    {

                        if (folder.IsDirectory&& folder.Name!="..")
                        {
                            fileNameList.Add(folder.Name);
                        }
                        // Console.WriteLine("File Name: {0},LastWriteTime: {1},IsDirectory: {2},File Length: {3},", file.Name, file.LastWriteTime, file.IsDirectory, file.Length);
                    }

                }

                // return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                //  return 1;
            }
            finally
            {
                if (session != null)
                {
                    session.Dispose();
                }
            }

            return fileNameList;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                installPath = folderBrowserDialog1.SelectedPath;
                textBox12.Text = installPath;
                button7.Enabled = true;
               
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            try
            {

                //Using WinSCP for deleting directories
                string bareServer = confList["Server"].Split(':')[0];

                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = bareServer,
                    UserName = confList["Username"],
                    Password = confList["Password"],
                };
                sessionOptions.AddRawSettings("ProxyPort", "0");

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    string servName = radioButton1.Checked ? "LabZ_Server_Upper" : "LabZ_Server_Lower";
                    // Transfer files
                    session.GetFiles(confList["Fpath"]+"/"+listBox1.SelectedItem, installPath+'\\'+servName).Check();
                    saveLog("Directory:--" + listBox1.SelectedItem + "--Downloaded to:--" + installPath + "--as:--" + servName);
                    MessageBox.Show("All files were download to:--" + installPath, "Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                }

            }
            catch (Exception ex)
            {

                logs = ex.Message;
                saveLog(logs);
                logs = "";
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            updateTreeView();
        }

        public void updateTreeView()
        {
            treeView1.Nodes.Clear();
            string bareServer = confList["Server"].Split(':')[0];
            ArrayList filesShow = ListFiles(bareServer, confList["Username"], confList["Password"], confList["Fpath"]);

            if (filesShow.Count > 0)
            {
                TreeNode rootNode = new TreeNode("Backups");
                treeView1.Nodes.Add(rootNode);
                TreeNode[] myTreeNodeArray = new TreeNode[filesShow.Count];
                int index = 0;
                foreach (var item in filesShow)
                {
                    treeView1.Nodes[0].Nodes.Add(new TreeNode(item.ToString(), 0, 0));
                    index++;
                }

            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Server serv = new Server();
            serv.name = "Upper";
            serv.ip = "192.168.2.102";
            serv.mask = "255.255.0.0";
            serv.dns = "";

            string xml = XmlClass.GetXMLFromObject(serv);



        }

        private void button12_Click(object sender, EventArgs e)
        {

        }
    }
}
