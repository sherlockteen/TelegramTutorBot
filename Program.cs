using System.ComponentModel.Design;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;
using dotenv.net;


namespace telegram_Dictionary
{

    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        private static readonly Tutor _tutor = new();

        const string COMMAND_LIST =
            @"Список команд:
/add <eng> <rus> - добавить слово в словарь
/get - получить случайное слово из словаря
/check - проверяем правильность перевода английского слова";

        static async Task Main(string[] args)
        {
            DotEnv.Load();
            string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Ошибка: Не найден токен бота. Убедитесь, что переменная окружения TELEGRAM_BOT_TOKEN установлена.");
                return;
            }

            var _botClient = new TelegramBotClient(token);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            var me = await _botClient.GetMe();
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type != UpdateType.Message || update.Message?.Type != MessageType.Text)
                    return;

                var message = update.Message;
                var chat = message.Chat;

                if (message.Text == "/start")
                {
                    await botClient.SendMessage(chat.Id, COMMAND_LIST, cancellationToken: cancellationToken);
                    return;
                }

                if (message.Text.StartsWith("/add"))
                {
                    var partOfMessage = message.Text.Split(" ");
                    if (partOfMessage.Length == 3)
                    {
                        try
                        {
                            _tutor.AddWord(partOfMessage[1], partOfMessage[2]);
                            await botClient.SendMessage(chat.Id, $"Слово {partOfMessage[1]} - {partOfMessage[2]} добавлено.", cancellationToken: cancellationToken);
                        }
                        catch (FoundDublicateException)
                        {
                            await botClient.SendMessage(chat.Id, $"Слово {partOfMessage[1]} уже существует.", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.SendMessage(chat.Id, "Используй: /add <английское> <русское>", cancellationToken: cancellationToken);
                    }
                }
                else if (message.Text == "/get")
                {
                    await botClient.SendMessage(chat.Id, $"Переведите слово: {_tutor.GetRandomEngWord()}", cancellationToken: cancellationToken);
                }
                else if (message.Text.StartsWith("/check"))
                {
                    var partOfMessage = message.Text.Split(" ");
                    if (partOfMessage.Length == 3)
                    {
                        if (_tutor.CheckWord(partOfMessage[1], partOfMessage[2]))
                        {
                            await botClient.SendMessage(chat.Id, $"Правильно!", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            var translation = _tutor.Translate(partOfMessage[1]) ?? "не найдено";
                            await botClient.SendMessage(chat.Id, $"Неправильно! Правильный перевод: {translation}", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.SendMessage(chat.Id, "Используй: /check <английское> <русское>", cancellationToken: cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке апдейта: {ex}");
            }
        }



        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
