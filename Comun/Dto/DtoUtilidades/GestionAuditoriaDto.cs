using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comun.Dto.DtoUtilidades
{
    public class GestionAuditoriaDto
    {
        public ParametrosAuditoriaDto ParametrosAuditoriaDto { get; set; } = new ParametrosAuditoriaDto();
    }

    public class ParametrosAuditoriaDto
    {
        #region parametros del servicio web para auditoria		

        public string? AuditoriaUsuario { get; set; } = "Sistema";
        public string? AuditoriaMaquina { get; set; } = "000.000.000.000";

        #endregion
    }
}
