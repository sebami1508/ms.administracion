namespace Comun.Dto
{
    using Comun.Enumeracion;

    public class RespuestaDto<T>
    {
        public RespuestaDto() { }

        public RespuestaDto(EstadoOperacion _codigo, string _mensaje)
        {
            Codigo = _codigo;
            Mensaje = _mensaje;
        }

        public RespuestaDto(EstadoOperacion _codigo, string _mensaje, T _respuesta)
        {
            Codigo = _codigo;
            Mensaje = _mensaje;
            Respuesta = _respuesta;
        }

        public EstadoOperacion Codigo { get; set; }
        public string Mensaje { get; set; }
        public bool Estado { get => Codigo == EstadoOperacion.Bueno ? true : false; }

        private T respuesta;

        public T Respuesta
        {
            get
            {
                if (typeof(T) == typeof(bool))
                    return (T)Convert.ChangeType(Estado, typeof(bool));
                else
                    return respuesta;
            }
            set
            {
                respuesta = value;
            }
        }
    }
}