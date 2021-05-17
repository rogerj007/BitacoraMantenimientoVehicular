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
                var usuario = await context.Client.SingleOrDefaultAsync(u => u.Telegram == pTelegram.ToString());
                var vehiculo = await context.Vehicle.SingleOrDefaultAsync(u => u.Name == pPlaque);
                if (usuario == null)
                    return "Su usuario no existe";
                if (!usuario.IsEnable)
                    return "Su usuario esta deshabilitado";
                if (vehiculo == null)
                    return $"La placa del vehiculo {pPlaque} no existe";
               
                var actividad = new VehicleRecordActivityEntity {Vehicle = vehiculo, Km = pKm, CreatedDate=DateTime.UtcNow, RegisterBy=usuario};
                context.Add(actividad);
                await context.SaveChangesAsync();
                var existeMantenimiento=await ValidarMantenimiento(vehiculo, pKm);
                var mensaje=new StringBuilder($"Estimado el registro de la placa {pPlaque.ToUpper()} fue asignado correctamente registrado\n");
                if (!string.IsNullOrEmpty(existeMantenimiento))
                    mensaje.Append(existeMantenimiento);

                return mensaje.ToString();
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


        public async Task<string> ValidarMantenimiento(VehicleEntity vehicle, long pKm)
        {
            try
            {
                await using var context = _context.CreateDbContext();


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error en Validar Mantenimiento";
            }
            return string.Empty;
        }
    }
}
