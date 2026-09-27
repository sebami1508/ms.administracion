namespace Comun.Enumeracion
{
    public class Constantes
    {
        public const bool Vigente = true;
        public const bool NoVigente = false;

        public const string RolDistribuidor = "20eb3630-d6f3-4922-8cbd-0c03f2b562fb";
        public const string GuidColombiaId = "4b409ec1-ac13-48ab-9eda-dad4ea24eeda";

        #region Estados Ordenes
        public const string Pendiente = "3a2cec99-d19a-4f3d-a7df-177688b9bd8c";
        public const string Facturada = "671bf211-dc67-4d6d-a926-613e8d3ad1cc";
        public const string Cancelada = "59837ce6-faad-409b-80d6-7d2b055480ee";
        public const string PorValidar = "f4f739f2-5892-4fc1-82d3-1130c81217f2";
        public const string Despachado = "216d3fee-bdb6-4491-96dd-37cc082c0c97";
        #endregion

        #region Roles (RolId en el claim "role" del token)
        public const string RolSuperAdministrador = "b815aa4b-3e9c-44a9-a40b-05c033d01411";
        public const string RolAdministrador = "e08c70ed-94b5-4692-9b47-e6f8a9a23f1f";
        public const string RolVentas = "2c98cf7a-bdd8-418a-886f-ac38642aeeda";
        public const string RolCliente = "5791103e-11ed-4c6a-9a87-63fcaf3c046c";

        /// <summary>Para [Authorize(Roles = ...)]: super administrador o administrador.</summary>
        public const string RolesAdministradores = RolSuperAdministrador + "," + RolAdministrador;

        /// <summary>Para [Authorize(Roles = ...)]: personal del negocio (administradores y ventas).</summary>
        public const string RolesPersonal = RolesAdministradores + "," + RolVentas;
        #endregion

        #region Estados Ordenes
        public const string PendienteIniciar = "98558c7f-9dbd-4f2a-82df-91fdfc5f993c";
        public const string TurnoVigente = "63f01c99-372d-4735-aca8-736ab14aa0a5";
        public const string Finalizado = "55d30003-05e7-437c-83fc-3e8f332193fa";
        #endregion
    }
}
