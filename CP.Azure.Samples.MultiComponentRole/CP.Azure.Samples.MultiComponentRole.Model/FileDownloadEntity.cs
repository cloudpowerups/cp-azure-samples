namespace CP.Azure.Samples.MultiComponentRole.Model
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    public class FileDownloadEntity : TableEntity
    {
        public FileDownloadEntity(string sourceUrl, Guid pingRequestId)
        {
            this.PartitionKey = new Uri(sourceUrl).DnsSafeHost;
            this.RowKey = pingRequestId.ToString();
            SourceUrl = sourceUrl;
            IsDownloaded = false;
            SavedLocation = null;
            IsImage = sourceUrl.EndsWith(".png")
                      || sourceUrl.EndsWith(".gif")
                      || sourceUrl.EndsWith(".jpg")
                      || sourceUrl.EndsWith(".jpeg")
                      || sourceUrl.EndsWith(".bmp");
        }

        public FileDownloadEntity() { }

        public string SourceUrl { get; set; }
        public bool IsDownloaded { get; set; }

        public string SavedLocation { get; set; }
        public bool IsImage { get; set; }

        public string ProcessedBy { get; set; }
    }
}
