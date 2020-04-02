using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPost.API.Models
{
    public class PostTag
    {
     
        public string PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public int TagId { get; set; }
        [ForeignKey("TagId")]
        public Tag Tag { get; set; }


    }
}
