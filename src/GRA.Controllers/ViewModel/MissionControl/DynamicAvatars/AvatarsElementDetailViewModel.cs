using GRA.Domain.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GRA.Controllers.ViewModel.MissionControl.DynamicAvatars
{
    public class AvatarsElementDetailViewModel
    {
        public int AvatarId { get; set; }
        public int LayerId { get; set; }
        public IFormFile UploadImage { get; set; }
        public string BaseAvatarUrl { get; set; }
    }
}
