using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPost.API.Models.Output
{
    public class PostOut
    {
        public Post BlogPost { get; set; }
        public class Post
        {
            public string Slug { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Body { get; set; }
            public List<string> Tags { get; set; }

            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
        }
    }
}
