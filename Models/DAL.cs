using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EFA_DEMO.Models
{
    public static class DAL
    {
        public static DateTime PostsLastUpdate
        {
            get
            {
                if (HttpRuntime.Cache["PostsUpdate"] == null)
                    HttpRuntime.Cache["PostsUpdate"] = DateTime.Now;
                return (DateTime)HttpRuntime.Cache["PostsUpdate"];
            }
            set
            {
                HttpRuntime.Cache["PostsUpdate"] = value;
            }
        }
        public static bool PostsNeedUpdate(DateTime date)
        {
            return DateTime.Compare(date, PostsLastUpdate) < 0;
        }
        public static UserView ToUserView(this User user)
        {
            return new UserView()
            {
                Id = user.Id,
                AvatarId = user.AvatarId,
                UserName = user.UserName,
                FullName = user.FullName,
                Password = user.Password,
                Admin = user.Admin
            };
        }
        public static PostView ToPostView(this Post post)
        {
            return new PostView()
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Tags = post.Tags,
                UserId = post.UserId,
                CreationDate = post.CreationDate,
                User = (post.User != null ? post.User.ToUserView() : null)
            };
        }

        public static List<Post> SearchPostsByTags(this DBEntities DB, string tags)
        {
            List<Post> posts = new List<Post>();
            string[] tagArray = tags.Split(' ');

            foreach (var post in DB.Posts.ToList())
            {
                if (post.Tags != null)
                {
                    bool containsAllTags = true;
                    foreach (var tag in tagArray)
                    {
                        if (!post.Tags.ToLower().Contains(tag.ToLower()))
                        {
                            containsAllTags = false;
                            break;
                        }
                    }
                    if (containsAllTags)
                        posts.Add(post);
                }
            }

            return posts;
        }
        public static List<PostView> ToPostViewList(this IEnumerable<Post> posts)
        {
            List<PostView> postViews = new List<PostView>();
            foreach (var post in posts)
            {
                postViews.Add(post.ToPostView());
            }
            return postViews;
        }

        public static bool UserNameExist(this DBEntities DB, string userName)
        {
            User user = DB.Users.Where(u => u.UserName == userName).FirstOrDefault();
            return (user != null);
        }
        public static User FindByUserName(this DBEntities DB, string userName)
        {
            User user = DB.Users.Where(u => u.UserName == userName).FirstOrDefault();
            return user;
        }
        public static UserView AddUser(this DBEntities DB, UserView userView)
        {
            userView.SaveAvatar();
            User user = userView.ToUser();
            user = DB.Users.Add(user);
            DB.SaveChanges();
            return user.ToUserView();
        }
        public static bool UpdateUser(this DBEntities DB, UserView userView)
        {
            userView.SaveAvatar();
            User userToUpdate = DB.Users.Find(userView.Id);
            userView.CopyToUser(userToUpdate);
            DB.Entry(userToUpdate).State = EntityState.Modified;
            DB.SaveChanges();
            return true;
        }
        public static bool RemoveUser(this DBEntities DB, UserView userView)
        {
            userView.RemoveAvatar();
            User userToDelete = DB.Users.Find(userView.Id);
            DB.Users.Remove(userToDelete);
            DB.SaveChanges();
            return true;
        }

        public static Log AddUserLog(this DBEntities DB, UserView userView)
        {
            Log log = new Log();
            log.UserId = userView.Id;
            log.LoginDate = DateTime.Now;
            DB.Logs.Add(log);
            DB.SaveChanges();
            return log;
        }
        public static void ClearAllLogs(this DBEntities DB)
        {
            DB.Logs.RemoveRange(DB.Logs);
            DB.SaveChanges();
        }

        public static PostView AddPost(this DBEntities DB, PostView postView)
        {
            Post post = postView.ToPost();
            post = DB.Posts.Add(post);
            DB.SaveChanges();
            PostsLastUpdate = DateTime.Now;
            return post.ToPostView();
        }
        public static bool UpdatePost(this DBEntities DB, PostView postView)
        {
            Post postToUpdate = DB.Posts.Find(postView.Id);
            postView.CopyToPost(postToUpdate);
            DB.Entry(postToUpdate).State = EntityState.Modified;
            DB.SaveChanges();
            PostsLastUpdate = DateTime.Now;
            return true;
        }
        public static bool RePost(this DBEntities DB, int id)
        {
            Post postToUpdate = DB.Posts.Find(id);
            postToUpdate.CreationDate = DateTime.Now;
            DB.Entry(postToUpdate).State = EntityState.Modified;
            DB.SaveChanges();
            PostsLastUpdate = DateTime.Now;
            return true;
        }
        public static bool RemovePost(this DBEntities DB, int Id)
        {
            Post postToDelete = DB.Posts.Find(Id);
            DB.Posts.Remove(postToDelete);
            DB.SaveChanges();
            PostsLastUpdate = DateTime.Now;
            return true;
        }
    }
}