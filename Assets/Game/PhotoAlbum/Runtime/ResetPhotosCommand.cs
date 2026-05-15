using MemoryAlbum.PhotoAlbum;

namespace VNovelizer.Core.Commands
{
    public sealed class ResetPhotosCommand : VNCommand
    {
        public override string CommandName => "resetphotos";

        public override bool Execute(string args)
        {
            PhotoAlbumManager.GetInstance().ResetAllPhotos();
            return true;
        }
    }
}
