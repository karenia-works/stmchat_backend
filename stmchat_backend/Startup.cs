using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Dahomey.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using stmchat_backend.Models.Settings;
using stmchat_backend.Services;
using stmchat_backend.Store;
using stmchat_backend.Models;
using stmchat_backend.Helpers;
using Dahomey.Json.Serialization.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace stmchat_backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ID4
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryApiResources(Config.GetApiResources()).AddResourceOwnerValidator<UserStore>();

            // DB
            services.Configure<DbSettings>(
                Configuration.GetSection(nameof(DbSettings))
            );
            services.AddSingleton<IDbSettings>(
                setting => setting.GetRequiredService<IOptions<DbSettings>>().Value
            );

            // Singleton Service
            services.AddSingleton<GroupService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<FileService>();

            // Web service
            services.AddSingleton<ICorsPolicyService>(
                new DefaultCorsPolicyService(
                    new LoggerFactory().CreateLogger<DefaultCorsPolicyService>()
                )
            );
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.AddRouting(options => { options.LowercaseUrls = true; });

            services.AddControllers()
                .AddJsonOptions(option => { ConfigJsonOptions(option.JsonSerializerOptions); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseWebSockets();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/ws")
                {
                    if (ctx.WebSockets.IsWebSocketRequest)
                    {
                        var socket = await ctx.WebSockets.AcceptWebSocketAsync();
                        var jsonConfig = new JsonSerializerOptions();
                        ConfigJsonOptions(jsonConfig);
                        var socketWrapper = new JsonWebsocketWrapper<Message, Message>(socket, jsonConfig);
                        socketWrapper.Messages.Subscribe(
                            (msg) => { Console.WriteLine($"recv: {msg}"); },
                            (err) => { Console.WriteLine($"err: {err}"); },
                            () => { Console.WriteLine("Completed"); });
                        await socketWrapper.WaitUntilClose();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                        await ctx.Response.Body.WriteAsync(
                            System.Text.Encoding.UTF8.GetBytes("Not a websocket request!"));
                    }
                }
                else
                {
                    await next();
                }
            });

            // File and Image
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"data")),
                RequestPath = new PathString("/file"),
                EnableDirectoryBrowsing = env.IsDevelopment()
            });

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public static void ConfigJsonOptions(JsonSerializerOptions options)
        {
            options.SetupExtensions();
            options.Converters.Add(new ObjectIdConverter());
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "_t"));
            registry.RegisterType<TextMsg>();
            registry.RegisterType<FileMsg>();
            registry.RegisterType<ImageMsg>();
            registry.DiscriminatorPolicy = DiscriminatorPolicy.Always;
        }
    }
}
