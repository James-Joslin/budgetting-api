using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using financesApi.models;

namespace financesApi.FilterBuilder
{
    public static class ClientFilterBuilder
    {
        public static string BuildFilter(TransactionQueryRequest queryParameters)
        {
            StringBuilder filter = new StringBuilder();
            bool hasCondition = false;

            if (queryParameters.AccountName != null)
            {
                filter.Append("WHERE client_theseus_id = ").Append(queryParameters.AccountName).Append(' ');
                hasCondition = true;
            }

            
            return filter.ToString();
        }
    }
}