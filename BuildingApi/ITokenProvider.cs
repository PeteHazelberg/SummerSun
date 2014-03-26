namespace BuildingApi
{
    public interface ITokenProvider
    {
        /// <summary>
        /// Get a JCI security token
        /// </summary>
        /// <param name="company"></param>
        /// <param name="refreshCache">if set to true, gets a new token from IMS even if a valid one is already in the cache</param>
        /// <returns></returns>
        Token Get(Company company = null, bool refreshCache = false);
    }
}