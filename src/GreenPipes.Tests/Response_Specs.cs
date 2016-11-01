// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Tests
{
    namespace ResponseAge
    {
        using System;
        using System.Linq;
        using System.Threading.Tasks;
        using Filters;
        using GreenPipes.Internals.Extensions;
        using NUnit.Framework;


        [TestFixture]
        public class Response_Specs
        {
            [Test]
            public async Task Should_obtain_the_result()
            {
                IPipe<RequestContext> pipe = Pipe.New<RequestContext>(cfg =>
                {
                    cfg.UseDispatch(new RequestConverterFactory(), d =>
                    {
                        d.Pipe<RequestContext<GetMemberEmail>>(p =>
                        {
                            p.UseInlineFilter(async (context, next) =>
                            {
                                context.TrySetResponse(new MemberEmail()
                                {
                                    MemberName = context.Request.MemberName,
                                    EmailAddress = "member@hostname.net"
                                });
                            });
                        });
                    });
                });

                var responseContext = await pipe.Request(new GetMemberEmail
                {
                    MemberName = "Chris"
                });

                var response = await responseContext.Match<MemberEmail>();

                Assert.That(response.EmailAddress, Is.EqualTo("member@hostname.net"));
            }
        }


        /// <summary>
        /// Request class
        /// </summary>
        public class GetMemberEmail
        {
            public string MemberName { get; set; }
        }


        /// <summary>
        /// response class
        /// </summary>
        public class MemberEmail
        {
            public string MemberName { get; set; }
            public string EmailAddress { get; set; }
        }


        /// <summary>
        /// The request extension
        /// </summary>
        public static class RequestExtensions
        {
            public static async Task<ResponseContext> Request<TRequest>(this IPipe<RequestContext> pipe, TRequest request)
            {
                var context = new DispatchRequestContext<TRequest>(request);

                await pipe.Send(context);

                return await context.ResponseContext;
            }
        }


        /// <summary>
        /// The base request interface used by the dispatch engine for requests
        /// </summary>
        public interface RequestContext :
            PipeContext
        {
            /// <summary>
            /// Returns the response context once available
            /// </summary>
            Task<ResponseContext> ResponseContext { get; }
        }



        public interface RequestContext<out TRequest> :
            RequestContext
        {
            /// <summary>
            /// The request object
            /// </summary>
            TRequest Request { get; }

            bool TrySetResponse<TResponse>(TResponse response);

            bool TrySetResponse<TResponse>(Task<TResponse> response);
        }


        /// <summary>
        /// It would be cool if these could be dynamically added to the request, and then
        /// pattern matches to the response instead of having to hard code them. So you could
        /// match different response types and await something that gets the first matched
        /// response type. Because honestly, then you could dispatch the ResponseContext
        /// into another pipe that dispatches by response type!
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        public interface RequestContext<out TRequest, TResponse> :
            RequestContext<TRequest>
        {
            /// <summary>
            /// The request's response, once available, will be available here
            /// </summary>
            Task<TResponse> Response { get; }

            bool TrySetResponse(TResponse value);
        }


        /// <summary>
        /// Forward thinking that we could dispatch on the response type
        /// to another pipe.
        /// </summary>
        public interface ResponseContext :
            PipeContext
        {
            Task<T> Match<T>();
        }


        /// <summary>
        /// The specialized response type
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        public interface ResponseContext<TResponse> :
            ResponseContext
        {
            Task<TResponse> Response { get; }
        }


        /// <summary>
        /// The context implementation for the request/response.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        public class DispatchRequestContext<TRequest> :
            BasePipeContext,
            RequestContext<TRequest>
        {
            readonly TaskCompletionSource<ResponseContext> _responseContext;

            public DispatchRequestContext(TRequest request)
            {
                Request = request;
                _responseContext = new TaskCompletionSource<ResponseContext>();
            }

            public TRequest Request { get; }

            public bool TrySetResponse<TResponse>(TResponse response)
            {
                return TrySetResponse(Task.FromResult(response));
            }

            public bool TrySetResponse<TResponse>(Task<TResponse> response)
            {
                var responseContext = new ResponseRequestContext<TRequest, TResponse>(this, response);

                return _responseContext.TrySetResult(responseContext);
            }

            public Task<ResponseContext> ResponseContext => _responseContext.Task;
        }


        /// <summary>
        /// The context implementation for the request/response.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        public class ResponseRequestContext<TRequest, TResponse> :
            BasePipeContext,
            ResponseContext<TResponse>
        {
            readonly RequestContext<TRequest> _requestContext;

            public ResponseRequestContext(RequestContext<TRequest> requestContext, Task<TResponse> response)
            {
                _requestContext = requestContext;
                Response = response;
            }

            public Task<TResponse> Response { get; }

            public Task<T> Match<T>()
            {
                var result = this as ResponseContext<T>;
                if (result != null)
                    return result.Response;

                throw new ArgumentException("The response type was not matched: " + TypeCache<T>.ShortName, nameof(T));
            }
        }


        public class RequestConverterFactory :
            IPipeContextConverterFactory<PipeContext>
        {
            IPipeContextConverter<PipeContext, TOutput> IPipeContextConverterFactory<PipeContext>.GetConverter<TOutput>()
            {
                if (typeof(TOutput).HasInterface<RequestContext>())
                {
                    var innerType = typeof(TOutput).GetClosingArguments(typeof(RequestContext<>)).Single();

                    return (IPipeContextConverter<PipeContext, TOutput>)Activator.CreateInstance(typeof(RequestContextConverter<>).MakeGenericType(innerType));
                }

                throw new ArgumentException($"The output type is not supported: {TypeCache<TOutput>.ShortName}", nameof(TOutput));
            }


            class RequestContextConverter<TRequest> :
                IPipeContextConverter<PipeContext, RequestContext<TRequest>>
                where TRequest : class
            {
                bool IPipeContextConverter<PipeContext, RequestContext<TRequest>>.TryConvert(PipeContext input,
                    out RequestContext<TRequest> output)
                {
                    var outputContext = input as RequestContext<TRequest>;

                    if (outputContext != null)
                    {
                        output = outputContext;
                        return true;
                    }

                    output = null;
                    return false;
                }
            }
        }
    }
}