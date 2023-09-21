namespace DataToolKit.Models
{
    public class BatchDataFile
    {
        public int BatchDataFileId { get; set; }
        public int BatchId { get; set; }
        public int NPI { get; set; }
        public string? Segment { get; set; }

    }

}
