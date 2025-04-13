namespace Model.VO.Rbac
{
    public class TokenResult
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
