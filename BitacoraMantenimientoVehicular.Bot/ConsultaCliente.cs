using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Bot.Helpers;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

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
        public async Task<(bool, string)> RegistroActividadAsync(string pPlaque,long pKm, long pTelegram)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var usuario = await context.Client.SingleOrDefaultAsync(u => u.Telegram == pTelegram.ToString());
                var vehiculo = await context.Vehicle.Include(v => v.VehicleBrand).SingleOrDefaultAsync(u => u.Name == pPlaque);
                if (usuario == null)
                    return (false,"Su usuario no existe");
                if (!usuario.IsEnable)
                    return (false, "Su usuario esta deshabilitado)");
                if (vehiculo == null)
                    return (false, $"La placa del vehiculo {pPlaque} no existe");

                var validar = await context.VehicleRecordActivities.Where(r => r.Km >= pKm).ToListAsync();
                if (validar.Count > 0)
                {
                    return (false, "No puede ingresar un Km menor al antes registrado");
                }

                if (vehiculo.KmRegistro >= pKm)
                {
                    return (false, "No puede ingresar un Km menor al registrado inicialmente");
                }

                var kmTemp = vehiculo.KmActual;
                vehiculo.KmActual = pKm;
                var actividad = new VehicleRecordActivityEntity {Vehicle = vehiculo, Km = pKm, CreatedDate=DateTime.UtcNow, RegisterBy=usuario};
                context.Add(actividad);
                await context.SaveChangesAsync();
                var existeMantenimiento=await ValidarMantenimiento(vehiculo, kmTemp, pKm);
                var mensaje=new StringBuilder($"Estimado el registro de la placa {pPlaque.ToUpper()} fue asignado correctamente registrado\n");
                if (!string.IsNullOrEmpty(existeMantenimiento))
                    mensaje.Append(existeMantenimiento);

                return (true, mensaje.ToString());
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return (false, "Error en registrar actividad");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return (false, "Error en registrar actividad");
            }

        }

        public async Task<string> RegistroUbicacionAsync(Message message)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var usuario = await context.Client.SingleOrDefaultAsync(u => u.Telegram == message.From.Id.ToString());
                var existeRegistro = await context.VehicleRecordActivities.Include(v => v.Vehicle).SingleOrDefaultAsync(r => r.RegisterBy.Id.Equals(usuario.Id) &&  (r.Latitud == null || r.Longitud==null));
                string mensaje;
                if (existeRegistro == null)
                    mensaje =
                        $"No existe bitacora para actualizar, ejecute el registro de Km recorridos {message.From.FirstName} {message.From.LastName}";
                else
                {
                    existeRegistro.Latitud = (decimal?)message.Location.Latitude;
                    existeRegistro.Longitud = (decimal?)message.Location.Longitude;
                    await context.SaveChangesAsync();
                    mensaje = $"Estimado el registro de la placa {existeRegistro.Vehicle.Name.ToUpper()} fue asignado correctamente registrado\n";
                }
              

                return mensaje;
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return "Error en registro de ubicacion";

            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
                return "Error en registro de ubicacion";

            }

        }

        public async Task<string> ValidarMantenimiento(VehicleEntity vehicle,long pKmAnterior, long pKm)
        {
            try
            {
                var mensaje = new StringBuilder();
                var recorridoUltimo = pKm - pKmAnterior;
                await using var context = _context.CreateDbContext();
                var componentes = await context.Component.AsNoTracking().ToListAsync();
                var vehicleDb = await context.Vehicle.SingleAsync(v => v.Id == vehicle.Id);
                var componentsNextChange = await context.ComponentNextChange.Include(v => v.Vehicle).Include(c=>c.Component)
                    .Where(v => vehicle != null && v.Vehicle.Id == vehicle.Id).ToListAsync();
                var mensajeNotificar = new StringBuilder();
                if (componentsNextChange.Any(nc => !nc.IsComplete))
                {
                  
                    mensaje.Append("Aun no completa el mantenimiento anterior\n");
                    foreach (var component in componentsNextChange)
                    {
                        mensaje.Append($"Componente :{component.Component.Name} debe ser remplazado\n");
                    }
                    return mensaje.ToString();
                }

                foreach (var componente in componentes)
                {
                    try
                    {
                        if (recorridoUltimo <= componente.Ttl) continue;
                        var componetDb = await context.Component.SingleAsync(c=>c.Id.Equals(componente.Id));
                        var registroComponente = new ComponentNextChangeEntity
                        {
                            Vehicle = vehicleDb,
                            CreatedDate = DateTime.UtcNow,
                            Km = pKm,
                            Component = componetDb,
                            IsComplete = false
                        };

                        mensaje.Append($"Componente : {componente.Name} debe ser remplazado cada {componente.Ttl} Km\n");
                        await context.AddAsync(registroComponente);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToMessageAndCompleteStacktrace());
                    }
                }

                return recorridoUltimo > pKm ? mensajeNotificar.ToString() : string.Empty;



                ////Registro Inicial para vehiculos nuevos...
                //foreach (var componente in componentes)
                //{
                //    if (vehicleDb.KmActual + componente.Ttl > pKm)
                //    {
                //        var registroComponente = new ComponentNextChangeEntity
                //        {
                //            Vehicle = vehicle,
                //            CreatedDate = DateTime.UtcNow,
                //            Km = pKm,
                //            Component = componente,
                //            IsComplete=false
                //        };
                //        await context.AddAsync(registroComponente);
                //        await context.SaveChangesAsync();
                //    }
                //}

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
