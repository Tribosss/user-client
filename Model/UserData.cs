using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{
    public class UserData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public int? Age { get; set; }
        public string Salt { get; set; }
        public string CreatedAt { get; set; }
        public string Email { get; set; }   // 추가(OTP->메인화면 때 사용)
    }
}
