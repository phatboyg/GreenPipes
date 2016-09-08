using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using GreenPipes.Payloads;
using GreenPipes.Validation;
using NUnit.Framework;

namespace GreenPipes.Tests
{
    public class Authentication_Specs
    {
        [Test]
        public async Task Authorized()
        {
            bool visited = false;
            var pipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseAuthFilter("bob");
                cfg.UseExecute(cxt =>
                {
                    visited = true;
                });
            });


            var request = new RequestContext();
            request.GetOrAddPayload(() => new GenericPrincipal(new GenericIdentity("test"), new []{"bob"} ));

            await pipe.Send(request).ConfigureAwait(false);

            Assert.That(visited, Is.True);


            Console.WriteLine(pipe.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task Unauthorized()
        {
            bool visited = false;
            var pipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseAuthFilter("bob");
                cfg.UseExecute(cxt =>
                {
                    visited = true;
                });
            });

            
            var request = new RequestContext();
            request.GetOrAddPayload(() => System.Threading.Thread.CurrentPrincipal);

            await pipe.Send(request).ConfigureAwait(false);

            Assert.That(visited, Is.False);


            Console.WriteLine(pipe.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task InvalidSetup()
        {
            bool visited = false;


            Assert.That(() =>
            {
                Pipe.New<RequestContext>(cfg =>
                {
                    cfg.UseAuthFilter();
                    cfg.UseExecute(cxt =>
                    {
                        visited = true;
                    });
                });
            }, Throws.TypeOf<PipeConfigurationException>());

        }
    }

    //A random context
    //Notice: No mention of IPrincipal any where
    public class RequestContext :
        BasePipeContext,
        PipeContext
    {
        public RequestContext() : base(new PayloadCache())
        {
        }

    }

    //Play nice with CFG DSL using an extension method
    public static class AuthExtensions
    {
        public static void UseAuthFilter<T>(this IPipeConfigurator<T> cfg, params string[] allowedRoles)
             where T : class, PipeContext
        {
            cfg.AddPipeSpecification(new SampleAuthFilterSpecification<T>(allowedRoles));
        }
    }

    //Your custom filter specification with validation support
    public class SampleAuthFilterSpecification<T> : IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly string[] _allowedRoles;

        public SampleAuthFilterSpecification(string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new SampleAuthenticationFilter<T>(_allowedRoles));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_allowedRoles == null || !_allowedRoles.Any())
                yield return this.Failure("SampleAuthenticationFilter", "You need to supply roles");
        }
    }

    //your actual filter
    public class SampleAuthenticationFilter<T> : IFilter<T> where T : class, PipeContext
    {
        readonly string[] _allowedRoles;

        public SampleAuthenticationFilter(string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("SampleAuthFilter");
            scope.Add("allowed-roles", _allowedRoles);
        }

        public async Task Send(T context, IPipe<T> next)
        {
            IPrincipal prin;
            if (context.TryGetPayload(out prin))
            {
                if (_allowedRoles.Any(r => prin.IsInRole(r)))
                {
                    //you are allowed so continue to the next "segment"
                    await next.Send(context);

                }

                //TODO: handle unauthed
            }

            //TODO: handle unauthed
        }
    }
}