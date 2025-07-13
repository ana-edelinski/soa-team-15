namespace StakeholdersService.UseCases
{
    public interface IImageService
    {
        string SaveImage(string folderPath, byte[] imageData, string folderName);
        void DeleteOldImage(string oldPath);
    }
}
