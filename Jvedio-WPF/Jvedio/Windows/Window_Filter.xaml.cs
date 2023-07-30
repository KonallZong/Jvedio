﻿using SuperControls.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Filter : BaseWindow
    {
        #region "属性"

        private Main main { get; set; } = GetWindowByName("Main", App.Current.Windows) as Main;

        #endregion

        #region "静态属性"

        private static List<string> FilterList { get; set; } = new List<string>()
         {
            "Year",
            "Month",
            "Duration",
            "Genre",
            "Grade",
            "Series",
            "Director",
            "Studio",
            "Publisher",
        };

        List<string> baseList { get; set; } = new List<string>()
            {
                "可播放",
                "不可播放",
                "分段视频",
                "含磁力",
            };

        List<string> imageList { get; set; } = new List<string>()
            {
                "含海报",
                "含缩略图",
                "含预览图",
                "含截图",
                "含演员头像",
            };

        #endregion


        public Window_Filter()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            InitSize();         // 调整位置和大小

            SetCommonFilter(); // 加载默认
            LoadData(); // 加载过滤器
        }

        private void InitSize()
        {
            if (ConfigManager.Filter.Width > 0)
                this.Width = ConfigManager.Filter.Width;
            if (ConfigManager.Filter.Height > 0)
                this.Height = ConfigManager.Filter.Height;

            if (ConfigManager.Filter.X > 0)
                this.Left = ConfigManager.Filter.X;
            else
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;

            if (ConfigManager.Filter.Y > 0)
                this.Top = ConfigManager.Filter.Y;
            else
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
        }

        private void SaveConfig()
        {
            ConfigManager.Filter.X = this.Left;
            ConfigManager.Filter.Y = this.Top;
            ConfigManager.Filter.Width = this.Width;
            ConfigManager.Filter.Height = this.Height;
            ConfigManager.Filter.Save();
        }


        private void SetCommonFilter()
        {
            foreach (string item in baseList)
                basicWrapPanel.Children.Add(buildToggleButton(item));

            foreach (string item in Enum.GetValues(typeof(Jvedio.Core.Enums.VideoType))
                .Cast<Jvedio.Core.Enums.VideoType>().Select(v => v.ToString()).ToList())
                videoTypeWrapPanel.Children.Add(buildToggleButton(item));

            foreach (string item in imageList)
                imageWrapPanel.Children.Add(buildToggleButton(item));
        }

        private void LoadData()
        {
            string sql = $"SELECT DISTINCT ReleaseDate FROM metadata " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);

            // 2020-02-10
            List<string> dates = list.Select(x => x["ReleaseDate"].ToString())
                .Where(arg => !string.IsNullOrEmpty(arg) && arg.LastIndexOf('-') > 5).ToList();

            HashSet<string> years = dates.Select(arg => arg.Split('-')[0]).ToHashSet().OrderBy(x => x).Where(arg => arg != "1900" && arg != "0001").ToHashSet();
            HashSet<string> months = dates.Select(arg => arg.Split('-')[1]).ToHashSet().OrderBy(x => x).ToHashSet();

            // 年份
            yearStackPanel.Children.Clear();
            foreach (string year in years)
                yearStackPanel.Children.Add(buildToggleButton(year));

            // 月份
            monthStackPanel.Children.Clear();
            foreach (string month in months)
                monthStackPanel.Children.Add(buildToggleButton(month));

            // 时长
            durationWrapPanel.Children.Clear();
            durationWrapPanel.Children.Add(buildToggleButton("全部", true));
            foreach (string item in new string[] { "<30min", "30min-1h", "1h-2h", ">2h" })
                durationWrapPanel.Children.Add(buildToggleButton(item));

            LoadSingleDataFromMetaData(genreWrapPanel, "Genre"); // 类别
            LoadSingleData(seriesWrapPanel, "Series"); // 系列
            LoadSingleData(directorWrapPanel, "Director"); // 导演
            LoadSingleData(studioWrapPanel, "Studio"); // 制作商
            LoadSingleData(publisherWrapPanel, "Publisher"); // 发行商
        }

        private void LoadSingleData(WrapPanel wrapPanel, string field)
        {
            string sql = $"SELECT DISTINCT {field} FROM metadata_video join metadata on metadata.DataID=metadata_video.DataID " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);
            wrapPanel.Children.Clear();
            wrapPanel.Children.Add(buildToggleButton("全部", true));
            foreach (string item in set)
                wrapPanel.Children.Add(buildToggleButton(item));
        }

        private void LoadSingleDataFromMetaData(WrapPanel wrapPanel, string field)
        {
            string sql = $"SELECT DISTINCT {field} FROM metadata " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);
            wrapPanel.Children.Clear();
            wrapPanel.Children.Add(buildToggleButton("全部", true));
            foreach (string item in set)
                wrapPanel.Children.Add(buildToggleButton(item));
        }

        private ToggleButton buildToggleButton(string content, bool isChecked = false)
        {
            ToggleButton toggleButton = new ToggleButton();

            toggleButton.Margin = new Thickness(5);
            toggleButton.Padding = new Thickness(10, 5, 10, 5);
            toggleButton.Content = content;
            toggleButton.IsChecked = isChecked;
            toggleButton.Style = (System.Windows.Style)App.Current.Resources["FlatToggleButtonStyle"];
            return toggleButton;
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            this.Hide();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadData();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void SetTopMost(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            this.Topmost = (bool)toggleButton.IsChecked;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
