using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EFA_DEMO.Models
{
    public class PostView
    {
        public int Id { get; set; }
        [Display(Name="Titre"), Required(ErrorMessage = "Obligatoire")]
        public string Title { get; set; }
        [Display(Name = "Post"), Required(ErrorMessage = "Obligatoire")]
        public string Content { get; set; }
        [Display(Name = "Étiquettes")]
        public string Tags { get; set; }
        [Display(Name = "Date")]
        public System.DateTime CreationDate { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Usager")]
        public UserView User { get; set; }
        public int LikeCount { get; set; }
        public bool CurrentUserLike { get; set; }
        public bool EditMode { get; set; }

        public int ParentPostId { get; set; }
        public ICollection<PostsChild> PostsChilds { get; set; }

        public PostView()
        {
            CreationDate = DateTime.Now;
            CurrentUserLike = false;
            EditMode = false;
        }

        public bool IsOwner(UserView user)
        {
            return (user.Admin || UserId == user.Id);
        }

        public Post ToPost()
        {
            return new Post()
            {
                Id = this.Id,
                Title = this.Title,
                Content = this.Content,
                CreationDate = this.CreationDate,
                Tags = this.Tags,
                UserId = this.UserId,
                LikeCount = this.LikeCount,
                ParentPostId = this.ParentPostId
            };
        }

        public void CopyToPost(Post post)
        {
            post.Id = Id;
            post.Title = Title;
            post.Content = Content;
            post.CreationDate = CreationDate;
            post.Tags = Tags;
            post.UserId = UserId;
            post.LikeCount = LikeCount;
            post.ParentPostId = ParentPostId;
        }

        public void Copy(Post post)
        {
            Id = post.Id;
            Title = post.Title;
            Content = post.Content;
            CreationDate = post.CreationDate;
            Tags = post.Tags;
            UserId = post.UserId;
            User = (post.User != null ? post.User.ToUserView() : null);
            LikeCount = post.LikeCount;
            ParentPostId = post.ParentPostId;
            PostsChilds = post.PostsChilds;
        }
    }
}