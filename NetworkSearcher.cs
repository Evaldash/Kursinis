using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Kursinis {
    class NetworkSearcher {
        /// Base IP which to search. E.g. 192.168.1. to search from 192.168.1.0 to 192.168.1.255
        public string BaseIP { get; set; }
        public int TTL { get; set; } = 64;
        /// Timeout in miliseconds.
        public int TimeOut { get; set; } = 1000;

        private object @lock = new object();
        private int instances = 0;
        private List<Ping> pingList = new List<Ping>();
        private List<string> activeIPs = new List<string>();

        public NetworkSearcher() : this("192.168.1.") { }
        public NetworkSearcher(string BaseIP) {
            this.BaseIP = BaseIP;
        }

        public List<string> SearchForActiveIPs() {
            CreatePings(255);
            var po = new PingOptions(TTL, true);
            var encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes("abababababababababababababababab");
            SpinWait wait = new SpinWait();
            int counter = 1;

            foreach (var p in pingList) {
                lock (@lock) {
                    instances += 1;
                }
                p.SendAsync(string.Concat(BaseIP, counter.ToString()), TimeOut, data, po);
                counter++;
            }
            while (instances > 0) {
                wait.SpinOnce();
            }
            DestroyPings();
            return activeIPs;
        }
        private void CreatePings(int count) {
            for (int i = 1; i <= count; i++) {
                var p = new Ping();
                p.PingCompleted += OnPingCompleted;
                pingList.Add(p);
            }
        }
        private void OnPingCompleted(object sender, PingCompletedEventArgs args) {
            lock (@lock) {

                if (args.Reply.Status == IPStatus.Success) {
                    activeIPs.Add(args.Reply.Address.ToString());
                }
                instances -= 1;
            }
        }
        private void DestroyPings() {
            foreach (var ping in pingList) {
                ping.PingCompleted -= OnPingCompleted;
                ping.Dispose();
            }
            pingList.Clear();
        }
    }
}