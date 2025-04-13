using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Mes.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Model.Other;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 配置服务
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
    });

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new AutofacModuleRegister(builder.Configuration));
});

builder.Services.AddCors(p =>
    p.AddPolicy(
        "Cors",
        policy => policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader()
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer(
        (document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Mes系统Api",
                Version = "v1",
                Description = "Mes接口文档",
            };
            return Task.CompletedTask;
        }
    ).AddDocumentTransformer<BearerSecuritySchemeTransformer>()
);

// 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecurityKey))
{
    var (publicKey, privateKey) = GenerateRsaKeys();
    jwtSettings = new JwtSettings
    {
        SecurityKey = privateKey,
        PublicKey = publicKey,
        Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "https://localhost:7092",
        Audience = "http://localhost:5173/",
        ExpirationMinutes = 60,
        RefreshTokenExpirationDays = 7,
    };
}
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecurityKey = jwtSettings.SecurityKey;
    options.PublicKey = jwtSettings.PublicKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationMinutes = jwtSettings.ExpirationMinutes;
    options.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
});

// JWT 认证配置
builder
    .Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(option =>
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(jwtSettings.PublicKey.ToCharArray());

        option.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.FromSeconds(0),
            IssuerSigningKey = new RsaSecurityKey(rsa),
        };
    });

// 配置限流
builder.Services.AddRateLimiter(options =>
{
    // 全局限流策略，每分钟最多10次请求
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30, // 最大请求数
                Window = TimeSpan.FromMinutes(1), // 时间窗口
                QueueLimit =
                    0 // 无排队，直接拒绝
                ,
            }
        )
    );

    // 拒绝请求时的状态码
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // 定义 "LoginLimiter" 限流策略，针对 IP 地址，每 2 秒 1 次请求
    options.AddPolicy(
        "LoginLimiter",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1, // 每 2 秒钟 1 次请求
                    Window = TimeSpan.FromSeconds(2), // 时间窗口 2 秒
                    QueueLimit =
                        0 // 无排队，直接拒绝
                    ,
                }
            )
    );

    // 自定义限流拒绝响应
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            JsonConvert.SerializeObject(
                FormattedResponse<string>.Error("请求太频繁，请稍后再试", 429)
            ),
            token
        );
    };
});

var app = builder.Build();

// 配置中间件
app.UseRouting();
app.UseCors("Cors");
app.UseHttpsRedirection();
app.UseStaticFiles();

// 使用限流中间件
app.UseRateLimiter();
app.UseAuthentication(); // 使用认证中间件
app.UseAuthorization();

app.UseGlobalExceptionHandling();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Mes.Api - API 文档")
        .WithTheme(ScalarTheme.Mars)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithSidebar(true)
        .WithCustomCss(".scalar-client { font-family: 'Fir Code', sans-serif; }");
});
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
app.MapControllers();

app.Run();
return;

// 生成 RSA 密钥对
static (string PublicKey, string PrivateKey) GenerateRsaKeys()
{
    using var rsa = new RSACryptoServiceProvider(2048);
    RSAParameters parameters = rsa.ExportParameters(true);
    var keyPair = DotNetUtilities.GetRsaKeyPair(parameters);
    var privateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
    var publicKeyParams = (RsaKeyParameters)keyPair.Public;

    StringBuilder publicKey = new();
    using (var sw = new StringWriter(publicKey))
    {
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(publicKeyParams);
    }

    StringBuilder privateKey = new();
    using (var sw = new StringWriter(privateKey))
    {
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(privateKeyParams);
    }

    return (publicKey.ToString(), privateKey.ToString());
}
