using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class Team : Abstract.BaseDomainEntity
    {
        [Required]
        public int SiteId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
}
