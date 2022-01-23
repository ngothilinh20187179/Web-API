using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("accounts")]
    public class Account
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("phone", TypeName = "varchar(10)")]
        public string Phone { get; set; }

        [Required]
        [Column("password", TypeName = "varchar(10)")]
        public string Password { get; set; }

        [Required]
        [Column("name", TypeName = "nvarchar(31)")]
        public string Name { get; set; }

        [Column("date_created", TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [Column("description", TypeName = "nvarchar(127)")]
        public string Description { get; set; }

        [Column("address", TypeName = "nvarchar(127)")]
        public string Address { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }
        #endregion
    }
}
