﻿using FontAwesome.WPF;
using Jvedio.Core.Config;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.External;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.App;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Settings : SuperControls.Style.BaseWindow
    {
        public static string AIBaseImage64 { get; set; }

        public VieModel_Settings vieModel { get; set; }

        private Main windowMain { get; set; }

        public static Video SampleVideo { get; set; }

        public static string DEFAULT_TEST_URL = "https://www.baidu.com/";

        public static string SupportVideoFormat { get; set; }

        public static string SupportPictureFormat { get; set; } // bmp,gif,ico,jpe,jpeg,jpg,png

        #region "热键"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int HOTKEY_ID = 2415;
        public static uint VK;
        public static IntPtr _windowHandle;
        public static HwndSource _source;
        //public static bool WindowsVisible = true;
        //public static List<string> OpeningWindows = new List<string>();
        public static List<Key> funcKeys = new List<Key>();     // 功能键 [1,3] 个
        public static Key key = Key.None;                       // 基础键 1 个
        public static List<Key> _funcKeys = new List<Key>();
        public static Key _key = Key.None;

        public enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008,
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList) {
                if (!keys.Contains(item)) {
                    result = false;
                    break;
                }
            }

            return result;
        }

        #endregion

        static Window_Settings()
        {
            AIBaseImage64 = "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";

            SampleVideo = new Video() {
                VID = "IRONMAN-01",
                Title = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Title"),
                VideoType = VideoType.Normal,
                ReleaseDate = "2020-01-01",
                Director = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Director"),
                Genre = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Genre"),
                Series = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Tag"),
                ActorNames = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Actor"),
                Studio = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Studio"),
                Rating = 9.0f,
                Label = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Label"),
                ReleaseYear = 2020,
                Duration = 126,

                Country = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Country"),
            };

            SupportVideoFormat = $"{SuperControls.Style.LangManager.GetValueByKey("NormalVideo")}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{SuperControls.Style.LangManager.GetValueByKey("OtherVedio")}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{SuperControls.Style.LangManager.GetValueByKey("AllFile")} (*.*)|*.*";
            SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";
        }

        public Window_Settings()
        {
            InitializeComponent();

            windowMain = GetWindowByName("Main", App.Current.Windows) as Main;
            if (StyleManager.GlobalFont != null)
                this.FontFamily = StyleManager.GlobalFont;
            vieModel = new VieModel_Settings();
            this.DataContext = vieModel;

            // 绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList()) {
                item.Click += AddToRename;
            }

            vieModel.MainWindowVisible = windowMain != null;
        }

        #region "热键"

        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (!funcKeys.Contains(currentKey))
                    funcKeys.Add(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9)) {
                key = currentKey;
            } else {
                // Console.WriteLine("不支持");
            }

            string singleKey = key.ToString();
            if (key.ToString().Length > 1) {
                singleKey = singleKey.ToString().Replace("D", string.Empty);
            }

            if (funcKeys.Count > 0) {
                if (key == Key.None) {
                    hotkeyTextBox.Text = string.Join("+", funcKeys);
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = Key.None;
                } else {
                    hotkeyTextBox.Text = string.Join("+", funcKeys) + "+" + singleKey;
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = key;
                }
            } else {
                if (key != Key.None) {
                    hotkeyTextBox.Text = singleKey;
                    _funcKeys = new List<Key>();
                    _key = key;
                }
            }
        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (funcKeys.Contains(currentKey))
                    funcKeys.Remove(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12)) {
                if (currentKey == key) {
                    key = Key.None;
                }
            }
        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _funcKeys.Contains(Key.LeftAlt) | _funcKeys.Contains(Key.LeftCtrl) | _funcKeys.Contains(Key.LeftShift) | _funcKeys.Contains(Key.CapsLock);

            if (!containsFunKey | _key == Key.None) {
                SuperControls.Style.MessageCard.Error(LangManager.GetValueByKey("HotKeyWarning"));
            } else {
                // 注册热键
                if (_key != Key.None & IsProperFuncKey(_funcKeys)) {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _funcKeys) {
                        if (key == Key.LeftCtrl)
                            fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt)
                            fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift)
                            fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }

                    VK = (uint)KeyInterop.VirtualKeyFromKey(_key);

                    UnregisterHotKey(_windowHandle, HOTKEY_ID); // 取消之前的热键
                    bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) {
                        new MsgBox(LangManager.GetValueByKey("HotKeyConflict")).ShowDialog(this);
                    }

                    {
                        // 保存设置
                        Properties.Settings.Default.HotKey_Modifiers = fsModifiers;
                        Properties.Settings.Default.HotKey_VK = VK;
                        Properties.Settings.Default.HotKey_Enable = true;
                        Properties.Settings.Default.HotKey_String = hotkeyTextBox.Text;
                        Properties.Settings.Default.Save();
                        MessageNotify.Success(LangManager.GetValueByKey("HotKeySetSuccess"));
                    }
                }
            }
        }

        #endregion

        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (vieModel.ScanPath == null)
                    vieModel.ScanPath = new ObservableCollection<string>();
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                    vieModel.ScanPath.Add(path);
                else
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
            }
        }

        public async void TestAI(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["Window.Foreground"];
            if (checkBox.Content.ToString() == SuperControls.Style.LangManager.GetValueByKey("BaiduFaceRecognition")) {
                string base64 = AIBaseImage64;
                System.Drawing.Bitmap bitmap = ImageHelper.Base64ToBitmap(base64);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = await TestBaiduAI(bitmap);
                if (result != null && int32Rect != Int32Rect.Empty) {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));
                    string clientId = Properties.Settings.Default.Baidu_API_KEY.Replace(" ", string.Empty);
                    string clientSecret = Properties.Settings.Default.Baidu_SECRET_KEY.Replace(" ", string.Empty);

                    // SaveKeyValue(clientId, clientSecret, "BaiduAI.key");
                } else {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public static Task<(Dictionary<string, string>, Int32Rect)> TestBaiduAI(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() => {
                string token = AccessToken.getAccessToken();
                string faceJson = FaceDetect.faceDetect(token, bitmap);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = FaceParse.Parse(faceJson);
                return (result, int32Rect);
            });
        }

        public async void TestTranslate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["Window.Foreground"];

            if (checkBox.Content.ToString() == "百度翻译") {
            } else if (checkBox.Content.ToString() == SuperControls.Style.LangManager.GetValueByKey("Youdao")) {
                string result = await Translate.Youdao("のマ○コに");
                if (result != string.Empty) {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));

                    string youdao_appKey = Properties.Settings.Default.TL_YOUDAO_APIKEY.Replace(" ", string.Empty);
                    string Youdao_appSecret = Properties.Settings.Default.TL_YOUDAO_SECRETKEY.Replace(" ", string.Empty);

                    // 成功，保存在本地
                    SaveKeyValue(youdao_appKey, Youdao_appSecret, "youdao.key");
                } else {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public void SaveKeyValue(string key, string value, string filename)
        {
            // string v = Encrypt.AesEncrypt(key + " " + value, EncryptKeys[0]);
            // try
            // {
            //    using (StreamWriter sw = new StreamWriter(filename, append: false))
            //    {
            //        sw.Write(v);
            //    }
            // }
            // catch (Exception ex)
            // {
            //    Logger.Error(ex);
            // }
        }

        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex >= 0) {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--) {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanPath?.Clear();
        }

        private void ListenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox?.IsVisible == false)
                return;
            if ((bool)checkBox.IsChecked) {
                // 测试是否能监听
                if (!TestListen())
                    checkBox.IsChecked = false;
                else
                    SuperControls.Style.MessageCard.Info(SuperControls.Style.LangManager.GetValueByKey("RebootToTakeEffect"));
            }
        }

        FileSystemWatcher[] watchers;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++) {
                try {
                    if (drives[i] == @"C:\") {
                        continue;
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                } catch {
                    SuperControls.Style.MessageNotify.Error($"{SuperControls.Style.LangManager.GetValueByKey("NoPermissionToListen")} {drives[i]}");
                    return false;
                }
            }

            return true;
        }

        private void SetVideoPlayerPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("Choose");
            openFileDialog1.Filter = "exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath))
                    ConfigManager.Settings.VideoPlayerPath = exePath;
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            ApplySettings(null, null);
            this.Close();
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            // if (Properties.Settings.Default.Opacity_Main >= 0.5)
            //    App.Current.Windows[0].Opacity = Properties.Settings.Default.Opacity_Main;
            // else
            //    App.Current.Windows[0].Opacity = 1;

            // 保存扫描库

            ConfigManager.DownloadConfig.Save();

            if (DatabaseComboBox.ItemsSource != null && DatabaseComboBox.SelectedItem != null) {
                AppDatabase db = DatabaseComboBox.SelectedItem as AppDatabase;
                List<string> list = new List<string>();
                if (vieModel.ScanPath != null)
                    list = vieModel.ScanPath.ToList();
                db.ScanPath = JsonConvert.SerializeObject(list);
                MapperManager.appDatabaseMapper.UpdateById(db);
                int idx = DatabaseComboBox.SelectedIndex;

                List<AppDatabase> appDatabases = windowMain?.vieModel.DataBases.ToList();
                if (appDatabases != null && idx < windowMain.vieModel.DataBases.Count) {
                    windowMain.vieModel.DataBases[idx].ScanPath = db.ScanPath;
                }
            }

            bool success = vieModel.SaveServers((msg) => {
                MessageCard.Error(msg);
            });
            if (success) {
                ScanHelper.InitSearchPattern();
                SavePath();
                SaveSettings();

                SuperControls.Style.MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
            }
            UtilsManager.OnUtilSettingChange();


            SaveNFOParseValues();
        }


        /// <summary>
        /// 保存 NFO 解析
        /// </summary>
        public void SaveNFOParseValues()
        {
            vieModel.SaveNFOParseData();
        }

        private void SetListenStatus()
        {
            if (ConfigManager.Settings.ListenEnabled) {
                // 开启端口监听
            }
        }

        private void SavePath()
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)vieModel.PicPaths[PathType.RelativeToData.ToString()];
            dict["BigImagePath"] = vieModel.BigImagePath;
            dict["SmallImagePath"] = vieModel.SmallImagePath;
            dict["PreviewImagePath"] = vieModel.PreviewImagePath;
            dict["ScreenShotPath"] = vieModel.ScreenShotPath;
            dict["ActorImagePath"] = vieModel.ActorImagePath;
            vieModel.PicPaths[PathType.RelativeToData.ToString()] = dict;
            ConfigManager.Settings.PicPathJson = JsonConvert.SerializeObject(vieModel.PicPaths);
        }

        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFFmpeg");
            openFileDialog1.FileName = "ffmpeg.exe";
            openFileDialog1.Filter = "ffmpeg.exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath)) {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        vieModel.FFMPEG_Path = exePath;
                }
            }
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            AppDatabase db = e.AddedItems[0] as AppDatabase;
            vieModel.LoadScanPath(db);
        }

        private void SetScanDatabases()
        {
            List<AppDatabase> appDatabases = windowMain?.vieModel.DataBases.ToList();
            AppDatabase db = windowMain?.vieModel.CurrentAppDataBase;
            if (appDatabases != null) {
                DatabaseComboBox.ItemsSource = appDatabases;
                for (int i = 0; i < appDatabases.Count; i++) {
                    if (appDatabases[i].Equals(db)) {
                        DatabaseComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // 初次启动后不给设置默认打开上一次库，否则会提示无此数据库
            if (ConfigManager.Settings.DefaultDBID <= 0)
                openDefaultCheckBox.IsEnabled = false;

            // 设置 crawlerIndex
            serverListBox.SelectedIndex = (int)ConfigManager.Settings.CrawlerSelectedIndex;

            // 设置当前数据库
            SetScanDatabases();

            // if (vieModel.DataBases?.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;
            ShowViewRename(ConfigManager.RenameConfig.FormatString);

            SetCheckedBoxChecked();
            foreach (ComboBoxItem item in OutComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.OutSplit)) {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (OutComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;

            foreach (ComboBoxItem item in InComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.InSplit)) {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (InComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;

            // if (!findTheme)
            // {
            //    for (int i = 0; i < ThemesDataGrid.Items.Count; i++)
            //    {
            //        DataGridRow row = (DataGridRow)ThemesDataGrid.ItemContainerGenerator.ContainerFromItem(ThemesDataGrid.Items[i]);
            //        if (row != null)
            //        {
            //            var cell = ThemesDataGrid.Columns[0];
            //            var cp = (ContentPresenter)cell?.GetCellContent(row);
            //            RadioButton rb = (RadioButton)cp?.ContentTemplate.FindName("rb", cp);
            //            if (rb != null && rb.Content.ToString() == Properties.Settings.Default.Themes)
            //            {
            //                rb.IsChecked = true;

            // break;
            //            }
            //        }

            // }
            // }

            // 设置代理选中
            List<RadioButton> proxies = proxyStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxies.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyMode)
                    proxies[i].IsChecked = true;
                int idx = i;
                proxies[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyMode = idx;
                };
            }

            List<RadioButton> proxyTypes = proxyTypesStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxyTypes.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyType)
                    proxyTypes[i].IsChecked = true;
                int idx = i;
                proxyTypes[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyType = idx;
                };
            }

            // 设置代理密码
            passwordBox.Password = vieModel.ProxyPwd;

            passwordBox.PasswordChanged += (s, ev) => {
                if (!string.IsNullOrEmpty(passwordBox.Password))
                    vieModel.ProxyPwd = JvedioLib.Security.Encrypt.AesEncrypt(passwordBox.Password, 0);
            };

            // 设置语言
            int langIdx = 0;
            if (!string.IsNullOrEmpty(ConfigManager.Settings.CurrentLanguage)) {
                for (int i = 0; i < langComboBox.Items.Count; i++) {
                    ComboBoxItem item = langComboBox.Items[i] as ComboBoxItem;
                    if (item.Tag.ToString().Equals(ConfigManager.Settings.CurrentLanguage)) {
                        langIdx = i;
                        break;
                    }
                }
            }
            langComboBox.SelectedIndex = langIdx;
            langComboBox.SelectionChanged += (s, ev) => {
                if (ev.AddedItems?.Count > 0) {
                    ComboBoxItem comboBoxItem = ev.AddedItems[0] as ComboBoxItem;
                    string lang = comboBoxItem.Tag.ToString();
                    SuperControls.Style.LangManager.SetLang(lang);
                    Jvedio.Core.Lang.LangManager.SetLang(lang);
                    vieModel.CurrentLanguage = lang;
                }
            };
        }


        // todo 检视
        private void SetCheckedBoxChecked()
        {
            List<ToggleButton> toggleButtons = CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList();
            List<string> list = toggleButtons.Select(arg => Video.ToSqlField(arg.Content.ToString())).ToList();

            // 按照顺序
            string formatString = ConfigManager.RenameConfig.FormatString;
            if (!string.IsNullOrEmpty(formatString)) {
                int left = formatString.IndexOf("{"), right = formatString.IndexOf("}");
                while (right > 0 && right < formatString.Length) {
                    string name = formatString.Substring(left + 1, right - left - 1);
                    if (list.Contains(name)) {
                        //RenameList.Add(name);
                        toggleButtons[list.IndexOf(name)].IsChecked = true;
                    }
                    left = formatString.IndexOf("{", left + 1);
                    right = formatString.IndexOf("}", right + 1);
                    Console.WriteLine();
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(YoudaoUrl);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(BaiduUrl);
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        // 检视
        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel.ScanPath == null)
                vieModel.ScanPath = new ObservableCollection<string>();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles) {
                if (!FileHelper.IsFile(item)) {
                    if (!vieModel.ScanPath.Contains(item) && !vieModel.ScanPath.IsIntersectWith(item))
                        vieModel.ScanPath.Add(item);
                    else
                        MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
                }
            }
        }

        private void SelectNfoPath(object sender, RoutedEventArgs e)
        {
            // 选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path = path + "\\";
                vieModel.NFOSavePath = path;
            } else {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_CanNotBeNull"));
            }
        }

        private void NewServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            CrawlerServer server = new CrawlerServer() {
                PluginID = pluginID,
                Enabled = true,
                Url = DEFAULT_TEST_URL,
                Cookies = string.Empty,
                Available = 0,
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            if (list == null)
                list = new ObservableCollection<CrawlerServer>();
            list.Add(server);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private string GetPluginID()
        {
            int idx = serverListBox.SelectedIndex;
            if (idx < 0 || vieModel.CrawlerServers?.Count == 0)
                return null;
            return vieModel.CrawlerServers.Keys.ToList()[idx];
        }

        private int CurrentRowIndex = 0;

        private void TestServer(object sender, RoutedEventArgs e)
        {
            int idx = CurrentRowIndex;
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            CrawlerServer server = list[idx];

            if (!server.IsHeaderProper()) {
                MessageNotify.Error(LangManager.GetValueByKey("HeaderNotProper"));
                return;
            }

            server.Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(server, (s) => {
                ServersDataGrid.IsEnabled = true;
                list[idx].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            Console.WriteLine(CurrentRowIndex);
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            list.RemoveAt(CurrentRowIndex);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private void SetCurrentRowIndex(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null) {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }

            if (dgr == null) {
                return;
            }

            CurrentRowIndex = dgr.GetIndex();
        }

        private async void CheckUrl(CrawlerServer server, Action<int> callback)
        {
            // library 需要保证 Cookies 和 UserAgent完全一致
            RequestHeader header = CrawlerServer.ParseHeader(server);
            try {
                string title = await HttpHelper.AsyncGetWebTitle(server.Url, header);
                if (string.IsNullOrEmpty(title)) {
                    server.Available = -1;
                } else {
                    server.Available = 1;
                }

                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                    if (!string.IsNullOrEmpty(title))
                        MessageCard.Success(title);
                });
                callback.Invoke(0);
            } catch (WebException ex) {
                MessageCard.Error(ex.Message);
                server.Available = -1;
                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                });
                callback.Invoke(0);
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null) {
                    child = GetVisualChild<T>

                    (v);
                }

                if (child != null) {
                    break;
                }
            }

            return child;
        }

        private void SetServerEnable(object sender, MouseButtonEventArgs e)
        {
            // bool enable = !(bool)((CheckBox)sender).IsChecked;
            // vieModel.Servers[CurrentRowIndex].IsEnable = enable;
            // ServerConfig.Instance.SaveServer(vieModel.Servers[CurrentRowIndex]);
            // InitVariable();
            // ServersDataGrid.Items.Refresh();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // 注册热键
            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (modifier != 0 && vk != 0) {
                UnregisterHotKey(_windowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success) {
                    SuperControls.Style.MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("BossKeyError"));
                    Properties.Settings.Default.HotKey_Enable = false;
                }
            }
        }

        private void Unregister_HotKey(object sender, RoutedEventArgs e)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID); // 取消之前的热键
        }

        private void ReplaceWithValue(string property)
        {
            string inSplit = ConfigManager.RenameConfig.InSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.InSplit;
            PropertyInfo[] propertyList = SampleVideo.GetType().GetProperties();
            foreach (PropertyInfo item in propertyList) {
                string name = item.Name;
                if (name == property) {
                    object o = item.GetValue(SampleVideo);
                    if (o != null) {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (vieModel.RemoveTitleSpace && property.Equals("Title"))
                            value = value.Trim();

                        if (property == "VideoType") {
                            int v = 0;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = SuperControls.Style.LangManager.GetValueByKey("Uncensored");
                            else if (v == 2)
                                value = SuperControls.Style.LangManager.GetValueByKey("Censored");
                            else if (v == 3)
                                value = SuperControls.Style.LangManager.GetValueByKey("Europe");
                        }

                        vieModel.ViewRenameFormat = vieModel.ViewRenameFormat.Replace("{" + property + "}", value);
                    }

                    break;
                }
            }
        }

        //private List<string> RenameList = new List<string>();


        private void SetRenameFormat()
        {
            //string format = vieModel.FormatString;
            //if (RenameList.Count > 0)
            //{

            //    StringBuilder builder = new StringBuilder();
            //    string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
            //    List<string> formatNames = new List<string>();
            //    foreach (string name in RenameList)
            //    {
            //        formatNames.Add($"{{{name}}}");
            //    }

            //    vieModel.FormatString = string.Join(sep, formatNames);
            //}
            //else
            //    vieModel.FormatString = string.Empty;
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton != null) {
                string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
                string format = vieModel.FormatString;
                string value = Video.ToSqlField(toggleButton.Content.ToString());
                if ((bool)toggleButton.IsChecked) {

                    if (format.IndexOf($"{{{value}}}") < 0) {
                        // 加到最后
                        if (format.Length > 0 && !string.IsNullOrEmpty(sep) && !format[format.Length - 1].Equals(sep.ToCharArray()[0]))
                            format += sep;
                        format += $"{{{value}}}";
                        //RenameList.Add(value);
                    }
                } else {
                    // 移除所有
                    format = format.Replace($"{sep}{{{value}}}", "");
                    format = format.Replace($"{{{value}}}", "");
                }
                vieModel.FormatString = format;
            }
            SetRenameFormat();
            ShowViewRename(vieModel.FormatString);
        }

        private char getSplit(string formatString)
        {
            int idx = vieModel.FormatString.IndexOf(formatString);
            if (idx > 0)
                return vieModel.FormatString[idx - 1];
            else
                return '\0';
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel == null)
                return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            ShowViewRename(txt);
        }

        private void ShowViewRename(string txt)
        {
            if (string.IsNullOrEmpty(txt)) {
                vieModel.ViewRenameFormat = string.Empty;
                return;
            }

            MatchCollection matches = Regex.Matches(txt, "\\{[a-zA-Z]+\\}");
            if (matches != null && matches.Count > 0) {
                vieModel.ViewRenameFormat = txt;
                foreach (Match match in matches) {
                    string property = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                    ReplaceWithValue(property);
                }
            } else {
                vieModel.ViewRenameFormat = string.Empty;
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
            ShowViewRename(vieModel.FormatString);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
            ConfigManager.Settings.Save();
            ConfigManager.ProxyConfig.Save();
            ConfigManager.ScanConfig.Save();
            ConfigManager.FFmpegConfig.Save();
            ConfigManager.RenameConfig.Save();
        }

        private void SaveSettings()
        {
            ConfigManager.Main.ShowSearchHistory = vieModel.ShowSearchHistory;

            ConfigManager.Settings.TabControlSelectedIndex = vieModel.TabControlSelectedIndex;
            ConfigManager.Settings.OpenDataBaseDefault = vieModel.OpenDataBaseDefault;
            ConfigManager.Settings.AutoGenScreenShot = vieModel.AutoGenScreenShot;
            ConfigManager.Settings.CloseToTaskBar = vieModel.CloseToTaskBar;
            //if (windowMain != null)
            //    windowMain.CloseToTaskBar = ConfigManager.Settings.CloseToTaskBar;
            ConfigManager.Settings.DetailShowBg = vieModel.DetailShowBg;
            ConfigManager.Settings.CurrentLanguage = vieModel.CurrentLanguage;
            ConfigManager.Settings.SaveInfoToNFO = vieModel.SaveInfoToNFO;
            ConfigManager.Settings.NFOSavePath = vieModel.NFOSavePath;
            ConfigManager.Settings.OverwriteNFO = vieModel.OverwriteNFO;
            ConfigManager.Settings.AutoHandleHeader = vieModel.AutoHandleHeader;

            ConfigManager.Settings.PicPathMode = vieModel.PicPathMode;
            ConfigManager.Settings.DownloadPreviewImage = vieModel.DownloadPreviewImage;
            ConfigManager.Settings.SkipExistImage = vieModel.SkipExistImage;
            ConfigManager.Settings.DownloadWhenTitleNull = vieModel.DownloadWhenTitleNull;
            ConfigManager.Settings.OverrideInfo = vieModel.OverrideInfo;
            ConfigManager.Settings.IgnoreCertVal = vieModel.IgnoreCertVal;
            ConfigManager.Settings.AutoBackup = vieModel.AutoBackup;
            ConfigManager.Settings.AutoBackupPeriodIndex = vieModel.AutoBackupPeriodIndex;

            // 代理
            ConfigManager.ProxyConfig.Server = vieModel.ProxyServer;
            ConfigManager.ProxyConfig.Port = vieModel.ProxyPort;
            ConfigManager.ProxyConfig.UserName = vieModel.ProxyUserName;
            ConfigManager.ProxyConfig.Password = vieModel.ProxyPwd;
            ConfigManager.ProxyConfig.HttpTimeout = vieModel.HttpTimeout;

            // 扫描
            ConfigManager.ScanConfig.MinFileSize = vieModel.MinFileSize;
            ConfigManager.ScanConfig.FetchVID = vieModel.FetchVID;
            ConfigManager.ScanConfig.LoadDataAfterScan = vieModel.LoadDataAfterScan;
            ConfigManager.ScanConfig.DataExistsIndexAfterScan = vieModel.DataExistsIndexAfterScan;
            ConfigManager.ScanConfig.ImageExistsIndexAfterScan = vieModel.ImageExistsIndexAfterScan;
            ConfigManager.ScanConfig.ScanOnStartUp = vieModel.ScanOnStartUp;
            ConfigManager.ScanConfig.CopyNFOOverwriteImage = vieModel.CopyNFOOverwriteImage;
            ConfigManager.ScanConfig.CopyNFOPicture = vieModel.CopyNFOPicture;
            ConfigManager.ScanConfig.CopyNFOActorPicture = vieModel.CopyNFOActorPicture;
            ConfigManager.ScanConfig.CopyNFOPreview = vieModel.CopyNFOPreview;
            ConfigManager.ScanConfig.CopyNFOScreenShot = vieModel.CopyNFOScreenShot;
            ConfigManager.ScanConfig.CopyNFOActorPath = vieModel.CopyNFOActorPath;
            ConfigManager.ScanConfig.CopyNFOPreviewPath = vieModel.CopyNFOPreviewPath;
            ConfigManager.ScanConfig.CopyNFOScreenShotPath = vieModel.CopyNFOScreenShotPath;

            // ffmpeg
            ConfigManager.FFmpegConfig.Path = vieModel.FFMPEG_Path;
            ConfigManager.FFmpegConfig.ThreadNum = vieModel.ScreenShot_ThreadNum;
            ConfigManager.FFmpegConfig.TimeOut = vieModel.ScreenShot_TimeOut;
            ConfigManager.FFmpegConfig.ScreenShotNum = vieModel.ScreenShotNum;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreStart = vieModel.ScreenShotIgnoreStart;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd = vieModel.ScreenShotIgnoreEnd;
            ConfigManager.FFmpegConfig.SkipExistGif = vieModel.SkipExistGif;
            ConfigManager.FFmpegConfig.SkipExistScreenShot = vieModel.SkipExistScreenShot;
            ConfigManager.FFmpegConfig.ScreenShotAfterImport = vieModel.ScreenShotAfterImport;
            ConfigManager.FFmpegConfig.GifAutoHeight = vieModel.GifAutoHeight;
            ConfigManager.FFmpegConfig.GifWidth = vieModel.GifWidth;
            ConfigManager.FFmpegConfig.GifHeight = vieModel.GifHeight;
            ConfigManager.FFmpegConfig.GifDuration = vieModel.GifDuration;

            // 重命名
            ConfigManager.RenameConfig.AddRenameTag = vieModel.AddRenameTag;
            ConfigManager.RenameConfig.RemoveTitleSpace = vieModel.RemoveTitleSpace;
            ConfigManager.RenameConfig.FormatString = vieModel.FormatString;

            // 监听
            ConfigManager.Settings.ListenEnabled = vieModel.ListenEnabled;
            ConfigManager.Settings.ListenPort = vieModel.ListenPort;
        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(FFMPEG_URL);
        }

        private void LoadTranslate(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("youdao.key"))
                return;
            string v = GetValueKey("youdao.key");
            if (v.Split(' ').Length == 2) {
                Properties.Settings.Default.TL_YOUDAO_APIKEY = v.Split(' ')[0];
                Properties.Settings.Default.TL_YOUDAO_SECRETKEY = v.Split(' ')[1];
            }
        }

        public string GetValueKey(string filename)
        {
            string v = string.Empty;
            try {
                using (StreamReader sr = new StreamReader(filename)) {
                    v = sr.ReadToEnd();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            // if (v != "")
            //    return Encrypt.AesDecrypt(v, EncryptKeys[0]);
            // else
            //    return "";
            return string.Empty;
        }

        private void LoadAI(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("BaiduAI.key"))
                return;
            string v = GetValueKey("BaiduAI.key");
            if (v.Split(' ').Length == 2) {
                Properties.Settings.Default.Baidu_API_KEY = v.Split(' ')[0];
                Properties.Settings.Default.Baidu_SECRET_KEY = v.Split(' ')[1];
            }
        }

        private int GetRowIndex(RoutedEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null) {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }

            if (dgr == null)
                return -1;
            else
                return dgr.GetIndex();
        }

        private void SetScanRe(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ScanRe = (sender as TextBox).Text.Replace("；", ";");
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int idx = (sender as ListBox).SelectedIndex;
            //if (idx < 0) return;
            //if (vieModel.CrawlerServers?.Count > 0)
            //{
            //    string pluginID = PluginType.Crawler.ToString() + "/" + vieModel.DisplayCrawlerServers[idx];
            //    PluginMetaData pluginMetaData = CrawlerManager.PluginMetaDatas.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
            //    if (pluginMetaData != null && pluginMetaData.Enabled) vieModel.PluginEnabled = true;
            //    else vieModel.PluginEnabled = false;
            //    if (vieModel.CrawlerServers.ContainsKey(pluginID))
            //    {
            //        ServersDataGrid.ItemsSource = null;
            //        ServersDataGrid.ItemsSource = vieModel.CrawlerServers[pluginID];
            //        ConfigManager.Settings.CrawlerSelectedIndex = idx;
            //    }

            //}
        }






        private void ImageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ComboBox).SelectedIndex;
            if (idx >= 0 && vieModel != null && idx < vieModel.PIC_PATH_MODE_COUNT) {
                PathType type = (PathType)idx;
                if (type != PathType.RelativeToData)
                    vieModel.BasePicPath = vieModel.PicPaths[type.ToString()].ToString();
            }
        }



        private void url_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchBox searchBox = sender as SearchBox;
            string cookies = searchBox.Text;
            DialogInput dialogInput = new DialogInput($"{LangManager.GetValueByKey("PleaseEnter")} cookie", cookies);
            if (dialogInput.ShowDialog(this) == true) {
                searchBox.Text = dialogInput.Text;
            }
        }

        CrawlerServer currentCrawlerServer;

        private void url_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            setHeaderPopup.IsOpen = true;
            SearchBox searchBox = sender as SearchBox;
            string headers = searchBox.Text;
            if (!string.IsNullOrEmpty(headers)) {
                try {
                    Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(headers);
                    if (dict != null && dict.Count > 0) {
                        StringBuilder builder = new StringBuilder();
                        foreach (string key in dict.Keys) {
                            builder.Append($"{key}: {dict[key]}{Environment.NewLine}");
                        }

                        inputTextbox.Text = builder.ToString();
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            currentCrawlerServer = vieModel.CrawlerServers[pluginID][ServersDataGrid.SelectedIndex];
        }

        private void CancelHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
        }

        private void ConfirmHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            if (currentCrawlerServer != null) {
                currentCrawlerServer.Headers = parsedTextbox.Text.Replace("{" + Environment.NewLine + "    ", "{")
                    .Replace(Environment.NewLine + "}", "}")
                    .Replace($"\",{Environment.NewLine}    \"", "\",\"");
                Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(currentCrawlerServer.Headers);

                if (dict != null && dict.ContainsKey("cookie"))
                    currentCrawlerServer.Cookies = dict["cookie"];
            }
        }

        private void InputHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse((sender as TextBox).Text);
        }

        private string parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            foreach (string item in array) {
                int idx = item.IndexOf(':');
                if (idx <= 0 || idx >= item.Length - 1)
                    continue;
                string key = item.Substring(0, idx).Trim().ToLower();
                string value = item.Substring(idx + 1).Trim();

                if (!data.ContainsKey(key))
                    data.Add(key, value);
            }

            // if (vieModel.AutoHandleHeader)
            // {
            data.Remove("content-encoding");
            data.Remove("accept-encoding");
            data.Remove("host");

            data = data.Where(arg => arg.Key.IndexOf(" ") < 0).ToDictionary(x => x.Key, y => y.Value);

            // }
            string json = JsonConvert.SerializeObject(data);
            if (json.Equals("{}"))
                return json;

            return json.Replace("{", "{" + Environment.NewLine + "    ")
                .Replace("}", Environment.NewLine + "}")
                .Replace("\",\"", $"\",{Environment.NewLine}    \"");
        }

        private void SetAutoHeader(object sender, RoutedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse(inputTextbox.Text);
        }

        private async void TestProxy(object sender, RoutedEventArgs e)
        {
            vieModel.TestProxyStatus = TaskStatus.Running;
            SaveSettings();
            Button button = sender as Button;
            button.IsEnabled = false;
            string url = textProxyUrl.Text;

            // url = "https://www.baidu.com";
            // string url = "https://www.google.com";

            // WebProxy proxy = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RequestHeader header = new RequestHeader();
            IWebProxy proxy = ConfigManager.ProxyConfig.GetWebProxy();
            header.TimeOut = ConfigManager.ProxyConfig.HttpTimeout * 1000; // 转为 ms
            header.WebProxy = proxy;
            string error = LangManager.GetValueByKey("Error");
            HttpResult httpResult = null;
            try {
                httpResult = await HttpClient.Get(url, header, SuperUtils.NetWork.Enums.HttpMode.String);
            } catch (TimeoutException ex) { error = ex.Message; } catch (Exception ex) { error = ex.Message; }
            if (httpResult != null) {
                if (httpResult.StatusCode == HttpStatusCode.OK) {
                    MessageCard.Success($"{LangManager.GetValueByKey("Success")} {LangManager.GetValueByKey("Delay")} {stopwatch.ElapsedMilliseconds} ms");
                    vieModel.TestProxyStatus = TaskStatus.RanToCompletion;
                } else {
                    MessageCard.Error(httpResult.Error);
                    vieModel.TestProxyStatus = TaskStatus.Canceled;
                }
            } else {
                MessageCard.Error(error);
                vieModel.TestProxyStatus = TaskStatus.Canceled;
            }

            stopwatch.Stop();
            button.IsEnabled = true;
        }

        private void ShowHeaderHelp(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            FileHelper.TryOpenUrl(UrlManager.HEADER_HELP);
        }







        private async void CreatePlayableIndex(object sender, RoutedEventArgs e)
        {
            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                List<MetaData> metaDatas = MapperManager.metaDataMapper.SelectList();
                total = metaDatas.Count;
                if (total <= 0)
                    return false;
                StringBuilder builder = new StringBuilder();
                List<string> list = new List<string>();
                for (int i = 0; i < total; i++) {
                    MetaData metaData = metaDatas[i];
                    if (!File.Exists(metaData.Path))
                        builder.Append($"update metadata set PathExist=0 where DataID='{metaData.DataID}';");
                    if (IndexCanceled)
                        return false;
                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string sql = $"begin;update metadata set PathExist=1;{builder};commit;"; // 因为大多数资源都是存在的，默认先设为1
                MapperManager.videoMapper.ExecuteNonQuery(sql);
                return true;
            });
            ConfigManager.Settings.PlayableIndexCreated = true;
            vieModel.IndexCreating = false;
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
        }

        private async void CreatePictureIndex(object sender, RoutedEventArgs e)
        {
            if (new MsgBox($"{LangManager.GetValueByKey("CurrentImageType")} {((PathType)ConfigManager.Settings.PicPathMode).ToString()}，{LangManager.GetValueByKey("TakeEffectToCurrent")}")
                .ShowDialog() == false) {
                return;
            }

            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                string sql = VideoMapper.BASE_SQL;
                IWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("metadata.DataID", "Path", "VID", "Hash");
                sql = wrapper.ToSelect(false) + sql;
                List<Dictionary<string, object>> temp = MapperManager.metaDataMapper.Select(sql);
                List<Video> videos = MapperManager.metaDataMapper.ToEntity<Video>(temp, typeof(Video).GetProperties(), true);
                total = videos.Count;
                if (total <= 0)
                    return false;
                List<string> list = new List<string>();
                long pathType = ConfigManager.Settings.PicPathMode;
                for (int i = 0; i < total; i++) {
                    Video video = videos[i];

                    // 小图
                    list.Add($"({video.DataID},{pathType},0,{(File.Exists(video.GetSmallImage()) ? 1 : 0)})");

                    // 大图
                    list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.GetBigImage()) ? 1 : 0)})");
                    if (IndexCanceled)
                        return false;
                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
                MapperManager.videoMapper.ExecuteNonQuery(insertSql);
                return true;
            });
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
            ConfigManager.Settings.PictureIndexCreated = true;
            vieModel.IndexCreating = false;
        }

        private bool IndexCanceled = false;

        private void CancelCreateIndex(object sender, RoutedEventArgs e)
        {
            IndexCanceled = true;
        }

        private void ViewSearchHistory(object sender, RoutedEventArgs e)
        {
        }



        private void SetDetailBg(object sender, RoutedEventArgs e)
        {
            // todo
            ConfigManager.Settings.DetailShowBg = vieModel.DetailShowBg;
        }

        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            if (idx < 0)
                return;
            if (vieModel.CrawlerServers?.Count > 0) {
                string pluginID = PluginType.Crawler.ToString().ToLower() + "/" + vieModel.DisplayCrawlerServers[idx];
                PluginMetaData pluginMetaData = CrawlerManager.PluginMetaDatas.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
                if (vieModel.CrawlerServers.ContainsKey(pluginID)) {
                    ServersDataGrid.ItemsSource = null;
                    ServersDataGrid.ItemsSource = vieModel.CrawlerServers[pluginID];
                    vieModel.CurrentPlugin = pluginMetaData;
                    ConfigManager.Settings.CrawlerSelectedIndex = idx;
                    vieModel.ShowCurrentPlugin = true;
                }

            }
        }

        private void SavePluginSetting(object sender, RoutedEventArgs e)
        {
            // 保存插件启用情况
            vieModel.CurrentPlugin?.SaveConfig();
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled) {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void RestoreDefault(object sender, RoutedEventArgs e)
        {
            if ((bool)new MsgBox(LangManager.GetValueByKey("Restore") + "?").ShowDialog(this)) {

                ConfigManager.Restore();

                // 基本
                vieModel.OpenDataBaseDefault = false;
                vieModel.ScanOnStartUp = false;
                vieModel.CloseToTaskBar = false;
                Properties.Settings.Default.DetailWindowShowAllMovie = true;
                ConfigManager.Settings.DelInfoAfterDelFile = true;
                Properties.Settings.Default.HotKey_Enable = false;
                Properties.Settings.Default.HotKey_String = "";
                langComboBox.SelectedIndex = 0;
                ConfigManager.Settings.VideoPlayerPath = "";

                // 图片
                vieModel.AutoGenScreenShot = true;
                Properties.Settings.Default.MainImageAutoMode = true;
                ImageSelectComboBox.SelectedIndex = 0;
                vieModel.BasePicPath = Path.Combine(PathManager.CurrentUserFolder, "pic");

                // 扫描与导入
                vieModel.FetchVID = true;
                vieModel.LoadDataAfterScan = true;
                vieModel.MinFileSize = ScanConfig.DEFAULT_MIN_FILE_SIZE;
                vieModel.DataExistsIndexAfterScan = true;
                Properties.Settings.Default.ScanNfo = false;
                vieModel.CopyNFOOverwriteImage = false;
                vieModel.CopyNFOPicture = true;
                vieModel.CopyNFOActorPicture = true;
                vieModel.CopyNFOActorPath = ".actor";
                vieModel.CopyNFOPreview = true;
                vieModel.CopyNFOPreviewPath = ".preview";

                vieModel.CopyNFOScreenShot = true;
                vieModel.CopyNFOScreenShotPath = ".screenshot";

                // NFO 解析规则
                NfoParse.RestoreDefault();
                NfoParse.SaveData(NfoParse.CurrentNFOParse);
                vieModel.LoadNfoParseData();

                // 网络
                vieModel.IgnoreCertVal = true;
                vieModel.HttpTimeout = ProxyConfig.DEFAULT_TIMEOUT;
                vieModel.DownloadWhenTitleNull = true;
                vieModel.SkipExistImage = false;
                vieModel.OverrideInfo = false;
                vieModel.SaveInfoToNFO = false;

                ConfigManager.DownloadConfig.DownloadThumbNail = true;
                ConfigManager.DownloadConfig.DownloadPoster = true;
                ConfigManager.DownloadConfig.DownloadPreviewImage = false;
                ConfigManager.DownloadConfig.DownloadActor = true;
                ConfigManager.DownloadConfig.Save();

                ConfigManager.ProxyConfig.ProxyMode = (int)ProxyConfig.DEFAULT_PROXY_MODE;
                ConfigManager.ProxyConfig.ProxyType = (int)ProxyConfig.DEFAULT_PROXY_TYPE;
                ConfigManager.ProxyConfig.Save();

                // 显示
                ConfigManager.Main.DisplaySearchBox = true;
                ConfigManager.Main.DisplayPage = true;
                ConfigManager.Main.PaginationCombobox = true;
                ConfigManager.Main.DisplayStatusBar = true;
                ConfigManager.Main.DisplayFunBar = true;
                ConfigManager.Main.DisplayNavigation = true;

                Properties.Settings.Default.ShowFilter = false;
                vieModel.DetailShowBg = true;
                Properties.Settings.Default.ScrollSpeedFactor = 1.5;

                ConfigManager.VideoConfig.DisplayID = true;
                ConfigManager.VideoConfig.DisplayTitle = true;
                ConfigManager.VideoConfig.DisplayDate = true;
                ConfigManager.VideoConfig.DisplayStamp = true;
                ConfigManager.VideoConfig.DisplayFavorites = true;

                Properties.Settings.Default.ShowFileNameIfTitleEmpty = true;
                Properties.Settings.Default.ShowCreateDateIfReleaseDateEmpty = true;
                Properties.Settings.Default.MovieOpacity = 1;

                // 视频处理
                vieModel.FFMPEG_Path = "";
                vieModel.ScreenShot_ThreadNum = FFmpegConfig.DEFAULT_THREAD_NUM;
                vieModel.SkipExistScreenShot = true;
                vieModel.ScreenShotAfterImport = true;
                vieModel.ScreenShotNum = FFmpegConfig.DEFAULT_SCREEN_SHOT_NUM;
                vieModel.ScreenShotIgnoreStart = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_START;
                vieModel.ScreenShotIgnoreEnd = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_END;
                vieModel.SkipExistGif = false;
                vieModel.GifWidth = FFmpegConfig.DEFAULT_GIF_WIDTH;
                vieModel.GifHeight = FFmpegConfig.DEFAULT_GIF_HEIGHT;
                vieModel.GifAutoHeight = true;
                vieModel.GifDuration = FFmpegConfig.DEFAULT_GIF_DURATION;

                // 重命名
                vieModel.RemoveTitleSpace = false;
                vieModel.AddRenameTag = false;
                ConfigManager.RenameConfig.OutSplit = RenameConfig.DEFAULT_OUT_SPLIT;
                ConfigManager.RenameConfig.InSplit = RenameConfig.DEFAULT_IN_SPLIT;
                vieModel.FormatString = "";
                ConfigManager.RenameConfig.Save();

                // 库
                vieModel.AutoBackup = true;
                vieModel.AutoBackupPeriodIndex = Jvedio.Core.WindowConfig.Settings.DEFAULT_BACKUP_PERIOD_INDEX;

                ConfigManager.Main.Save();
                Properties.Settings.Default.Save();
                ApplySettings(null, null);

            }
        }

        private void SetBasePicPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path += "\\";
                vieModel.BasePicPath = path;
            }
        }

        private void ShowScanReHelp(object sender, RoutedEventArgs e)
        {
            // MessageCard.Info("在扫描时，对于视频 VID 的识别，例如填写正则为 .*钢铁侠.* 则只要文件名含有钢铁侠，");

        }

        private void ShowCrawlerHelp(object sender, RoutedEventArgs e)
        {
            MessageCard.Info(LangManager.GetValueByKey("CrawlerServerHint"));
        }
    }
}
