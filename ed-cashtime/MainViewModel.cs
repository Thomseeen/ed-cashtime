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

namespace EdCashtime {
    public class MainViewModel : IDisposable, INotifyPropertyChanged {
        #region Constants
        private const string DEFAULT_LISTEN_URI = "tcp://eddn.edcd.io:9500";
        #endregion

        #region Private Fields
        private CancellationTokenSource cancellationTokenSource;

        private List<SearchCommodityModel> searchCommodities = new List<SearchCommodityModel>() {
            new SearchCommodityModel() {
                Name="musgarvite",
                DisplayName="Musgarvite",
                MinBuyPrice=500,
                MinBuyDemand=200
            },
            new SearchCommodityModel() {
                Name ="beer",
                DisplayName="Beer",
                MinBuyPrice=100,
                MinBuyDemand=50
            },
            new SearchCommodityModel() {
                Name ="beer",
                DisplayName="Beer",
                MaxSellPrice=100,
                MinStock=50
            }
        };
        #endregion

        #region Properties
        public ObservableCollection<string> RawLogStrings { get; private set; }
        public ObservableCollection<string> Alerts { get; private set; }

        public string CombinedLogStrings => string.Join("\r\n", RawLogStrings);
        public string CombinedAlertStrings => string.Join("\r\n", Alerts);

        public bool Listening { get; private set; }
        #endregion

        #region Constructor
        public MainViewModel() {
            RawLogStrings = new ObservableCollection<string>();
            RawLogStrings.CollectionChanged += RawLogStrings_CollectionChanged;
            Alerts = new ObservableCollection<string>();
            Alerts.CollectionChanged += Alerts_CollectionChanged;
        }
        #endregion

        #region Public Methods
        public async void StartListening() {
            cancellationTokenSource = new CancellationTokenSource();
            await ListenLoop(cancellationTokenSource.Token);
        }

        public void StopListening() {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
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

                    if (!(parseddata.ContainsKey("$schemaRef") && ((string)parseddata["$schemaRef"]) == "https://eddn.edcd.io/schemas/commodity/3"))
                        continue;

                    Commodities data = Commodities.FromJson(rawjson);

                    foreach (SearchCommodityModel scom in searchCommodities) {
                        Commodity com = data.Message.Commodities.SingleOrDefault(com => com.Name.ToLower(CultureInfo.InvariantCulture) == scom.Name);
                        if (com != null) {
                            if (com.BuyPrice >= scom.MinBuyPrice && com.Demand >= scom.MinBuyDemand)
                                Alerts.Add($"Buying {scom.DisplayName} @{data.Message.SystemName} - {data.Message.StationName}: {com.Demand}x {com.BuyPrice}");

                            if (com.SellPrice >= scom.MaxSellPrice && com.Stock >= scom.MinStock)
                                Alerts.Add($"Selling {scom.DisplayName} @{data.Message.SystemName} - {data.Message.StationName}: {com.Stock}x {com.SellPrice}");
                        }
                    }

                    RawLogStrings.Add(rawjson);
                }
            }, token);
        }
        #endregion

        #region Event Handler
        private void RawLogStrings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(CombinedLogStrings));
        }

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

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
