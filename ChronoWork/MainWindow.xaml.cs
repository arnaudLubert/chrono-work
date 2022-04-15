using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChronoWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private bool updateLoop = false;
        private const int chronoCount = 3;
        private const string projectName = "ChronoWork";
        private const string filename = "chrono.save";
        private string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), projectName);

        private long[] chronoBegin = new long[chronoCount];
        private long[] chronoOffset = new long[chronoCount];
        private TextBox[] textProject = new TextBox[chronoCount];
        private TextBlock[] textChrono = new TextBlock[chronoCount];
        private TextBlock[] textJH = new TextBlock[chronoCount];
        private Button[] buttonStart = new Button[chronoCount];
        private Button[] buttonStop = new Button[chronoCount];
        private Button[] buttonReset = new Button[chronoCount];

        public MainWindow()
        {
            InitializeComponent();
            InitializeChrono();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // prepare all chronos to be saved
            for (int i = 0; i != chronoCount; i++)
            {
                if (chronoBegin[i] != 0L)
                {
                    chronoOffset[i] += DateTimeOffset.Now.ToUnixTimeSeconds() - chronoBegin[i];
                    chronoBegin[i] = 0;
                }
            }
            SaveData();
        }

        private void InitializeChrono()
        {
            textProject[0] = UITextBox;
            textProject[1] = UITextBox2;
            textProject[2] = UITextBox3;
            textChrono[0] = UITextChrono;
            textChrono[1] = UITextChrono2;
            textChrono[2] = UITextChrono3;
            textJH[0] = UITextJH;
            textJH[1] = UITextJH2;
            textJH[2] = UITextJH3;
            buttonStart[0] = UIButtonStart;
            buttonStart[1] = UIButtonStart2;
            buttonStart[2] = UIButtonStart3;
            buttonStop[0] = UIButtonStop;
            buttonStop[1] = UIButtonStop2;
            buttonStop[2] = UIButtonStop3;
            buttonReset[0] = UIButtonReset;
            buttonReset[1] = UIButtonReset2;
            buttonReset[2] = UIButtonReset3;

            credit.PreviewMouseDown += CreditClicked;

            ReadData();
            UpdateDisplay();
        }

        private void ClickStartChrono(object sender, RoutedEventArgs e)
        {
            int currentChrono = 0;

            while (currentChrono != chronoCount)
            {
                if (buttonStart[currentChrono] == sender)
                    break;
                currentChrono++;
            }

            if (currentChrono == chronoCount)
                return; // error button not found

            if (chronoBegin[currentChrono] == 0L)
            {
                chronoBegin[currentChrono] = DateTimeOffset.Now.ToUnixTimeSeconds();
                UpdateDisplayLoop(true);
            }
        }

        private void ClickStopChrono(object sender, RoutedEventArgs e)
        {
            int currentChrono = 0;

            while (currentChrono != chronoCount)
            {
                if (buttonStop[currentChrono] == sender)
                    break;
                currentChrono++;
            }

            if (currentChrono == chronoCount)
                return; // error button not found

            if (chronoBegin[currentChrono] != 0L)
            {
                chronoOffset[currentChrono] += DateTimeOffset.Now.ToUnixTimeSeconds() - chronoBegin[currentChrono];
                chronoBegin[currentChrono] = 0;

                if (!IsChronoLaunched())
                    UpdateDisplayLoop(false);

                SaveData();
            }
        }

        private void ClickResetChrono(object sender, RoutedEventArgs e)
        {
            int currentChrono = 0;

            while (currentChrono != chronoCount)
            {
                if (buttonReset[currentChrono] == sender)
                    break;
                currentChrono++;
            }

            if (currentChrono == chronoCount)
                return; // error button not found

            chronoBegin[currentChrono] = 0;
            chronoOffset[currentChrono] = 0;

            UpdateDisplayLoop(IsChronoLaunched());
        }
        
        private void CreditClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://citadelle-du-web.com",
                UseShellExecute = true
            });
        }

        private bool IsChronoLaunched()
        {
            for (int i = 0; i != chronoCount; i++)
            {
                if (chronoBegin[i] != 0L)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateDisplay()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                long elapsedTime;

                for (int currentChrono = 0; currentChrono != chronoCount; currentChrono++)
                {
                    if (chronoBegin[currentChrono] != 0L)
                        elapsedTime = DateTimeOffset.Now.ToUnixTimeSeconds() - chronoBegin[currentChrono] + chronoOffset[currentChrono];
                    else
                        elapsedTime = chronoOffset[currentChrono];
                    textChrono[currentChrono].Text = FormatTime(elapsedTime);
                    textJH[currentChrono].Text = FormatJH(elapsedTime);
                }
            }));
        }

        private async void UpdateDisplayLoop(bool state)
        {
            updateLoop = state;

            if (!state)
                UpdateDisplay();

            while (updateLoop)
            {
                await Task.Delay(1000);
                await Task.Run(() => UpdateDisplay());
            }
        }

        // HH:mm:ss
        private string FormatTime(long time)
        {
            long seconds = time % 60;
            long minutes = ((time - seconds) / 60) % 60;
            long hour = (time - minutes * 60 - seconds) / 3600;

            return hour.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        // 0,0 JH
        private string FormatJH(long time)
        {
            return ((float)Math.Round((double)time / 360 / 7) / 10).ToString() + " J/H";
        }

        private void SaveData()
        {
            try
            {
                if (!Directory.Exists(filepath))
                    Directory.CreateDirectory(filepath);

                FileStream file = File.Open(filepath + "\\" + filename, FileMode.Create);

                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    for (int i = 0; i != chronoCount; i++)
                    {
                        writer.Write(chronoOffset[i]);
                        writer.Write(textProject[i].Text);
                       // writer.Write("\r\n");
                    }
                }
                file.Close();
            }
            catch
            {
                // error
            }
        }

        private void ReadData()
        {
            try
            {
                FileStream file = File.Open(filepath + "\\" + filename, FileMode.Open);

                using (BinaryReader reader = new BinaryReader(file))
                {
                    for (int i = 0; i != chronoCount; i++)
                    {
                        chronoOffset[i] = reader.ReadInt64();
                        textProject[i].Text = reader.ReadString();
                    }
                }
                file.Close();
            }
            catch
            {
                // error
            }
        }
    }
}
