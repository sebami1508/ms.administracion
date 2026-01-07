using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Orm.Entidades
{
    public class TaConsecutivoFacturaModel
    {
        public int Id { get; set; }
        public string Prefijo { get; set; } = null!;
        public int Anio { get; set; }
        public int UltimoNumero { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
