using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using NetMQ;
using NetMQ.Sockets;
using Ionic.Zlib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;
using EdCashtime.Models;

namespace EdCashtime {
    public class MainViewModel : IDisposable, INotifyPropertyChanged {
        #region Constants
        private const string DEFAULT_LISTEN_URI = "tcp://eddn.edcd.io:9500";
        #endregion

        #region Private Fields
        private CancellationTokenSource cancellationTokenSource;
        private bool listening;
        private long messagesCnt;

        private List<SearchCommodityModel> searchCommodities = new() {
            new SearchCommodityModel() {
                Name = "musgarvite",
                DisplayName = "Musgarvite",
                MinBuyPrice = 500,
                MinBuyDemand = 200
            },
            new SearchCommodityModel() {
                Name = "beer",
                DisplayName = "Beer",
                MinBuyPrice = 100,
                MinBuyDemand = 50
            },
            new SearchCommodityModel() {
                Name = "beer",
                DisplayName = "Beer",
                MaxSellPrice = 100,
                MinStock = 50
            }
        };
        #endregion

        #region Properties
        public ObservableCollection<string> Alerts { get; private set; }
        public string CombinedAlertStrings => string.Join("\r\n", Alerts);

        public long MessagesCnt {
            get { return messagesCnt; }
            private set { messagesCnt = value; OnPropertyChanged(); }
        }

        public bool Listening {
            get { return listening; }
            private set { listening = value; OnPropertyChanged(); }
        }
        #endregion

        #region Constructor
        public MainViewModel() {
            Alerts = new ObservableCollection<string>();
            Alerts.CollectionChanged += Alerts_CollectionChanged;

            Listening = false;
        }
        #endregion

        #region Public Methods
        public async void StartListening() {
            cancellationTokenSource = new CancellationTokenSource();
            Listening = true;

            await ListenLoop(cancellationTokenSource.Token);
        }

        public void StopListening() {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            Listening = false;
        }
        #endregion

        #region Private Methods
        public async Task ListenLoop(CancellationToken token) {
            await Task.Run(() => {
                UTF8Encoding utf8 = new();

                using SubscriberSocket client = new();
                client.Options.ReceiveHighWatermark = 1000;
                client.Connect(DEFAULT_LISTEN_URI);
                client.SubscribeToAnyTopic();

                while (true) {
                    byte[] bytes = client.ReceiveFrameBytes(out bool more);

                    if (token.IsCancellationRequested)
                        break;

                    byte[] uncompressed = ZlibStream.UncompressBuffer(bytes);
                    string rawjson = utf8.GetString(uncompressed);

                    using StringReader txtreader = new(rawjson);
                    using JsonTextReader jsonreader = new(txtreader);
                    JObject parseddata = JObject.Load(jsonreader);

                    MessagesCnt++;

                    if (!(parseddata.ContainsKey("$schemaRef") && ((string)parseddata["$schemaRef"]) == "https://eddn.edcd.io/schemas/commodity/3"))
                        continue;

                    Commodities data = Commodities.FromJson(rawjson);

                    foreach (SearchCommodityModel scom in searchCommodities) {
                        Commodity com = data.Message.Commodities.SingleOrDefault(com => com.Name.ToLower(CultureInfo.InvariantCulture) == scom.Name);
                        if (com != null) {
                            if (com.BuyPrice >= scom.MinBuyPrice && com.Demand >= scom.MinBuyDemand)
                                Alerts.Add($"Buying {scom.DisplayName} @{data.Message.SystemName} - {data.Message.StationName}: {com.Demand}x {com.BuyPrice}");

                            if (com.SellPrice <= scom.MaxSellPrice && com.Stock >= scom.MinStock)
                                Alerts.Add($"Selling {scom.DisplayName} @{data.Message.SystemName} - {data.Message.StationName}: {com.Stock}x {com.SellPrice}");
                        }
                    }
                }
            }, token);
        }
        #endregion

        #region Event Handler

        private void Alerts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(CombinedAlertStrings));
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            Listening = false;

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
