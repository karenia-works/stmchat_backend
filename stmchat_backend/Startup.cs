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
using Microsoft.Extensions.FileProviders;
using System.Net.WebSockets;
using System.Threading;
using Dahomey.Json.Serialization.Conventions;
using stmchat_backend.Helpers;
using stmchat_backend.Controllers;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

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

            services.AddSingleton<ChatService>();
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();


            // app.UseIdentityServer();
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.UseCors(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(new[] { "https://postwoman.io" });
            });
            var webSocketOptions = new WebSocketOptions();

            webSocketOptions.AllowedOrigins.Add("https://postwoman.io");
            webSocketOptions.AllowedOrigins.Add("https://localhost:8080");
            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                var scanUsernameResult = pathReg.Match(context.Request.Path.Value);
                if (scanUsernameResult.Success)
                {
                    var username = scanUsernameResult.Groups["name"].Value;
                    var websocket = await context.WebSockets.AcceptWebSocketAsync();
                    ChatService _chatservice;
                    using (var scope = context.RequestServices.CreateScope())
                    {
                        _chatservice = scope.ServiceProvider.GetService<ChatService>();
                    }

                    var tmp = context.Request.Path;
                    var jsonoption = new JsonSerializerOptions();
                    ConfigJsonOptions(jsonoption);
                    var ws = await _chatservice.Addsocket(username, websocket, jsonoption);
                    await ws.WaitUntilClose();
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
            registry.RegisterType<RTextMsg>();
            registry.RegisterType<RImageMsg>();
            registry.RegisterType<RFileMsg>();
            registry.RegisterType<RForwardMsg>();
            registry.DiscriminatorPolicy = DiscriminatorPolicy.Always;

            options.IgnoreNullValues = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;
        }
        static readonly Regex pathReg = new Regex("^/ws/(?<name>[^/]+)$");
    }
}
