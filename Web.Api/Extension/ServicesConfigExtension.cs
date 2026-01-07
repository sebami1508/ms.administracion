using Datos.Orm.Contexto;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Gestion;
using System.Globalization;

namespace Web.Api.Extension
{
    public static class ServicesConfigExtension
    {
        public static void ConfiguracionRegionalFecha(this WebApplicationBuilder _builder, string _parametro)
        {
            var defaultCulture = new CultureInfo(_parametro);
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
        }

        public static void ObjectDependencyInjector(this WebApplicationBuilder _builder)
        {
            _builder.Services.AddDbContext<ContextoDb>(options =>
                  options.UseNpgsql(
                      _builder.Configuration.GetConnectionString("DbConexion")
                  )
              );

            _builder.Services.AddDbContext<DbContextError>(options =>
                  options.UseNpgsql(
                      _builder.Configuration.GetConnectionString("DbConexion")
                  )
              );

            _builder.Services.AddScoped<IUsuario, UsuarioLogica>();
            _builder.Services.AddScoped<IRolUsuario, RolUsuarioLogica>();
            _builder.Services.AddScoped<IRol, RolLogica>();
            _builder.Services.AddScoped<IMenu, MenuLogica>();
            _builder.Services.AddScoped<IPerfil, PerfilLogica>();
            _builder.Services.AddScoped<IZonaGeografica, ZonaGeograficaLogica>();
            _builder.Services.AddScoped<IDominio, DominioLogica>();
            _builder.Services.AddScoped<IProducto, ProductoLogica>();
            _builder.Services.AddScoped<IOrden, OrdenLogica>();
            _builder.Services.AddScoped<IItem, ItemLogica>();
            _builder.Services.AddScoped<IFacturacionService, FacturacionService>();
        }
    }
}