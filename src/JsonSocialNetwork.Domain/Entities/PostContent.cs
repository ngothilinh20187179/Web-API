using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("post_contents")]
    public class PostContent
    {
        #region Properties
        [Column("order_id")]
        public int OrderId { get; set; }
        #endregion


        #region Foreign Keys
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("content_file_name")]
        public string ContentFileName { get; set; }
        #endregion
    }
}
