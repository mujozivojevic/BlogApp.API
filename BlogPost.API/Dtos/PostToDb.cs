using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPost.API.Dtos
{
    public class PostToDb
    {
        public BlogPost blogPost { get; set; }
        public class BlogPost
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Body { get; set; }
            public List<string> Tags { get; set; }
        }
    }
}
