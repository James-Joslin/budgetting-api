namespace financesApi.models
{
    public class NewAccountRequest
    {
        public required string AccountName { get; set; }
        public required string AccountHolder { get; set; }
        public decimal StartingBalance { get; set; }
        public required string StartingDate { get; set; }
    }

    public class OfxTransactionDto
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public required string Payee { get; set; }
        public string? Memo { get; set; }
    }

    public class OfxUploadRequest
    {
        public string OfxContent { get; set; } = "";
        public int AccountId { get; set; }
        public bool SkipDuplicates { get; set; } = true;
        public string? SourceFileName { get; set; }
    }

    public class TransactionQueryRequest
    {
        public string? AccountName { get; set; }
    }
}

