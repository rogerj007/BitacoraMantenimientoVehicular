using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Bot.Helpers;
using BitacoraMantenimientoVehicular.Datasource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using ILogger = Serilog.ILogger;

namespace BitacoraMantenimientoVehicular.Bot
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        public static ITelegramBotClient Bot;
        private static IServiceProvider _serviceProvider;
        private static Logger _serilogLogger;
        static void Main(string[] args)
        {
            try
            {
                MainAsync(args).Wait();
                //return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Error running service");
                Console.WriteLine(exception.ToMessageAndCompleteStacktrace());
            }
        }

        private static async Task MainAsync(string[] args)
        {
            using (var p = Process.GetCurrentProcess())
                p.PriorityClass = ProcessPriorityClass.RealTime;
            const string mutexName = @"TelegramBotBitacora";
            Thread.CurrentThread.Name ??= mutexName;
            var mutex = new Mutex(true, mutexName, out var createdNew);
            if (!createdNew)
            {
                Log.Logger.Warning(mutexName + " ya esta corriendo! Saliendo de la aplicación.");
                Thread.Sleep(3000);
                GC.Collect();
                return;
            }

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            _serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
              //  .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            var cts = new CancellationTokenSource();
            var ctsValidacionUsuario = new CancellationTokenSource();
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, _serilogLogger);
            _serilogLogger.Information("Software desarrollado por Roger Jaimes H.");
            // Create service provider
            _serilogLogger.Information("Building service provider Bot Telegram Cero.");
            _serviceProvider = serviceCollection.BuildServiceProvider();
            try
            {

#if USE_PROXY
                            var Proxy =
 new WebProxy(Configuration.Proxy.Host, Configuration.Proxy.Port) { UseDefaultCredentials =
                 true };
                            _bot = new TelegramBotClient(Configuration.BotToken, webProxy: Proxy);
#else
                var httpClient = new HttpClient { Timeout = new TimeSpan(0, 30, 0) };
                // 5 min
                Bot = new TelegramBotClient(_configuration.GetSection("BotToken").Value, httpClient);
#endif

                var me = await Bot.GetMeAsync(cts.Token);
               
                Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cts.Token);
                var envioAlertaTelegram = _serviceProvider.GetService<EnvioAlertaTelegram>();
                _serilogLogger.Information("Inicio de Envio Alertas Telegram.");
                envioAlertaTelegram?.EnvioRegistroKmActual();
                envioAlertaTelegram?.EnvioComponenteCambiar();
                Console.Title = $"Start listening for Id:{me.Id} @{me.Username} CanJoinGroups:{me.CanJoinGroups}";
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                _serilogLogger.Fatal(ex.ToMessageAndCompleteStacktrace());
                Console.ReadLine();
            }
            finally
            {
                // Send cancellation request to stop bot
                // cts.Cancel();
                //ctsValidacionUsuario.Cancel();
                //ctsConsultaNotasAlertasTelegram.Cancel();
                //ctsConsultaMediosDigitales.Cancel();
                _serilogLogger.Information("Ending service Bitacora Vehiculos...");

            }
        }



        private static void ConfigureServices(IServiceCollection serviceCollection, ILogger serilogLogger)
        {


            serviceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(serilogLogger, true);
            });

            //Add AddDbContext
            serviceCollection.AddDbContextFactory<DataContext>(options =>
            {
                options.EnableSensitiveDataLogging(true);
                options.UseSqlServer(_configuration.GetConnectionString("DefaultConnectionV2"), providerOptions =>
                {
                    providerOptions.EnableRetryOnFailure();
                    
                    providerOptions.CommandTimeout(360);
                });
            });

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(_configuration);
            // Add app
            serviceCollection.AddScoped<EnvioAlertaTelegram>();
            serviceCollection.AddScoped<ConsultaCliente>();

        }


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {

                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),

                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            if (message == null) return;
            _serilogLogger.Warning($"Receive message type: {message.Type} - From: User: {message.From.Id} {message.From.FirstName} {message.From.LastName}");



            switch (message.Type)
            {
                default:
                {
                    if (message.Type == MessageType.Location)
                    {
                        await UpdateLocationVehicle(message);
                    }
                    if (message.Type != MessageType.Text)
                        return;

                    var action = (message.Text.ToLower().Split(' ').First()) switch
                    {
                        "/start" => SendWelcomeMessages(message),
                        "/help" => SendHelp(message),
                        "/infouser" => SendInfoUser(message),
                        "/registro" => UpdateRecordVehicle(message),
                        "/mantenimiento" => ExecuteVehicle(message),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    await action;

                      

                    #region Mensajes de Texto


                    static async Task SendWelcomeMessages(Message message)
                    {
                        var inicioMensaje = DateTime.Now.Hour switch
                        {
                            >= 0 and < 12 => "Hola buen dia",
                            > 12 and <= 18 => "Hola buenas tardes",
                            > 18 and < 23 => "Hola buenas noches",
                            _ => string.Empty
                        };

                        var mensajeBase = $"{inicioMensaje}, <b>{message.From.FirstName} {message.From.LastName}</b>";
                        await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: mensajeBase,
                            parseMode: ParseMode.Html,
                            replyMarkup: new ReplyKeyboardRemove()
                        );
                    }
                  
                    static async Task SendInfoUser(Message message)
                    {
                        try
                        {
                            await Bot.SendTextMessageAsync(
                                message.Chat.Id,
                                $"Id chat:{message.Chat.Id} - IdUsuario: <b>{message.From.Id}</b> " +
                                $"Nombre Usuario: {message.From.FirstName} {message.From.LastName}" +
                                $"UsuarioTelegram: {message.From.Username}",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove()
                            );
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error($"Error {ex.Message}");
                        }

                    }

                    static async Task UpdateRecordVehicle(Message message)
                    {
                        var inicioMensaje = DateTime.Now.Hour switch
                        {
                            >= 0 and < 12 => "Hola buen dia",
                            >= 12 and <= 18 => "Hola buenas tardes",
                            > 18 and <= 23 => "Hola buenas noches",
                            _ => string.Empty
                        };
                            
                        var mensajeBase = $"{inicioMensaje}, <b>{message.From.FirstName} {message.From.LastName}</b>";
                        _serilogLogger.Information(message.Text);
                        var mensajeTelegram = message.Text.ToLower().Replace("/registro ", "").ToLower().Split(' ');
                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                        await Task.Delay(500);
                        if (mensajeTelegram.Length != 2)
                        {
                            var mensaje =
                                $"{mensajeBase} escribe bien los parametros\nEjemplo: <b>/registro placa KM (ultimo) </b>";
                            await Bot.SendTextMessageAsync(
                                message.Chat.Id,
                                mensaje,
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove()
                            );

                        }
                        else
                        {
                            var placa = mensajeTelegram[0];
                            if (!mensajeTelegram[1].IsInt())
                            {
                                await Bot.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "\U0001F5F7 Km NO valido!!!",
                                    replyMarkup: new ReplyKeyboardRemove()
                                );
                                return;
                                
                            }
                            var km = Convert.ToInt64(mensajeTelegram[1]);
                            if (km > 600)
                            {
                                    InlineKeyboardMarkup rmu = new(new[]
                                    {
                                        // first row
                                        new []
                                        {
                                            InlineKeyboardButton.WithCallbackData("\U0001F525 Si, es correcto!", $"Correcto|{placa}|{km}"),
                                            InlineKeyboardButton.WithCallbackData("\U0001F61E No, el valor es equivocado!", $"Equivocado"),
                                        },
                                    });
                                    
                                    var mensaje = $"\U0001F44B Hola {message.From.FirstName} , bienvenido a nuestro sistema, el valor registrado {km} es correcto?";

                                    await Bot.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: mensaje,
                                    replyMarkup: rmu
                                    );
                                   // Bot.SendTextMessageAsync(e.Message.Chat.Id, message, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, rmu, System.Threading.CancellationToken.None);

                            }

                            
                        }
                    }

                    static async Task UpdateLocationVehicle(Message message)
                    {
                          
                        var consulta = _serviceProvider.GetService<ConsultaCliente>();

                        if (consulta != null)
                        {
                          var mensaje= await consulta.RegistroUbicacionAsync(message);
                           await Bot.SendTextMessageAsync(
                               chatId: message.Chat.Id,
                               text: mensaje,
                               parseMode: ParseMode.Html,
                               replyMarkup: new ReplyKeyboardRemove()
                           );
                        }
                        
                    }

                    //Ya no necesita
                    static async Task ExecuteVehicle(Message message)
                    {
                            var inicioMensaje = DateTime.Now.Hour switch
                            {
                                >= 0 and < 12 => "Hola buen dia",
                                >= 12 and <= 18 => "Hola buenas tardes",
                                > 18 and <= 23 => "Hola buenas noches",
                                _ => string.Empty
                            };

                            var mensajeBase = $"{inicioMensaje}, <b>{message.From.FirstName} {message.From.LastName}</b>";
                            _serilogLogger.Information(message.Text);
                            var mensajeTelegram = message.Text.ToLower().Replace("/mantenimiento ", "").ToLower().Split(' ');
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                            await Task.Delay(500);
                            if (mensajeTelegram.Length != 2)
                            {
                                var mensaje =
                                    $"{mensajeBase} escribe bien los parametros\nEjemplo: <b>/Mantenimiento Placa</b>";
                                await Bot.SendTextMessageAsync(
                                    message.Chat.Id,
                                    mensaje,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove()
                                );

                            }
                            else
                            {
                                var placa = mensajeTelegram[0];
                                var consulta = _serviceProvider.GetService<ConsultaCliente>();
                                if (consulta != null)
                                {
                                    var mensaje = await consulta.MantenimientoEjecutadoAsync(placa);
                                   await Bot.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: mensaje,
                                            replyMarkup: new ReplyKeyboardRemove());
                                }
                            }
                    }


                    static async Task SendHelp(Message message)
                    {
                        const string usage = "Lista de Comandos:\n" +
                                             "<b>/help</b>   - Listado de Comandos\n" +
                                             "<b>/infoUser</b> - Informacion del Usuario\n" +
                                             "<b>/Registro</b> - Registro de Km del vehiculo\n";
                        await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: usage,
                            parseMode: ParseMode.Html,
                            replyMarkup: new ReplyKeyboardRemove()
                        );
                    }

                    #endregion

                    break;
                    }
            }
        }

        

        private static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            //TODO: REGISTRO KM
            var mensajeCallBack = callbackQuery.Data.Split('|');
            var consulta = _serviceProvider.GetService<ConsultaCliente>();
            var placa = Convert.ToString(mensajeCallBack[1]);
            var km = Convert.ToInt64(mensajeCallBack[2]);

            switch (mensajeCallBack[0])
            {
                case "Correcto":
                   
                    if (consulta != null)
                    {
                      
                        var mensaje = await consulta.RegistroActividadAsync(placa, km, callbackQuery.Message.Chat.Id);
                        if (!mensaje.Item1)
                        {
                            await Bot.SendTextMessageAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: mensaje.Item2,
                                replyMarkup: new ReplyKeyboardRemove()
                            );
                        }
                        else
                        {
                            var requestReplyKeyboard =
                                new ReplyKeyboardMarkup(
                                    new[]
                                    {
                                        KeyboardButton.WithRequestLocation(
                                            @$"Enviar Ubicacion para registro")
                                    }, true)
                                { ResizeKeyboard = true };

                            await Bot.SendTextMessageAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "Enviar Ubicacion",
                                replyMarkup: requestReplyKeyboard
                            );
                        }

                    }
                    break;
                case "Equivocado":
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Ok, vuelve a realizar el registro"
                    );
                    break;
                case "MantenimientoEjecutado":
                    if (consulta != null)
                    {
                        var mensaje = await consulta.MantenimientoEjecutadoAsync(placa);
                        await Bot.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: mensaje,
                            replyMarkup: new ReplyKeyboardRemove());

                    }
                    break;
                case "MantenimientoNoEjecutado":
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"No se puede realizar ningun registo, porque existe un mantenimiento pendiente"
                    );
                    break;
            }

            
        }

        #region Inline Mode

        private static async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            _serilogLogger.Warning($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "1",
                    title: "Saludo",
                    inputMessageContent: new InputTextMessageContent(
                        "Hola mucho gusto, cualquier cosa me escriben..."
                    )
                ), new InlineQueryResultArticle(
                    id: "2",
                    title: "Saludos",
                    inputMessageContent: new InputTextMessageContent(
                        "Que novedad..."
                    )
                )
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private static async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            Log.Logger.Warning($"Received inline result: {chosenInlineResult.ResultId}");
        }

        #endregion

        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Log.Logger.Fatal($"Unknown update type: {update.Type}");
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log.Logger.Fatal($"EjecutarAsignacionPersonajes\n{errorMessage}");
        }

    }
}
