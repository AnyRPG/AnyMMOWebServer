namespace AnyMMOWebServer.Models
{
    public class PlayerCharacterListResponse
    {
        public List<PlayerCharacter> PlayerCharacters { get; set; }

        public PlayerCharacterListResponse()
        {
                PlayerCharacters = new List<PlayerCharacter>();
        }
    }
}
