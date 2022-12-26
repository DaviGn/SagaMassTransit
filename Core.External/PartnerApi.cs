namespace Core.External
{
    public class PartnerApi
    {
        public async Task<Guid> SendRequest(int customerId, decimal amount)
        {
            await Task.Delay(1000);
            //throw new Exception("Error on Authorize");
            return await Task.FromResult(Guid.NewGuid());
        }
    }
}