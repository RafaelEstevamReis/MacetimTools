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
using System.Speech.Synthesis.TtsEngine;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Reflection;
using System.Drawing;
using System.Data;

namespace MacetimTools
{
    public partial class Form1 : Form, ISharpUpdatable
    {
        public static string ipV4, exWay;
        public static int ipIndex = 0;
        int tempo = 0, hora = 0, minuto = 0, segundo = 0;
        HH_Lib hwh = new HH_Lib();
        KeyboardHook hook = new KeyboardHook();
        ComparateImage comparate = new ComparateImage();
        private SharpUpdate updater;
        public static bool versionX = false;

        public Form1()
        {
            InitializeComponent();

            this.labelVersion.Text = this.ApplicationAssembly.GetName().Version.ToString();
            updater = new SharpUpdate(this);
            updater.DoUpdate();

        }

        #region SharpUpdate
        public string ApplicationName
        {
            get { return "MacetimTools"; }
        }

        public string ApplicationID
        {
            get { return "MacetimTools"; }
        }

        public Assembly ApplicationAssembly
        {
            get { return Assembly.GetExecutingAssembly(); }
        }
        public Icon ApplicationIcon
        {
            get { return this.Icon; }
        }

        public Uri UpdateXmlLocation
        {
            get { return new Uri("https://raw.githubusercontent.com/h4rdrew/MacetimTools/master/update.xml"); }
        }

        public Form Context
        {
            get { return this; }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            // Verificacao se a pasta "Macetim" existe ou nao, se nao, ele cria.
            if (Directory.Exists("C:\\Program Files\\Macetim") == false)
            {
                Directory.CreateDirectory("C:\\Program Files\\Macetim\\True");
            }

            // register the event that is fired after the key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F1);  //--- ALT+F1: Digital Casino Heist Hotkey
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F9);  //--- ALT+F9: Network Disable Hotkey
            hook.RegisterHotKey(GlobalHotKey.ModifierKeys.Alt, Keys.F12); //--- ALT+F12: Solo Public Game Hotkey

            //dateTimePicker1.Format = DateTimePickerFormat.Custom;
            //dateTimePicker1.CustomFormat = "00:00:00"; //HH:mm:ss

            GetSettings(); //Carrega os dados que foram salvos em Form1_FormClosing()
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
                DeviceList();

