namespace BookAdvisorWebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BookUserLike")]
    public partial class BookUserLike
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProfileId { get; set; }

        public int? Value { get; set; }

        public int? Scale { get; set; }

        public virtual Book Book { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
