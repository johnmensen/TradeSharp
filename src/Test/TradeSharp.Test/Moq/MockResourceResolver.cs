using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    class MockResourceResolver : IResourceResolver
    {
        public string TryGetResourceValue(string resxKey)
        {
            return "the" + resxKey;
        }
    }
}
