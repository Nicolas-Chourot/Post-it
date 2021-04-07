using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EFA_DEMO.Models;

namespace EFA_DEMO.Controllers
{
    [UserAccess]
    public class PostsController : Controller
    {
        private DBEntities db = new DBEntities();
        private DateTime RefreshPostsLastRequest
        {
            get
            {
                if (Session["RefreshPostsLastRequest"] == null)
                    Session["RefreshPostsLastRequest"] = new DateTime(0);
                return (DateTime)Session["RefreshPostsLastRequest"];
            }
            set
            {
                Session["RefreshPostsLastRequest"] = value;
            }
        }

        // GET: Posts
        public ActionResult Index()
        {
            if (Session["HoldPartialRefresh"] == null) Session["HoldPartialRefresh"] = false;
            if (Session["EditingPostId"] == null) Session["EditingPostId"] = 0;
            if (Session["PostsSourceUser"] == null) Session["PostsSourceUser"] = null;
            if (Session["ShowSearchTags"] == null) Session["ShowSearchTags"] = false;
            if (Session["PostsPageSize"] == null) Session["PostsPageSize"] = 5;
            if (Session["PostPageCount"] == null) Session["PostPageCount"] = 1;
            RefreshPostsLastRequest = new DateTime(0);
            if (Session["Tags"] == null)
                Session["Tags"] = "";

            return View();
        }

        public ActionResult ForceRefresh()
        {
            RefreshPostsLastRequest = new DateTime(0);
            return null;
        }

        public ActionResult Posts()
        {
            if (DAL.PostsNeedUpdate(RefreshPostsLastRequest) && (!(bool)Session["HoldPartialRefresh"]))
            {
                Session["HoldPartialRefresh"] = (int)Session["EditingPostId"] != 0;
                RefreshPostsLastRequest = DateTime.Now;
                var posts = ((string)Session["Tags"] == "" ?
                            db.Posts.Where(p => p.ParentPostId == 0).ToList() :
                            db.SearchPostsByTags((string)Session["Tags"]));

                if (Session["PostsSourceUser"] != null)
                {
                    int sourceId = ((UserView)Session["PostsSourceUser"]).Id;
                    return PartialView(db.ToPostViewList(posts.Where(p => p.UserId == sourceId).OrderByDescending(p => p.CreationDate)));
                }
                else
                {
                    Session["PostsSourceName"] = "Tous les posts";
                    return PartialView(db.ToPostViewList(posts.OrderByDescending(p => p.CreationDate)));
                }
            }
            return null;
        }

