using System.Text.Json;

namespace TemplateAngularCoreSAML.API.Services
{
    public class Encription<T>
    {
        private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        public T? DecryptDataMethod(string data, string token)
        {
            var result = CryptographycFunctions.Decrypt(data, token);

            if (!result.Key)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(result.Value, options);
            }
            catch (Exception e)
            {
                return default;
            }

        }
    }
}
