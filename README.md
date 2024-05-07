# AuthJWT_WebApi.NETCore8

* [Probar el Token](https://jwt.io/)

### Dependencias Nuget
```
Dapper
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.EntityFrameworkCore.SqlServer
Swashbuckle.AspNetCore
Swashbuckle.AspNetCore.Filters
```

### appsettings.json
* Se debe configurar las entradas basicas del JWT
* Se debe configurar la conexion ala BD
```
  "JwtSettings": {
    "Key": "TheSecretKey",
    "Issuer": "TheIssuer",
    "Audience": "TheAudience",
    "ExpirationMinutes": 60
  },
  "ConnectionStrings": {
    "RutaWebSQL": "Data Source=Servidor; Initial Catalog=BaseDeDatos; User ID=Usuario; Password=Contraseña; TrustServerCertificate=True;",
    "RutaSQL": "Server=Servidor; Database=BaseDeDatos; User ID=Usuario; Password=Contraseña; TrustServerCertificate=True; Trusted_Connection=True;"
  }
```
### Program.cs y Utils/SwaggerApiPadLockFilter.cs
* La clase SwaggerApiPadLockFilter permite flitrar con candado en Swagger
* Configurar Swagger en Program.cs
```
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

app.UseAuthentication();
app.UseAuthorization();
```
### AuthController.cs
* Se implementa la Autenticacion
* Se implementa el Token
```
  var login = await _dapper.AuthIniciarSesion(user, cancellationToken);

  if (login.Estado)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
    var issuer = _configuration["JwtSettings:Issuer"];
    var audience = _configuration["JwtSettings:Audience"];
    var expire = _configuration["JwtSettings:ExpirationMinutes"];
    String TokenId = Guid.NewGuid().ToString();

    var tokenOptions = new JwtSecurityToken(
      issuer: issuer,
      audience: audience,
      claims: new List<Claim> {
        new (JwtRegisteredClaimNames.Jti, TokenId),
        new (JwtRegisteredClaimNames.Sub, user.Email),
        new (JwtRegisteredClaimNames.Email, user.Email),
      },
      expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expire)),
      signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    return Ok(new { Token = tokenString });
  }

  return Unauthorized();
```