        public ActionResult NextPage()
        {
            RefreshPostsLastRequest = new DateTime(0);
            Session["PostPageCount"] = (int)Session["PostPageCount"] + 1;
            return new JsonResult { Data = "Next page", JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        // GET: Posts/Create
        public ActionResult Create(int? parentId)
        {
            ViewBag.UserId = OnlineUsers.CurrentUser.Id;
            Session["ParentPost"] = null;
            if (parentId != null)
            {
                Session["ParentPost"] = db.Posts.Find(parentId);
            }
            return View(new PostView());
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Content,Tags")] PostView postView)
        {
            if (ModelState.IsValid)
            {
                if (Session["ParentPost"] != null)
                    postView.ParentPostId = ((Post)Session["ParentPost"]).Id;
                else
                    postView.ParentPostId = 0;
                postView.UserId = OnlineUsers.CurrentUser.Id;
                postView.CreationDate = DateTime.Now;
                db.AddPost(postView);
                return RedirectToAction("Index");
            }
            return View(postView);
        }

        // GET: Posts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            Session["editPostId"] = post.Id;
            Session["editParentPostId"] = post.ParentPostId;
            Session["editPostUserId"] = post.UserId;
            Session["editPostCreationDate"] = post.CreationDate;
            if (post == null)
            {
                return HttpNotFound();
            }
            PostView postview = post.ToPostView();
            if (postview.IsOwner(OnlineUsers.CurrentUser))
                return View(post.ToPostView());
            return RedirectToAction("Logout", "Users");
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Content,Tags,CreationDate,UserId")] PostView postview)
        {
            if (ModelState.IsValid)
            {
                postview.Id = (int)Session["editPostId"];
                postview.UserId = (int)Session["editPostUserId"];
                postview.CreationDate = (DateTime)Session["editPostCreationDate"];
                postview.ParentPostId = (int)Session["editParentPostId"];
                db.UpdatePost(postview);
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "AvatarId", postview.UserId);
            return View(postview);
        }

        public ActionResult Repost(int id)
        {
            Post post = db.Posts.Find(id);
            if (post != null)
            {
                if (post.UserId == OnlineUsers.CurrentUser.Id || OnlineUsers.CurrentUser.Admin)
                    db.RePost(id);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            db.RemovePost(id);
            return new JsonResult { Data = "Deleted", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Tags()
        {
            Session["ShowSearchTags"] = true;
            return Redirect("Index");
        }
        [HttpPost]
        public ActionResult Tags(string Tags)
        {
            Session["ShowSearchTags"] = true;
            RefreshPostsLastRequest = new DateTime(0);
            Session["Tags"] = Tags.Trim();
            return RedirectToAction("Index");
        }

        public ActionResult SetTag(string tag)
        {
            Session["ShowSearchTags"] = true;
            RefreshPostsLastRequest = new DateTime(0);
            Session["Tags"] = tag;
            return RedirectToAction("Index");
        }
        public ActionResult SetTags(string tags)
        {
            Session["ShowSearchTags"] = true;
            RefreshPostsLastRequest = new DateTime(0);
            Session["Tags"] = tags;
            return new JsonResult { Data = "SetTags done: " + tags, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult CreatePost(int id /*parent id*/)
        {
            Post parentPost = db.Posts.Find(id);

            Post post = new Post();

            post.Content = "";
            post.Tags = "";
            post.CreationDate = DateTime.Now;
            post.UserId = OnlineUsers.CurrentUser.Id;
            if (parentPost != null)
            {
                post.ParentPostId = id;
                post.Title = "Réponse à " + parentPost.User.FullName;
            }
            else
            {
                post.ParentPostId = 0;
                post.Title = "";
            }
            PostView createdPost = db.AddPost(post.ToPostView());
            Session["EditingPostId"] = createdPost.Id;
            RefreshPostsLastRequest = new DateTime(0);
            return new JsonResult { Data = "CreatePost to post Id: " + id, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult SetEditMode(int id)
        {
            Session["EditingPostId"] = id;
            RefreshPostsLastRequest = new DateTime(0);
            return new JsonResult { Data = "SetEditMode to post Id: " + id, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult CancelEditMode()
        {
            Session["EditingPostId"] = 0;
            Session["HoldPartialRefresh"] = false;
            RefreshPostsLastRequest = new DateTime(0);
            return new JsonResult { Data = "Cancel Edit mode done", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public ActionResult SetPost(string title, string content, string tags, int postId)
        {
            Post post = db.Posts.Find(postId);
            if (post != null)
            {
                post.Title = title;
                post.Content = content;
                post.Tags = tags.Trim();
                db.UpdatePost(post.ToPostView());
                RefreshPostsLastRequest = new DateTime(0);
                Session["EditingPostId"] = 0;
                Session["HoldPartialRefresh"] = false;
            }
            return null;
        }
        [HttpPost]
        public ActionResult SetReplyContent(string content, int postId)
        {
            Post post = db.Posts.Find(postId);
            if (post != null)
            {
                post.Content = content;
                db.UpdatePost(post.ToPostView());
                RefreshPostsLastRequest = new DateTime(0);
                Session["EditingPostId"] = 0;
                Session["HoldPartialRefresh"] = false;
            }
            return null;
        }
        public ActionResult SetRootUserPosts(int id/*target userId*/)
        {
            User user = db.Users.Find(id);
            if (user != null)
            {
                Session["PostsSourceUser"] = user.ToUserView();
            }
            else
            {
                Session["PostsSourceUser"] = null;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ClearTag()
        {
            Session["ShowSearchTags"] = false;
            RefreshPostsLastRequest = new DateTime(0);
            Session["Tags"] = "";
            return RedirectToAction("Index");
        }

        public ActionResult GetAllTags()
        {
            return new JsonResult { Data = db.GetAllTags(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult AddLike(int id)
        {
            RefreshPostsLastRequest = new DateTime(0);
            db.AddLikes(id, OnlineUsers.CurrentUser.Id);
            return new JsonResult { Data = "Added", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult RemoveLike(int id)
        {
            RefreshPostsLastRequest = new DateTime(0);
            db.RemoveLikes(id, OnlineUsers.CurrentUser.Id);
            return new JsonResult { Data = "Removed", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetLikersList(int id /*post id*/)
        {
            List<UserView> likers = db.FindLikers(id);
            return PartialView(likers);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
