namespace BMS.Dto
{
    public class RoomDto
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public int BuildingId { get; set; }
        public List<HubDto>? Hubs { get; set; }
        
    }
}
