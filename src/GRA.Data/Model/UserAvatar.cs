using System;

namespace GRA.Data.Model
{
    public class UserAvatar
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int DynamicAvatarId { get; set; }
        public DynamicAvatar DynamicAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
