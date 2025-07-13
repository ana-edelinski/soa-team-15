namespace StakeholdersService.UseCases
{
    public class ImageService : IImageService
    {
        public ImageService() { }

        public string SaveImage(string folderPath, byte[] imageData, string folderName)
        {
            var fileName = Guid.NewGuid() + ".png";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            System.IO.File.WriteAllBytes(filePath, imageData);
            return $"images/{folderName}/{fileName}";



        }

        public void DeleteOldImage(string oldImagePath)
        {
            if (System.IO.File.Exists(oldImagePath))
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

        }
    }

}
