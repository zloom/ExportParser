namespace ExportParser.Common
{
    public static class Utils 
    {
        public static TValue? TryGet<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) => source.TryGetValue(key, out var value) ? value : default;
    }
}