using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Bot.Helpers;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Enums;
using DevExpress.Office.Utils;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

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
                    var componenteCambio = await context.ComponentNextChange.Include(v => v.Vehicle).Include(c=>c.Component).Where(c=>!c.IsComplete).OrderBy(v=>v.Vehicle).ToListAsync(cancellationToken.Token);
                    var vehiculos = componenteCambio.Select(v => v.Vehicle).Distinct().ToList();
                    foreach (var vehiculo in vehiculos)
                    {
                        var listadoComponentesMail = new StringBuilder($"<b>Registro de Componentes a cambiar Vehiculo: {vehiculo.Name}</b> ");
                        listadoComponentesMail.Append("<br><br>");
                        var listadoComponentesTelegram = new StringBuilder($"<b>Registro de Componentes a cambiar Vehiculo: {vehiculo.Name}</b> ");
                        listadoComponentesTelegram.Append("\n");
                        var usuarios = await context.ClientEntityVehicle.Where(v => v.VehicleEntity.Id.Equals(vehiculo.Id)).Select(v=>v.ClientEntity).ToListAsync(cancellationToken: cancellationToken.Token);
                        if (usuarios == null || usuarios.Count==0)
                        {
                            _logger.LogCritical($"Vehiculo {vehiculo.Name} no tiene ningun usuario asignado!!!");
                            return;
                        }
                        var componentesNotificar = componenteCambio.Where(c => c.Vehicle.Id.Equals(vehiculo.Id)).ToList();

                        var dueno = usuarios.SingleOrDefault(d => d.UserType == UserType.Owner);

                        if (dueno == null)
                        {
                            _logger.LogCritical($"Vehiculo {vehiculo.Name} no tiene dueño!!!");
                            return;
                        }
                       
                        using var srv = new RichEditDocumentServer();
                        var doc = srv.Document;

                        var cp = doc.BeginUpdateCharacters(doc.Paragraphs[0].Range);
                        cp.ForeColor = System.Drawing.Color.FromArgb(0x83, 0x92, 0x96);
                        cp.Italic = true;
                        doc.EndUpdateCharacters(cp);
                        var pp = doc.BeginUpdateParagraphs(doc.Paragraphs[0].Range);
                        pp.Alignment = ParagraphAlignment.Right;
                        doc.EndUpdateParagraphs(pp);

                        var tbl = doc.Tables.Create(doc.Range.Start, 1, 5, AutoFitBehaviorType.AutoFitToWindow);
                      
                        // Create a table header.
                        doc.InsertText(tbl[0, 0].Range.Start, "Nro");
                        doc.InsertText(tbl[0, 1].Range.Start, "Nombre");
                        doc.InsertText(tbl[0, 2].Range.Start, "Cambio cada KM");
                        doc.InsertText(tbl[0, 3].Range.Start, "Revisado");
                        doc.InsertText(tbl[0, 4].Range.Start, "Observacion");
                        //Set the width of the first column
                        var table = doc.Tables[0];
                        table.BeginUpdate();
                        table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
                        table.Rows[0].FirstCell.PreferredWidth = Units.InchesToDocumentsF(0.8f);
                        table.EndUpdate();
                        var i = 1;
                        foreach (var listado in componentesNotificar)
                        {
                            listadoComponentesMail.Append($"Componente a Cambiar: <b>{listado.Component.Name}</b><br>");
                            listadoComponentesTelegram.Append($"Componente a Cambiar: <b>{listado.Component.Name}</b>\n");
                            var row = tbl.Rows.Append();
                            var cell = row.FirstCell;
                            doc.InsertSingleLineText(cell.Range.Start, i.ToString());
                            doc.InsertSingleLineText(cell.Next.Range.Start, listado.Component.Name);
                            doc.InsertSingleLineText(cell.Next.Next.Range.Start, listado.Component.Ttl.ToString());
                            doc.InsertSingleLineText(cell.Next.Next.Next.Range.Start, string.Empty);
                            doc.InsertSingleLineText(cell.Next.Next.Next.Next.Range.Start, string.Empty);
                            i++;
                        }

                        doc.AppendText($"Dueño: {dueno.Name} Ceular: {dueno.CellPhone} {Environment.NewLine}");
                        doc.AppendText($"Plan de Mantenimiento Vehiculo: {vehiculo.Name} Km Actual: {vehiculo.KmActual}");
                        // Insert an image using its URI.

                        var docHeader = doc.Sections[0].BeginUpdateHeader();
                        docHeader.Images.Append(DocumentImageSource.FromFile("LogoFinal.png"));
                        doc.Sections[0].EndUpdateHeader(docHeader);
                        foreach (var p in doc.Paragraphs.Get(tbl.FirstRow.Range)) p.Alignment = ParagraphAlignment.Center;
                        var fileName = $"{Guid.NewGuid()}-{vehiculo.Name}.pdf";
                        srv.ExportToPdf(fileName);

                        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        Thread.Sleep(TimeSpan.FromSeconds(15));
                        foreach (var usuario in usuarios)
                        {

                            await Program.Bot.SendChatActionAsync(usuario.Telegram, ChatAction.Typing, cancellationToken.Token); 
                            await Program.Bot?.SendDocumentAsync(
                                                chatId:  usuario.Telegram,
                                                document: new InputOnlineFile(fileStream, $"{vehiculo.Name}.pdf"),
                                                caption:"Listado Componentes a Cambiar",
                                                parseMode: ParseMode.Html,
                                                disableNotification: true,
                                                cancellationToken: cancellationToken.Token);

                            Thread.Sleep(TimeSpan.FromSeconds(15));
                            listadoComponentesMail.ToString().SendMail(usuario.Mail, "Listado de Componentes a Cambiar", fileName);
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
