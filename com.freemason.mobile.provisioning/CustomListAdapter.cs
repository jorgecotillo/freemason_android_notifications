using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace com.freemason.mobile.provisioning
{
    public class CustomListAdapter : BaseAdapter<LatestAnnouncementsModel>
    {
        List<LatestAnnouncementsModel> _latestAnnouncementList =
            new List<LatestAnnouncementsModel>();
        Activity _context;


        public CustomListAdapter(
        Activity context,
        List<LatestAnnouncementsModel> list)
        {
            _context = context;
            _latestAnnouncementList = list;
        }

        public override LatestAnnouncementsModel this[int position] => 
            _latestAnnouncementList[position];


        public override int Count => _latestAnnouncementList.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(
            int position, 
            View convertView, 
            ViewGroup parent)
        {
			View view = convertView;

            // re-use an existing view, if one is available
            // otherwise create a new one
            if (view == null)
            {
                view = 
                    _context
                        .LayoutInflater
                        .Inflate(Resource.Layout.ListItemRow, parent, false);
            }

			LatestAnnouncementsModel item = this[position];
            view.FindViewById<TextView>(Resource.Id.Title).Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.Description).Text = 
                item.Message;
            
			return view;
		}
    }
}
