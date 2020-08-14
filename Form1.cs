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
using System.Collections.Generic;

namespace MacetimTools
{
    public partial class Form1 : Form
    {
        public static string ipV4;
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
            checkBox2.Checked = Properties.Settings.Default.cb2;
            if(checkBox2.Checked == true)
            {
                DisableDevice();
                comboBox1.SelectedIndex = Properties.Settings.Default.cbox1;
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
                    DisableAdapter("Ethernet");
                    System.Threading.Thread.Sleep(3000);
                    EnableAdapter("Ethernet");
                }
            }
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F12)
            {
                if (radioButton1.Checked == true)
                {
                    Process[] remoteByName = Process.GetProcessesByName("GTA5");
                    int teste = remoteByName[0].Id;
                    StopProcess.SuspendProcess(teste);
                    System.Threading.Thread.Sleep(10000);
                    StopProcess.ResumeProcess(teste);
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
                if(radioButton3.Checked == true)
                {
                    FirewallSet(true, "ATest");
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
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                DisableDevice();
                comboBox1.Enabled = true;
            }
            else
            {
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
                refreshList();
            }
            else
            {
                groupBox4.Enabled = false;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
                return;

            ipV4 = textBox2.Text;
            CheckRules("ATest");
            IpVerf();
            textBox2.Clear();
            textBox2.Focus();
            refreshList();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            IpRemove(listBox1.SelectedIndex);
            refreshList();
        }
        public void refreshList()
        {
            listBox1.DataSource = null;
            listBox1.Items.Clear();
            listBox1.DataSource = ListIP;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            hwh.CutLooseHardwareNotifications(this.Handle);
            hwh = null;
            Properties.Settings.Default.cb2 = checkBox2.Checked;
            Properties.Settings.Default.cbox1 = comboBox1.SelectedIndex;
            Properties.Settings.Default.radb1 = radioButton1.Checked;
            Properties.Settings.Default.radb2 = radioButton2.Checked;
            Properties.Settings.Default.radb3 = radioButton3.Checked;
            Properties.Settings.Default.Save();
        }
        }
}
