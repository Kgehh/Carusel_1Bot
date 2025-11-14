using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Carusel_1Bot
{
    internal class Program
    {
        private const long _caruselChatId = -194313227;
        static async Task Main()
        {

            string token = "8546477711:AAE9GiIydb19tR7riBBq3fv2MmsPo5l3mZ4";
            var botClient = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            // создаём обработчик, реализующий IUpdateHandler
            var updateHandler = new BotUpdateHandler(botClient);

            // теперь StartReceiving принимает только IUpdateHandler
            botClient.StartReceiving(
                updateHandler,
                receiverOptions,
                cts.Token);

            await botClient.SendTextMessageAsync(
                chatId: _caruselChatId,
                 text: "Я ожил, ебашьте"
            );

            var me = await botClient.GetMeAsync();

            await Task.Delay(-1, cts.Token);
        }
    }
}
