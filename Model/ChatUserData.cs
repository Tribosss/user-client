using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{
    public class ChatUserData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RecentChattingLog { get; set; }   
        public string SentAt { get; set; }
    }
}
