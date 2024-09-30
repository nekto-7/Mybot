using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private DiscordSocketClient _client;
    private IAudioClient _audioClient;

    public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    public async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
        };

        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        _client.MessageReceived += HandleCommandAsync;

        var token = " "; // Замените на ваш токен

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }
// FF
    private async Task HandleCommandAsync(SocketMessage message)
    {
        if (message is SocketUserMessage userMessage && !userMessage.Author.IsBot)
        {
            var command = userMessage.Content.Trim().ToLower();

            if (command.StartsWith("/play"))
            {
                var voiceChannel = (message.Author as IGuildUser)?.VoiceChannel;
                if (voiceChannel != null)
                {
                    try
                    {
                        Console.WriteLine($"Попытка подключения к каналу {voiceChannel.Name} (ID: {voiceChannel.Id})...");
                        
                        // Подключаемся без флагов selfMute и selfDeaf
                        _audioClient = await voiceChannel.ConnectAsync();

                        await Task.Delay(2000); // Задержка в 2 секунды перед воспроизведением
                        await message.Channel.SendMessageAsync("Подключился к голосовому каналу!");

                        // Воспроизводим указанную песню
                        await PlayMusic("C:/Users/Slevin/Desktop/Треш Виталика/1.mp3");
                    }
                    catch (TimeoutException ex)
                    {
                        Console.WriteLine($"Тайм-аут подключения к каналу: {ex.Message}");
                        await message.Channel.SendMessageAsync("Не удалось подключиться к голосовому каналу из-за тайм-аута.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка подключения к каналу: {ex.Message}");
                        await message.Channel.SendMessageAsync("Не удалось подключиться к голосовому каналу.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("Вы должны находиться в голосовом канале.");
                }

            }

            if (command.StartsWith("/stop"))
            {
                if (_audioClient != null)
                {
                    await _audioClient.StopAsync();
                    await message.Channel.SendMessageAsync("Отключился от голосового канала.");
                }
            }
        }
    }

    private async Task PlayMusic(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Файл не найден: {filePath}");
            return;
        }

        Console.WriteLine($"Воспроизведение файла: {filePath}");

        var ffmpeg = CreateProcess(filePath);
        var output = ffmpeg.StandardOutput.BaseStream;
        var discord = _audioClient.CreatePCMStream(AudioApplication.Music);

        try
        {
            Console.WriteLine("Начало передачи аудио...");
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
            Console.WriteLine("Аудио успешно воспроизведено.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка во время воспроизведения: {ex.Message}");
        }
        finally
        {
            await discord.DisposeAsync();
            ffmpeg.Dispose();
        }
    }

    private Process CreateProcess(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
    }

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}
