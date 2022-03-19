using System;

namespace Currency.models
{
    public class CurrenciesRates
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
