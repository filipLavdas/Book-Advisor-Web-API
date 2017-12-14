namespace BookAdvisorWebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Book")]
    public partial class Book
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Book()
        {
          
            IndustryIdentifier = new HashSet<IndustryIdentifier>();
            Author = new HashSet<Author>();
            Category = new HashSet<Category>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string F_Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string URL { get; set; }

        public string Decription { get; set; }

        public int? PublisherId { get; set; }

        public virtual Publisher Publisher { get; set; }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IndustryIdentifier> IndustryIdentifier { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Author> Author { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Category> Category { get; set; }
    }
}
