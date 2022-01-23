using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonSocialNetwork.Domain.Entities
{
    [Table("conversations")]
    public class Conversation
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        #endregion


        #region Foreign Keys
        [Column("owner_account_id")]
        public int OwnerAccountId { get; set; }

        [Column("partner_account_id")]
        public int PartnerAccountId { get; set; }
        #endregion
    }
}
