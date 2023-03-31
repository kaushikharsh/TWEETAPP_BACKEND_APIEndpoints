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

    public class AddReplyTweetHandler : IRequestHandler<AddReplyTweetModel, ValidatableResponse<object>>
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public AddReplyTweetHandler(IConfiguration configuration,ILogger logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        [Obsolete]
        public async Task<ValidatableResponse<object>> Handle(AddReplyTweetModel request, CancellationToken cancellationToken)
        {
            ValidatableResponse<object> validatableResponse;
            AddReplyTweetValidation validator = new();

            var result = validator.Validate(request);
            if (result.IsValid)
            {
                try
                {
                    MongoDbTweetReplyHelper mongoDbTweetReplyHelper = new(_configuration);

                    TweetReply reply = new();
                    reply.TweetId = request.TweetId;
                    reply.EmailId = request.Email;
                    reply.ReplyMsg = request.Replymsg;
                    reply.CreatedDate = DateTime.UtcNow;

                    mongoDbTweetReplyHelper.InsertDocument<TweetReply>("TweetReplies", reply);

                    validatableResponse = new ValidatableResponse<object>("Tweet Successfully Replied", null, null);
                    validatableResponse.StatusCode = (int)HttpStatusCode.OK;
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
