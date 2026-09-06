using Negocio.Contrato;

namespace Negocio.Gestion
{
    public class MyConfig : IMyConfig
    {
        public string Key { get; set; }
        public string UrlInicioSesion { get; set; }
        public string CorreoNotificacion { get; set; }
        public string PasswordCorreo { get; set; }
        public string Municipio { get; set; }

        /// <summary>ID del proyecto de Firebase (para FCM HTTP v1).</summary>
        public string? FirebaseProjectId { get; set; }

        /// <summary>Ruta al JSON de la cuenta de servicio de Firebase.</summary>
        public string? FirebaseCredentialPath { get; set; }

    }
}
