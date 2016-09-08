﻿using System;
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
        IPipe<RequestContext> _thePipe;
        bool protectedBusinessAction;
        bool cleanUp;

        [SetUp]
        public void SetUp()
        {
            protectedBusinessAction = false;
            cleanUp = false;

            var authPipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    protectedBusinessAction = true;
                });
            });

            var unauthPipe = Pipe.New<RequestContext>(cfg =>
            {

            });

            _thePipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseAuthFilter(authPipe, unauthPipe, "bob");
                cfg.UseExecute(cxt =>
                {
                    cleanUp = true;
                });
            });


        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine(_thePipe.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task Authenticated()
        {
            var request = new RequestContext();
            request.GetOrAddPayload(() => new GenericPrincipal(new GenericIdentity("Gizmo"), new []{"bob"} ));

            await _thePipe.Send(request).ConfigureAwait(false);

            Assert.That(protectedBusinessAction, Is.True);
            Assert.That(cleanUp, Is.True);
        }

        [Test]
        public async Task Unauthenticated()
        {
            var request = new RequestContext();
            request.GetOrAddPayload(() => System.Threading.Thread.CurrentPrincipal);

            await _thePipe.Send(request).ConfigureAwait(false);

            Assert.That(protectedBusinessAction, Is.False);
            Assert.That(cleanUp, Is.True);

        }

        [Test]
        public async Task InvalidSetup()
        {
            bool protectedBusinessAction = false;

            var authPipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    protectedBusinessAction = true;
                });
            });

            var unauthPipe = Pipe.New<RequestContext>(cfg =>
            {

            });

            Assert.That(() =>
            {
                Pipe.New<RequestContext>(cfg =>
                {
                    cfg.UseAuthFilter(authPipe, unauthPipe);
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
        public static void UseAuthFilter<T>(this IPipeConfigurator<T> cfg, IPipe<T> authPipe, IPipe<T> unauthPipe,
            params string[] allowedRoles)
            where T : class, PipeContext
        {
            cfg.AddPipeSpecification(new SampleAuthFilterSpecification<T>(authPipe, unauthPipe, allowedRoles));
        }
    }

    //Your custom filter specification with validation support
    public class SampleAuthFilterSpecification<T> : IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly IPipe<T> _authPipe;
        readonly IPipe<T> _unauthPipe;
        readonly string[] _allowedRoles;

        public SampleAuthFilterSpecification(IPipe<T> authPipe, IPipe<T> unauthPipe, string[] allowedRoles)
        {
            _authPipe = authPipe;
            _unauthPipe = unauthPipe;
            _allowedRoles = allowedRoles;
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new SampleAuthenticationFilter<T>(_authPipe, _unauthPipe, _allowedRoles));
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
        readonly IPipe<T> _authPipe;
        readonly IPipe<T> _unauthPipe;
        readonly string[] _allowedRoles;

        public SampleAuthenticationFilter(IPipe<T> authPipe, IPipe<T> unauthPipe, string[] allowedRoles)
        {
            _authPipe = authPipe;
            _unauthPipe = unauthPipe;
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
                    await _authPipe.Send(context);
                }
                else
                {
                    await _unauthPipe.Send(context);
                }
            }

            await next.Send(context);
        }
    }
}