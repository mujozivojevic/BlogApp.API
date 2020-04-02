using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPost.API.Models.Output
{
    public class TagsOut
    {
        public List<string> Tags { get; } = new List<string>();
    }
}
