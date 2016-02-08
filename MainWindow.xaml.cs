using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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

        private Game _currentGame = null;
        public Game CurrentGame
        {
            get { return _currentGame; }
            set { SetProperty(ref _currentGame, value); }
        }

        public string CurrentServer
        {
            get { return ServerComboBox.Text; }
            set { ServerComboBox.Text = value; }
        }

        private ServerInfo _lastServer = null;
        public ServerInfo LastServer
        {
            get { return _lastServer; }
            set { SetProperty(ref _lastServer, value); }
        }

        private ObservableCollection<LastMap> _lastMaps = new ObservableCollection<LastMap>();
        public ObservableCollection<LastMap> LastMaps
        {
            get { return _lastMaps; }
            set { SetProperty(ref _lastMaps, value); }
        }

        private ObservableCollection<string> _serverList = new ObservableCollection<string>();
        public ObservableCollection<string> ServerList
        {
            get { return _serverList; }
            set { SetProperty(ref _serverList, value); }
        }

        TextBoxOutputter _outputter;

        Timer _timer = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            foreach (string server in Properties.Settings.Default.ServerList)
            {
                ServerList.Add(server);
            }
            CurrentServer = Properties.Settings.Default.DefaultServer;

            RefreshRateComboBox.ItemsSource = new List<int> { 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 15 };
            RefreshRateComboBox.SelectedItem = Properties.Settings.Default.RefreshRate;

            _outputter = new TextBoxOutputter(ConsoleTextBox);
            Console.SetOut(_outputter);
            ResetConsoleText();
        }

        private void ResetConsoleText()
        {
            ConsoleTextBox.Text = "";
            Logger.Log(Title);
            Logger.Log("-------------------------------------------------");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
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
            try
            {
                Logger.Log($"Spouštím hru na serveru {server}... ", Logger.MessageType.WithoutNewLine);
                ProcessStartInfo p = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.GameExe),
                    FileName = Path.GetFileName(Properties.Settings.Default.GameExe),
                    Arguments = "connect " + server
                };
                Process.Start(p);
                Logger.Log("OK", Logger.MessageType.Continue);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        private void KillAll()
        {
            Logger.Log("Zabíjím všechny programy... ");
            Kill(Path.GetFileNameWithoutExtension(Properties.Settings.Default.GameExe));
            Kill("PnkBstrA");
            Kill("PnkBstrB");
        }

        private bool Kill(string name)
        {
            bool ok = true;
            foreach (Process process in Process.GetProcessesByName(name))
            {
                try
                {
                    Logger.Log($"Zabíjím '{name}' ", Logger.MessageType.WithoutNewLine);
                    process.Kill();
                    Logger.Log("OK", Logger.MessageType.Continue);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
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

        private void DefaultServer_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DefaultServer = CurrentServer;
            Properties.Settings.Default.Save();
            Logger.Log($"Nastaven výchozí server: {CurrentServer}");
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

            Logger.Log($"Přidán oblíbený server: {server}");
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

            Logger.Log($"Odebrán oblíbený server: {server}");
        }

        private string ShowFileDialog(string path)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                if (path != null)
                {
                    dialog.FileName = Path.GetFileName(path);
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                dialog.CheckFileExists = true;
                dialog.Filter = "Program|*.exe";

                Nullable<bool> result = dialog.ShowDialog();
                if (result == true)
                {
                    return dialog.FileName;
                }
            }
            catch { }
            return null;
        }

        private void ChangeGameDir_Click(object sender, RoutedEventArgs e)
        {
            string path = ShowFileDialog(Properties.Settings.Default.GameExe);
            if (path != null)
            {
                Properties.Settings.Default.GameExe = path;
                Properties.Settings.Default.Save();

                Logger.Log($"Nové umístění hry: {path}");
            }
        }

        private void ServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshStatus();
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (_timer == null)
            {
                int minute = 1000 * 60 * Properties.Settings.Default.RefreshRate;
                _timer = new Timer(o =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ServerInfo si = ServerInfo.Parse(CurrentServer);
                        if (LastServer == null || !LastServer.Equals(si))
                        {
                            LastServer = si;
                            LastMaps.Clear();
                        }

                        CurrentGame = Game.GetStatus(si);
                        if (CurrentGame != null)
                        {
                            if (LastMaps.Count == 0 || (LastMaps.First().Map != CurrentGame.Map || LastMaps.First().Type != CurrentGame.Type))
                            {
                                LastMaps.Insert(0, new LastMap
                                {
                                    DateTime = DateTime.Now,
                                    Map = CurrentGame.Map,
                                    Type = CurrentGame.Type,
                                    ShortType = CurrentGame.ShortType
                                });
                                LastMapsComboBox.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            LastServer = null;
                        }

                        while (LastMaps.Count > 8)
                        {
                            LastMaps.RemoveAt(LastMaps.Count - 1);
                        }
                    }));
                }, null, 0, minute);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResetConsoleText();
        }

        private void RefreshRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.RefreshRate = (int)RefreshRateComboBox.SelectedItem;
            Properties.Settings.Default.Save();

            RefreshStatus();
        }
    }
}
