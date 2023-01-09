namespace ExportParser.Common
{
    public record Entry(DateTime? date, decimal? sum, string addr, string mcc, Bank bank);
}