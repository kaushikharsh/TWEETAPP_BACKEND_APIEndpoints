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

    public class UpdateTweetHandler : IRequestHandler<UpdateTweetModel, ValidatableResponse<object>>
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public UpdateTweetHandler(IConfiguration configuration,ILogger logger )
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        [Obsolete]
        public async Task<ValidatableResponse<object>> Handle(UpdateTweetModel request, CancellationToken cancellationToken)
        {
            ValidatableResponse<object> validatableResponse;
            UpdateTweetValidation validator = new();

            var result = validator.Validate(request);
            if (result.IsValid)
            {
                try
                {
                    MongoDbTweetHelper mongoDbTweetHelper = new(_configuration);

                    var dbtweet = mongoDbTweetHelper.LoadDocumentById<Tweet>("Tweets", request.TweetId);

                    if(dbtweet.CreatedById == request.UserId)
                    {
                        dbtweet.Message = request.Message;
                        dbtweet.Tag = request.Tag;
                        mongoDbTweetHelper.UpdateDocument<Tweet>("Tweets", new Guid(request.TweetId), dbtweet);
                        validatableResponse = new ValidatableResponse<object>("Tweet Sucessfully Updated", null, null);
                        validatableResponse.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        validatableResponse = new ValidatableResponse<object>("UnAuthorised to Updated", (int)HttpStatusCode.Unauthorized);
                        validatableResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Internal Server Error Something Bad Happened {CustomProperty}", 500);
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
