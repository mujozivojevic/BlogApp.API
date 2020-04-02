using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlogPost.API.Data;
using BlogPost.API.Dtos;
using BlogPost.API.Models;
using BlogPost.API.Models.Output;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BlogPost.API.Dtos.PostToDb;

namespace BlogPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DataContext _context;
        public PostsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<IActionResult> Get(string tag)
        {


            var allPosts = _context.Posts.AsQueryable(); // getting list of all posts with Queryable method, so I can make query later 
            List<Models.Post> lista = new List<Models.Post>();
            if (!string.IsNullOrEmpty(tag)) // check if tag is null or empty
            {
                var postoviTags = await _context.PostTags.Include(x => x.Tag)
                       .Where(w => w.Tag.Name.ToLower() == tag.ToLower()).ToListAsync(); // getting all record from table PostTags with 
                                                                                    // filtering with chosen tag
                foreach (var p in postoviTags) // adding Post in list with chosen tag
                {
                    lista.Add(allPosts.Where(w => w.Slug == p.PostId).FirstOrDefault());
                }

                lista = lista.OrderByDescending(x => x.CreatedAt).ToList(); // ordering by the recent date
            }

            if (lista.Count == 0) { lista = await _context.Posts.OrderByDescending(x=> x.CreatedAt).ToListAsync(); } // if tag is not chosen
                                                                                                          // then output all posts sorted by the date

            PostsOut posts = new PostsOut
            {
                BlogPosts = new List<PostsOut.Posts>()
            };
            foreach (var x in lista)
            {
                posts.BlogPosts.Add(new PostsOut.Posts
                {
                    Slug = x.Slug,
                    Title = x.Title,
                    Description = x.Description,
                    Body = x.Body,
                    CreatedAt = x.CreatedAt.ToString("yyyy-MM-ddThh:mm:ss.mmmZ"),
                    UpdatedAt = x.UpdatedAt != null ? x.UpdatedAt.Value.ToString("yyyy-MM-ddThh:mm:ss.mmmZ") : "n/a", // if UpdateAt is null then output "n/a"
                    Tags = _context.PostTags.Where(w => w.PostId == x.Slug).Select(x => x.Tag.Name).ToList()
                });
            }
            posts.PostCount = posts.BlogPosts.Count();
          
            return Ok(posts);

        }

        // GET: api/Post/some_post
        [HttpGet("{Slug}", Name = "GetPost")]
        public async Task<IActionResult> GetPost(string Slug)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(w => w.Slug == Slug);

            if (post == null)
                return NotFound();

            PostOut postOut = new PostOut
            {
                BlogPost = new PostOut.Post
                {
                    Slug = post.Slug,
                    Title = post.Title,
                    Description = post.Description,
                    Body = post.Body,
                    CreatedAt = post.CreatedAt.ToString("yyyy-MM-ddThh:mm:ss.mmmZ"),
                    UpdatedAt = post.UpdatedAt != null ? post.UpdatedAt.Value.ToString("yyyy-MM-ddThh:mm:ss.mmmZ") : "n/a",
                    Tags = await _context.PostTags.Where(w => w.PostId == post.Slug).Select(x => x.Tag.Name).ToListAsync()

                }
            };
            
            return Ok(postOut);
        }

        // POST: api/Post
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostToDb _postToDb)
        {

            // removing multiple whitespaces and then replacing with single whitespace,
            //then single whitespace with  lowercase ' _ '

            var slug = _postToDb.blogPost.Title;
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            slug = regex.Replace(slug, " ");
            slug = slug.Replace(" ", "_").ToLower();




            Models.Post post = new Models.Post // creating new post without tags
            {
                Slug = slug,
                Title = _postToDb.blogPost.Title,
                Body = _postToDb.blogPost.Body,
                Description = _postToDb.blogPost.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };
            await _context.Posts.AddAsync(post);  // post added



            var tagsForPost = _postToDb.blogPost.Tags; // inserted tags from client
            var tags = await _context.Tags.ToListAsync(); // all tags from database


            // see if tag did exist in database
            bool y = true;
            for (int i = 0; i < tagsForPost.Count; i++)
            {
                y = true;
                for (int j = 0; j < tags.Count; j++)
                {
                    if(tags[j].Name.ToLower() == tagsForPost[i].ToLower())  
                    {
                        y = false;
                    } 
                }

                if (y) // tag is not in table
                {
                    Tag t = new Tag { Name = tagsForPost[i] };   // adding tag in Tag table
                    await _context.Tags.AddAsync(t);
                }

            }
            // i did this check so we can eliminate duplicate tags in database

             await _context.SaveChangesAsync();
             var allTags = await _context.Tags.ToListAsync(); // all tags from database

            foreach (var tagsToPost in tagsForPost) // link post with tags
            {
                PostTag postTag = new PostTag // new record of table PostTag with post and related tag
                {
                    PostId = post.Slug,
                    TagId = await _context.Tags.Where(w=> 
                                        w.Name.ToLower() == tagsToPost.ToLower())
                                        .Select(x=> x.Id).FirstOrDefaultAsync()  // we added tag before in database,
                };                                                               // so we do not need to worry if it exist
                await _context.PostTags.AddAsync(postTag);
            }            
            await _context.SaveChangesAsync();

            return  Ok(post);
        }

        // PUT: api/Post/5
        [HttpPut("{slug}")]
        public async Task<IActionResult> Put(string slug, PostToDb update)
        {
            var post = await _context.Posts.Where(w => w.Slug == slug).FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

           
            if (!string.IsNullOrEmpty(update.blogPost.Title))
            {

                var updateSlug = update.blogPost.Title;
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                updateSlug = regex.Replace(updateSlug, " ");
                updateSlug = updateSlug.Replace(" ", "_").ToLower();

                //post.Slug = updateSlug; -- important rule of EF said that we can't change ID, so I will go in another way

                Post p = new Post // create new post with new slug and title
                {
                    Slug = updateSlug,
                    Body = post.Body,  //  update.blogPost.Body = maybe is empty so I will update body in next if statement
                    CreatedAt = post.CreatedAt,
                    Title = update.blogPost.Title,
                    Description = post.Description  
                };
                await _context.Posts.AddAsync(p);


                var postsTags = _context.PostTags.Include(x => x.Tag).Where(w => w.PostId == slug).ToList(); // all tags related to choosen post

                foreach (var postTags in postsTags) // After I created new post, adding tags with new post slug
                {
                    PostTag pT = new PostTag
                    {
                        PostId = p.Slug,
                        TagId = postTags.TagId
                    };

                    await _context.PostTags.AddAsync(pT);
                }


                //then delete post and related tags
                var deletePostTags = _context.PostTags.Where(w => w.PostId == slug).ToList(); // find all rows in table PostTags

                _context.PostTags.RemoveRange(deletePostTags); // delete all rows with wanted slug
                _context.Posts.Remove(post); // last but not least, removing post


                post = p; //  post is global variable so I can use it further 
                await _context.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(update.blogPost.Description))
            {
                post.Description = update.blogPost.Description;
            }
            if (!string.IsNullOrEmpty(update.blogPost.Body))
            {
                post.Body = update.blogPost.Body;
            }

            post.UpdatedAt = DateTime.Now;
            _context.Attach(post); //new post is already in database
            _context.Update(post);
            await _context.SaveChangesAsync(); // update Description and Body if its changed

            PostOut postOut = new PostOut
            {
                BlogPost = new PostOut.Post
                {
                    Slug = post.Slug,
                    Title = post.Title,
                    Description = post.Description,
                    Body = post.Body,
                    CreatedAt = post.CreatedAt.ToString("yyyy-MM-ddThh:mm:ss.mmmZ"),
                    UpdatedAt = post.UpdatedAt != null ? post.UpdatedAt.Value.ToString("yyyy-MM-ddThh:mm:ss.mmmZ") : "n/a",
                    Tags = await _context.PostTags.Where(w => w.PostId == post.Slug).Select(x => x.Tag.Name).ToListAsync()

                }
            };

            return Ok(postOut);

        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{slug}")]
        public async Task<IActionResult> Delete(string slug)
        {
            var deletePost = await _context.Posts.FirstOrDefaultAsync(w => w.Slug == slug); // find post

            if (deletePost == null)
                return NoContent();

            var deletePostTags = _context.PostTags.Where(w => w.PostId == slug).ToList(); // find all rows in table PostTags

            _context.PostTags.RemoveRange(deletePostTags); // delete all rows with wanted slug
            _context.Posts.Remove(deletePost); // last but not least, removing post
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
