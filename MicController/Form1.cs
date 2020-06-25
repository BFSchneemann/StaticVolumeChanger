using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace MicController
{
    public partial class Form1 : Form
    {
        private MMDevice defaultDevice;
        private static System.Timers.Timer timer;
        private Int32 globalChangeCounter = 0;
        private static NotifyIcon notifyIcon;

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

            timer = new System.Timers.Timer(50);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            this.WindowState = FormWindowState.Minimized;
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

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (lblCurrVolume.InvokeRequired)
            {
                lblCurrVolume.Invoke((MethodInvoker)delegate ()
                {
                    Manipulate_Timer();
                }
                );
            }
            else
            {
                Manipulate_Timer();
            }
            
        }

        private void Manipulate_Timer()
        {
            float oldVol = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            float newVol = float.Parse(txtVolume.Text, System.Globalization.CultureInfo.InvariantCulture);

            lblCurrVolume.Text = oldVol.ToString();

            defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVol;

            if (oldVol != newVol)
            {
                globalChangeCounter++;
                lblChangeCounter.Text = globalChangeCounter.ToString();
            }
        }

        private void TxtVolume_TextChanged(object sender, EventArgs e)
        {
            globalChangeCounter = -1;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }
    }
}
