using TemplateAngularCoreSAML.API.Models.Dtos;

namespace TemplateAngularCoreSAML.API.Services
{
    public interface IJwtToken
    {
        string? CreateTokenApi(UserProfileDto authTokenDto);

        string TokenFrontEnd();
    }
}
