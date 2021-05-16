using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Bot.Helpers;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitacoraMantenimientoVehicular.Bot
{
    class ConsultaCliente
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<ConsultaCliente> _logger;
        private readonly IDbContextFactory<DataContext> _context;
        public ConsultaCliente(IConfigurationRoot config,
                                ILogger<ConsultaCliente> loggerFactor,
                                IDbContextFactory<DataContext> context)
        {
            _config = config;
            _logger = loggerFactor;
            _context = context;
        }
        public async Task<string> RegistroActividadAsync(string pPlaque,long pKm, long pTelegram)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var usuario = await context.Users.SingleOrDefaultAsync(u => u.Telegram == pTelegram.ToString());
                var vehiculo = await context.Vehicle.SingleOrDefaultAsync(u => u.Name == pPlaque);
                if (usuario == null)
                    return "su usuario no existe";
                if (!usuario.IsEnable)
                    return "su usuario esta deshabilitado";
                if (vehiculo == null)
                    return $"la placa del vehiculo {pPlaque} no existe";

                var actividad = new VehicleRecordActivityEntity {Vehicle = vehiculo, Km = pKm, CreatedDate=DateTime.UtcNow};
                context.Add(actividad);
                await context.SaveChangesAsync();
                return $"Estimado el registro de la placa {pPlaque} fue asignado correctamente registrado\n";


            }
            catch (InvalidOperationException exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return "Error en registrar actividad";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return "Error en registrar actividad";
            }

        }
    }
}
