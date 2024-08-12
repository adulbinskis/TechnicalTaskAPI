
namespace TechnicalTaskAPI.Tests.Constants
{
    public static class TestUserConstants
    {
        public const string Default_Password = "P@ssw0rd";
        public const string Default_Username = "testuserdefault";
        public const string Default_Email = "testuserdefault@example.com";

        public const string New_User_Username = "newuser";
        public const string New_User_Email = "newuser@example.com";

        public const string Refresh_User_Username = "refreshtestuser";
        public const string Refresh_User_Email = "refreshtestuser@example.com";

        public const string Logout_User_Username = "logouttestuser";
        public const string Logout_User_Email = "logouttestuser@example.com";

        public const string InvalidEmailFormat = "invalid-email";
        public const string IncorrectPassword = "wrongpassword";
        public const string NonExistingUser = "nonexisting@example.com";
        public const string EmptyString = "";
        public const string ShortPassword = "Sh0r!";
        public static readonly string LongPassword = new string('a', 255);

        public const string ValidRefreshToken = "valid_refresh_token";
        public const string NonExistentRefreshToken = "non_existent_refresh_token";
        public const string InvalidRefreshToken = "invalid_refresh_token";
        public const string ExpiredRefreshToken = "expired_refresh_token";

        public const string LogoutValidRefreshToken = "logout_valid_refresh_token";
    }
}
