using AuthJWT_WebApi.NETCore8.Connection;
using AuthJWT_WebApi.NETCore8.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Servicio Conexion -------------------------------------------------
builder.Services.AddSingleton<IDbConnection>(provider =>
    new SqlConnection(builder.Configuration.GetConnectionString("RutaSQL"))
);

builder.Services.AddTransient<ApiDbContext>();

// Servicio JWT ------------------------------------------------------
builder.Services
    .AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(jwtOptions => 
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
    };
});

//builder.Services.AddAuthorization();

// -------------------------------------------------------------------

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Modificar Swagger par aque muestre los candados -------------------
builder.Services.AddSwaggerGen(options =>
{
    // Agrega el login arriba de la Api
    options.AddSecurityDefinition(
        //name: JwtBearerDefaults.AuthenticationScheme,
        name: "Bearer",
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Ingrese Token Bearer",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            //Scheme = "Bearer"
        }
    );

    // Agrega los candados a cada uno del CRUD
    //opcion.OperationFilter<SecurityRequirementsOperationFilter>();
    options.OperationFilter<SwaggerApiPadLockFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar JWT ----------------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();
// -------------------------------------------------------------------

app.MapControllers();

app.Run();
