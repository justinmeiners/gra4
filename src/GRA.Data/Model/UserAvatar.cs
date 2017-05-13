using System;

namespace GRA.Data.Model
{
    public class UserAvatar
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int AvatarId { get; set; }
        public DynamicAvatar Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
