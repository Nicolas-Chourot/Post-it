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
        private DateTime PostsLastUpdate
        {
            get
            {
                if (Session["PostsUpdate"] == null)
                    Session["PostsUpdate"] = new DateTime(0);
                return (DateTime)Session["PostsUpdate"];
            }
            set
            {
                Session["PostsUpdate"] = value;
            }
        }

        // GET: Posts
        public ActionResult Index()
        {
            PostsLastUpdate = new DateTime(0);
            if (Session["Tags"] == null)
                Session["Tags"] = "";

            return View();
        }

        public ActionResult Posts()
        {
            if (DAL.PostsNeedUpdate(PostsLastUpdate))
            {
                PostsLastUpdate = DateTime.Now;
                var posts = ((string)Session["Tags"] == "" ?
                db.Posts.Include(p => p.User).ToList() :
                db.SearchPostsByTags((string)Session["Tags"]));
                return PartialView(posts.OrderByDescending(p => p.CreationDate).ToPostViewList());
            }
            return null;
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            ViewBag.UserId = OnlineUsers.CurrentUser.Id;
            return View(new PostView());
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Content,Tags,CreationDate")] PostView postView)
        {
            if (ModelState.IsValid)
            {
                postView.UserId = OnlineUsers.CurrentUser.Id;
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
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "AvatarId", post.UserId);
            return View(post.ToPostView());
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
                db.UpdatePost(postview);
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "AvatarId", postview.UserId);
            return View(postview);
        }

        public ActionResult ConfirmDelete(int id)
        {
            Post postToDelete = db.Posts.Find(id);
            if (postToDelete != null)
                return View(postToDelete.ToPostView());
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ConfirmDelete(int id, string nothing)
        {
            db.RemovePost(id);
            return RedirectToAction("Index");
        }

        public ActionResult Tags()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Tags(string tags)
        {
            PostsLastUpdate = new DateTime(0);
            Session["Tags"] = tags;
            return RedirectToAction("Index");
        }

        public ActionResult SetTag(string tag)
        {
            PostsLastUpdate = new DateTime(0);
            Session["Tags"] = tag;
            return RedirectToAction("Index");
        }

        public ActionResult ClearTag()
        {
            PostsLastUpdate = new DateTime(0);
            Session["Tags"] = "";
            return RedirectToAction("Index");
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
