using System.ComponentModel.DataAnnotations;

namespace GRA.Data.Model
{
    public class DynamicAvatarElement
    {
        [Required]
        public int AvatarId { get; set; }

        [Required]
        public int LayerId { get; set; }
    }
}
