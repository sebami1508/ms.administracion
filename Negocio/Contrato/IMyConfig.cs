namespace Negocio.Contrato
{
    public interface IMyConfig
	{
        public string Key { get; set; }
        public string UrlInicioSesion { get; set; }
        public string CorreoNotificacion { get; set; }
        public string PasswordCorreo { get; set; }
        public string Municipio { get; set; }
        public string? FirebaseProjectId { get; set; }
        public string? FirebaseCredentialPath { get; set; }

    }
}
