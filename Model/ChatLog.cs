using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{
    public class ChatLog
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public string CreatedAt { get; set; }
        public bool IsMine { get; set; }
    }
}
