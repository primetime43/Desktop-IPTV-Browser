using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static X_IPTV.XtreamCodes;

namespace X_IPTV.Models
{
    public class XtreamChannelModel
    {
        private bool isInitialized = false;

        public XtreamChannelModel()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                // Get the category ID from the selected category name
                int selectedCategory = int.Parse(Instance.XtreamCategoryList
                    .FirstOrDefault(category => category.CategoryName == Instance.selectedCategory).CategoryId);

                // Find all channels that belong to the selected category by CategoryId
                var channelsInCategory = Instance.XtreamChannels
                    .Where(channel => channel.CategoryIds.Contains(selectedCategory))
                    .ToList();

                foreach (var channel in channelsInCategory)
                {
                    MyListBoxItems.Add(channel);
                }
            }
            isInitialized = true;
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }
    }
}
