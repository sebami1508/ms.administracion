namespace Negocio.Contrato
{
    public interface IMyConfig
	{
        public string Key { get; set; }
        public string UrlInicioSesion { get; set; }
        public string CorreoNotificacion { get; set; }
        public string PasswordCorreo { get; set; }

    }
}
