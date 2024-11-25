using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_IPTV_Browser.Models
{
    public class M3UChannelModel
    {
        private bool isInitialized = false;

        public M3UChannelModel()
        {
            //MyListBoxItems = new ObservableCollection<M3UChannel>();
        }


        public void Initialize()
        {
            /*if (isInitialized) return; // Ensure logic runs only once

            List<M3UChannel> channels;
            if (Instance.M3uChecked)
            {
                // Load M3U channels
                if (Instance.allM3uEpgData != null)
                {
                    channels = Instance.M3UChannels.Where(c => c.CategoryName == Instance.selectedCategory).ToList();
                }
                else
                {
                    channels = Instance.M3UChannels.ToList();
                }
                foreach (var channel in channels)
                {
                    MyListBoxItems.Add(channel);
                }
            }
            isInitialized = true;*/
        }

        //public ObservableCollection<M3UChannel> MyListBoxItems { get; set; }
    }
}
