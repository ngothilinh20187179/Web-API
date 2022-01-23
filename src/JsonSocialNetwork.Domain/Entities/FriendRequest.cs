using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("friend_requests")]
    public class FriendRequest
    {
        #region Foreign Keys
        [Column("sender_account_id")]
        public int SenderAccountId { get; set; }

        [Column("receiver_account_id")]
        public int ReceiverAccountId { get; set; }
        #endregion
    }
}