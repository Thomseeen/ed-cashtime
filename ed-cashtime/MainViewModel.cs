using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;
using Ionic.Zlib;
using System.Threading;

namespace ed_cashtime {
    public class MainViewModel : IDisposable, INotifyPropertyChanged {
        #region Constants
        private const string DEFAULT_LISTEN_URI = "tcp://eddn.edcd.io:9500";
        #endregion

        #region Private Fields
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Properties
        public ObservableCollection<string> RawLogStrings { get; private set; }
        public string CombinedLogStrings { get => string.Join("\r\n", RawLogStrings); }
        #endregion

        #region Constructor
        public MainViewModel() {
            _cancellationTokenSource = new CancellationTokenSource();

            RawLogStrings = new ObservableCollection<string>();
            RawLogStrings.CollectionChanged += RawLogStrings_CollectionChanged;
        }
        #endregion

        #region Public Methods
        public async Task StartListeningAsync() {
            await Task.Run(() => {
                ListenLoop(_cancellationTokenSource.Token);
            });
        }

        public void StopListening() {
            _cancellationTokenSource.Cancel();
        }
        #endregion

        #region Private Methods
        public void ListenLoop(CancellationToken token) {
            var utf8 = new UTF8Encoding();

            using var client = new SubscriberSocket();
            client.Options.ReceiveHighWatermark = 1000;
            client.Connect(DEFAULT_LISTEN_URI);
            client.SubscribeToAnyTopic();

            while (true) {
                byte[] bytes = client.ReceiveFrameBytes();

                if (token.IsCancellationRequested)
                    break;

                var uncompressed = ZlibStream.UncompressBuffer(bytes);

                RawLogStrings.Add(utf8.GetString(uncompressed));
            }
        }
        #endregion

        #region Event Handler
        private void RawLogStrings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(CombinedLogStrings));
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
            _cancellationTokenSource.Dispose();
        }
        #endregion
    }
}
