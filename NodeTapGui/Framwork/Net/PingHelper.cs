using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace NodeTapGui.Framwork.Net
{
    public class PingHelper
    {

        public delegate void DlgPingCompletedHandler(object sender, PingCompletedEventArgs p, params object[] parameters);
        public event DlgPingCompletedHandler PingCompleted = null;

        private object[] ps;

        public bool Ping(string ip, params object[] ps)
        {
            Ping p = new Ping();
            this.ps = ps;
            p.PingCompleted += new PingCompletedEventHandler(PingCompletedEx);
            if (!string.IsNullOrWhiteSpace(ip))
            {
                try
                {
                    p.SendAsync(ip, 5000, null);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private void PingCompletedEx(object sender, PingCompletedEventArgs e)
        {
            PingCompleted(this, e, ps);
            Ping ping = (Ping)sender;
            ping.Dispose();
        }

        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

    }
}
