using CoD2_Launcher.Models;
using CoD2_Launcher.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrayIcon = System.Windows.Forms.NotifyIcon;

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

        private TextBoxOutputter _outputter;

        private Timer _timer = null;

        private bool _loaded = false;

        private TrayIcon _trayIcon = null;


        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();

            DataContext = this;

            foreach (string server in Properties.Settings.Default.ServerList)
            {
                ServerList.Add(server);
            }
            CurrentServer = Properties.Settings.Default.DefaultServer;

            RefreshRateComboBox.ItemsSource = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 15 };
            RefreshRateComboBox.SelectedItem = Properties.Settings.Default.RefreshRate;

            _outputter = new TextBoxOutputter(ConsoleTextBox);
            Console.SetOut(_outputter);
            ClearConsole();

        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new TrayIcon();
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick += TrayIcon_DoubleClick;
            _trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.None;
            _trayIcon.BalloonTipClicked += TrayIcon_BalloonClick;

            var resourceInfo = Application.GetResourceStream(new Uri(@"Resources/Icon.ico", UriKind.Relative));
            _trayIcon.Icon = new Icon(resourceInfo.Stream);

            var menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("Hrát", TrayIcon_Play);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Konec", TrayIcon_Exit);
            _trayIcon.ContextMenu = menu;
        }

        private void Play()
        {
            KillAll();
            Play(CurrentServer);
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

        private void AddServer(string server)
        {
            bool isNew = false;
            if (!Properties.Settings.Default.ServerList.Contains(server))
            {
                Properties.Settings.Default.ServerList.Add(server);
                Properties.Settings.Default.Save();
                isNew = true;
            }

            if (!ServerList.Contains(server))
            {
                ServerList.Add(server);
                isNew = true;
            }

            if (isNew)
            {
                Logger.Log($"Přidán oblíbený server: {server}");
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

            Logger.Log($"Odebrán oblíbený server: {server}");
        }

        private void DefaultServer(string server)
        {
            AddServer(server);

            if (!Properties.Settings.Default.ServerList.Contains(server))
            {
                Properties.Settings.Default.ServerList.Add(server);
            }
            Properties.Settings.Default.DefaultServer = server;
            Properties.Settings.Default.Save();

            Logger.Log($"Nastaven výchozí server: {server}");
        }

        private string ShowFileDialog(string path = null)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    dialog.FileName = Path.GetFileName(path);
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                else
                {
                    dialog.InitialDirectory = GetProgramFilesPath();
                }
                dialog.CheckFileExists = true;
                dialog.Filter = "Program|*.exe";

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    return dialog.FileName;
                }
            }
            catch { }
            return null;
        }

        private string GetProgramFilesPath()
        {
            if (8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private void Refresh()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            int period = 1000 * 60 * Properties.Settings.Default.RefreshRate;
            _timer = new Timer(o => Dispatcher.BeginInvoke((Action)OnRefresh), null, 0, period);
        }

        private void OnRefresh()
        {
            ServerInfo si = ServerInfo.Parse(CurrentServer);
            if (LastServer == null || !LastServer.Equals(si))
            {
                LastServer = si;
                LastMaps.Clear();
            }

            CurrentGame = Game.Download(si);
            if (CurrentGame != null)
            {
                var lastMap = LastMaps.FirstOrDefault();
                if (lastMap == null || (lastMap.Name != CurrentGame.Map.Name || lastMap.Type != CurrentGame.Map.Type))
                {
                    var newMap = new LastMap
                    {
                        DateTime = DateTime.Now,
                        Name = CurrentGame.Map.Name,
                        Type = CurrentGame.Map.Type,
                        ShortType = CurrentGame.Map.ShortType
                    };
                    OnNewMap();
                    LastMaps.Insert(0, newMap);
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
        }

        private void OnNewMap()
        {
            if (WindowState == WindowState.Minimized && CurrentGame.Players.Count > 2)
            {
                _trayIcon.BalloonTipTitle = CurrentGame.Map.Name;
                _trayIcon.BalloonTipText = $"{CurrentGame.Map.Type}\nPočet hráčů: {CurrentGame.Players.Count}";
                _trayIcon.ShowBalloonTip(2500);
            }
        }

        private void ClearConsole()
        {
            ConsoleTextBox.Text = "";
            Logger.Log(Title);
            Logger.Log("-------------------------------------------------");
        }


        private void TrayIcon_BalloonClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void TrayIcon_Play(object sender, EventArgs e)
        {
            Play();
        }

        private void TrayIcon_Exit(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
            }
            base.OnClosed(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Kill_Click(object sender, RoutedEventArgs e)
        {
            KillAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.P)
                {
                    Play();
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
            RemoveServer(ServerComboBox.Text);
        }

        private void DefaultServer_Click(object sender, RoutedEventArgs e)
        {
            DefaultServer(ServerComboBox.Text);
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
            if (_loaded)
            {
                Refresh();
            }
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ClearConsole();
        }

        private void RefreshRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.RefreshRate = (int)RefreshRateComboBox.SelectedItem;
            Properties.Settings.Default.Save();

            Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }
    }
}
