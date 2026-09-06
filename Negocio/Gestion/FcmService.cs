using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;

namespace Negocio.Gestion
{
    public class FcmService : IFcmService
    {
        private readonly ContextoDb db;
        private readonly MyConfig _config;
        private static readonly HttpClient _http = new HttpClient();

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public FcmService(ContextoDb _db, MyConfig config)
        {
            db = _db;
            _config = config;
        }

        public async Task RegistrarTokenAsync(string usuarioId, string token, string? plataforma)
        {
            if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(token))
                return;

            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            var existente = await db.TaDispositivoFcmModel
                .FirstOrDefaultAsync(x => x.Token == token);

            if (existente != null)
            {
                existente.UsuarioId = usuarioId.Trim();
                existente.Plataforma = plataforma;
                existente.FechaRegistro = ahora;
                db.Update(existente);
            }
            else
            {
                db.Add(new TaDispositivoFcmModel
                {
                    DispositivoId = Guid.NewGuid(),
                    UsuarioId = usuarioId.Trim(),
                    Token = token.Trim(),
                    Plataforma = plataforma,
                    FechaRegistro = ahora
                });
            }

            await db.SaveChangesAsync();
        }

        public async Task EnviarAsync(string usuarioId, string titulo, string cuerpo,
            Dictionary<string, string>? data = null)
        {
            // Si Firebase no está configurado, no-op (permite operar sin push).
            if (string.IsNullOrWhiteSpace(_config.FirebaseProjectId) ||
                string.IsNullOrWhiteSpace(_config.FirebaseCredentialPath) ||
                !File.Exists(_config.FirebaseCredentialPath))
                return;

            try
            {
                var tokens = await db.TaDispositivoFcmModel
                    .Where(x => x.UsuarioId == usuarioId)
                    .Select(x => x.Token)
                    .ToListAsync();

                if (tokens.Count == 0) return;

                var accessToken = await ObtenerAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken)) return;

                var url = $"https://fcm.googleapis.com/v1/projects/{_config.FirebaseProjectId}/messages:send";
                var tokensInvalidos = new List<string>();

                foreach (var t in tokens)
                {
                    var payload = new
                    {
                        message = new
                        {
                            token = t,
                            notification = new { title = titulo, body = cuerpo },
                            data = data,
                            android = new { priority = "high" }
                        }
                    };

                    var json = JsonSerializer.Serialize(payload, _jsonOpts);
                    using var req = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    try
                    {
                        var resp = await _http.SendAsync(req);
                        // Token inválido/no registrado: marcar para eliminar.
                        if (resp.StatusCode == HttpStatusCode.NotFound ||
                            resp.StatusCode == HttpStatusCode.BadRequest)
                            tokensInvalidos.Add(t);
                    }
                    catch
                    {
                        // Ignorar fallos individuales de envío.
                    }
                }

                if (tokensInvalidos.Count > 0)
                {
                    var muertos = await db.TaDispositivoFcmModel
                        .Where(x => tokensInvalidos.Contains(x.Token))
                        .ToListAsync();
                    if (muertos.Count > 0)
                    {
                        db.RemoveRange(muertos);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // El envío de push nunca debe romper el flujo de negocio.
            }
        }

        private async Task<string?> ObtenerAccessTokenAsync()
        {
            try
            {
                var credential = GoogleCredential
                    .FromFile(_config.FirebaseCredentialPath)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                return await credential.UnderlyingCredential
                    .GetAccessTokenForRequestAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
