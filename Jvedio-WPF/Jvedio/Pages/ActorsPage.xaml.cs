﻿using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.VisualHelper;

namespace Jvedio.Pages
{
    /// <summary>
    /// ActorsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ActorsPage : Page, INotifyPropertyChanged
    {
        public event EventHandler ActorPageChangedCompleted;


        public ActorsPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        static ActorsPage()
        {
            for (int i = 0; i < ActorSortDictList.Count; i++) {
                ActorSortDict.Add(i, ActorSortDictList[i]);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshActorRenderToken();
            BindingEvent();
            ActorSetSelected();
        }

        #region "属性"

        private bool _SearchingActor = false;

        public bool SearchingActor {
            get { return _SearchingActor; }

            set {
                _SearchingActor = value;
                RaisePropertyChanged();
            }
        }

        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();

                // BeginSearch();
            }
        }

        private int _ActorPageSize = 10;

        public int ActorPageSize {
            get { return _ActorPageSize; }

            set {
                _ActorPageSize = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorTotalCount = 0;

        public long ActorTotalCount {
            get { return _ActorTotalCount; }

            set {
                _ActorTotalCount = value;
                RaisePropertyChanged();
            }
        }


        private int _CurrentActorPage = 1;

        public int CurrentActorPage {
            get { return _CurrentActorPage; }

            set {
                _CurrentActorPage = value;

                RaisePropertyChanged();
            }
        }

        private List<ActorInfo> actorlist;

        public List<ActorInfo> ActorList {
            get { return actorlist; }

            set {
                actorlist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ActorInfo> _CurrentActorList;

        public ObservableCollection<ActorInfo> CurrentActorList {
            get { return _CurrentActorList; }

            set {
                _CurrentActorList = value;
                RaisePropertyChanged();
            }
        }


        public int _CurrentActorCount = 0;

        public int CurrentActorCount {
            get { return _CurrentActorCount; }

            set {
                _CurrentActorCount = value;
                RaisePropertyChanged();
            }
        }

        private List<ActorInfo> _SelectedActors = new List<ActorInfo>();

        public List<ActorInfo> SelectedActors {
            get { return _SelectedActors; }

            set {
                _SelectedActors = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public void BindingEvent()
        {
            // 设置演员排序类型
            int actorSortType = Properties.Settings.Default.ActorSortType;
            var ActorMenuItems = ActorSortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < ActorMenuItems.Count; i++) {
                ActorMenuItems[i].Click += ActorSortMenu_Click;
                ActorMenuItems[i].IsCheckable = true;
                if (i == actorSortType)
                    ActorMenuItems[i].IsChecked = true;
            }


            // 设置演员显示模式
            var arbs = ActorViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            for (int i = 0; i < arbs.Count; i++) {
                arbs[i].Click += SetActorViewMode;
                if (i == Properties.Settings.Default.ActorViewMode)
                    arbs[i].IsChecked = true;
            }


            this.ActorPageChangedCompleted += (s, ev) => {
                if (Properties.Settings.Default.ActorEditMode)
                    ActorSetSelected();
            };
        }


        public void ActorSetSelected()
        {
            ItemsControl itemsControl = ActorItemsControl;
            if (itemsControl == null)
                return;

            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                Border border = FindElementByName<Border>(presenter, "rootBorder");
                if (border == null)
                    continue;
                long actorID = GetDataID(border);
                if (border != null && actorID > 0) {
                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (Properties.Settings.Default.ActorEditMode && SelectedActors != null &&
                        SelectedActors.Where(arg => arg.ActorID == actorID).Any()) {
                        border.Background = StyleManager.Common.HighLight.Background;
                        border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
                    }
                }
            }
        }

        private long GetDataID(UIElement o, bool findParent = true)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null)
                return -1;

            FrameworkElement target = element;
            if (findParent)
                target = element.FindParentOfType<SimplePanel>("rootGrid");

            if (target != null &&
                target.Tag != null &&
                target.Tag.ToString() is string tag &&
                long.TryParse(target.Tag.ToString(), out long id))
                return id;

            return -1;
        }

        public void ActorBorderMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
                return;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode && grid != null) {
                Border border = grid.Children[0] as Border;
                if (border != null)
                    border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
            }
        }

