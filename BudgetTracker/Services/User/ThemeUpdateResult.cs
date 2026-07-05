namespace BudgetTracker.Services.User
{
    public class ThemeUpdateResult
    {
        public bool SavedToDatabase { get; set; }
        public bool IsDarkTheme { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
