using System;
using System.Collections.Generic;

namespace com.freemason.mobile.provisioning
{
    public class LatestAnnouncementsModel
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public LatestAnnouncementsModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
