#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class UserEntity :IdentityUser
    {
        [Display(Name = "Document")]
        [StringLength(20, MinimumLength = 2)]
        [MaxLength(20)]
        public string Document { get; set; }


        
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 2)]
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 2)]
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [Required]
        [MaxLength(100)]
        public string? Address { get; set; }

        [Display(Name = "Picture")]
        public string? PicturePath { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string FullNameWithDocument => $"{FirstName} {LastName} - {Document}";
        
        public bool IsEnable { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Creation Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm}", ApplyFormatInEditMode = false)]
       public DateTimeOffset CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Modification Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm}", ApplyFormatInEditMode = false)]
        public DateTimeOffset? ModifiedDate { get; set; }

    }
}
