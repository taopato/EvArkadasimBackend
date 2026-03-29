namespace Application.Features.Houses.Dtos
{
    public class UserHouseListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CreatorUserId { get; set; }
        public string CreatorFullName { get; set; } = string.Empty;
    }
}
