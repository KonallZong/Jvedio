﻿
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Scan;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.Reflections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    [Table(tableName: "actor_info")]
    public class ActorInfo : INotifyPropertyChanged
    {
        public ActorInfo()
        {
            Cup = 'Z';
            Gender = Gender.Girl;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [TableId(IdType.AUTO)]
        public long ActorID { get; set; }

        public string ActorName { get; set; }

        public string Country { get; set; }

        public string Nation { get; set; }

        public string BirthPlace { get; set; }

        public string Birthday { get; set; }

        public int Age { get; set; }

        public string BloodType { get; set; }

        public int Height { get; set; }

        public int Weight { get; set; }

        public Gender Gender { get; set; }

        public string Hobby { get; set; }

        public char Cup { get; set; }

        public int Chest { get; set; }

        public int Waist { get; set; }

        public int Hipline { get; set; }

        public string WebType { get; set; }

        public string WebUrl { get; set; }

        public float Grade { get; set; }

        public string ExtraInfo { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }

        public string ImageUrl { get; set; }

        [TableField(exist: false)]
        public long ImageID { get; set; }

        /// <summary>
        /// 出演的作品的数量
        /// </summary>
        [TableField(exist: false)]
        public long Count { get; set; }

        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage {
            get { return _smallimage; }

            set {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        public static void SetImage(ref ActorInfo actorInfo)
        {
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
        }

        public string GetImagePath(string dataPath = "", string ext = ".jpg", bool searchExt = true)
        {
            string result = string.Empty;
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType != PathType.RelativeToData) {
                if (pathType == PathType.RelativeToApp)
                    basePicPath = System.IO.Path.Combine(PathManager.CurrentUserFolder, basePicPath);
                string saveDir = System.IO.Path.Combine(basePicPath, "Actresses");
                if (!Directory.Exists(saveDir))
                    FileHelper.TryCreateDir(saveDir);

                // 优先使用 1_name.jpg 的方式
                result = System.IO.Path.Combine(saveDir, $"{ActorID}_{ActorName}{ext}");
                if (!File.Exists(result))
                    result = System.IO.Path.Combine(saveDir, $"{ActorName}{ext}");
            } else if (!string.IsNullOrEmpty(dataPath)) {
                string basePath = System.IO.Path.GetDirectoryName(dataPath);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, dict["ActorImagePath"]));
                string[] arr = FileHelper.TryGetAllFiles(path, "*.*");
                if (arr != null && arr.Length > 0) {
                    List<string> list = arr.ToList();
                    list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();

                    foreach (string item in list) {
                        if (System.IO.Path.GetFileNameWithoutExtension(item).ToLower().Equals(ActorName))
                            return item;
                    }
                }

            }

            // 替换成其他扩展名
            if (searchExt && !File.Exists(result))
                result = FileHelper.FindWithExt(result, ScanTask.PICTURE_EXTENSIONS_LIST);
            return result;
        }


        public override bool Equals(object obj)
        {
            ActorInfo actorInfo = obj as ActorInfo;
            if (actorInfo == null)
                return false;
            return this.ActorID == actorInfo.ActorID;
        }

        public override int GetHashCode()
        {
            return this.ActorID.GetHashCode();
        }


        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }
    }
}
