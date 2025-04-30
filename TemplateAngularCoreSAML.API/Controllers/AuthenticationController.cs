using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TemplateAngularCoreSAML.API.Models.Common;
using TemplateAngularCoreSAML.API.Models.Dtos;
using TemplateAngularCoreSAML.API.Services;

namespace TemplateAngularCoreSAML.API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class AuthenticationController(IJwtToken jwtToken) : ControllerBase
    {
        protected readonly IJwtToken _jwtToken = jwtToken;
        /// <summary>
        /// Obtiene el token de autorización del usuario que ingresa al modulo administrador.
        /// </summary>
        /// <param name="sendEncryptDto">Datos encriptados necesarios para el login </param>
        /// <response code="200">Regresa un token</response>  
        /// <response code="500">Response</response>  
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetToken([FromBody] SendEncryptDto sendEncryptDto)
        {
            // 1. Desencripta la información del usuario.
            UserProfileDto? userProfileDto = new Encription<UserProfileDto>().DecryptDataMethod(sendEncryptDto.Data, _jwtToken.TokenFrontEnd());
            if (userProfileDto == null)
                return Ok(new Response<string>("Error al desencriptar la información."));

            // 2. Validaciones internar de la aplicación, en este caso solo se valida que los datos no vengan nulos o vacios.
            if (string.IsNullOrEmpty(userProfileDto.PayrollId) || string.IsNullOrEmpty(userProfileDto.Email) || string.IsNullOrEmpty(userProfileDto.PersonId))
                return Ok(new Response<string>("Error la información no se encuentra completa."));

            // 3. Generamos el jwt
            var jwtSecurityToken = _jwtToken.CreateTokenApi(userProfileDto);

            // 4. Regresa un response
            return Ok(
                new Response<string>()
                {
                    Succeeded = jwtSecurityToken != null,
                    Message = jwtSecurityToken == null ? "Ocurrió un error al generar el JWT." : "Se genera con éxito el JWT.",
                    Data = jwtSecurityToken ?? "",
                }
            );
        }
    }
}
