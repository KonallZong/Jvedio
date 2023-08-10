﻿using Jvedio.Core.Enums;
using Jvedio.Core.Media;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.WPF.VieModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.ViewModel
{
    class VieModel_Edit : ViewModelBase
    {

        #region "事件"
        private delegate void LoadLabelDelegate(string str);

        private void LoadLabel(string str) => CurrentLabelList.Add(str);
        #endregion

        #region "属性"
        private Window_Edit WindowEdit { get; set; }

        private List<string> OldLabels { get; set; }

        private bool LoadingLabel { get; set; }

        private Video _CurrentVideo;

        public Video CurrentVideo {
            get { return _CurrentVideo; }

            set {
                _CurrentVideo = value;
                RaisePropertyChanged();
            }
        }

        private bool _MoreExpanded = ConfigManager.Edit.MoreExpanded;

        public bool MoreExpanded {
            get { return _MoreExpanded; }

            set {
                _MoreExpanded = value;
                RaisePropertyChanged();
            }
        }

        private long _DataID;

        public long DataID {
            get { return _DataID; }

            set {
                _DataID = value;
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

        private ObservableCollection<string> _CurrentLabelList;

        public ObservableCollection<string> CurrentLabelList {
            get { return _CurrentLabelList; }

            set {
                _CurrentLabelList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ActorInfo> _ViewActors;

        /// <summary>
        /// 用户可见的 ActorLIst
        /// </summary>
        public ObservableCollection<ActorInfo> ViewActors {
            get { return _ViewActors; }

            set {
                _ViewActors = value;
                RaisePropertyChanged();
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

        private int _CurrentActorCount = 0;

        public int CurrentActorCount {
            get { return _CurrentActorCount; }

            set {
                _CurrentActorCount = value;
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

        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();
                SelectActor();
            }
        }

        private string _LabelText = string.Empty;

        public string LabelText {
            get { return _LabelText; }

            set {
                _LabelText = value;
                RaisePropertyChanged();
                GetLabels();
            }
        }

        private string _ActorName = string.Empty;

        public string ActorName {
            get { return _ActorName; }

            set {
                _ActorName = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorID;

        public long ActorID {
            get { return _ActorID; }

            set {
                _ActorID = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _CurrentImage;

        public BitmapSource CurrentImage {
            get { return _CurrentImage; }

            set {
                _CurrentImage = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        public VieModel_Edit(long dataId, Window_Edit windowEdit)
        {
            WindowEdit = windowEdit;
            if (dataId <= 0) {
                Logger.Error("data id must > 0");
                return;
            }
            DataID = dataId;
            Init();
        }

        public override void Init()
        {
            CurrentVideo = null;
            CurrentVideo = MapperManager.videoMapper.SelectVideoByID(DataID);
            OldLabels = CurrentVideo.LabelList?.Select(arg => arg.Value).ToList();
            ViewActors = new ObservableCollection<ActorInfo>();
            foreach (ActorInfo info in CurrentVideo.ActorInfos)
                ViewActors.Add(info);

            GetLabels();

            Logger.Info("init ok");
        }

        public async void GetLabels()
        {
            if (LoadingLabel) {
                Logger.Warn("label is loading");
                return;
            }

            LoadingLabel = true;
            string like_sql = string.Empty;

            string search = LabelText.ToProperSql().Trim();
            if (!string.IsNullOrEmpty(search))
                like_sql = $" and LabelName like '%{search}%' ";

            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}" + like_sql +
                $" GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            if (list != null) {
                foreach (Dictionary<string, object> item in list) {
                    if (!item.ContainsKey("LabelName") || !item.ContainsKey("Count") ||
                        item["LabelName"] == null || item["Count"] == null)
                        continue;
                    string labelName = item["LabelName"].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    labels.Add($"{labelName}({count})");
                }
            }

            CurrentLabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++) {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadLabel), labels[i]);
            }

            LoadingLabel = false;
        }


        // todo 演员
        public bool Save()
        {
            if (CurrentVideo == null)
                return false;
            MetaData data = CurrentVideo.toMetaData();
            if (data == null)
                return false;

            data.DataID = DataID;
            int update1 = MapperManager.metaDataMapper.UpdateById(data);
            int update2 = MapperManager.videoMapper.UpdateById(CurrentVideo);

            Logger.Info($"save metadata ret[{update1}], video ret[{update2}]");

            // 标签
            MapperManager.metaDataMapper.SaveLabel(data);
            Logger.Info("save label");

            // 演员
            MapperManager.videoMapper.SaveActor(CurrentVideo, ViewActors.ToList());
            Logger.Info("save actors");

            return update1 > 0 & update2 > 0;
        }

        private delegate void LoadActorDelegate(ActorInfo actor, int idx);

        private void LoadActor(ActorInfo actor, int idx)
        {
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


        // todo 模块化
        public async void SelectActor()
        {
            string search = SearchText.ToProperSql().Trim();
            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                         "on metadata_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadata_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                         $"{(!string.IsNullOrEmpty(search) ? $"and actor_info.ActorName like '%{search}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID " +
                         "UNION " +
                         "select actor_info.ActorID  " +
                         "FROM actor_info WHERE NOT EXISTS " +
                         "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) " +
                         $"{(!string.IsNullOrEmpty(search) ? $"and actor_info.ActorName like '%{search}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID)";

            ActorTotalCount = actorMapper.SelectCount(count_sql);
            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();

            string sql = $"{wrapper.Select(Jvedio.Core.UserControls.ActorList.ActorSelectedField).ToSelect(false)} FROM actor_info " +
                        $"join metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                        $"join metadata on metadata_to_actor.DataID=metadata.DataID " +
                        $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                       $"{(!string.IsNullOrEmpty(search) ? $"and actor_info.ActorName like '%{search}%' " : string.Empty)} " +
                        $"GROUP BY actor_info.ActorID " +
                        "UNION " +
                       $"{wrapper.Select(Jvedio.Core.UserControls.ActorList.ActorSelectedField).ToSelect(false)} FROM actor_info " +
                        "WHERE NOT EXISTS(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) GROUP BY actor_info.ActorID " +
                         $"{(!string.IsNullOrEmpty(search) ? $"and actor_info.ActorName like '%{search}%' " : string.Empty)} "
                         + ActorToLimit();

            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { WindowEdit.actorPagination.Total = ActorTotalCount; });

            List<Dictionary<string, object>> list = actorMapper.Select(sql);
            List<ActorInfo> actors = actorMapper.ToEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            ActorList = new List<ActorInfo>();
            if (actors == null)
                actors = new List<ActorInfo>();
            ActorList.AddRange(actors);

            if (CurrentActorList == null)
                CurrentActorList = new ObservableCollection<ActorInfo>();
            for (int i = 0; i < ActorList.Count; i++) {
                ActorInfo actorInfo = ActorList[i];

                // 加载图片
                PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
                BitmapImage smallimage = null;
                if (pathType != PathType.RelativeToData) {
                    // 如果是相对于影片格式的，则不设置图片
                    string smallImagePath = actorInfo.GetImagePath();
                    smallimage = ImageCache.Get(smallImagePath);
                }

                if (smallimage == null)
                    smallimage = MetaData.DefaultActorImage;
                actorInfo.SmallImage = smallimage;
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo, i);
            }

            // 清除
            for (int i = CurrentActorList.Count - 1; i > ActorList.Count - 1; i--) {
                CurrentActorList.RemoveAt(i);
            }
        }

        public string ActorToLimit()
        {
            int row_count = ActorPageSize;
            long offset = ActorPageSize * (CurrentActorPage - 1);
            return $" LIMIT {offset},{row_count}";
        }

    }
}
