using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogPost.API.Data;
using BlogPost.API.Models.Output;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {

        private readonly DataContext _context;
        public TagsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tags = await _context.Tags.ToListAsync();

            TagsOut tagsOut = new TagsOut();
            foreach (var tag in tags)
            {
                tagsOut.Tags.Add(tag.Name);
            }
            return Ok(tagsOut);
        }

    }

}