        public void ActorBorderMouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
                return;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode && grid != null) {
                long actorID = GetDataID(element);
                Border border = grid.Children[0] as Border;
                if (actorID <= 0 || border == null || SelectedActors == null)
                    return;
                if (SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                    border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
                else
                    border.BorderBrush = Brushes.Transparent;
            }
        }

        private int firstIdx { get; set; } = -1;
        private int secondIdx { get; set; } = -1;
        private int actorFirstIdx { get; set; } = -1;
        private int actorSecondIdx { get; set; } = -1;

        // todo 优化多选
        public void SelectActor(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement; // 点击 border 也能选中
            long actorID = GetDataID(element);
            if (actorID <= 0)
                return;
            if (Properties.Settings.Default.ActorEditMode && CurrentActorList != null) {
                ActorInfo actorInfo = CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                if (actorInfo == null)
                    return;
                int selectIdx = CurrentActorList.IndexOf(actorInfo);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                    if (actorFirstIdx == -1)
                        actorFirstIdx = selectIdx;
                    else
                        actorSecondIdx = selectIdx;
                }

                if (actorFirstIdx >= 0 && actorSecondIdx >= 0) {
                    if (actorFirstIdx > actorSecondIdx) {
                        // 交换一下顺序
                        int temp = actorFirstIdx;
                        actorFirstIdx = actorSecondIdx - 1;
                        actorSecondIdx = temp - 1;
                    }

                    for (int i = actorFirstIdx + 1; i <= actorSecondIdx; i++) {
                        ActorInfo m = CurrentActorList[i];
                        if (SelectedActors.Contains(m))
                            SelectedActors.Remove(m);
                        else
                            SelectedActors.Add(m);
                    }

                    actorFirstIdx = -1;
                    actorSecondIdx = -1;
                } else {
                    if (SelectedActors.Contains(actorInfo))
                        SelectedActors.Remove(actorInfo);
                    else
                        SelectedActors.Add(actorInfo);
                }

                ActorSetSelected();
            }
        }

        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ActorEditMode = true;
            bool allContain = true; // 检测是否取消选中
            foreach (var item in CurrentActorList) {
                if (!SelectedActors.Contains(item)) {
                    SelectedActors.Add(item);
                    allContain = false;
                }
            }

            if (allContain)
                SelectedActors.RemoveMany(CurrentActorList);
            ActorSetSelected();
        }


        public void SetActorViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null)
                return;
            var rbs = ActorViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            Properties.Settings.Default.ActorViewMode = idx;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();
        }

        private void ActorSortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++) {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem) {
                    item.IsChecked = true;
                    if (i == Properties.Settings.Default.ActorSortType) {
                        Properties.Settings.Default.ActorSortDescending = !Properties.Settings.Default.ActorSortDescending;
                    }

                    Properties.Settings.Default.ActorSortType = i;
                } else
                    item.IsChecked = false;
            }

            SelectActor();
        }


        public static Queue<int> ActorPageQueue { get; set; } = new Queue<int>();
        public static Dictionary<int, string> ActorSortDict { get; set; } = new Dictionary<int, string>();


        public bool RenderingActor { get; set; }

        public static List<string> ActorSortDictList = new List<string>()
        {
            "actor_info.Grade",
            "actor_info.ActorName",
            "Count",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Waist",
            "Hipline",
            "actor_info.Grade",
            "Age",
        };


        public static string[] ActorSelectedField = new string[]
        {
            "count(ActorName) as Count",
            "actor_info.ActorID",
            "actor_info.ActorName",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Waist",
            "Hipline",
            "WebType",
            "WebUrl",
            "actor_info.Grade",
            "actor_info.ExtraInfo",
            "actor_info.CreateDate",
            "actor_info.UpdateDate",
        };

        public CancellationTokenSource RenderActorCTS { get; set; }

        public CancellationToken RenderActorCT { get; set; }

        public void RefreshActorRenderToken()
        {
            RenderActorCTS = new CancellationTokenSource();
            RenderActorCTS.Token.Register(() => { Logger.Warn("cancel load actor page task"); });
            RenderActorCT = RenderActorCTS.Token;
        }

        public async void SelectActor()
        {

            // 判断当前获取的队列
            while (ActorPageQueue.Count > 1) {
                int page = ActorPageQueue.Dequeue();
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (RenderingActor) {
                RenderActorCTS?.Cancel(); // 取消加载
                await Task.Delay(100);
            }

            // todo
            //App.Current.Dispatcher.Invoke((Action)delegate {
            //    MainWindow.ActorScrollViewer.ScrollToTop(); // 滚到顶部
            //});

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            ActorSetActorSortOrder(wrapper);

            bool search = SearchingActor && !string.IsNullOrEmpty(SearchText);

            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                         "on metadata_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadata_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                         $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID " +
                         "UNION " +
                         "select actor_info.ActorID  " +
                         "FROM actor_info WHERE NOT EXISTS " +
                         "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) " +
                         $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID)";

            ActorTotalCount = actorMapper.SelectCount(count_sql);

            string sql = $"{wrapper.Select(ActorSelectedField).ToSelect(false)} FROM actor_info " +
                $"join metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                $"join metadata on metadata_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : string.Empty)} " +
                $"GROUP BY actor_info.ActorID " +
                "UNION " +
                $"{wrapper.Select(ActorSelectedField).ToSelect(false)} FROM actor_info " +
                "WHERE NOT EXISTS(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) GROUP BY actor_info.ActorID " +
                $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : string.Empty)} " +
                wrapper.ToOrder() + ActorToLimit();

            // todo 只能手动设置页码，很奇怪
            //App.Current.Dispatcher.Invoke(() => { MainWindow.actorPagination.Total = ActorTotalCount; });
            RenderCurrentActors(sql);
        }



        public void ActorSetActorSortOrder<T>(IWrapper<T> wrapper)
        {
            if (wrapper == null || Properties.Settings.Default.ActorSortType >= ActorSortDict.Count)
                return;
            string sortField = ActorSortDict[Properties.Settings.Default.ActorSortType];
            if (Properties.Settings.Default.ActorSortDescending)
                wrapper.Desc(sortField);
            else
                wrapper.Asc(sortField);
        }

        public string ActorToLimit()
        {
            int row_count = ActorPageSize;
            long offset = ActorPageSize * (CurrentActorPage - 1);
            return $" LIMIT {offset},{row_count}";
        }

        public static Dictionary<string, string> Actor_SELECT_TYPE = new Dictionary<string, string>()
        {
            { "All", "  " },
            { "Favorite", "  " },
        };


        public void RenderCurrentActors(string sql)
        {
            List<Dictionary<string, object>> list = actorMapper.Select(sql);
            List<ActorInfo> actors = actorMapper.ToEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            ActorList = new List<ActorInfo>();
            if (actors == null)
                actors = new List<ActorInfo>();
            ActorList.AddRange(actors);
            RenderActor();
        }

        public async void RenderActor()
        {
            if (CurrentActorList == null)
                CurrentActorList = new ObservableCollection<ActorInfo>();

            for (int i = 0; i < ActorList.Count; i++) {
                try {
                    RenderActorCT.ThrowIfCancellationRequested();
                } catch (OperationCanceledException) {
                    RenderActorCTS?.Dispose();
                    break;
                }

                RenderingActor = true;
                ActorInfo actorInfo = ActorList[i];
                ActorInfo.SetImage(ref actorInfo);
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new LoadActorDelegate(LoadActor), actorInfo, i);
            }

            // 清除
            for (int i = CurrentActorList.Count - 1; i > ActorList.Count - 1; i--) {
                CurrentActorList.RemoveAt(i);
            }

            if (RenderActorCT.IsCancellationRequested)
                RefreshActorRenderToken();
            RenderingActor = false;

            // if (pageQueue.Count > 0) pageQueue.Dequeue();
            ActorPageChangedCompleted?.Invoke(this, null);
        }

        private delegate void LoadActorDelegate(ActorInfo actor, int idx);

        private void LoadActor(ActorInfo actor, int idx)
        {
            if (RenderActorCT.IsCancellationRequested)
                return;
            if (CurrentActorList.Count < ActorPageSize) {
                if (idx < CurrentActorList.Count) {
                    CurrentActorList[idx] = null;
                    CurrentActorList[idx] = actor;
                } else {
                    CurrentActorList.Add(actor);
                }
            } else {
                CurrentActorList[idx] = null;
                CurrentActorList[idx] = actor;
            }

            CurrentActorCount = CurrentActorList.Count;
        }

        private void Page_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right) {
                // 末页
                ActorSetSelected();
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left) {
                CurrentActorPage = 1;
                ActorSetSelected();

            }
        }

        private void SetActorSelectMode(object sender, RoutedEventArgs e)
        {
            SelectedActors.Clear();
            ActorSetSelected();
        }

        private void CurrentActorPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            CurrentActorPage = pagination.CurrentPage;
            ActorPageQueue.Enqueue(pagination.CurrentPage);
            SelectActor();
        }

        private void ActorPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            ActorPageSize = pagination.PageSize;
        }

        public void RefreshActor(long actorID)
        {
            if (CurrentActorList?.Count <= 0)
                return;
            for (int i = 0; i < CurrentActorList.Count; i++) {
                if (CurrentActorList[i]?.ActorID == actorID) {
                    long count = CurrentActorList[i].Count;
                    CurrentActorList[i].SmallImage = null;
                    CurrentActorList[i] = null;

                    SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                    wrapper.Eq("ActorID", actorID);
                    ActorInfo actorInfo = actorMapper.SelectById(wrapper);
                    if (actorInfo == null)
                        continue;
                    ActorInfo.SetImage(ref actorInfo);
                    actorInfo.Count = count;

                    CurrentActorList[i] = actorInfo;
                    CurrentActorList[i].SmallImage = actorInfo.SmallImage;
                    break;
                }
            }
        }

        private void DeleteActors(object sender, RoutedEventArgs e)
        {
            if (new MsgBox("即将删除演员信息，是否继续？").ShowDialog() == true) {
                MenuItem mnu = sender as MenuItem;
                ContextMenu contextMenu = mnu.Parent as ContextMenu;

                // FrameworkElement image = contextMenu.PlacementTarget as FrameworkElement;
                long.TryParse(contextMenu.Tag.ToString(), out long actorID);
                if (actorID <= 0)
                    return;

                if (!Properties.Settings.Default.ActorEditMode)
                    SelectedActors.Clear();
                ActorInfo actor = CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                if (!SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                    SelectedActors.Add(actor);

                foreach (ActorInfo actorInfo in SelectedActors) {
                    actorMapper.DeleteById(actorInfo.ActorID);
                    string sql = $"delete from metadata_to_actor where metadata_to_actor.ActorID='{actorInfo.ActorID}'";
                    actorMapper.ExecuteNonQuery(sql);
                }

                SelectActor();
            }
        }

        // todo
        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            // if (downLoadActress?.State == DownLoadState.DownLoading)
            // {
            //    msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_WaitForDownload")); return;
            // }

            // if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            // StackPanel sp = null;
            // if (sender is MenuItem mnu)
            // {
            //    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            //    string name = sp.Tag.ToString();
            //    Actress CurrentActress = GetActressFromVieModel(name);
            //    if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
            //    StartDownLoadActor(SelectedActress);

            // }
            // if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
        }

        private void ActorImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            // string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            //    string file = dragdropFiles[0];

            // Image image = sender as Image;
            //    StackPanel stackPanel = image.Parent as StackPanel;
            //    TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            //    string name = textBox.Text.Split('(')[0];

            // Actress currentActress = null;
            //    for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            //    {
            //        if (vieModel.CurrentActorList[i].name == name)
            //        {
            //            currentActress = vieModel.CurrentActorList[i];
            //            break;
            //        }
            //    }

            // if (currentActress == null) return;

            // if (IsFile(file))
            //    {
            //        FileInfo fileInfo = new FileInfo(file);
            //        if (fileInfo.Extension.ToLower() == ".jpg")
            //        {
            //            FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{currentActress.name}.jpg", true);
            //            Actress actress = currentActress;
            //            actress.smallimage = null;
            //            actress.smallimage = GetActorImage(actress.name);

            // if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

            // for (int i = 0; i < vieModel.ActorList.Count; i++)
            //            {
            //                if (vieModel.ActorList[i].name == actress.name)
            //                {
            //                    vieModel.ActorList[i] = null;
            //                    vieModel.ActorList[i] = actress;
            //                    break;
            //                }
            //            }

            // for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            //            {
            //                if (vieModel.CurrentActorList[i].name == actress.name)
            //                {
            //                    vieModel.CurrentActorList[i] = null;
            //                    vieModel.CurrentActorList[i] = actress;
            //                    break;
            //                }
            //            }

            // }
            //        else
            //        {
            //            msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_OnlySupportJPG"));
            //        }
            //    }
        }

        private void OpenActorImagePath(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = mnu.Parent as ContextMenu;
            long.TryParse(contextMenu.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            ActorInfo actorInfo = actorMapper.SelectById(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            string path = Path.GetFullPath(actorInfo.GetImagePath());
            FileHelper.TryOpenSelectPath(path);
        }


        private void SideActorRate_ValueChanged(object sender, EventArgs e)
        {
            Rate rate = sender as Rate;
            if (rate.Tag == null)
                return;
            long.TryParse(rate.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            actorMapper.UpdateFieldById("Grade", rate.Value.ToString(), actorID);
        }
        private void EditActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;

            Window_EditActor window_EditActor = new Window_EditActor(actorID);
            window_EditActor.ShowDialog();
        }

        private void ShowSameActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            //ShowSameActor(actorID);
        }



        private void EditActor(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;

            Window_EditActor window_EditActor = new Window_EditActor(actorID);
            window_EditActor.ShowDialog();
        }

        private void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            //ShowSameActor(actorID);
        }

        private void NewActor(object sender, RoutedEventArgs e)
        {
            bool? success = new Window_EditActor(0).ShowDialog();
            if ((bool)success) {
                MessageNotify.Success(LangManager.GetValueByKey("AddSuccess"));

                // todo
                //vieModel.Statistic();
            }
        }
    }
}
