using AuthJWT_WebApi.NETCore8.DTOs;
using Dapper;
using System.Data;

namespace AuthJWT_WebApi.NETCore8.Connection
{
    public class ApiDbContext
    {
        private readonly IDbConnection _connection;

        public ApiDbContext(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<AuthUserRespDTO> AuthIniciarSesion(
            AuthUserDTO dto,
            CancellationToken cancellationToken)
        {
            var result = await _connection.QueryFirstAsync<AuthUserRespDTO>(new CommandDefinition(
                $"AuthIniciarSesion @{nameof(dto.Email)}, @{nameof(dto.Password)}",
                new { dto.Email, dto.Password },
                cancellationToken: cancellationToken));

            return result;
        }

        public async Task<dynamic?> AuthCrearUsuario(
            String Id,
            AuthUserDTO dto,
            CancellationToken cancellationToken)
        {
            var result = await _connection.QueryFirstOrDefaultAsync(new CommandDefinition(
                $"AuthCrearUsuario @{nameof(Id)}, @{nameof(dto.Email)}, @{nameof(dto.Password)}",
                new { Id, dto.Email, dto.Password },
                cancellationToken: cancellationToken));

            return result;
        }
    }
}
