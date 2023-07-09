﻿using SuperUtils.NetWork;
using SuperUtils.WPF.VieModel;

namespace Jvedio.Entity
{
    /// <summary>
    /// 服务器源
    /// </summary>
    public class Server : ViewModelBase
    {
        public Server(string name)
        {
            this.Name = name;
        }

        public Server()
        {
        }

        private bool isEnable = false;
        private string url = string.Empty;
        private string cookie = string.Empty;
        private int available = 0; // 指示测试是否通过
        private string name = string.Empty;
        private string lastRefreshDate = string.Empty;

        public bool IsEnable {
            get => isEnable;
            set {
                isEnable = value;
                RaisePropertyChanged();
            }
        }

        public string Url {
            get => url;
            set {
                url = value.ToString().ToProperUrl();
                RaisePropertyChanged();
            }
        }

        public string Cookie {
            get => cookie;
            set {
                cookie = value;
                RaisePropertyChanged();
            }
        }

        public int Available {
            get => available;
            set {
                available = value;
                RaisePropertyChanged();
            }
        }

        public string Name {
            get => name;
            set {
                name = value;
                RaisePropertyChanged();
            }
        }

        public string LastRefreshDate {
            get => lastRefreshDate;
            set {
                lastRefreshDate = value;
                RaisePropertyChanged();
            }
        }
    }
}
