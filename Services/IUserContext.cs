namespace HomeCare.Services
{
    public interface IUserContext
    {
        int? CurrentUserId { get; }
        string? CurrentUserName { get; }
        string? CurrentUserEmail { get; }
        bool IsAuthenticated { get; }
        int? ActiveHomeId { get; set; }
    }
}
