using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("avatars")]
    public class Avatar
    {
        #region Foreign Keys
        [Key]
        [Column("account_id")]
        public int AccountId { get; set; }

        [Column("content_file_name")]
        public string ContentFileName { get; set; }
        #endregion
    }
}
