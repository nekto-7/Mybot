using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;   
using System;
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

        var token = " . . "; // Замените на ваш токен 
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        if (message is SocketUserMessage userMessage && !userMessage.Author.IsBot)
        {
            if (userMessage.Content.StartsWith("/play ", StringComparison.OrdinalIgnoreCase))
            {
                var url = userMessage.Content.Substring(6).Trim(); // Получаем URL после команды /play
                var user = message.Author as SocketGuildUser;
                var userVoiceChannel = user?.VoiceChannel;

                if (userVoiceChannel == null)
                {
                    await message.Channel.SendMessageAsync("Вы должны быть в голосовом канале, чтобы я мог воспроизвести музыку.");
                    return;
                }

                await JoinVoiceChannel(userVoiceChannel);
                await PlayAudioAsync(url);
            }
            else if (userMessage.Content.Equals("/stop", StringComparison.OrdinalIgnoreCase))
            {
                await StopAudioAsync();
            }
        }
    }

    private async Task JoinVoiceChannel(SocketVoiceChannel voiceChannel)
    {
        if (_audioClient != null && _audioClient.ConnectionState == ConnectionState.Connected)
        {
            Console.WriteLine($"Бот уже подключен к голосовому каналу на сервере {voiceChannel.Guild.Name}.");
            return;
        }

        try
        {
            Console.WriteLine($"Подключение к голосовому каналу: {voiceChannel.Name} на сервере {voiceChannel.Guild.Name}");
            _audioClient = await voiceChannel.ConnectAsync();
            Console.WriteLine($"Бот успешно подключен к голосовому каналу на сервере {voiceChannel.Guild.Name}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при подключении к голосовому каналу: {ex.Message}");
        }
    }

    private async Task PlayAudioAsync(string url)
    {
        // Проверка на наличие _audioClient
        if (_audioClient == null || _audioClient.ConnectionState != ConnectionState.Connected)
        {
            Console.WriteLine("Бот не подключен к голосовому каналу.");
            return;
        }

        // Пример загрузки аудиофайла. Замените на свой способ получения аудиоданных.
        var audioFilePath = @"E:\ds\Авэ.mp3";

        if (!File.Exists(audioFilePath))
        {
            Console.WriteLine("Аудиофайл не найден!");
            return;
        }

        try
        {
            using (var audioStream = new AudioFileReader(audioFilePath))
            using (var outputStream = _audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                await Task.Run(() =>
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = audioStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                });
            }
            
            Console.WriteLine($"Воспроизведение аудио с URL: {url}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при воспроизведении аудио: {ex.Message}");
        }
    }


    private async Task StopAudioAsync()
    {
        if (_audioClient != null && _audioClient.ConnectionState == ConnectionState.Connected)
        {
            await _audioClient.StopAsync();
            _audioClient = null; // Отключаем аудиоклиент
            Console.WriteLine("Бот отключился от голосового канала.");
        }
    }

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
} вот сдесь нужно скрыть токен от git