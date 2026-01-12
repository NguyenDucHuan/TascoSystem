using Grpc.Core.Interceptors;
using Grpc.Core;

namespace Tasco.Orchestrator.Api.Midlewares
{
    public class GlobalGrpcExceptionInterceptor : Interceptor
    {
        private readonly ILogger<GlobalGrpcExceptionInterceptor> _logger;

        public GlobalGrpcExceptionInterceptor(ILogger<GlobalGrpcExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled gRPC exception in pipeline");
                throw;
            }
        }
    }
}
