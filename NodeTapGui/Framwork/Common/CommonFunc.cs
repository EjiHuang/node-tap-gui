using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Common
{
    public class CommonFunc
    {
        #region methods

        /// <summary>
        ///     获取地址的IP
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetIp(string address)
        {
            try
            {
                // ipv4
                var ip = Dns.GetHostEntry(address)
                  .AddressList
                  .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
                  ?.ToString();
                if (string.IsNullOrWhiteSpace(address))
                {
                    // ipv6?
                    ip = Dns.GetHostEntry(address)
                      .AddressList
                      .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetworkV6)
                      ?.ToString();
                }

                return ip;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return string.Empty;
            }
        }

        /// <summary>
        ///     读取文本文件行信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<List<string>> ReadLineAsync(string filePath)
        {
            var list = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    list.Add(line);
                }
            }
            return list;
        }

        #endregion
    }
}
