namespace BuildingApi
{
    /// <summary>
    /// Represents a set of credentials for a particular company for use via the Password OAuth 2.0 grant type
    /// </summary>
    public class PasswordCredential
    {
        public PasswordCredential(string companyId, string accountId, string password)
        {
            this.AccountId = accountId;
            this.CompanyId = companyId;
            this.Password = password;
        }

        public string AccountId { get; set; }

        public string Password { get; set; }

        public string CompanyId { get; set; }
    }
}
