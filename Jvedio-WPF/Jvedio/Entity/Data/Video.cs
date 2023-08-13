﻿using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Scan;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.Reflections;
using SuperUtils.Time;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using static Jvedio.App;

namespace Jvedio.Entity
{
    // todo 检视
    [Table(tableName: "metadata_video")]
    public class Video : MetaData
    {

        #region "事件"
        public static Action onPlayVideo;

        #endregion


        #region "静态属性"

        public static string[] HDV { get; set; } =
            new string[] { "hd", "high_definition", "high definition", "高清", "2k", "4k", "8k", "16k", "32k" };

        #endregion


        #region "属性"

        [TableId(IdType.AUTO)]
        public long MVID { get; set; }


        /// <summary>
        /// 此处不可删除，需要保证 select 查出 id
        /// </summary>
        public new long DataID { get; set; }

        public string VID { get; set; }


        private string _Series;

        public string Series {
            get { return _Series; }

            set {
                _Series = value;
                SeriesList = new ObservableCollection<ObservableString>();
                if (!string.IsNullOrEmpty(value))
                    foreach (var item in value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries))
                        SeriesList.Add(new ObservableString(item));

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ObservableString> _SeriesList;
        [TableField(exist: false)]
        public ObservableCollection<ObservableString> SeriesList {
            get { return _SeriesList; }
            set {
                _SeriesList = value;
                RaisePropertyChanged();
            }
        }



        private VideoType _VideoType;

        public VideoType VideoType {
            get { return _VideoType; }

            set {
                _VideoType = value;
                RaisePropertyChanged();
            }
        }

        public string Director { get; set; }

        public string Studio { get; set; }

        public string Publisher { get; set; }

        public string Plot { get; set; }

        public string Outline { get; set; }

        public int Duration { get; set; }


        private ObservableCollection<ObservableString> _SubSectionList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<ObservableString> SubSectionList {
            get { return _SubSectionList; }
            set {
                _SubSectionList = value;
                RaisePropertyChanged();
            }
        }

        private string _SubSection = string.Empty;

        public string SubSection {
            get { return _SubSection; }

            set {
                _SubSection = value;
                SubSectionList = new ObservableCollection<ObservableString>();
                if (!string.IsNullOrEmpty(value))
                    foreach (var item in value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries))
                        SubSectionList.Add(new ObservableString(item));

                if (SubSectionList.Count >= 2)
                    HasSubSection = true;
                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public bool HasSubSection { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<string> PreviewImagePathList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<BitmapSource> PreviewImageList { get; set; }

        public string ImageUrls { get; set; }

        public string WebType { get; set; }

        public string WebUrl { get; set; }

        public string ExtraInfo { get; set; }

        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage {
            get { return _smallimage; }

            set {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _bigimage;

        [TableField(exist: false)]
        public BitmapSource BigImage {
            get { return _bigimage; }

            set {
                _bigimage = value;
                RaisePropertyChanged();
            }
        }

        private Uri _GifUri;

        [TableField(exist: false)]
        public Uri GifUri {
            get { return _GifUri; }

            set {
                _GifUri = value;
                RaisePropertyChanged();
            }
        }

        private string _ActorNames;

        [TableField(exist: false)]
        public string ActorNames {
            get { return _ActorNames; }

            set {
                _ActorNames = value;
                if (!string.IsNullOrEmpty(value))
                    ActorNameList = value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> ActorNameList { get; set; }

        /// <summary>
        /// 旧数据库的 actorID 列表
        /// </summary>
        [TableField(exist: false)]
        public string OldActorIDs { get; set; }

        private List<ActorInfo> _ActorInfos;

        [TableField(exist: false)]
        public List<ActorInfo> ActorInfos {
            get { return _ActorInfos; }

            set {
                _ActorInfos = value;
                if (value != null) {
                    ActorNames = string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                        value.Select(arg => arg.ActorName).ToList());
                }

                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<Magnet> Magnets { get; set; }

        [TableField(exist: false)]
        public bool HasAssociation { get; set; }

        [TableField(exist: false)]
        public List<long> AssociationList { get; set; }

        // 仅用于 NFO 导入的时候的图片地址
        [TableField(exist: false)]
        public List<string> ActorThumbs { get; set; }

        #endregion



        public Video() : this(true)
        {
        }

        public Video(bool _initDefaultImage = true)
        {
            if (_initDefaultImage)
                InitDefaultImage();
        }

        // 延迟加载图片
        public void InitDefaultImage()
        {
            SmallImage = MetaData.DefaultSmallImage;
            BigImage = MetaData.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            PreviewImageList = new ObservableCollection<BitmapSource>();
        }

        public static void PlayVideoWithPlayer(string filepath, long dataID = 0)
        {
            if (File.Exists(filepath)) {
                bool success = false;
                if (!string.IsNullOrEmpty(ConfigManager.Settings.VideoPlayerPath) && File.Exists(ConfigManager.Settings.VideoPlayerPath)) {
                    success = FileHelper.TryOpenFile(ConfigManager.Settings.VideoPlayerPath, filepath);
                } else {
                    // 使用默认播放器
                    success = FileHelper.TryOpenFile(filepath);
                }

                if (success && dataID > 0) {
                    MapperManager.metaDataMapper.UpdateFieldById("ViewDate", DateHelper.Now(), dataID);
                    onPlayVideo?.Invoke();
                }
            } else {
                MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_OpenFail") + "：" + filepath);
            }
        }

        public static SelectWrapper<Video> InitWrapper()
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("metadata.DBId", ConfigManager.Main.CurrentDBId)
                .Eq("metadata.DataType", 0);
            return wrapper;
        }

        public static void SetTagStamps(ref Video video)
        {
            if (video == null || string.IsNullOrEmpty(video.TagIDs))
                return;
            List<long> list = video.TagIDs.Split(',').Select(arg => long.Parse(arg)).ToList();
            if (list != null && list.Count > 0) {
                video.TagStamp = new ObservableCollection<TagStamp>();
                foreach (var item in Jvedio.Entity.CommonSQL.TagStamp.TagStamps.Where(arg => list.Contains(arg.TagID)).ToList())
                    video.TagStamp.Add(item);
            }
        }

        public static void HandleEmpty(ref Video video)
        {
            if (ConfigManager.VideoConfig.ShowFileNameIfTitleEmpty
                && !string.IsNullOrEmpty(video.Path) && string.IsNullOrEmpty(video.Title))
                video.Title = System.IO.Path.GetFileNameWithoutExtension(video.Path);
            if (ConfigManager.VideoConfig.ShowCreateDateIfReleaseDateEmpty
                && !string.IsNullOrEmpty(video.LastScanDate) && string.IsNullOrEmpty(video.ReleaseDate))
                video.ReleaseDate = DateHelper.ToLocalDate(video.LastScanDate);
        }


        public bool toDownload()
        {
            if (ConfigManager.Settings.DownloadWhenTitleNull) {
                return string.IsNullOrEmpty(Title);
            }
            return string.IsNullOrEmpty(Title) ||
                string.IsNullOrEmpty(WebUrl) ||
                string.IsNullOrEmpty(ImageUrls);
        }

        /// <summary>
        /// ext 必须要带上 '.'
        /// </summary>
        /// <param name="imageType"></param>
        /// <param name="ext">ext 必须要带上 '.'</param>
        /// <returns></returns>
        private string GetImagePath(ImageType imageType, string ext = null)
        {
            string result = string.Empty;
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType != PathType.RelativeToData) {
                if (pathType == PathType.RelativeToApp)
                    basePicPath = System.IO.Path.Combine(PathManager.CurrentUserFolder, basePicPath);
                string saveDir = string.Empty;
                if (imageType == ImageType.Big)
                    saveDir = System.IO.Path.Combine(basePicPath, "BigPic");
                else if (imageType == ImageType.Small)
                    saveDir = System.IO.Path.Combine(basePicPath, "SmallPic");
                else if (imageType == ImageType.Preview)
                    saveDir = System.IO.Path.Combine(basePicPath, "ExtraPic");
                else if (imageType == ImageType.ScreenShot)
                    saveDir = System.IO.Path.Combine(basePicPath, "ScreenShot");
                else if (imageType == ImageType.Gif)
                    saveDir = System.IO.Path.Combine(basePicPath, "Gif");
                else if (imageType == ImageType.Actor) {
                    saveDir = System.IO.Path.Combine(basePicPath, "Actresses");
                    return System.IO.Path.GetFullPath(saveDir);
                }
                if (!Directory.Exists(saveDir))
                    FileHelper.TryCreateDir(saveDir);
                if (!string.IsNullOrEmpty(VID))
                    result = System.IO.Path.Combine(saveDir, $"{VID}{(string.IsNullOrEmpty(ext) ? string.Empty : ext)}");
                else
                    result = System.IO.Path.Combine(saveDir, $"{Hash}{(string.IsNullOrEmpty(ext) ? string.Empty : ext)}");
            } else {
                // todo 其它图片模式
            }

            if (!string.IsNullOrEmpty(result))
                return System.IO.Path.GetFullPath(result);
            return string.Empty;
        }

        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }

        public Dictionary<string, object> ToDictionary()
        {
            List<string> fields = new List<string> { "VideoType", "DataID", "VID", "Size", "Path", "Hash", "DataType" };
            Dictionary<string, object> dict = new Dictionary<string, object>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos) {
                if (fields.Contains(info.Name)) {
                    object value = info.GetValue(this);
                    if (value == null)
                        value = string.Empty;
                    dict.Add(info.Name, value);
                }
            }

            return dict;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Video video = obj as Video;
            return video != null && (video.DataID == this.DataID || video.MVID == this.MVID);
        }

        private static string ParseRelativeImageFileName(string path)
        {
            string dirName = System.IO.Path.GetDirectoryName(path);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();

            string[] arr = FileHelper.TryGetAllFiles(dirName, "*.*");
            if (arr == null || arr.Length == 0)
                return "";
            List<string> list = arr.ToList();

            list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();

            foreach (string item in list) {
                if (System.IO.Path.GetFileNameWithoutExtension(item).ToLower().IndexOf(fileName) >= 0)
                    return item;
            }

            return FileHelper.TryGetFullPath(path);
        }

        private static string ParseRelativePath(string path)
        {
            string rootDir = System.IO.Path.GetDirectoryName(path);
            List<string> list = DirHelper.TryGetDirList(rootDir).ToList();
            string dirName = System.IO.Path.GetFileName(path);
            foreach (string item in list) {
                if (System.IO.Path.GetFileName(item).ToLower().IndexOf(dirName.ToLower()) >= 0)
                    return item;
            }

            return FileHelper.TryGetFullPath(path);
        }

        public string GetSmallImage(string ext = ".jpg", bool searchExt = true)
        {
            string smallImagePath = GetImagePath(ImageType.Small, ext);
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string smallPath = System.IO.Path.Combine(basePicPath, dict["SmallImagePath"]);
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(smallPath)))
                    smallPath += ext;
                smallImagePath = ParseRelativeImageFileName(smallPath);
            }

            // 替换成其他扩展名
            if (searchExt && !File.Exists(smallImagePath))
                smallImagePath = FileHelper.FindWithExt(smallImagePath, ScanTask.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(smallImagePath);
        }

        public string GetBigImage(string ext = ".jpg", bool searchExt = true)
        {
            string bigImagePath = GetImagePath(ImageType.Big, ext);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string bigPath = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["BigImagePath"]));
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(bigPath)))
                    bigPath += ext;
                bigImagePath = ParseRelativeImageFileName(bigPath);
            }

            // 替换成其他扩展名
            if (searchExt && !File.Exists(bigImagePath))
                bigImagePath = FileHelper.FindWithExt(bigImagePath, ScanTask.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(bigImagePath);
        }

        public string GetExtraImage()
        {
            string imagePath = GetImagePath(ImageType.Preview);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["PreviewImagePath"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }
        public string GetActorPath()
        {
            string imagePath = GetImagePath(ImageType.Actor);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["ActorImagePath"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }

        public string GetScreenShot()
        {
            string imagePath = GetImagePath(ImageType.ScreenShot);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["ScreenShotPath"]));
                imagePath = ParseRelativePath(path);
            }

            return imagePath;
        }

        public string GetGifPath()
        {
            string imagePath = GetImagePath(ImageType.Gif, ".gif");

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["Gif"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }



        public bool ParseDictInfo(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos) {
                if (dict.ContainsKey(info.Name)) {
                    object value = dict[info.Name];
                    if (value == null)
                        continue;
                    if (value is List<string> list) {
                        info.SetValue(this, string.Join(SuperUtils.Values.ConstValues.SeparatorString, list));
                    } else if (value is string str) {
                        if (info.PropertyType == typeof(string)) {
                            info.SetValue(this, str);
                        } else if (info.PropertyType == typeof(int)) {
                            int.TryParse(str, out int val);
                            info.SetValue(this, val);
                        }
                    }
                }
            }

            // 图片地址
            ImageUrls = ParseImageUrlFromDict(dict);
            return true;
        }

        private string ParseImageUrlFromDict(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;
            Dictionary<string, object> result = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(ImageUrls);
            if (result == null)
                result = new Dictionary<string, object>();
            if (dict.ContainsKey("SmallImageUrl"))
                result["SmallImageUrl"] = dict["SmallImageUrl"];
            if (dict.ContainsKey("BigImageUrl"))
                result["BigImageUrl"] = dict["BigImageUrl"];
            if (dict.ContainsKey("ExtraImageUrl"))
                result["ExtraImageUrl"] = dict["ExtraImageUrl"];
            if (dict.ContainsKey("ActressImageUrl"))
                result["ActressImageUrl"] = dict["ActressImageUrl"];
            if (dict.ContainsKey("ActorNames"))
                result["ActorNames"] = dict["ActorNames"];
            return JsonConvert.SerializeObject(result);
        }

        public void SaveNfo()
        {
            if (!ConfigManager.Settings.SaveInfoToNFO)
                return;
            string dir = ConfigManager.Settings.NFOSavePath;
            bool overrideInfo = ConfigManager.Settings.OverrideInfo;

            string saveName = $"{VID.ToProperFileName()}.nfo";
            if (string.IsNullOrEmpty(VID))
                saveName = $"{System.IO.Path.GetFileNameWithoutExtension(Path)}.nfo";

            string saveFileName = string.Empty;

            if (Directory.Exists(dir))
                saveFileName = System.IO.Path.Combine(dir, saveName);

            // 与视频同路径，视频存在才行
            if (!Directory.Exists(dir) && File.Exists(Path))
                saveFileName = System.IO.Path.Combine(new FileInfo(Path).DirectoryName, saveName);

            if (string.IsNullOrEmpty(saveFileName))
                return;
            if (overrideInfo || !File.Exists(saveFileName))
                SaveToNFO(this, saveFileName);
        }

        public static string ToSqlField(string content)
        {
            if (content == SuperControls.Style.LangManager.GetValueByKey("ID")) {
                return "VID";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Title")) {
                return "Title";
            }

              // else if (content == SuperControls.Style.LangManager.GetValueByKey("TranslatedTitle"))
              // {
              //    return "chinesetitle";
              // }
              else if (content == SuperControls.Style.LangManager.GetValueByKey("VideoType")) {
                return "VideoType";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Tag")) {
                return "Series";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("ReleaseDate")) {
                return "ReleaseDate";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Year")) {
                return "ReleaseYear";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Duration")) {
                return "Duration";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Country")) {
                return "Country";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Director")) {
                return "Director";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Genre")) {
                return "Genre";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Label")) {
                return "Label";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Actor")) {
                return "ActorNames";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Studio")) {
                return "Studio";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Rating")) {
                return "Rating";
            } else {
                return content;
            }
        }

        public string[] ToFileName()
        {
            bool addTag = ConfigManager.RenameConfig.AddRenameTag;
            string formatString = ConfigManager.RenameConfig.FormatString;
            FileInfo fileInfo = new FileInfo(Path);
            string name = System.IO.Path.GetFileNameWithoutExtension(Path);
            string dir = fileInfo.Directory.FullName;
            string ext = fileInfo.Extension;
            string newName = string.Empty;
            MatchCollection matches = Regex.Matches(formatString, "\\{[a-zA-Z]+\\}");
            PropertyInfo[] propertyList = this.GetType().GetProperties();

            if (matches != null && matches.Count > 0) {
                newName = formatString;
                foreach (Match match in matches) {
                    string property = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                    try {
                        ReplaceWithValue(ref newName, property, propertyList);
                    } catch (Exception ex) {
                        throw ex;
                    }
                }
            }

            // 替换掉特殊字符
            foreach (char item in System.IO.Path.GetInvalidFileNameChars()) {
                newName = newName.Replace(item.ToString(), string.Empty);
            }

            if (ConfigManager.RenameConfig.RemoveTitleSpace)
                newName = newName.Trim();

            if (HasSubSection) {
                string[] result = new string[SubSectionList.Count];
                for (int i = 0; i < SubSectionList.Count; i++) {
                    if (addTag && JvedioLib.Security.Identify.IsCHS(Path))
                        result[i] = System.IO.Path.Combine(dir, $"{newName}-{i + 1}_{SuperControls.Style.LangManager.GetValueByKey("Translated")}{ext}");
                    else
                        result[i] = System.IO.Path.Combine(dir, $"{newName}-{i + 1}{ext}");
                }

                return result;
            } else {
                if (addTag && JvedioLib.Security.Identify.IsCHS(Path))
                    return new string[] { System.IO.Path.Combine(dir, $"{newName}_{SuperControls.Style.LangManager.GetValueByKey("Translated")}{ext}") };
                else
                    return new string[] { System.IO.Path.Combine(dir, $"{newName}{ext}") };
            }
        }

        private void ReplaceWithValue(ref string result, string property, PropertyInfo[] propertyList)
        {
            string inSplit = ConfigManager.RenameConfig.InSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.InSplit;
            foreach (PropertyInfo item in propertyList) {
                string name = item.Name;
                if (name == property) {
                    object o = item.GetValue(this);
                    if (o != null) {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(SuperUtils.Values.ConstValues.SeparatorString, inSplit);

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

                        if (string.IsNullOrEmpty(value)) {
                            // 如果值为空，则删掉前面的分隔符
                            int idx = result.IndexOf("{" + property + "}");
                            if (idx >= 1) {
                                result = result.Remove(idx - 1, 1);
                            }

                            result = result.Replace("{" + property + "}", string.Empty);
                        } else
                            result = result.Replace("{" + property + "}", value);
                    } else {
                        int idx = result.IndexOf("{" + property + "}");
                        if (idx >= 1) {
                            result = result.Remove(idx - 1);
                        }

                        result = result.Replace("{" + property + "}", string.Empty);
                    }

                    break;
                }
            }
        }


        public bool IsHDV()
        {
            return JvedioLib.Security.Identify.IsHDV(Size) ||
                   JvedioLib.Security.Identify.IsHDV(Path) ||
                   Genre?.IndexOfAnyString(Main.TagStringHD) >= 0 ||
                   Series?.IndexOfAnyString(Main.TagStringHD) >= 0 ||
                   Label?.IndexOfAnyString(Main.TagStringHD) >= 0;
        }

        public bool IsCHS()
        {
            return JvedioLib.Security.Identify.IsCHS(Path) ||
                   Genre?.IndexOfAnyString(Main.TagStringTranslated) >= 0 ||
                   Series?.IndexOfAnyString(Main.TagStringTranslated) >= 0 ||
                   Label?.IndexOfAnyString(Main.TagStringTranslated) >= 0;
        }

        public static void SetImage(ref Video video, string imgPath)
        {
            if (video == null)
                return;
            BitmapImage image =
                ImageCache.Get(imgPath, Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
            if (image == null)
                image = MetaData.DefaultBigImage;
            video.ViewImage = image;
        }
        public static void SetImage(ref Video video, int imageMode = 0)
        {
            if (imageMode < 2) {

                BitmapImage smallimage = ImageCache.Get(video.GetSmallImage(),
                    Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
                BitmapImage bigimage = ImageCache.Get(video.GetBigImage(),
                    Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);

                if (smallimage == null)
                    smallimage = MetaData.DefaultSmallImage;
                if (bigimage == null)
                    bigimage = MetaData.DefaultBigImage;
                video.SmallImage = smallimage;
                video.BigImage = bigimage;
            } else if (imageMode == 2) {
                // string gifpath = Video.parseImagePath(video.GifImagePath);
                // if (File.Exists(gifpath)) video.GifUri = new Uri(gifpath);
            }
        }

        /// <summary>
        /// 获取视频信息 （wmv  10ms，其他  100ms）
        /// </summary>
        public static VideoInfo GetMediaInfo(string videoPath)
        {
            VideoInfo videoInfo = new VideoInfo();
            if (File.Exists(videoPath)) {
                MediaInfo mI = null;
                try {
                    mI = new MediaInfo();
                    mI.Open(videoPath);

                    // 全局
                    string format = mI.Get(StreamKind.General, 0, "Format");
                    string bitrate = mI.Get(StreamKind.General, 0, "BitRate/String");
                    string duration = mI.Get(StreamKind.General, 0, "Duration/String1");
                    string fileSize = mI.Get(StreamKind.General, 0, "FileSize/String");

                    // 视频
                    string vid = mI.Get(StreamKind.Video, 0, "ID");
                    string video = mI.Get(StreamKind.Video, 0, "Format");
                    string vBitRate = mI.Get(StreamKind.Video, 0, "BitRate/String");
                    string vSize = mI.Get(StreamKind.Video, 0, "StreamSize/String");
                    string width = mI.Get(StreamKind.Video, 0, "Width");
                    string height = mI.Get(StreamKind.Video, 0, "Height");
                    string risplayAspectRatio = mI.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
                    string risplayAspectRatio2 = mI.Get(StreamKind.Video, 0, "DisplayAspectRatio");
                    string frameRate = mI.Get(StreamKind.Video, 0, "FrameRate/String");
                    string bitDepth = mI.Get(StreamKind.Video, 0, "BitDepth/String");
                    string pixelAspectRatio = mI.Get(StreamKind.Video, 0, "PixelAspectRatio");
                    string encodedLibrary = mI.Get(StreamKind.Video, 0, "Encoded_Library");
                    string encodeTime = mI.Get(StreamKind.Video, 0, "Encoded_Date");
                    string codecProfile = mI.Get(StreamKind.Video, 0, "Codec_Profile");
                    string frameCount = mI.Get(StreamKind.Video, 0, "FrameCount");

                    // 音频
                    string aid = mI.Get(StreamKind.Audio, 0, "ID");
                    string audio = mI.Get(StreamKind.Audio, 0, "Format");
                    string aBitRate = mI.Get(StreamKind.Audio, 0, "BitRate/String");
                    string samplingRate = mI.Get(StreamKind.Audio, 0, "SamplingRate/String");
                    string channel = mI.Get(StreamKind.Audio, 0, "Channel(s)");
                    string aSize = mI.Get(StreamKind.Audio, 0, "StreamSize/String");

                    string audioInfo = mI.Get(StreamKind.Audio, 0, "Inform") + mI.Get(StreamKind.Audio, 1, "Inform") + mI.Get(StreamKind.Audio, 2, "Inform") + mI.Get(StreamKind.Audio, 3, "Inform");
                    string vi = mI.Get(StreamKind.Video, 0, "Inform");

                    videoInfo = new VideoInfo() {
                        Format = format,
                        BitRate = vBitRate,
                        Duration = duration,
                        FileSize = fileSize,
                        Width = width,
                        Height = height,

                        DisplayAspectRatio = risplayAspectRatio,
                        FrameRate = frameRate,
                        BitDepth = bitDepth,
                        PixelAspectRatio = pixelAspectRatio,
                        Encoded_Library = encodedLibrary,
                        FrameCount = frameCount,
                        AudioFormat = audio,
                        AudioBitRate = aBitRate,
                        AudioSamplingRate = samplingRate,
                        Channel = channel,
                    };
                } catch (Exception ex) {
                    Logger.Error(ex);
                } finally {
                    mI?.Close();
                }
            }

            if (!string.IsNullOrEmpty(videoInfo.Width) && !string.IsNullOrEmpty(videoInfo.Height))
                videoInfo.Resolution = videoInfo.Width + "x" + videoInfo.Height;
            if (!string.IsNullOrEmpty(videoPath)) {
                videoInfo.Extension = System.IO.Path.GetExtension(videoPath)?.ToUpper().Replace(".", string.Empty);
                videoInfo.FileName = System.IO.Path.GetFileNameWithoutExtension(videoPath);
            }

            return videoInfo;
        }

        /// <summary>
        /// 保存信息到 NFO 文件
        /// </summary>
        /// <param name="video"></param>
        /// <param name="nfoPath"></param>
        public static void SaveToNFO(Video video, string nfoPath)
        {
            var nfo = new NFO(nfoPath, "movie");
            nfo.SetNodeText("source", video.WebUrl);
            nfo.SetNodeText("title", video.Title);
            nfo.SetNodeText("director", video.Director);
            nfo.SetNodeText("rating", video.Rating.ToString());
            nfo.SetNodeText("year", video.ReleaseYear.ToString());

            // nfo.SetNodeText("countrycode", video.Country.ToString());
            nfo.SetNodeText("release", video.ReleaseDate);
            nfo.SetNodeText("premiered", video.ReleaseDate);
            nfo.SetNodeText("runtime", video.Duration.ToString());
            nfo.SetNodeText("country", video.Country);
            nfo.SetNodeText("studio", video.Studio);
            nfo.SetNodeText("id", video.VID);
            nfo.SetNodeText("num", video.VID);

            // 类别
            foreach (var item in video.Genre?.Split(SuperUtils.Values.ConstValues.Separator)) {
                if (!string.IsNullOrEmpty(item))
                    nfo.AppendNewNode("genre", item);
            }

            // 系列
            foreach (var item in video.Series?.Split(SuperUtils.Values.ConstValues.Separator)) {
                if (!string.IsNullOrEmpty(item))
                    nfo.AppendNewNode("tag", item);
            }

            try {
                Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(video.ImageUrls);
                if (dict != null && dict.ContainsKey("ExtraImageUrl")) {
                    List<string> imageUrls = JsonUtils.TryDeserializeObject<List<string>>(dict["ExtraImageUrl"].ToString());
                    if (imageUrls != null && imageUrls.Count > 0) {
                        nfo.AppendNewNode("fanart");
                        foreach (var item in imageUrls) {
                            if (!string.IsNullOrEmpty(item))
                                nfo.AppendNodeToNode("fanart", "thumb", item, "preview", item);
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (video.ActorInfos != null && video.ActorInfos.Count > 0) {
                foreach (ActorInfo info in video.ActorInfos) {
                    if (!string.IsNullOrEmpty(info.ActorName)) {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", info.ActorName);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            int hashCode = -485885450;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DataID.GetHashCode();
            return hashCode;
        }
    }
}
