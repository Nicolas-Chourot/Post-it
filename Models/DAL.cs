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
                User = (post.User != null ? post.User.ToUserView() : null),
                LikeCount = post.LikeCount,
                ParentPostId = post.ParentPostId,
                PostsChilds = post.PostsChilds
            };
        }

        public static List<Post> SearchPostsByTags(this DBEntities DB, string tags)
        {
            List<Post> posts = new List<Post>();
            string[] tagArray = tags.Split(' ');

            foreach (var post in DB.Posts.ToList())
            {
                if (post.Tags != null && post.ParentPostId == 0)
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

        public static List<PostView> ToPostViewList(this DBEntities DB, IEnumerable<Post> posts)
        {

            List<PostView> postViews = new List<PostView>();
            foreach (var post in posts)
            {
                PostView postview = post.ToPostView();
                postview.CurrentUserLike = DB.AlreadyLike(post.Id, OnlineUsers.CurrentUser.Id);
                postViews.Add(postview);
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
            if (post.ParentPostId != 0)
            {
                PostsChild pc = new PostsChild { PostId = post.ParentPostId, ChildPostId = post.Id };
                DB.PostsChilds.Add(pc);
                DB.SaveChanges();
            }
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
            if (postToUpdate.ParentPostId == 0)
            {
                postToUpdate.CreationDate = DateTime.Now;
                DB.Entry(postToUpdate).State = EntityState.Modified;
                DB.SaveChanges();
                PostsLastUpdate = DateTime.Now;
                return true;
            }
            return false;
        }


        public static void RemoveChilds(this DBEntities DB, int postId)
        {
            var childs = DB.PostsChilds.Where(pc => pc.PostId == postId);

            if (childs != null)
                foreach (PostsChild child in childs)
                {
                    DB.RemoveChilds(child.PostId);
                    Post post = DB.Posts.Find(child.PostId);
                    DB.PostsChilds.Remove(child);
                    if (post != null)
                        DB.Posts.Remove(post);
                }
        }
        public static bool RemovePost(this DBEntities DB, int Id)
        {
            Post postToDelete = DB.Posts.Find(Id);
            if (postToDelete != null)
            {
                var childs = DB.PostsChilds.Where(pc => pc.PostId == Id).ToArray();

                if (childs != null)
                {
                    for (int i = 0; i < childs.Count(); i++)
                    {
                        DB.RemovePost(childs[i].ChildPostId);
                    }
                }
                PostsChild childpostToDelete = DB.PostsChilds.Where(pc => pc.ChildPostId == Id).FirstOrDefault();
                if (childpostToDelete != null)
                    DB.PostsChilds.Remove(childpostToDelete);
                DB.Posts.Remove(postToDelete);
                DB.SaveChanges();
                PostsLastUpdate = DateTime.Now;
                return true;
            }
            return false;
        }
        public static List<string> PostParents(this DBEntities DB, int postId)
        {
            List<string> parents = new List<string>();
            Post post = DB.Posts.Find(postId);
            do
            {
                if (post != null)
                {
                    parents.Insert(0, post.Title + "_" + post.Id);
                    Post parent = DB.Posts.Find(post.ParentPostId);
                }
            }
            while (post != null);
            return parents;
        }

        public static bool AlreadyLike(this DBEntities DB, int postId, int userId)
        {
            Like like = DB.Likes.Where(l => (l.UserId == userId) && (l.PostId == postId)).FirstOrDefault();
            return (like != null);
        }
        public static void AddLikes(this DBEntities DB, int postId, int userId)
        {
            Post post = DB.Posts.Find(postId);
            if (post != null)
            {
                User user = DB.Users.Find(userId);
                if (user != null)
                {
                    Like like = DB.Likes.Where(l => (l.UserId == userId) && (l.PostId == postId)).FirstOrDefault();
                    if (like == null)
                    {
                        like = new Like { UserId = userId, PostId = postId };
                        DB.Likes.Add(like);
                        post.LikeCount++;
                        DB.Entry(post).State = EntityState.Modified;
                        DB.SaveChanges();
                        PostsLastUpdate = DateTime.Now;
                    }
                    else
                    {
                        DB.Likes.Remove(like);
                        post.LikeCount--;
                        if (post.LikeCount < 0) post.LikeCount = 0;
                        DB.Entry(post).State = EntityState.Modified;
                        DB.SaveChanges();
                        PostsLastUpdate = DateTime.Now;
                    }
                }
            }
        }
        public static void RemoveLikes(this DBEntities DB, int postId, int userId)
        {
            Post post = DB.Posts.Find(postId);
            if (post != null)
            {
                User user = DB.Users.Find(userId);
                if (user != null)
                {
                    Like like = DB.Likes.Where(l => (l.UserId == userId) && (l.PostId == postId)).FirstOrDefault();
                    if (like != null)
                    {
                        DB.Likes.Remove(like);
                        post.LikeCount--;
                        if (post.LikeCount < 0) post.LikeCount = 0;
                        DB.Entry(post).State = EntityState.Modified;
                        DB.SaveChanges();
                        PostsLastUpdate = DateTime.Now;
                    }
                }
            }
        }
        public static List<UserView> FindLikers(this DBEntities DB, int postId)
        {
            Post post = DB.Posts.Find(postId);
            if (post != null)
            {
                List<UserView> likers = new List<UserView>();
                var likes = DB.Likes.Where(l => l.PostId == postId);
                foreach (Like like in likes)
                {
                    likers.Add(like.User.ToUserView());
                }
                return likers.OrderBy(l => l.FullName).ToList();
            }
            return null;
        }

        public static List<string> GetAllTags(this DBEntities DB)
        {
            List<string> tags = new List<string>();
            foreach (Post post in DB.Posts)
            {
                if (post.Tags != null)
                {
                    string[] pTags = post.Tags.Split(' ');
                    if (pTags != null)
                    {
                        foreach (string pTag in pTags)
                        {
                            var pt = pTag.ToLower();
                            if (!tags.Contains(pt))
                                tags.Add(pt);
                        }
                    }
                }
            }
            return tags.OrderBy(s => s).ToList();
        }

    }
}