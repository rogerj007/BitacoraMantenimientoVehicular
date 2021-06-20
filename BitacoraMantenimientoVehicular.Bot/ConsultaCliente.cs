using System;
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
                var vehiculo = await context.Vehicle.Include(v => v.VehicleBrand).Include(v=>v.VehicleStatus).SingleOrDefaultAsync(u => u.Name == pPlaque);
                if (usuario == null)
                    return (false,"Su usuario en Telegram No existe");
             
                if (vehiculo == null)
                    return (false, $"La placa del vehiculo {pPlaque} no existe");
                if (!usuario.IsEnable)
                    return (false, "Su usuario esta deshabilitado, comuniquese con el admin");

                var usuarioHabilitado = await context.ClientEntityVehicle.SingleOrDefaultAsync(cv => cv.VehicleEntity.Id.Equals(vehiculo.Id) && cv.ClientEntity.Id.Equals(usuario.Id));
                if(usuarioHabilitado==null)
                    return (false, "Su usuario No esta habilitado para este Vehiculo, comuniquese con el admin");

                if (vehiculo.VehicleStatus.Name == "EN TALLER")
                    return (false, $"El vehiculo {pPlaque.ToUpper()} esta en estado de Taller, no puede registar el Km");
                var validar = await context.VehicleRecordActivity.Where(r => r.Km >= pKm && r.Vehicle.Name.Equals(pPlaque)).ToListAsync();
                if (validar.Count > 0)
                    return (false, "No puede ingresar un Km menor al antes registrado");

                if (vehiculo.KmRegistro >= pKm)
                    return (false, "No puede ingresar un Km menor al registrado inicialmente");



                var kmTemp = vehiculo.KmActual;
                vehiculo.KmActual = pKm;
                var actividad = new VehicleRecordActivityEntity {Vehicle = vehiculo, Km = pKm, CreatedDate=DateTime.UtcNow, RegisterBy=usuario};
                context.Add(actividad);
                await context.SaveChangesAsync();
                var existeMantenimiento=await ValidarMantenimiento(vehiculo, usuario, kmTemp, pKm);
                var mensaje=new StringBuilder($"Estimado el registro de la placa {pPlaque.ToUpper()} fue asignado correctamente registrado\n");
                if (!string.IsNullOrEmpty(existeMantenimiento))
                    mensaje.Append(existeMantenimiento);

                return (true, existeMantenimiento);
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

        public async Task<string> MantenimientoEjecutadoAsync(string pPlaque)
        {
            try
            {
                await using var context = _context.CreateDbContext();
              //  var usuario = await context.Client.SingleOrDefaultAsync(u => u.Telegram == message.From.Id.ToString());
                var vehicle = await context.Vehicle.SingleOrDefaultAsync(v => v.Name.Equals(pPlaque));
                if (vehicle == null)
                {
                    return "No existe vehiculo en el ";
                }
                var existeRegistro = await context.ComponentNextChange.Include(v => v.Vehicle).Where(v => v.Vehicle.Id.Equals(vehicle.Id)).ToListAsync();
                                     //&& (r.Latitud == null || r.Longitud == null));
                string mensaje;
                if (existeRegistro == null)
                    mensaje = $"No existe vehiculo para realizar mantenimiento";
                else
                {

                    foreach (var registro in existeRegistro)
                    {
                        var db = await context.ComponentNextChange.SingleAsync(c=>c.Id.Equals(registro.Id));
                        db.IsComplete = true;
                        db.ModifiedDate = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                    mensaje = $"Estimado el mantenimiento fue correctamente registrado al vehiculo {pPlaque}\n";
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

        public async Task<string> RegistroUbicacionAsync(Message message)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var usuario = await context.Client.FirstOrDefaultAsync(u => u.Telegram == message.From.Id.ToString());
                //TODO: VALIDAR SI NO TIENE REGISTRO ANTERIOR
                var existeRegistro = await context.VehicleRecordActivity.Include(v => v.Vehicle).SingleOrDefaultAsync(r => r.RegisterBy.Id.Equals(usuario.Id) &&  (r.Latitud == null || r.Longitud==null));
                string mensaje;
                if (existeRegistro == null)
                    mensaje =
                        $"No existe bitacora para actualizar, ejecute el registro de Km recorridos {message.From.FirstName} {message.From.LastName}";
                else
                {
                    existeRegistro.Latitud = (decimal?)message.Location.Latitude;
                    existeRegistro.Longitud = (decimal?)message.Location.Longitude;
                    await context.SaveChangesAsync();
                    await NotificarClientesAsync( existeRegistro);

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

        public async Task NotificarClientesAsync(VehicleRecordActivityEntity record)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var clients = await context.ClientEntityVehicle
                                .Where(u => u.VehicleEntity.Id == record.Vehicle.Id)
                                .Select(x=>x.ClientEntity).AsNoTracking().ToListAsync();;

                foreach (var client in clients)
                {

                    var clientdb = await context.Client.AsNoTracking().SingleAsync(u => u.Id.Equals(client.Id));
                    var registroNotificacion = new RecordNotificationEntity
                                            {    Client = clientdb,
                                                VehicleRecordActivity = record, 
                                                Vehicle = record.Vehicle,
                                                CreatedDate = DateTime.UtcNow
                                            };
                    var entry = context.Entry(registroNotificacion);
                    entry.State = EntityState.Added;
                    //context.Attach(registroNotificacion);
                   // await context.AddAsync(registroNotificacion);
                    await context.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
            }
           
            catch (Exception exception)
            {
                _logger.LogError(exception.ToMessageAndCompleteStacktrace());
            }

        }


        public async Task<string> RegistroCambioComponenteRealizadoAsync(Message message)
        {
            try
            {
                await using var context = _context.CreateDbContext();
                var usuario = await context.Client.SingleOrDefaultAsync(u => u.Telegram == message.From.Id.ToString());
                var existeRegistro = await context.VehicleRecordActivity.Include(v => v.Vehicle).SingleOrDefaultAsync(r => r.RegisterBy.Id.Equals(usuario.Id) && (r.Latitud == null || r.Longitud == null));
                string mensaje;
                if (existeRegistro == null)
                    mensaje =
                        $"No existe bitacora para actualizar, ejecute el registro de Km recorridos {message.From.FirstName} {message.From.LastName}";
                else
                {
                    existeRegistro.Latitud = (decimal?)message.Location.Latitude;
                    existeRegistro.Longitud = (decimal?)message.Location.Longitude;
                    await context.SaveChangesAsync();
                    await NotificarClientesAsync(existeRegistro);

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

        public async Task<string> ValidarMantenimiento(VehicleEntity vehicle, ClientEntity client,long pKmAnterior, long pKm)
        {
            try
            {
                var mensaje = new StringBuilder();
                var recorridoUltimo = pKm - pKmAnterior;
                await using var context = _context.CreateDbContext();
                var componentes = await context.Component.Where(c=>c.IsEnable).AsNoTracking().ToListAsync();
                var vehicleDb = await context.Vehicle.SingleAsync(v => v.Id == vehicle.Id);
                var componentsNextChange = await context.ComponentNextChange.Include(v => v.Vehicle)
                    .Include(c => c.Component)
                    .Where(v => vehicle != null && v.Vehicle.Id == vehicle.Id).ToListAsync();
                var kmRecorrido = pKm - pKmAnterior;
                //Creacion por primera vez componente
                foreach (var componente in componentes)
                {
                    try
                    {
                        var validarTiempoVidaComponente = componentsNextChange.SingleOrDefault(c => c.Component.Id.Equals(componente.Id) && c.Vehicle.Id.Equals(vehicle.Id));
                        if (validarTiempoVidaComponente != null)
                        {
                            var ttl = validarTiempoVidaComponente.Km - kmRecorrido;
                            validarTiempoVidaComponente.Km = ttl>0?ttl:0;
                            validarTiempoVidaComponente.ModifiedDate = DateTimeOffset.UtcNow;
                            if (validarTiempoVidaComponente.Km <= 250)
                            {
                                mensaje.Append($"Componente : {componente.Name} debe ser remplazado cada {componente.Ttl} y tiene menos de {validarTiempoVidaComponente.Km} Km\n");
                                validarTiempoVidaComponente.IsComplete = false;
                            }
                        }
                        else
                        {
                            var componetDb = await context.Component.SingleAsync(c => c.Id.Equals(componente.Id));
                            var registroComponente = new ComponentNextChangeEntity
                            {
                                Vehicle = vehicleDb,
                                CreatedDate = DateTime.UtcNow,
                                Km = componente.Ttl - kmRecorrido,
                                Component = componetDb,
                                IsComplete = true
                            };
                            await context.AddAsync(registroComponente);
                        }
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToMessageAndCompleteStacktrace());
                    }
                }
              
                return recorridoUltimo <= 0 ? string.Empty : mensaje.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error en Validar Mantenimiento";
            }
        }
    }
}
