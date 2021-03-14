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

        public PostView()
        {
            CreationDate = DateTime.Now;
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
                UserId = this.UserId
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
        }
    }
}