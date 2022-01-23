using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("friends")]
    public class Friend
    {
        #region Foreign Keys
        [Column("smaller_account_id")]
        public int SmallerAccountId { get; set; }

        [Column("bigger_account_id")]
        public int BiggerAccountId { get; set; }
        #endregion
    }
}