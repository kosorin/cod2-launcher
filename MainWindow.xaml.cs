using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoD2_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;

            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion // end of INotifyPropertyChanged

        public string CurrentServer
        {
            get { return ServerComboBox.Text; }
            set { ServerComboBox.Text = value; }
        }

        private ObservableCollection<string> _serverList = new ObservableCollection<string>();
        public ObservableCollection<string> ServerList
        {
            get { return _serverList; }
            set { SetProperty(ref _serverList, value); }
        }

        TextBoxOutputter _outputter;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            foreach (string server in Properties.Settings.Default.ServerList)
            {
                ServerList.Add(server);
            }
            CurrentServer = "kafemlynek.cz";

            _outputter = new TextBoxOutputter(ConsoleTextBox);
            Console.SetOut(_outputter);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Play(CurrentServer);
        }

        private void KillPlay_Click(object sender, RoutedEventArgs e)
        {
            KillAll();
            Play(CurrentServer);
        }

        private void Kill_Click(object sender, RoutedEventArgs e)
        {
            KillAll();
        }

        private void Play(string server)
        {
            AddServer(server);

            try
            {
                Console.Write("Spouštím hru ({0})... ", server);
                ProcessStartInfo p = new ProcessStartInfo
                {
                    WorkingDirectory = @"D:\Call of Duty 2\",
                    FileName = "CoD2MP_s.exe",
                    Arguments = "connect " + server
                };
                //Process.Start(p);
                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void KillAll()
        {
            Console.WriteLine("Zabíjím všechny programy... ");
            bool ok = true;
            ok &= Kill("CoD2MP_s");
            ok &= Kill("PnkBstrA");
            ok &= Kill("PnkBstrB");
            Console.WriteLine(ok ? "OK" : "FAIL");
        }

        private bool Kill(string name)
        {
            bool ok = true;
            foreach (Process process in Process.GetProcessesByName(name))
            {
                try
                {
                    Console.Write("Zabíjím '{0}' ", name);
                    process.Kill();
                    Console.WriteLine("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return ok;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.P)
                {
                    KillAll();
                    Play(CurrentServer);
                }
                else if (e.Key == Key.K)
                {
                    KillAll();
                }
            }
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            AddServer(ServerComboBox.Text);
        }

        private void RemoveServer_Click(object sender, RoutedEventArgs e)
        {
            RemoveServer(CurrentServer);
        }

        private void AddServer(string server)
        {
            if (!Properties.Settings.Default.ServerList.Contains(server))
            {
                Properties.Settings.Default.ServerList.Add(server);
                Properties.Settings.Default.Save();
            }

            if (!ServerList.Contains(server))
            {
                ServerList.Add(server);
            }
        }

        private void RemoveServer(string server)
        {
            if (Properties.Settings.Default.ServerList.Contains(server))
            {
                Properties.Settings.Default.ServerList.Remove(server);
                Properties.Settings.Default.Save();
            }

            if (ServerList.Contains(server))
            {
                ServerList.Remove(server);
            }
        }
    }
}
