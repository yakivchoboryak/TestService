namespace TestService.DTO
{
    public class AssetsResponse
    {
        public PagingInfo Paging { get; set; }
        public List<AssetApiResponse> Data { get; set; }

    }
    public class PagingInfo
    {
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Items { get; set; }
    }

    public class AssetApiResponse
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
    }
}
