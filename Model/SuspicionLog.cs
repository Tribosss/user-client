namespace user_client.Model
{
    class SuspicionLog
    {
        public int Id {  get; set; }
        public required string Msg {  get; set; }
        public required int EmpId { get; set; }
        public required string SourceIp { get; set; }
        public required string DestIp { get; set; }
        public required int SourcePort { get; set; }
        public required int DestPort { get; set; }
        public required string keyword { get; set; }
        public required string DetectedAt { get; set; }
    }
}
