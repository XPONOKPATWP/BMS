
namespace BMS.Dto
{
    
    public class BuildingDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public List<RoomDto> Rooms { get; set; }
        
    }
}
