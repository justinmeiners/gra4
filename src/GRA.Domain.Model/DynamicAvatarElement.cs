using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class DynamicAvatarElement
    {
        [Required]
        public int DynamicAvatarId { get; set; }

        [Required]
        public int DynamicAvatarLayerId { get; set; }
    }
}
