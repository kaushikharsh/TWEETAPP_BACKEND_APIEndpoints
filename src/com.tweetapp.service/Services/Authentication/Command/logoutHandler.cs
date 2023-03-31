namespace com.tweetapp.service
{
    using com.tweetapp.DAO;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class logoutHandler : IRequestHandler<logoutModel, ValidatableResponse<object>>
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public logoutHandler(IConfiguration configuration,ILogger logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        [Obsolete]
        public async Task<ValidatableResponse<object>> Handle(logoutModel request, CancellationToken cancellationToken)
        {
            ValidatableResponse<object> validatableResponse;
            
            try
            {
                MongoDbUserHelper mongoDbUserHelper = new MongoDbUserHelper(_configuration);


                var dbUser = mongoDbUserHelper.LoadDocumentById<User>("Users", request.UserId);

                dbUser.IsActive = false;
                dbUser.LastSeen = DateTime.UtcNow;

                mongoDbUserHelper.UpdateDocument("Users", dbUser.Email, dbUser);

                validatableResponse = new ValidatableResponse<object>("Logout successful ", null, null);
                validatableResponse.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception)
            {
                _logger.Error("Something Bad Happened Internal Server Error {CustomProperty}", 500);
                validatableResponse = new ValidatableResponse<object>("We are experiencing an internal server error. Contact your site administrator.", (int)HttpStatusCode.InternalServerError);
                validatableResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return await Task.FromResult(validatableResponse);

        }
    }
}
