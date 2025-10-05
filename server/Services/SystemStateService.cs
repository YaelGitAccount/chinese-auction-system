namespace server_NET.Services
{
    public class SystemStateService : ISystemStateService
    {
        private static bool _lotteryOccurred = false;
        private readonly ILogger<SystemStateService> _logger;

        public SystemStateService(ILogger<SystemStateService> logger)
        {
            _logger = logger;
        }

        public bool IsLotteryOccurred()
        {
            _logger.LogDebug("Checking lottery state: {LotteryOccurred}", _lotteryOccurred);
            return _lotteryOccurred;
        }
        
        public void SetLotteryOccurred(bool occurred)
        {
            _logger.LogInformation("Setting lottery occurred state from {OldState} to {NewState}", _lotteryOccurred, occurred);
            _lotteryOccurred = occurred;
        }
    }
}
