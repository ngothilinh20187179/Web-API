using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("blocks")]
    public class Block
    {
        #region Foreign Keys
        [Column("blocker_account_id")]
        public int BlockerAccountId { get; set; }

        [Column("blocked_account_id")]
        public int BlockedAccountId { get; set; }
        #endregion
    }
}