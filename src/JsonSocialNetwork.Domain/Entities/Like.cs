using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("likes")]
    public class Like
    {
        #region Foreign Keys
        [Column("author_account_id")]
        public int AuthorAccountId { get; set; }

        [Column("owner_post_id")]
        public int OwnerPostId { get; set; }
        #endregion
    }
}
