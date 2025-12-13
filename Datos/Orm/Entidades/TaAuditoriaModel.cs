using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Orm.Entidades
{
    public class TaAuditoriaModel
    {
        public string AuditoriaId { get; set; } = null!;
        public string TablaId { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public string MaquinaIp { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;
        public string? ValorAntiguo { get; set; }
        public string? ValorNuevo { get; set; }
        public string Accion { get; set; } = null!;
        public string NombreTabla { get; set; } = null!;
        public string Modulo { get; set; } = null!;

        public TaUsuarioModel TaUsuarioModel { get; set; }
    }
}
