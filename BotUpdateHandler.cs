using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Carusel_1Bot
{
    public class BotUpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly RandomLib _randomLib = new RandomLib();
        private int _robertCounter = 0;
        private int _igorCounter = 0;
        private bool _robertStopper = false;

        private readonly Dictionary<BotCommand, (string TextCmd, string Callback, string Description)> _commands =
            new()
            {
                { BotCommand.Start,              ("/Start", "Start", "Старт бота") },
                { BotCommand.RandomCar,          ("/RandomCar", "RandomCar", "Выбор рандомной брички") },
                { BotCommand.RandomCoffee,       ("/RandomCoffee", "RandomCoffee", "Выбор рандомной кофейни") },
                { BotCommand.SmartRandomCoffee,  ("/SmartRandomCoffee", "SmartCoffee", "Выбор кофейни по весам") },
                { BotCommand.WhyAreYouGay,       ("/WhyAreYouGay", string.Empty, "Самая важная функция") },
                { BotCommand.RobertStopper,      ("/RobertStopper", "RobertStopper", "Буллинг Роберта") },
                { BotCommand.Help,               ("/help", "help", "Список команд") },
            };


        public BotUpdateHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        private async Task ExecuteCommand(BotCommand cmd, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var user = message.From;

            Console.WriteLine($"Get inline command or button\n" +
                $"DateTime -  {DateTime.Now}\n" +
                $"chatId -  {chatId}\n" +
                $"user -  {user}\n" +
                $"text -  {message.Text}\n" );
            try
            {

                switch (cmd)
                {
                    case BotCommand.Start:
                        await SendMenuAsync(chatId, ct);
                        break;

                    case BotCommand.RandomCar:
                        await HandleRandomCarAsync(user.Username, chatId, ct);
                        break;

                    case BotCommand.RandomCoffee:
                        await HandleRandomCoffeeAsync(chatId, ct);
                        break;

                    case BotCommand.SmartRandomCoffee:
                        await HandleSmartRandomCoffeeAsync(chatId, ct);
                        break;

                    case BotCommand.WhyAreYouGay:
                        await _botClient.SendTextMessageAsync(chatId,
                            $"Пользователь {user} на {new Random().Next(1000)}%",
                            cancellationToken: ct);
                        break;

                    case BotCommand.RobertStopper:
                        await ToggleRobertStopperAsync(user.Username, message, chatId, ct);
                        break;

                    case BotCommand.Help:
                        await _botClient.SendTextMessageAsync(chatId, GetHelpMessage(), cancellationToken: ct);
                        break;
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(chatId, ex.Message, cancellationToken: ct);
                Console.WriteLine(
                    $"Exc: {ex.GetType()}\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace:\n{ex.StackTrace}"
                );
            }
            
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackAsync(update.CallbackQuery!, cancellationToken);
                return;
            }

            if (update.Type != UpdateType.Message || update.Message?.Text == null)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;
            var user = message.From;

            if (user == null)
                return;

            await HandleCommandAsync(message, cancellationToken);
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка Telegram API: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(Message message, CancellationToken ct)
        {
            string text = message.Text.Split('@')[0]; // убираем @BotName

            var cmd = _commands.FirstOrDefault(p => p.Value.TextCmd == text);

            if (!cmd.Equals(default(KeyValuePair<BotCommand, (string, string, string)>)))
            {
                await ExecuteCommand(cmd.Key, message, ct);
                return;
            }

            // если нет — обработка особых юзеров
            await CheckSpecialUsersAsync(message.From.Username, message.Chat.Id, ct);
        }

        private async Task HandleRandomCarAsync(string username, long chatId, CancellationToken ct)
        {
            if (username == "robertsoon89")
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Пешеходу слово не давали",
                    cancellationToken: ct);
                return;
            }

            var rnd = new Random();
            var carName = _randomLib.Cars[rnd.Next(_randomLib.Cars.Count)];

            // Генерация полного пути
            var path = _randomLib.GetCarImagePath(carName);

            if (!System.IO.File.Exists(path))
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Файл не найден:\n{path}",
                    cancellationToken: ct);
                return;
            }

            await using var stream = System.IO.File.OpenRead(path);
            var file = InputFile.FromStream(stream, $"{carName}.jpg");

            await _botClient.SendPhotoAsync(
                chatId: chatId,
                photo: file,
                caption: $"Бричка сегодняшнего дня: {carName}",
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }

        private async Task HandleRandomCoffeeAsync(long chatId, CancellationToken ct)
        {
            var rnd = new Random();
            var coffee = _randomLib.Coffee[rnd.Next(_randomLib.Coffee.Count)];
            await _botClient.SendTextMessageAsync(
                chatId,
                $"Попьем говна в '{coffee}'",
                cancellationToken: ct);
        }

        private async Task HandleSmartRandomCoffeeAsync(long chatId, CancellationToken ct)
        {
            var coffee = GetSmartRandomCoffee();
            await _botClient.SendTextMessageAsync(
                chatId,
                $"Попьем говна в '{coffee}'",
                cancellationToken: ct);
        }

        private async Task ToggleRobertStopperAsync(string username, Message message, long chatId, CancellationToken ct)
        {
            string output;
            if (username == "robertsoon89")
            {
                output = "Хер там плавал.\nДанному кожанному мешку запрещена эта функция.";
                await _botClient.SendTextMessageAsync(
                    chatId,
                    output,
                    replyToMessageId: message.MessageId,
                    cancellationToken: ct);
            }
            else
            {
                _robertStopper = !_robertStopper;
                output = $"Режим буллинга Роберта {(_robertStopper ? "включен" : "выключен")}.";

                await _botClient.SendTextMessageAsync(
                    chatId,
                    output,
                    cancellationToken: ct);
            }
        }

        private async Task CheckSpecialUsersAsync(string username, long chatId, CancellationToken ct)
        {
            if (_robertStopper && username == "robertsoon89")
            {
                _robertCounter++;
                if (_robertCounter > 5)
                {
                    _robertCounter = 0;
                    for (int i = 0; i < 5; i++)
                        await _botClient.SendTextMessageAsync(chatId,
                            "Роберт, хватит писать сюда, всем похуй",
                            cancellationToken: ct);
                }
                else
                    await _botClient.SendTextMessageAsync(
                        chatId,
                        "Роберт Лох",
                        cancellationToken: ct);
            }

        }

        private async Task SendMenuAsync(long chatId, CancellationToken ct)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🚗 Рандом бричка", _commands[BotCommand.RandomCar].Callback),
                    InlineKeyboardButton.WithCallbackData("☕ Рандом кофе", _commands[BotCommand.RandomCoffee].Callback)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📊 Рандом кофе+", _commands[BotCommand.SmartRandomCoffee].Callback),
                    InlineKeyboardButton.WithCallbackData("🆘 Help", _commands[BotCommand.Help].Callback),
                },
            });

            await _botClient.SendTextMessageAsync(
                chatId,
                "Выберите действие:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }

        private async Task HandleCallbackAsync(CallbackQuery query, CancellationToken ct)
        {
            var chatId = query.Message.Chat.Id;

            var cmd = _commands.FirstOrDefault(p => p.Value.Callback == query.Data);

            if (!cmd.Equals(default(KeyValuePair<BotCommand, (string, string, string)>)))
            {
                // 1) Быстрый ответ на callback
                await _botClient.AnswerCallbackQueryAsync(query.Id, cancellationToken: ct);

                // 2) Выполняем команду асинхронно, чтобы не блокировать callback
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ExecuteCommand(cmd.Key, query.Message!, ct);
                    }
                    catch (Exception ex)
                    {
                        await _botClient.SendTextMessageAsync(
                            chatId,
                            $"Ошибка: {ex.Message}",
                            cancellationToken: ct);
                    }
                });
            }
        }



        private string GetSmartRandomCoffee()
        {
            int total = _randomLib.CoffeePercent.Values.Sum();
            int roll = new Random().Next(1, total + 1);
            int cumulative = 0;

            foreach (var item in _randomLib.CoffeePercent)
            {
                cumulative += item.Value;
                if (roll <= cumulative) return item.Key;
            }
            return _randomLib.CoffeePercent.Keys.Last();
        }

        private string GetHelpMessage()
        {
            return string.Join("\n\n",
                _commands.Select(c => $"{c.Value.TextCmd} — {c.Value.Description}"));
        }
    }
}