                try
                {
                    comboBox1.SelectedIndex = Properties.Settings.Default.cbox1;
                }
                catch
                {
                    DeviceList();
                }
            }
        }
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            //--- ALT+F1: Digital Casino Heist Hotkey
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F1)
            {
                Directory.CreateDirectory("C:\\Program Files\\Macetim\\Temp");
                printpoint();
                comparate.image_comparate();
                VIMH(comparate.image_comparate());
            }
            //--- ALT+F9: Network Disable Hotkey
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F9)
            {
                //string[] devices = new string[1];

                //------Disable by Device-----
                if (checkBox2.Checked == true) 
                {
                    SetDisableDevice(false);
                }

                //------Disable by Network Name-------
                else
                {
                    SetDisableAdapter(false);
                }
            }
            //--- ALT+F12: Solo Public Game Hotkey
            if (e.Modifier == GlobalHotKey.ModifierKeys.Alt && e.Key == Keys.F12)
            {
                //--- Stop Process
                if (radioButton1.Checked == true)
                {
                    try
                    {
                        SetStopProcess(false);
                    }
                    catch
                    {
                        MessageBox.Show("'GTA5.exe' not found. Please check and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                //--- Firewall
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
                //--- Firewall w/Friends
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
        public void DeviceList()
        {
            string[] HardwareList = hwh.GetAll();
            foreach (string s in HardwareList)
            {
                comboBox1.Items.Add(s);
            }

            hwh.HookHardwareNotifications(this.Handle, true);
        }
        public void SetStopProcess(bool resume)
        {
            Process[] remoteByName = Process.GetProcessesByName("GTA5");
            int idProcess = remoteByName[0].Id;

            if (resume == false)
            {
                Class.StopProcess.SuspendProcess(idProcess);
                tempo = 10;
                timerSP.Enabled = true;
            }


            if (resume == true)
            {
                Class.StopProcess.ResumeProcess(idProcess);
            }
        }
        public void SetDisableDevice(bool resume)
        {
            string[] devices = new string[1];

            if (resume == false)
            {
                hwh.CutLooseHardwareNotifications(this.Handle);
                devices[0] = comboBox1.SelectedItem.ToString();
                hwh.SetDeviceState(devices, false);
                hwh.HookHardwareNotifications(this.Handle, true);

                tempo = 6;
                timerDD.Enabled = true;
            }

            if(resume == true)
            {
                hwh.CutLooseHardwareNotifications(this.Handle);
                devices[0] = comboBox1.SelectedItem.ToString();
                hwh.SetDeviceState(devices, true);
                hwh.HookHardwareNotifications(this.Handle, true);
            }
        }
        public void SetDisableAdapter(bool resume)
        {
            if (checkBox1.Checked == true)
            {
                if(resume == false)
                {
                    DisableAdapter(textBox1.Text);
                    tempo = 3;
                    timerNM.Enabled = true;
                }

                if(resume == true)
                {
                    EnableAdapter(textBox1.Text);
                }
            }
            else
            {
                if(resume == false)
                {
                    DisableAdapter(comboBox2.SelectedItem.ToString());
                    tempo = 3;
                    timerNM.Enabled = true;
                }

                if (resume == true)
                {
                    EnableAdapter(comboBox2.SelectedItem.ToString());
                }
            }
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case HardwareHelperLib.Native.WM_DEVICECHANGE:
                    {
                        if (m.WParam.ToInt32() == HardwareHelperLib.Native.DBT_DEVNODES_CHANGED)
                        {
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
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox1.Enabled = true;
                comboBox2.Enabled = false;
            }

            if (checkBox1.Checked == false)
            {
                textBox1.Enabled = false;
                comboBox2.Enabled = true;

                if (comboBox2.Items.Count == 0)
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
                DeviceList();
                comboBox1.Enabled = true;
            }

            if (checkBox2.Checked == false)
            {
                if (checkBox1.Checked == true)
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
            if (radioButton3.Checked == true)
            {
                groupBox4.Enabled = true;
                IpVerf();
                ipRulesRefreshList();
                if (FirewallCheckStatus("GTASoloFriends") == true)
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
            /* 
            Checando se o textbox2 ou textbox3 nao estao vazios ou nulos
            caso algum nao esteja no parametro requisitado, o processo retorna.

            Com devidos dados inseridos em textBox2 e textBox3, outras duas variaveis publicas
            estarao recebendos os valores.

            IPList() eh acionado para que possa armazenar os dados no arquivo IPList.txt

            CheckRules() eh acionado para verificar se existe tal regra, se nao, ele cria
            a regra com o nome pre-difinido e insere o IP enviado pelo usuario.

            IpVerf() eh acionado para organizar os IP existentes na regra do firewall e adicionar
            a uma lista publica ListIP.

            Sera limpo os textBox e em seguida foco no textBox2

            ipRulesRefreshList() eh acionado para atualizar a listBox1 com o novo IP.
            */

            #region SwitchCase de tratamento

            int caseSwitch = 0;

            if (string.IsNullOrEmpty(textBox2.Text) && string.IsNullOrEmpty(textBox3.Text))
            {
                caseSwitch = 1;
            }

            if (string.IsNullOrEmpty(textBox2.Text) && !string.IsNullOrEmpty(textBox3.Text))
            {
                caseSwitch = 2;
            }

            if (string.IsNullOrEmpty(textBox3.Text) && !string.IsNullOrEmpty(textBox2.Text))
            {
                caseSwitch = 3;
            }

            switch (caseSwitch)
            {
                case 1:
                    MessageBox.Show("O campo de IP e GTA5.exe n�o foram preenchidos.");
                    caseSwitch = 0;
                    return;
                case 2:
                    MessageBox.Show("O campo de IP n�o foi preenchido.");
                    caseSwitch = 0;
                    return;
                case 3:
                    MessageBox.Show("O campo do caminho do GTA5.exe precisa ser preenchido.");
                    caseSwitch = 0;
                    return;
                case 0:
                    break;
            }

            #endregion

            exWay = textBox3.Text;
            ipV4 = textBox2.Text;

            //IPList(ipV4, textBox4.Text);

            CheckRules("GTASoloFriends");

            IPList(textBox2.Text, textBox4.Text);
            IpVerf();
            textBox2.Clear();
            textBox4.Clear();
            textBox2.Focus();
            ipRulesRefreshList();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                IpRemove(listBox1.SelectedIndex);
                IpRemoveTXT(listBox1.SelectedIndex);
                ipRulesRefreshList();
            }
            catch
            {
                ipRulesRefreshList();
            }
        }
        public void ipRulesRefreshList()
        {
            // ---ipRulesRefreshList() eh responsavel por pegar os dados do IPList.txt e transferir para a listBox1 para ser visivel

            string path = @"C:\Program Files\Macetim\IPList.txt";
            listBox1.DataSource = null;
            listBox1.Items.Clear();

            try
            {
                List<string> IpsName = File.ReadAllLines(path).ToList();
                listBox1.DataSource = IpsName;
            }
            catch
            {
                File.CreateText(path);
            }

        }
        public void networkIdenRefreshList()
        {
            // ---networkIdenRefreshList() eh responsavel em atualizar a lista de redes disponiveis.

            comboBox2.DataSource = null;
            comboBox2.Items.Clear();
            comboBox2.DataSource = NetworkIdentifier();
        }
        public void IPList(string ipv4TB, string nameTB)
        {
            // ---IPList() eh responsavel em adicionar novas linhas ao arquivo IPList.txt de acordo com os dados inseridos

            List<string> myList = new List<string>();

            string path = @"C:\Program Files\Macetim\IPList.txt";
            string[] text = File.ReadAllLines(path);

            if (text.Length != 0)
            {
                File.AppendAllText(path, $"{ipv4TB}={nameTB}" + Environment.NewLine);
                string[] text2 = File.ReadAllLines(path);

                for (int i = 0; i < text2.Length; i++)
                {
                    myList.Add(text2[i]);
                }

                List<string> sortedIP = myList.OrderBy(number => number).ToList(); //Organizando os IP em ordem crescente.

                File.Delete(path);
                File.CreateText(path).Dispose();

                for (int i = 0; i < sortedIP.Count; i++)
                {
                    File.AppendAllText(path, $"{sortedIP[i]}" + Environment.NewLine);
                }
            }
            else
            {
                File.AppendAllText(path, $"{ipv4TB}={nameTB}" + Environment.NewLine);
            }
        }
        public void IpRemoveTXT(int position)
        {
            //--- IpRemoveTXT() eh respons�vel por remove uma linha do arquivo IPList.txt

            string path = @"C:\Program Files\Macetim\IPList.txt";
            string aux;
            List<string> IpsName = File.ReadAllLines(path).ToList();
            IpVerf();
            IpsName.RemoveAt(position);

            if (File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    for (int i = 0; i < IpsName.Count; i++)
                    {
                        aux = IpsName[i];
                        sw.WriteLine($"{aux}");
                    }
                }
            }
        }
        public void VIMH(string text)
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {

                // Configure the audio output.   
                synth.SetOutputToDefaultAudioDevice();

                // Create a PromptBuilder object and append a text string.  
                PromptBuilder song = new PromptBuilder();
                song.AppendText($"{text}");

                // Speak the contents of the prompt synchronously.  
                synth.Speak(song);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            updater.DoUpdate();

            if (versionX == true)
            {
                MessageBox.Show($"Its version is the most recent. Version: {ProductVersion}");
            }
        }
        private void timerSP_Tick(object sender, EventArgs e)
        {
            tempo--;

            if (tempo == 0)
            {
                timerSP.Enabled = false;
                SetStopProcess(true);
            }
        }
        private void timerDD_Tick(object sender, EventArgs e)
        {
            tempo--;

            if(tempo == 0)
            {
                timerDD.Enabled = false;
                SetDisableDevice(true);
            }
        }
        private void timerNM_Tick(object sender, EventArgs e)
        {
            tempo--;

            if (tempo == 0)
            {
                timerNM.Enabled = false;
                SetDisableAdapter(true);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            segundo--;

            if (segundo <= 0 && minuto > 0)
            {
                segundo = 59;
                minuto--;
            }

            if(segundo <=0 && minuto <= 0 && hora > 0)
            {
                hora--;
                minuto = 59;
                segundo = 59;
            }

            #region If Pra Caralho(Apenas teste)
            if (hora < 10)
            {
                if (minuto < 10)
                {
                    if (segundo < 10)
                    {
                        label14.Text = "0" + hora + ":" + "0" + minuto + ":" + "0" + segundo;
                    }
                    if (segundo > 9)
                    {
                        label14.Text = "0" + hora + ":" + "0" + minuto + ":" + segundo;
                    }
                }
                if (minuto > 9)
                {
                    if (segundo < 10)
                    {
                        label14.Text = "0" + hora + ":" + minuto + ":" + "0" + segundo;
                    }
                    if (segundo > 9)
                    {
                        label14.Text = "0" + hora + ":" + minuto + ":" + segundo;
                    }
                }
            }

            if (hora > 9)
            {
                if (minuto < 10)
                {
                    if (segundo < 10)
                    {
                        label14.Text = hora + ":" + "0" + minuto + ":" + "0" + segundo;
                    }
                    if (segundo > 9)
                    {
                        label14.Text = hora + ":" + "0" + minuto + ":" + segundo;
                    }
                }
                if (minuto > 9)
                {
                    if (segundo < 10)
                    {
                        label14.Text = hora + ":" + minuto + ":" + "0" + segundo;
                    }
                    if (segundo > 9)
                    {
                        label14.Text = hora + ":" + minuto + ":" + segundo;
                    }
                }
            }

            //if (minuto < 10)
            //{
            //    if (segundo < 10)
            //    {
            //        label14.Text = "0" + hora + ":" + "0" + minuto + ":" + "0" + segundo;
            //    }
            //    if (segundo > 9)
            //    {
            //        label14.Text = "0" + hora + ":" + "0" + minuto + ":" + segundo;
            //    }
            //}

            //if (minuto > 9)
            //{
            //    if (segundo < 10)
            //    {
            //        label14.Text = "0" + hora + ":" + minuto + ":" + "0" + segundo;
            //    }
            //    if (segundo > 9)
            //    {
            //        label14.Text = "0" + hora + ":" + minuto + ":" + segundo;
            //    }
            //}

            //if (segundo < 10)
            //{
            //    label14.Text = "0" + hora + ":" + "0" + minuto + ":" + "0" + segundo;
            //}

            //if (segundo > 9)
            //{
            //    label14.Text = "0" + hora + ":" + "0" + minuto + ":" + segundo;
            //}
            #endregion


            if (hora == 0 && minuto == 0 && segundo == 0)
            {
                timer1.Enabled = false;
                label14.Text = "00:00:00";
                VIMH(textBox6.Text);
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            /* 
            O usuario seta o 'GTA5.exe' aonde esta atualmente instalado.
            */

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.SafeFileName != "GTA5.exe")
                {
                    MessageBox.Show("Selecione o 'GTA5.exe'.");
                }
                else
                {
                    textBox3.Text = ofd.FileName;
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            hora = Convert.ToInt16(hourNumeric.Value);
            minuto = Convert.ToInt16(minuteNumeric.Value);
            segundo = Convert.ToInt16(secondNumeric.Value);

            timer1.Enabled = true;
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

