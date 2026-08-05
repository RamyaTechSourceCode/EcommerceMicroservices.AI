namespace ApiGateway.Service
{
    using Microsoft.Identity.Web;
    using System.Security.Claims;

    public class OBOTokenService
    {
        private readonly ITokenAcquisition _tokenAcquisition;

        public OBOTokenService(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task<string> GetTokenAsync(string scope)
        {

            return await _tokenAcquisition.GetAccessTokenForUserAsync(
            new[] { scope }
     );
        }
    }
}
