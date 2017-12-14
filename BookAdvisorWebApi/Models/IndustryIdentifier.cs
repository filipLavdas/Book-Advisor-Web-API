namespace BookAdvisorWebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("IndustryIdentifier")]
    public partial class IndustryIdentifier
    {
        [Key]
        public string Identifier { get; set; }

        [Required]
        public string Type { get; set; }

        public int? BookId { get; set; }

        public virtual Book Book { get; set; }
    }
}
