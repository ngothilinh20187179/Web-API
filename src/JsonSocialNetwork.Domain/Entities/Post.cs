using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("posts")]
    public class Post
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("body", TypeName = "nvarchar(max)")]
        public string Body { get; set; }

        [Column("date_created", TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [Column("date_modified", TypeName = "datetime")]
        public DateTime DateModified { get; set; }
        #endregion


        #region Foreign Keys
        [Column("author_account_id")]
        public int AuthorAccountId { get; set; }
        #endregion
    }
}
