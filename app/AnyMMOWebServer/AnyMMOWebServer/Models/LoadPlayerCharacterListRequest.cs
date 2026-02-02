namespace AnyMMOWebServer.Models
{
    public class LoadPlayerCharacterListRequest {
        public int AccountId { get; set; }

        public LoadPlayerCharacterListRequest() { }

        public LoadPlayerCharacterListRequest(int accountId) {
            AccountId = accountId;
        }
    }
}
