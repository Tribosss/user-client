using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{

    // 게시글 클래스 정의
    public class Post
    {
        public required string Status { get; set; }  // 상태 (공지, 질문 등)
        public required string Title { get; set; }   // 제목
        public required string Author { get; set; }  // 작성자
        public required string Date { get; set; }    // 작성일
    }
}
