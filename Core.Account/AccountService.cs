namespace Core.Accounts
{
    public class AccountService
    {
        public async Task<bool> DebitAsync(int customerId, decimal amount)
        {
            await Task.Delay(1000);
            return await Task.FromResult(true);
        }

        public async Task<bool> Rollback(int customerId)
        {
            await Task.Delay(1000);
            return await Task.FromResult(true);
        }
    }
}