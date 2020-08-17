using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using MacetimTools.Class;
using static MacetimTools.Class.GlobalHotKey;
using static MacetimTools.Class.SpecificPrint;
using static MacetimTools.Class.DisableEthernet;
using static MacetimTools.Class.FirewallRules;
using HardwareHelperLib;

namespace MacetimTools
{
    public partial class Form1 : Form
    {
        public static string ipV4, exWay;
        public static int ipIndex = 0;
        HH_Lib hwh = new HH_Lib();
        KeyboardHook hook = new KeyboardHook();
        ComparateImage comparate = new ComparateImage();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Directory.Exists("C:\\Program Files\\Macetim") == false)
            {
                Directory.CreateDirectory("C:\\Program Files\\Macetim\\True");
            }

            // register the event that is fired after the key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F1); //Macetim da Digital
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F9); //Desconectar a internet (rede Ethernet por enquanto) e reconectar depois de 3sec
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F10);
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F12);  //Macetim de ficar solo na sess�o
            
            GetSettings();
        }
        public void GetSettings()
        {
            radioButton1.Checked = Properties.Settings.Default.radb1;
            radioButton2.Checked = Properties.Settings.Default.radb2;
            radioButton3.Checked = Properties.Settings.Default.radb3;
            checkBox1.Checked = Properties.Settings.Default.cb1;
            checkBox2.Checked = Properties.Settings.Default.cb2;
            textBox3.Text = Properties.Settings.Default.txtbx3;
            textBox1.Text = Properties.Settings.Default.txtbx1;
            

            if (checkBox1.Checked == false)
            {
                textBox1.Enabled = false;
                networkIdenRefreshList();

                try
                {
                    comboBox2.SelectedIndex = Properties.Settings.Default.cbox2;
                }
                catch
                {
                    networkIdenRefreshList();
                }
            }

            if (checkBox2.Checked == true)
            {
                DisableDevice();

                try
                {
                    comboBox1.SelectedIndex = Properties.Settings.Default.cbox1;
                }
                catch
                {
                    DisableDevice();
                }
            }
        }
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // show the keys pressed in a label.
            //label1.Text = e.Modifier.ToString() + " + " + e.Key.ToString();
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F1)
            {
                Directory.CreateDirectory("C:\\Program Files\\Macetim\\Temp");
                printpoint();
                comparate.image_comparate();
            }
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F9)
            {
                string[] devices = new string[1];
                if (checkBox2.Checked == true)
                {
                    //Disable
                    hwh.CutLooseHardwareNotifications(this.Handle);
                    devices[0] = comboBox1.SelectedItem.ToString();
                    hwh.SetDeviceState(devices, false);
                    hwh.HookHardwareNotifications(this.Handle, true);
                    //-----------------------------------------------
                    System.Threading.Thread.Sleep(3000);
                    //Enable
                    hwh.CutLooseHardwareNotifications(this.Handle);
                    devices[0] = comboBox1.SelectedItem.ToString();
                    hwh.SetDeviceState(devices, true);
                    hwh.HookHardwareNotifications(this.Handle, true);
                }
                else
                {
                    if(checkBox1.Checked == true)
                    {
                        DisableAdapter(textBox1.Text);
                        System.Threading.Thread.Sleep(3000);
                        EnableAdapter(textBox1.Text);
                    }
                    else
                    {
                        DisableAdapter(comboBox2.SelectedItem.ToString());
                        System.Threading.Thread.Sleep(3000);
                        EnableAdapter(comboBox2.SelectedItem.ToString());
                    }
                }
            }
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F12)
            {
                if (radioButton1.Checked == true)
                {
                    try
                    {
                        Process[] remoteByName = Process.GetProcessesByName("GTA5");
                        int idProcess = remoteByName[0].Id;
                        StopProcess.SuspendProcess(idProcess);
                        System.Threading.Thread.Sleep(10000);
                        StopProcess.ResumeProcess(idProcess);
                    }
                    catch
                    {
                        MessageBox.Show("'GTA5.exe' not found. Please check and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (radioButton2.Checked == true)
                {
                    CheckRules("Block Internet");
                    if (indicador == true)
                    {
                        FirewallSet(true, "Block Internet");
                        System.Threading.Thread.Sleep(10000);
                        FirewallSet(false, "Block Internet");
                    }
                }
                if (radioButton3.Checked == true)
                {
                    if (FirewallCheckStatus("GTASoloFriends") == true)
                    {
                        FirewallSet(false, "GTASoloFriends");
                        pictureBox2.Image = MacetimTools.Properties.Resources.uncheck;
                    }
                    else
                    {
                        FirewallSet(true, "GTASoloFriends");
                        pictureBox2.Image = MacetimTools.Properties.Resources.check;
                    }
                }
            }
        }
        public void DisableDevice()
        {
            string[] HardwareList = hwh.GetAll();
            foreach (string s in HardwareList)
            {
                comboBox1.Items.Add(s);
            }

            hwh.HookHardwareNotifications(this.Handle, true);
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case HardwareHelperLib.Native.WM_DEVICECHANGE:
                    {
                        if (m.WParam.ToInt32() == HardwareHelperLib.Native.DBT_DEVNODES_CHANGED)
                        {
                            //comboBox1.Items.Clear();
                            string[] HardwareList = hwh.GetAll();
                            foreach (string s in HardwareList)
                            {
                                comboBox1.Items.Add(s);
                            }
                        }
                        break;
                    }
            }
            base.WndProc(ref m);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Notas notas = new Notas();
            notas.Show();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                textBox1.Enabled = true;
                comboBox2.Enabled = false;
            }

            if (checkBox1.Checked == false)
            {
                textBox1.Enabled = false;
                comboBox2.Enabled = true;

                if(comboBox2.Items.Count == 0)
                {
                    networkIdenRefreshList();
                }
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                //-----------------------
                comboBox2.Enabled = false;
                checkBox1.Enabled = false;
                textBox1.Enabled = false;
                //-----------------------
                DisableDevice();
                comboBox1.Enabled = true;
            }

            if (checkBox2.Checked == false)
            {
                if(checkBox1.Checked == true)
                {
                    textBox1.Enabled = true;
                    comboBox2.Enabled = false;
                }

                if (checkBox1.Checked == false)
                {
                    textBox1.Enabled = false;
                    comboBox2.Enabled = true;
                }

                checkBox1.Enabled = true;
                comboBox1.Enabled = false;
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton3.Checked == true)
            {
                groupBox4.Enabled = true;
                IpVerf();
                ipRulesRefreshList();
                if(FirewallCheckStatus("GTASoloFriends") == true)
                {
                    pictureBox2.Visible = true;
                    pictureBox2.Image = MacetimTools.Properties.Resources.check;
                }
                else
                {
                    pictureBox2.Visible = true;
                    pictureBox2.Image = MacetimTools.Properties.Resources.uncheck;
                }
            }
            else
            {
                groupBox4.Enabled = false;
                pictureBox2.Visible = false;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
                
                return;

            exWay = textBox3.Text;
            ipV4 = textBox2.Text;
            CheckRules("GTASoloFriends");
            IpVerf();
            textBox2.Clear();
            textBox2.Focus();
            ipRulesRefreshList();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            IpRemove(listBox1.SelectedIndex);
            ipRulesRefreshList();
        }
        public void ipRulesRefreshList()
        {
            listBox1.DataSource = null;
            listBox1.Items.Clear();
            listBox1.DataSource = ListIP;
        }
        public void networkIdenRefreshList()
        {
            comboBox2.DataSource = null;
            comboBox2.Items.Clear();
            comboBox2.DataSource = NetworkIdentifier();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            hwh.CutLooseHardwareNotifications(this.Handle);
            hwh = null;
            Properties.Settings.Default.cb1 = checkBox1.Checked;
            Properties.Settings.Default.cb2 = checkBox2.Checked;
            Properties.Settings.Default.cbox1 = comboBox1.SelectedIndex;
            Properties.Settings.Default.cbox2 = comboBox2.SelectedIndex;
            Properties.Settings.Default.radb1 = radioButton1.Checked;
            Properties.Settings.Default.radb2 = radioButton2.Checked;
            Properties.Settings.Default.radb3 = radioButton3.Checked;
            Properties.Settings.Default.txtbx3 = textBox3.Text;
            Properties.Settings.Default.txtbx1 = textBox1.Text;
            Properties.Settings.Default.Save();
        }
    }
}
