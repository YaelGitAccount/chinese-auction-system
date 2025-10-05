namespace server_NET.Services
{
    public interface ISystemStateService
    {
        bool IsLotteryOccurred();
        void SetLotteryOccurred(bool occurred);
    }
}
