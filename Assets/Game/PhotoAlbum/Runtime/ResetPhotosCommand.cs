using MemoryAlbum.PhotoAlbum;
using UnityEngine;

namespace VNovelizer.Core.Commands
{
    public sealed class ResetPhotosCommand : VNCommand
    {
        public override string CommandName => "resetphotos";

        public override bool Execute(string args)
        {
            Debug.Log("[ResetPhotos] 命令执行，重置所有照片状态");
            PhotoAlbumManager.GetInstance().ResetAllPhotos();
            return true;
        }

        public override void Simulate(string args)
        {
            Debug.Log("[ResetPhotos] 预演中重置照片状态");
            PhotoAlbumManager.GetInstance().ResetAllPhotos();
        }
    }
}
