﻿namespace user_client.Model
{
    public class Post
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
