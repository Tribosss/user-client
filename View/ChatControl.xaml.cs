using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using user_client.Model;

namespace user_client.View
{
    public partial class ChatControl : System.Windows.Controls.UserControl
    {
        public ChatControl()
        {
            ChatClient cc = new ChatClient();
            cc.Init();
        }
    }
}
