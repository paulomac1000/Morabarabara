using Morabara.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Morabara.Logic
{
    public static class TopLogic
    {
        public static List<TopPlayer> ReadTop()
        {
            List<TopPlayer> tops = new List<TopPlayer>();

            string json = string.Empty;
            try
            {
                using (var sr = new StreamReader("Data/Data/top.dat"))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch
            {
                try
                {
                    using (var sw = new StreamWriter("Data/Data/top.dat")) sw.Write(string.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable create file Data/Data/top.dat: {ex.Message}.");
                }

                return tops;
            }

            try
            {
                tops = JsonConvert.DeserializeObject<List<TopPlayer>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable deserialize Data/Data/top.dat: {ex.Message}.");
            }
            return tops;
        }

        public static void AddToTop(TopPlayer topPlayer)
        {
            var currentTop = ReadTop();
            currentTop.Add(topPlayer);

            currentTop.OrderBy(t => t.Points);

            while (currentTop.Count > 10)
            {
                currentTop.RemoveAt(currentTop.Count - 1);
            }

            string json = string.Empty;
            try
            {
                json = JsonConvert.SerializeObject(currentTop, Formatting.Indented);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable serialize top10 entities: {ex.Message}.");
                return;
            }

            try
            {
                using (var sw = new StreamWriter("Data/Data/top.dat"))
                {
                    sw.Write(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable save top entities to file Data/Data/top.dat: {ex.Message}.");
            }
        }
    }
}