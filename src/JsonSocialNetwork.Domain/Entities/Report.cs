using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("reports")]
    public class Report
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("subject", TypeName = "tinyint")]
        public int Subject { get; set; }

        [Column("detail", TypeName = "nvarchar(max)")]
        public string Detail { get; set; }
        #endregion


        #region Foreign Keys
        [Column("post_id")]
        public int PostId { get; set; }
        #endregion
    }
}
