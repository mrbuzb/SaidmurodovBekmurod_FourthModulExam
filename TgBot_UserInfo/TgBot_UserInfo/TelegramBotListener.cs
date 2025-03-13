using BII.Services;
using MyFirstEBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBot_UserInfo;

public class TelegramBotListener
{
    private static string BotToken = "8080848231:AAEb_ugGCMpKZ9hb-Z8UCnmw4Rf-XynV9Qw";

    private long AdminID = 5979013794;

    private TelegramBotClient BotClient = new TelegramBotClient(BotToken);

    private Dictionary<long, string> UserForUserInfo = new Dictionary<long, string>();


    private readonly IUserService _userService;

    public TelegramBotListener(IUserService userService)
    {
        _userService = userService;
    }

    private bool ValidateFNameAndLName(string name)
    {
        foreach (var l in name)
        {
            if (!char.IsLetter(l) || l == ' ')
            {
                return false;
            }
        }
        return !string.IsNullOrEmpty(name) && name.Length <= 50;
    }
    private bool ValidatePhone(string phone)
    {
        foreach (var l in phone)
        {
            if (!char.IsDigit(l) || l == ' ')
            {
                return false;
            }
        }
        return phone.Length == 9;
    }

    public async Task StartBot()
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message, UpdateType.InlineQuery } };

        Console.WriteLine("Your bot is starting");

        BotClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions
            );

        Console.ReadKey();
    }



    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {

            var message = update.Message;
            var user = message.Chat;
            BotUser userObject = null;
            try
            {
                userObject = await _userService.GetUserByID(user.Id);
            }
            catch (Exception ex)
            {
            }

            Console.WriteLine($"{user.Id},  {user.FirstName}, {message.Text}");

            if (message.Text == "Informatsiya Kiritish")
            {

                if (userObject.FirstName is null)
                {
                    await bot.SendTextMessageAsync(user.Id, "(Quyidagi Malumotlarni Kiriting) Isminggizni Kiriting : ", cancellationToken: cancellationToken);
                    UserForUserInfo.Add(userObject.TelegramUserId, "Ism");
                }
                else if (userObject.LastName is null)
                {
                    try
                    {
                        UserForUserInfo.Add(userObject.TelegramUserId, "Fam");
                    }
                    catch (Exception ex)
                    {
                        UserForUserInfo.Remove(userObject.TelegramUserId);
                        UserForUserInfo.Add(userObject.TelegramUserId, "Fam");
                    }
                    await bot.SendTextMessageAsync(user.Id, "Familiyanggizni Kiriting : ", cancellationToken: cancellationToken);
                }
                else if(userObject.PhoneNumberr is null)
                {
                    try
                    {
                        UserForUserInfo.Add(userObject.TelegramUserId, "Pho");
                    }
                    catch (Exception ex)
                    {
                        UserForUserInfo.Remove(userObject.TelegramUserId);
                        UserForUserInfo.Add(userObject.TelegramUserId, "Pho");
                    }
                    await bot.SendTextMessageAsync(user.Id, "Telefon raqamni kiriting (909009090 formatida):", cancellationToken: cancellationToken);
                }
                else
                {
                    var str = $"FirstName : {userObject.FirstName}\n" +
                        $"LastName : {userObject.LastName}\n" +
                        $"PhoneNumber : {userObject.PhoneNumberr}\n" +
                        $"TelegramID : {userObject.TelegramUserId}";
                    await bot.SendTextMessageAsync(user.Id, $"Sizda Hamma Informatsiyalar Kiritilgan !!\n\n{str}!", cancellationToken: cancellationToken);

                }
            }
            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Ism")
            {
                var validate = ValidateFNameAndLName(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Isminggizni To'g'ri Kiriting !!!", cancellationToken: cancellationToken);
                    return;
                }
                userObject.FirstName = message.Text;
                var ch = userObject.FirstName[0];
                userObject.FirstName = userObject.FirstName.Remove(0, 1);
                userObject.FirstName = char.ToUpper(ch) + userObject.FirstName;
                await _userService.UpdateUser(userObject);
                if (userObject.LastName is null)
                {
                    UserForUserInfo[user.Id] = "Fam";
                    await bot.SendTextMessageAsync(user.Id, "Familiyanggizni Kiriting : ", cancellationToken: cancellationToken);
                }
                else if(userObject.PhoneNumberr is null)
                {
                    UserForUserInfo[user.Id] = "Pho";
                    await bot.SendTextMessageAsync(user.Id, "Telefon raqamni kiriting (909009090 formatida):", cancellationToken: cancellationToken);
                }
                else
                {
                    UserForUserInfo.Remove(user.Id);
                }
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Fam")
            {
                var validate = ValidateFNameAndLName(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Familiyanggizni To'g'ri Kiriting !!!", cancellationToken: cancellationToken);
                    return;
                }
                userObject.LastName = message.Text;
                var ch = userObject.LastName[0];
                userObject.LastName = userObject.LastName.Remove(0, 1);
                userObject.LastName = char.ToUpper(ch) + userObject.LastName;
                await _userService.UpdateUser(userObject);
                if (userObject.PhoneNumberr is null)
                {

                    UserForUserInfo[user.Id] = "Pho";
                    await bot.SendTextMessageAsync(user.Id, "Telefon raqamni kiriting (909009090 formatida):", cancellationToken: cancellationToken);
                }
                else
                {
                    UserForUserInfo.Remove(user.Id);
                }
            }
            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Pho")
            {
                var validate = ValidatePhone(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Telefon Nomerni To'g'ri Kiriting !!!", cancellationToken: cancellationToken);
                    return;
                }
                userObject.PhoneNumberr = message.Text;
                userObject.PhoneNumberr = "+998" + userObject.PhoneNumberr;
                UserForUserInfo.Remove(user.Id);
                await _userService.UpdateUser(userObject);

                var str = $"FirstName : {userObject.FirstName}\n" +
    $"LastName : {userObject.LastName}\n" +
    $"PhoneNumber : {userObject.PhoneNumberr}\n" +
    $"TelegramID : {userObject.TelegramUserId}";
                await bot.SendTextMessageAsync(user.Id, $"Informatsiyalar Saqlandi :\n\n{str} ", cancellationToken: cancellationToken);
            }




            if (message.Text == "Informatsiyani Ko'rish")
            {
                if (userObject.FirstName is null)
                {
                    await bot.SendTextMessageAsync(user.Id, "(Quyidagi Malumotlarni Kiriting) Isminggizni Kiriting : ", cancellationToken: cancellationToken);
                    UserForUserInfo.Add(userObject.TelegramUserId, "Ism");
                }
                else if (userObject.LastName is null)
                {
                    try
                    {
                        UserForUserInfo.Add(userObject.TelegramUserId, "Fam");
                    }
                    catch (Exception ex)
                    {
                        UserForUserInfo.Remove(userObject.TelegramUserId);
                        UserForUserInfo.Add(userObject.TelegramUserId, "Fam");
                    }
                    await bot.SendTextMessageAsync(user.Id, "Familiyanggizni Kiriting : ", cancellationToken: cancellationToken);
                }
                else if (userObject.PhoneNumberr is null)
                {
                    try
                    {
                        UserForUserInfo.Add(userObject.TelegramUserId, "Pho");
                    }
                    catch (Exception ex)
                    {
                        UserForUserInfo.Remove(userObject.TelegramUserId);
                        UserForUserInfo.Add(userObject.TelegramUserId, "Pho");
                    }
                    await bot.SendTextMessageAsync(user.Id, "Telefon raqamni kiriting (909009090 formatida):", cancellationToken: cancellationToken);
                }
                else
                {

                    var str = $"FirstName : {userObject.FirstName}\n" +
        $"LastName : {userObject.LastName}\n" +
        $"PhoneNumber : {userObject.PhoneNumberr}\n" +
        $"TelegramID : {userObject.TelegramUserId}";
                    await bot.SendTextMessageAsync(user.Id, $"Your Informations :\n\n{str} ", cancellationToken: cancellationToken);
                }

            }

            if (message.Text.StartsWith("Send All Users : "))
            {
                if (user.Id == AdminID)
                {
                    var users = await _userService.GetAllUser();

                    var str = message.Text;
                    str = str.Remove(0, 17);

                    foreach (var u in users)
                    {
                        if (u.TelegramUserId == AdminID)
                        {
                            continue;
                        }
                        await bot.SendTextMessageAsync(u.TelegramUserId, str, cancellationToken: cancellationToken);
                    }
                }
            }


            if (message.Text == "Informatsiyani O'Chirish")
            {
                _userService.DeleteUser(user.Id);
                await bot.SendTextMessageAsync(user.Id,"Informatsiyalar O'chirildi", cancellationToken: cancellationToken);
            }

            if (message.Text == "/start")
            {

                if (userObject == null)
                {
                    userObject = new BotUser()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumberr = null,
                        TelegramUserId = user.Id,
                    };
                    await _userService.AddUser(userObject);
                }

                var keyboard = new ReplyKeyboardMarkup(new[]
            {
                    new[]
                    {
                        new KeyboardButton("Informatsiya Kiritish"),
                        new KeyboardButton("Informatsiyani Ko'rish"),
                    },
                    new[]
                    {
                        new KeyboardButton("Informatsiyani O'Chirish"),
                    },
                })
                { ResizeKeyboard = true };

                await bot.SendTextMessageAsync(user.Id, "Assalomu Alaykum 👋", replyMarkup: keyboard);
                return;
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var id = update.CallbackQuery.From.Id;

            var text = update.CallbackQuery.Data;

            CallbackQuery res = update.CallbackQuery;
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
    }
}
