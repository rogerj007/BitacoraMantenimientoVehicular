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
        public async Task EnvioRegistro()
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
                            consulta.Mail = true;
                        }

                        await context.SaveChangesAsync(cancellationToken.Token);
                    }
                    //var componentesNc = await context.ComponentNextChange.Where(c=>!c.IsComplete).AsNoTracking().ToListAsync(cancellationToken.Token);

                    //foreach (var vehicle in componentesNc.Select(o => o.Vehicle).Distinct())
                    //{
                    //    foreach (var componente in componentesNc.Where(c=>c.Vehicle==vehicle))
                    //    {

                    //    }
                    //}
                   




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
