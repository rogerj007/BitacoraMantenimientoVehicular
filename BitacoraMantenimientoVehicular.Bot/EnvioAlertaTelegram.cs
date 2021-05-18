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
        public async Task Run()
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
                    var componentesNc = await context.ComponentNextChange.Where(c=>!c.IsComplete).AsNoTracking().ToListAsync(cancellationToken.Token);

                    foreach (var vehicle in componentesNc.Select(o => o.Vehicle).Distinct())
                    {
                        foreach (var componente in componentesNc.Where(c=>c.Vehicle==vehicle))
                        {

                        }
                    }
                   




                    var proxEnvio = 15;
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
