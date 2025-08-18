using System;

namespace financesApi.models
{
    public class TransactionKey
    {
        public required DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Payee { get; set; }
        public string Memo { get; set; }
        public string FitId { get; set; }
        public string TransType { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TransactionKey other &&
                   Date == other.Date &&
                   Amount == other.Amount &&
                   Payee == other.Payee &&
                   Memo == other.Memo &&
                   FitId == other.FitId &&
                   TransType == other.TransType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Date, Amount, Payee, Memo, FitId, TransType);
        }
    }
}
