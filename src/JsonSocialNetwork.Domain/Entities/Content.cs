using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("contents")]
    public class Content
    {
        #region Properties
        [Key]
        [Column("file_name", TypeName = "varchar(31)")]
        public string FileName { get; set; }

        [Column("content_type", TypeName = "varchar(31)")]
        public string ContentType { get; set; }
        #endregion
    }
}
