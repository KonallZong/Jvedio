﻿using ICSharpCode.AvalonEdit;
using Jvedio.Core.Media;
using Jvedio.Core.Scan;
using Jvedio.Core.UserControls;
using Jvedio.Core.UserControls.Tasks;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace Jvedio.ViewModels
{

    public class TabItemManager
    {

        private TabItemManager()
        {
        }

        private static TabItemManager instance { get; set; }

        private VieModel_Main vieModel { get; set; }
        private SimplePanel TabPanel { get; set; }

        public static TabItemManager CreateInstance(VieModel_Main vieModel, SimplePanel tabPanel)
        {
            if (instance == null) {
                instance = new TabItemManager();
                instance.vieModel = vieModel;
                instance.TabPanel = tabPanel;
            }
            return instance;
        }

        public void Add(TabType type, string tabName, object tabData)
        {
            if (vieModel == null) {
                return;
            }

            if (vieModel.TabItems == null) {
                vieModel.TabItems = new ObservableCollection<TabItemEx>();
                vieModel.TabItems.CollectionChanged += (s, ev) => {
                    if (vieModel.TabItems.Count == 0) {
                        vieModel.ShowSoft = true;
                    } else {
                        vieModel.ShowSoft = false;
                    }
                };
            }

            TabItemEx tabItem = vieModel.TabItems.FirstOrDefault(arg => arg.Name.Equals(tabName));

            if (tabItem != null) {

                SetTabSelected(vieModel.TabItems.IndexOf(tabItem));
                // 触发刷新
                RefreshTab(type);
                return;
                //RemoveTabItem(vieModel.TabItems.IndexOf(tabItem));
            }


            tabItem = new TabItemEx(tabName, type);
            vieModel.TabItems.Add(tabItem);


            int idx = -1;
            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                if (vieModel.TabItems[i].Name.Equals(tabName)) {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                SetTabSelected(idx);

            onAddData(tabItem, tabData);
        }

        private void OnViewAssoData(object sender, VideoItemEventArgs e)
        {
            OnViewAssoData(e.DataID);
        }


        public void OnViewAssoData(long dataID)
        {
            if (dataID <= 0)
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("DataID", dataID);
            Video currentVideo = MapperManager.videoMapper.SelectById(wrapper);

            // 设置关联
            HashSet<long> set = MapperManager.associationMapper.GetAssociationDatas(currentVideo.DataID);
            if (set != null) {
                currentVideo.HasAssociation = set.Count > 0;
                currentVideo.AssociationList = set.ToList();
            }


            if (currentVideo.AssociationList == null || currentVideo.AssociationList.Count <= 0)
                return;

            string tabName = currentVideo.VID;
            if (string.IsNullOrEmpty(tabName))
                tabName = currentVideo.Title;
            if (string.IsNullOrEmpty(tabName))
                tabName = System.IO.Path.GetFileNameWithoutExtension(currentVideo.Path);

            SelectWrapper<Video> extraWrapper = new SelectWrapper<Video>();

            currentVideo.AssociationList.Insert(0, dataID); // 自己也加入

            extraWrapper.In("metadata.DataID", currentVideo.AssociationList.Select(arg => arg.ToString()));
            Add(TabType.GeoAsso, $"关联：{tabName}", extraWrapper);
        }



        private void OnItemClick(object sender, VideoItemEventArgs e)
        {
            long dataID = e.DataID;
            onShowDetailData(dataID);
        }

        public void onShowDetailData(long dataID)
        {
            Window_Details windowDetails = new Window_Details(dataID);
            windowDetails.onViewAssoData += (id) => {
                OnViewAssoData(id);
                windowDetails.Close();
            };
            windowDetails.Show();
        }

        private void onAddData(TabItemEx tabItem, object tabData)
        {
            switch (tabItem.TabType) {
                case TabType.GeoVideo:
                case TabType.GeoStar:
                case TabType.GeoRecentPlay:
                case TabType.GeoAsso:
                    SelectWrapper<Video> ExtraWrapper = tabData as SelectWrapper<Video>;

                    VideoList videoList = new VideoList(ExtraWrapper, tabItem);

                    if (tabItem.TabType == TabType.GeoAsso)
                        videoList.SetAsso(false);
                    videoList.Uid = tabItem.UUID;
                    videoList.OnItemClick += OnItemClick;
                    videoList.OnItemViewAsso += OnViewAssoData;


                    TabPanel.Children.Add(videoList);


                    break;

                case TabType.GeoTask:
                    if (tabData is TaskType type) {
                        TaskList taskList = new TaskList(type);
                        taskList.Uid = tabItem.UUID;
                        SetTaskList(ref taskList, type);
                        TabPanel.Children.Add(taskList);
                    }
                    break;

                default:
                    break;
            }
        }




        private void SetTaskList(ref TaskList taskList, TaskType type)
        {
            switch (type) {
                case TaskType.ScreenShot:
                    taskList.TaskStatusList = App.ScreenShotManager.CurrentTasks;
                    taskList.onRemoveAll += () => App.ScreenShotManager.RemoveTask(TaskStatus.Canceled | TaskStatus.RanToCompletion);
                    taskList.onRemoveCancel += () => App.ScreenShotManager.RemoveTask(TaskStatus.Canceled);
                    taskList.onRemoveComplete += () => App.ScreenShotManager.RemoveTask(TaskStatus.RanToCompletion);
                    taskList.onCancel += App.ScreenShotManager.CancelTask;
                    taskList.onCancelAll += App.ScreenShotManager.CancelAll;
                    taskList.onShowDetail += (tList, id) => {
                        string logs = App.ScreenShotManager.GetTaskLogs(id);
                        tList.SetLogs(logs);
                        tList.ShowLog = true;
                    };

                    App.ScreenShotManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        list.AllTaskProgress = App.ScreenShotManager.Progress;
                    };

                    break;
                case TaskType.Scan:
                    taskList.TaskStatusList = App.ScanManager.CurrentTasks;
                    taskList.onRemoveAll += () => App.ScanManager.RemoveTask(TaskStatus.Canceled | TaskStatus.RanToCompletion);
                    taskList.onRemoveCancel += () => App.ScanManager.RemoveTask(TaskStatus.Canceled);
                    taskList.onRemoveComplete += () => App.ScanManager.RemoveTask(TaskStatus.RanToCompletion);
                    taskList.onCancel += App.ScanManager.CancelTask;
                    taskList.onCancelAll += App.ScanManager.CancelAll;
                    taskList.onShowDetail += (tList, id) => onShowScanDetail(id);

                    App.ScanManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        list.AllTaskProgress = App.ScanManager.Progress;
                    };


                    break;
                case TaskType.Download:
                    taskList.TaskStatusList = App.DownloadManager.CurrentTasks;
                    taskList.onRemoveAll += () => App.DownloadManager.RemoveTask(TaskStatus.Canceled | TaskStatus.RanToCompletion);
                    taskList.onRemoveCancel += () => App.DownloadManager.RemoveTask(TaskStatus.Canceled);
                    taskList.onRemoveComplete += () => App.DownloadManager.RemoveTask(TaskStatus.RanToCompletion);
                    taskList.onCancel += App.DownloadManager.CancelTask;
                    taskList.onCancelAll += App.DownloadManager.CancelAll;
                    taskList.onShowDetail += (tList, id) => {
                        string logs = App.DownloadManager.GetTaskLogs(id);
                        tList.SetLogs(logs);
                        tList.ShowLog = true;
                    };

                    taskList.onRestart += App.DownloadManager.Restart;

                    App.DownloadManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        if (list != null)
                            list.AllTaskProgress = App.DownloadManager.Progress;
                    };

                    break;

                default:

                    break;
            }
        }

        public VideoList GetVideoListByType(TabType type)
        {
            if (TabPanel.Children == null || TabPanel.Children.Count == 0)
                return null;

            List<VideoList> videoLists = TabPanel.Children.OfType<VideoList>().ToList();
            return videoLists.FirstOrDefault(arg => arg.TabItemEx.TabType == type);
        }

        public TaskList GetTaskListByType(TaskType type)
        {
            if (TabPanel.Children == null || TabPanel.Children.Count == 0)
                return null;

            List<TaskList> videoLists = TabPanel.Children.OfType<TaskList>().ToList();
            return videoLists.FirstOrDefault(arg => arg.TaskType == type);
        }

        public void RefreshTab(TabType type)
        {
            if (type == TabType.GeoTask)
                return;
            VideoList videoList = GetVideoListByType(type);
            videoList?.Refresh();
        }

        public void RefreshTab(int idx)
        {
            if (idx < 0 || idx >= TabPanel.Children.Count)
                return;
            UIElement uIElement = TabPanel.Children[idx];

            if (uIElement is VideoList videoList)
                videoList?.Refresh();
        }


        private void onShowScanDetail(string id)
        {
            ScanTask scanTask = App.ScanManager.CurrentTasks.FirstOrDefault(arg => arg.ID.Equals(id)) as ScanTask;
            if (scanTask == null)
                return;

            Window_ScanDetail scanDetail = new Window_ScanDetail(scanTask.ScanResult);
            scanDetail.Show();
        }



        public void RemovePanel(TabItemEx tabItem)
        {
            int idx = -1;
            foreach (UIElement item in TabPanel.Children) {
                idx++;
                string uid = item.Uid;
                if (string.IsNullOrEmpty(uid))
                    continue;
                if (uid.Equals(tabItem.UUID)) {
                    break;
                }
            }
            if (idx >= 0 && idx < TabPanel.Children.Count) {
                TabPanel.Children.RemoveAt(idx);
            }
        }

        public void RemoveTabItem(int idx)
        {
            if (vieModel.TabItems == null)
                return;
            if (idx >= 0 && idx < vieModel.TabItems.Count) {

                // 移除对应的 panel
                RemovePanel(vieModel.TabItems[idx]);
                vieModel.TabItems[idx].Pinned = false;
                vieModel.TabItems.RemoveAt(idx);
            }
            // 默认选中左边的
            int selectIndex = idx - 1;
            if (selectIndex < 0)
                selectIndex = 0;

            if (vieModel.TabItems.Count > 0)
                vieModel.TabItemManager?.SetTabSelected(selectIndex);
        }

        public void SetTabSelected(int idx)
        {
            if (vieModel.TabItems == null || idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                vieModel.TabItems[i].Selected = false;
            }
            vieModel.TabItems[idx].Selected = true;
            List<FrameworkElement> allData = TabPanel.Children.OfType<FrameworkElement>().ToList();

            int target = -1;
            for (int i = 0; i < allData.Count; i++) {
                allData[i].Visibility = System.Windows.Visibility.Hidden;
                if (allData[i].Uid.Equals(vieModel.TabItems[idx].UUID)) {
                    target = i;
                }
            }
            if (target >= 0 && target < allData.Count)
                allData[target].Visibility = System.Windows.Visibility.Visible;
        }

        public void PinByIndex(int idx)
        {

            if (idx < 0)
                return;

            if (vieModel == null || vieModel.TabItems == null || vieModel.TabItems.Count == 0 ||
                idx >= vieModel.TabItems.Count)
                return;
            TabItemEx tabItem = vieModel.TabItems[idx];
            if (tabItem.Pinned) {
                // 取消固定
                int targetIndex = vieModel.TabItems.Count;

                for (int i = vieModel.TabItems.Count - 1; i >= 0; i--) {
                    if (targetIndex == vieModel.TabItems.Count && vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex < vieModel.TabItems.Count && idx >= 0)
                        break;
                }

                if (targetIndex == vieModel.TabItems.Count)
                    targetIndex = 0;
                tabItem.Pinned = false;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            } else {
                // 固定
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (targetIndex < 0 && !vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex >= 0 && idx >= 0)
                        break;
                }
                if (targetIndex < 0)
                    return;
                tabItem.Pinned = true;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            }
        }

        public void MoveToLast(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned)
                        targetIndex = i;
                }
                vieModel.TabItems.Move(originIdx, targetIndex);
            } else {
                vieModel.TabItems.Move(originIdx, vieModel.TabItems.Count - 1);
            }
        }
        public void MoveToFirst(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                // 如果已经固定，则移动到所有固定的前面
                vieModel.TabItems.Move(originIdx, 0);
            } else {
                // 如果没有固定，则找到最后一个固定的
                bool hasPinned = false;
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned) {
                        hasPinned = true;
                        targetIndex = i;
                    }
                }

                if (targetIndex < 0 || targetIndex + 1 >= vieModel.TabItems.Count)
                    targetIndex = 0;
                if (hasPinned && targetIndex + 1 < vieModel.TabItems.Count)
                    vieModel.TabItems.Move(originIdx, targetIndex + 1);
                else
                    vieModel.TabItems.Move(originIdx, 0);
            }
        }

        public void RemoveRange(int start, int end)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int total = vieModel.TabItems.Count;

            if (start < 0 || start >= total || end < 0 || end >= total || start > end)
                return;

            for (int i = end; i >= start; i--) {
                if (vieModel.TabItems[i].Pinned)
                    continue;
                vieModel.TabItems.RemoveAt(i);
            }
        }
    }
}
