using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using System.IO;

namespace MicController
{
    public partial class Form1 : Form
    {
        private MMDevice defaultDevice;
        private static System.Timers.Timer timerCheck, timerSave;
        private Int32 globalChangeCounter = 0;
        private static NotifyIcon notifyIcon;
        private readonly String configFilename = "volume.txt";
        private readonly String configFolder = "MicController";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Create_KontextMenu();

            var enumerator = new MMDeviceEnumerator();
            defaultDevice = enumerator
                .GetDefaultAudioEndpoint(
                DataFlow.Capture,
                Role.Communications);

            lblDeviceName.Text = defaultDevice.FriendlyName;
            lblCurrVolume.Text = defaultDevice.AudioEndpointVolume.MasterVolumeLevel.ToString();

            timerCheck = new System.Timers.Timer(50);
            timerCheck.AutoReset = true;
            timerCheck.Elapsed += Timer_Check_Elapsed;
            timerCheck.Start();

            timerSave = new System.Timers.Timer(5000);
            timerSave.AutoReset = false;
            timerSave.Elapsed += Timer_Save_Elapsed;

            this.WindowState = FormWindowState.Minimized;

            LoadSettings();
        }

        private void Create_KontextMenu()
        {
            ContextMenu cm = new ContextMenu();
            MenuItem miCurr;
            int iIndex = 0;

            // Kontextmenüeinträge erzeugen
            miCurr = new MenuItem();
            miCurr.Index = iIndex++;
            miCurr.Text = "&Öffnen";
            miCurr.Click += new EventHandler(Show_Window);
            cm.MenuItems.Add(miCurr);

            // Kontextmenüeinträge erzeugen
            miCurr = new MenuItem();
            miCurr.Index = iIndex++;
            miCurr.Text = "&Beenden";
            miCurr.Click += new EventHandler(Exit_Clicked);
            cm.MenuItems.Add(miCurr);

            // NotifyIcon selbst erzeugen
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.mic; // Eigenes Icon einsetzen
            //notifyIcon.Text = "Doppelklick mich!";   // Eigenen Text einsetzen
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = cm;
            notifyIcon.DoubleClick += new EventHandler(Show_Window);
        }

        private void SaveSettings()
        {
            String defaultConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + configFolder;
            string absolutePath = defaultConfigFolder + Path.DirectorySeparatorChar + configFilename;

            if (!Directory.Exists(defaultConfigFolder))
            {
                Directory.CreateDirectory(defaultConfigFolder);
            }

            File.WriteAllText(absolutePath, numeric01.Text);
        }

        private void LoadSettings()
        {
            String defaultConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + configFolder;
            string absolutePath = defaultConfigFolder + Path.DirectorySeparatorChar + configFilename;

            if (File.Exists(absolutePath))
            {
                numeric01.Value = Convert.ToInt32(File.ReadAllText(absolutePath));
            }
        }

        private void Show_Window(object sender, System.EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void Exit_Clicked(object sender, System.EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void Timer_Check_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (lblCurrVolume.InvokeRequired)
            {
                lblCurrVolume.Invoke((MethodInvoker)delegate ()
                {
                    Manipulate_Volume();
                }
                );
            }
            else
            {
                Manipulate_Volume();
            }
            
        }

        private void Timer_Save_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerSave.Stop();
            SaveSettings();
        }

        private void Manipulate_Volume()
        {
            float oldVol = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            lblCurrVolume.Text = oldVol.ToString();

            float newVol = oldVol;
            float temp = 0;

            if (float.TryParse(numeric01.Text, out temp))
            {
                newVol = temp/100;
            }

            defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVol;

            if (oldVol != newVol)
            {
                globalChangeCounter++;
                lblChangeCounter.Text = globalChangeCounter.ToString();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void Numeric01_ValueChanged(object sender, EventArgs e)
        {
            globalChangeCounter = -1;
            timerSave.Stop();
            timerSave.Start();
        }

        
    }
}
