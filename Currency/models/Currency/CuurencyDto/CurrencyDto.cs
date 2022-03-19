namespace Currency.models.Currency.CuurencyDto
{
    public class CurrencyDto
    {
        public string time_last_update_utc { get; set; }
        public RelevantCurrenciesDto conversion_rates { get; set; }
    }

    public class RelevantCurrenciesDto
    {
        public string USD { get; set; }
        public string EUR { get; set; }
        public string GBP { get; set; }
        public string ILS { get; set; }
        public string JPY { get; set; }
    }
}
