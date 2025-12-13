using Negocio.Contrato;

namespace Negocio.Gestion
{
    public class MyConfig : IMyConfig
    {
        public string Key { get; set; }
        public string UrlInicioSesion { get; set; }
        public string CorreoNotificacion { get; set; }
        public string PasswordCorreo { get; set; }
       
    }
}
