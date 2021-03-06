using EFA_DEMO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EFA_DEMO.Controllers
{
    public class UsersController : Controller
    {
        private DBEntities DB = new DBEntities();

        private DateTime OnLineUsersLastUpdate
        {
            get
            {
                if (Session["OnLineUsersUpdate"] == null)
                    Session["OnLineUsersUpdate"] = new DateTime(0);
                return (DateTime)Session["OnLineUsersUpdate"];
            }
            set
            {
                Session["OnLineUsersUpdate"] = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            DB.Dispose();
            base.Dispose(disposing);
        }

        [UserAccess]
        public ActionResult Index()
        {
            OnLineUsersLastUpdate = new DateTime(0); // forcer le rafraichissement
            ViewBag.UserFullName = OnlineUsers.CurrentUser.FullName;
            return View();
        }

        [UserAccess]
        public ActionResult OnlineUsersList()
        {
            if (OnlineUsers.NeedUpdate(OnLineUsersLastUpdate))
            {
                OnLineUsersLastUpdate = DateTime.Now;
                return PartialView(OnlineUsers.Users);
            }
            return null;
        }

        public ActionResult Subscribe()
        {
            return View(new UserView());
        }
        [HttpPost]
        public ActionResult Subscribe(UserView userView)
        {
            if (ModelState.IsValid)
            {
                if (DB.UserNameExist(userView.UserName))
                {
                    ModelState.AddModelError("UserName", "Ce nom d'usager existe déjà.");
                    return View(userView);
                }
                userView.Admin = false;
                userView.Password = userView.NewPassword;
                DB.AddUser(userView);
                return RedirectToAction("Login");
            }
            return View(userView);
        }

        public ActionResult Login()
        {
          
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginView loginView)
        {
            if (ModelState.IsValid)
            {
                User userFound = DB.FindByUserName(loginView.UserName);
                if (userFound != null)
                {
                    if (userFound.Password != loginView.Password)
                    {
                        ModelState.AddModelError("Password", "Mot de passe incorrect");
                        return View(loginView);
                    }
                }
                else
                {
                    ModelState.AddModelError("UserName", "Ce non d'usager n'existe pas");
                    return View(loginView);
                }
                OnlineUsers.AddSessionUser(userFound.ToUserView());

                /*
                /// Code test pour contater l'effacement en casdade des logs
                userFound.ToUserView().RemoveAvatar();
                DB.Users.Remove(userFound);
                DB.SaveChanges();
                return null; */
            }
            return RedirectToAction("Index", "Posts");
        }

        public ActionResult Logout()
        {
            OnlineUsers.RemoveSessionUser();
            return RedirectToAction("Login");
        }

        [UserAccess]
        public ActionResult Profil()
        {
            UserView userView = OnlineUsers.CurrentUser;

            ViewBag.PasswordNotChangedToken = Guid.NewGuid().ToString().Substring(0,8);
            return View(userView);
        }
        [HttpPost]
        public ActionResult Profil(UserView userview)
        {
            if (ModelState.IsValid)
            {
                userview.Id = OnlineUsers.CurrentUser.Id;
                userview.Admin = OnlineUsers.CurrentUser.Admin;
                User user = DB.Users.Find(userview.Id);
                string PasswordNotChangedToken = (string)Request["PasswordNotChangedToken"];
                if (userview.NewPassword.Equals(PasswordNotChangedToken))
                    userview.Password = user.Password;
                else
                    userview.Password = userview.NewPassword;
                DB.UpdateUser(userview);
                userview.CopyToUserView(OnlineUsers.CurrentUser);
                OnlineUsers.LastUpdate = DateTime.Now;
            }
            return RedirectToAction("Index");
        }

        [UserAccess]
        public ActionResult UserLogs()
        {
            User user = DB.Users.Find(OnlineUsers.CurrentUser.Id);
            List<Log> logs = user.Logs.OrderByDescending(l => l.LoginDate).ToList();
            return View(logs);
        }

        [AdminAccess]
        public ActionResult AllUsersLogs()
        {
            OnLineUsersLastUpdate = new DateTime(0); // forcer le rafraichissement
            return View();
        }

        [AdminAccess]
        public ActionResult UsersLogs()
        {
            if (OnlineUsers.NeedUpdate(OnLineUsersLastUpdate))
            {
                OnLineUsersLastUpdate = DateTime.Now;
               
                List<Log> logs = DB.Logs.OrderByDescending(l => l.LoginDate).ToList();
                return PartialView(logs);
            }
            return null;
        }

        [AdminAccess]
        public ActionResult ClearAllLogs()
        {
            DB.ClearAllLogs();
            return RedirectToAction("AllUsersLogs");
        }

        public ActionResult About()
        {
            return View();
        }
    }
}