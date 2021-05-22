using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Bot.Helpers;
using BitacoraMantenimientoVehicular.Datasource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BitacoraMantenimientoVehicular.Bot
{
    public class EnvioAlertaTelegram
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<EnvioAlertaTelegram> _logger;
        private readonly IDbContextFactory<DataContext> _context;
        public EnvioAlertaTelegram(IConfigurationRoot config, ILogger<EnvioAlertaTelegram> loggerFactor,
            IDbContextFactory<DataContext> dataContext)
        {
            _config = config;
            _logger = loggerFactor;
            _context = dataContext;
        }
        public async Task EnvioRegistroKmActual()
        {
            var cancellationToken = new CancellationTokenSource();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    _logger.LogInformation("Consulta a Base para Envio Telegram");
                    Console.ResetColor();
                    await using var context = _context.CreateDbContext();

                    var notificaciones = await context.RecordNotifications.Include(v=>v.Vehicle)
                                                                            .Include(c=>c.Client)
                                                                            .Include(v=>v.VehicleRecordActivity)
                                                                            .Where(r => !r.Telegram || !r.Mail).ToListAsync(cancellationToken.Token);
                    foreach (var notificar in notificaciones)
                    {
                        var consulta = await context.RecordNotifications.SingleAsync(n=>n.Id.Equals(notificar.Id), cancellationToken: cancellationToken.Token);
                        if (!notificar.Telegram)
                        {
                            await Program.Bot.SendChatActionAsync(notificar.Client.Telegram, ChatAction.Typing, cancellationToken.Token);
                            if (notificar.VehicleRecordActivity.Latitud != null && notificar.VehicleRecordActivity.Longitud != null)
                                    await Program.Bot.SendVenueAsync(
                                        notificar.Client.Telegram,
                                   
                                        (float) notificar.VehicleRecordActivity.Latitud,
                                        (float) notificar.VehicleRecordActivity.Longitud,
                                        title: $"Registro Vehiculo:{notificar.Vehicle.Name} Km: {notificar.VehicleRecordActivity.Km}",
                                        address: "",
                                        cancellationToken: cancellationToken.Token);
                            consulta.Telegram = true;
                        }

                        if (!notificar.Mail)
                        {
                            var mensaje = new StringBuilder("<b>Registro de Actividad</b> ");
                            mensaje.Append("<br><br>");
                            mensaje.Append($"<b>Vehiculo:</b> {notificar.Vehicle.Name} registro el Km actual : <b>{notificar.VehicleRecordActivity.Km}<br>");
                            mensaje.Append($"<b>Latitud:</b> { notificar.VehicleRecordActivity.Latitud} <b>Longitud:</b> {notificar.VehicleRecordActivity.Longitud}<br>");
                            consulta.Mail = mensaje.ToString().SendMail(notificar.Client.Mail, "Registro de Km Actual");
                        }

                        await context.SaveChangesAsync(cancellationToken.Token);
                    }
                    var proxEnvio = 1;
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;

                    Console.WriteLine($"{DateTime.Now:s} - Termino envio de Telegram, prox envio en {proxEnvio} min");
                    Console.ResetColor();
                    Console.WriteLine(Environment.NewLine);
                    await Task.Delay(TimeSpan.FromMinutes(proxEnvio), cancellationToken.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error:EnvioAlertaTelegram - Run\n { ex.ToMessageAndCompleteStacktrace()}");

                }


            }



        }

        public async Task EnvioComponenteCambiar()
        {
            var cancellationToken = new CancellationTokenSource();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    _logger.LogInformation("Consulta a Base para proximo Cambio Componente");
                    Console.ResetColor();
                    await using var context = _context.CreateDbContext();
                    var componenteCambio = await context.ComponentNextChange.Include(v => v.Vehicle).Include(c=>c.Component).OrderBy(v=>v.Vehicle).ToListAsync(cancellationToken.Token);
                    var vehiculos = componenteCambio.Select(v => v.Vehicle).Distinct().ToList();
                    foreach (var vehiculo in vehiculos)
                    {
                        var listadoComponentesMail = new StringBuilder($"<b>Registro de Componentes a cambiar Vehiculo: {vehiculo.Name}</b> ");
                        listadoComponentesMail.Append("<br><br>");
                        var listadoComponentesTelegram = new StringBuilder($"<b>Registro de Componentes a cambiar Vehiculo: {vehiculo.Name}</b> ");
                        listadoComponentesTelegram.Append("\n");
                        var usuarios = await context.ClientEntityVehicle.Where(v => v.VehicleEntity.Id.Equals(vehiculo.Id)).Select(v=>v.ClientEntity).ToListAsync(cancellationToken: cancellationToken.Token);
                        var componentesNotificar = componenteCambio.Where(c => c.Vehicle.Id.Equals(vehiculo.Id)).ToList();
                        foreach (var listado in componentesNotificar)
                        {
                            listadoComponentesMail.Append($"Componente a Cambiar: <b>{listado.Component.Name}</b><br>");
                            listadoComponentesTelegram.Append($"Componente a Cambiar: <b>{listado.Component.Name}</b>\n");
                        }
                        foreach (var usuario in usuarios)
                        {

                            await Program.Bot.SendChatActionAsync(usuario.Telegram, ChatAction.Typing, cancellationToken.Token); 
                            await Program.Bot.SendTextMessageAsync(
                                                chatId:  usuario.Telegram,
                                                listadoComponentesTelegram.ToString(),
                                                parseMode: ParseMode.Html,
                                                disableNotification: true,
                                                cancellationToken: cancellationToken.Token);


                            listadoComponentesMail.ToString().SendMail(usuario.Mail, "Listado de Componentes a Cambiar");
                        }
                        await context.SaveChangesAsync(cancellationToken.Token);
                    }
                    var proxEnvio = 1;
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;

                    Console.WriteLine($"{DateTime.Now:s} - Termino envio de Telegram, prox envio en {proxEnvio} min");
                    Console.ResetColor();
                    Console.WriteLine(Environment.NewLine);
                    await Task.Delay(TimeSpan.FromMinutes(proxEnvio), cancellationToken.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error:EnvioAlertaTelegram - Run\n { ex.ToMessageAndCompleteStacktrace()}");

                }


            }



        }
    }
}
