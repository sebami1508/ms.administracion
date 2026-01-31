using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Orm.Entidades
{
    public class TaOtpModel
    {
        public Guid OtpId { get; set; }
        public string UsuarioId { get; set; } = null!;

        public string Proposito { get; set; } = "RESET_PASSWORD";

        public string OtpHash { get; set; } = null!;
        public string OtpSalt { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; }
        public DateTime? FechaUso { get; set; }

        public int Intentos { get; set; }
        public int MaxIntentos { get; set; }

        public string? IpSolicitud { get; set; }
        public string? UserAgent { get; set; }

        public TaUsuarioModel TaUsuarioModel { get; set; } = null!;
    }

}
