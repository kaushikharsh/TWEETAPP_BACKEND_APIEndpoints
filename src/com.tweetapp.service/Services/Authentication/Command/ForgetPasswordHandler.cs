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

    public class ForgetPasswordHandler : IRequestHandler<ForgetPasswordModel, ValidatableResponse<object>>
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public ForgetPasswordHandler(IConfiguration configuration,ILogger logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        [Obsolete]
        public async Task<ValidatableResponse<object>> Handle(ForgetPasswordModel request, CancellationToken cancellationToken)
        {
            ValidatableResponse<object> validatableResponse;
            ForgetPasswordValidation validator = new();

            var result = validator.Validate(request);
            if (result.IsValid)
            {
                try
                {
                    MongoDbUserHelper mongoDbUserHelper = new MongoDbUserHelper(_configuration);


                    var dbUser = mongoDbUserHelper.LoadDocumentById<User>("Users", request.Email);

                    if(dbUser != null && dbUser.MobilePhone == request.MobilePhone && dbUser.DOB.Date == request.DOB.ToUniversalTime().Date)
                    {
                        dbUser.Password = "asdfghjkl" + request.NewPassword + "zxcvbnm";

                        mongoDbUserHelper.UpdateDocument("Users", dbUser.Email, dbUser);

                        validatableResponse = new ValidatableResponse<object>("Password reset successful ", null, null);
                        validatableResponse.StatusCode = (int)HttpStatusCode.OK;

                    }
                    else
                    {
                        _logger.Information("Ivalid User Information, Passowrd can't reset");
                        validatableResponse = new ValidatableResponse<object>("Invalid User information", (int)HttpStatusCode.BadRequest);
                        validatableResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Internal Server Error , {CustomProperty}", 500);
                    validatableResponse = new ValidatableResponse<object>("We are experiencing an internal server error. Contact your site administrator.", (int)HttpStatusCode.InternalServerError);
                    validatableResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                validatableResponse = new ValidatableResponse<object>((result.ToString().Replace("\n", "")).Replace("\r", ""), (int)HttpStatusCode.BadRequest);
                validatableResponse.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return await Task.FromResult(validatableResponse);

        }
    }
}
