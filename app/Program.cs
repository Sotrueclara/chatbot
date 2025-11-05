// See https://aka.ms/new-console-template for more information
using System;
using System.Text.RegularExpressions;

//
// ======= CLASSES DE MENSAGENS =======
//

abstract class Message
{
    public string Text { get; protected set; }
    public DateTime SendDate { get; protected set; }

    protected Message(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("O texto da mensagem é obrigatório.");
        Text = text;
        SendDate = DateTime.Now;
    }

    public abstract string BuildPayloadPreview();
}

class TextMessage : Message
{
    public TextMessage(string text) : base(text) { }

    public override string BuildPayloadPreview()
        => $"[TEXTO] '{Text}' em {SendDate:dd/MM/yyyy HH:mm}";
}

abstract class MediaMessage : Message
{
    public string FilePath { get; protected set; }
    public string Format { get; protected set; }

    protected MediaMessage(string text, string filePath, string format)
        : base(text)
    {
        FilePath = filePath;
        Format = format.ToLower();
    }
}

class PhotoMessage : MediaMessage
{
    public PhotoMessage(string text, string filePath, string format)
        : base(text, filePath, format) { }

    public override string BuildPayloadPreview()
        => $"[FOTO] '{Text}' ({Format}) arquivo='{FilePath}' em {SendDate:HH:mm}";
}

class FileMessage : MediaMessage
{
    public FileMessage(string text, string filePath, string format)
        : base(text, filePath, format) { }

    public override string BuildPayloadPreview()
        => $"[ARQUIVO] '{Text}' ({Format}) arquivo='{FilePath}' em {SendDate:HH:mm}";
}

class VideoMessage : MediaMessage
{
    public TimeSpan Duration { get; private set; }

    public VideoMessage(string text, string filePath, string format, TimeSpan duration)
        : base(text, filePath, format)
    {
        Duration = duration;
    }

    public override string BuildPayloadPreview()
        => $"[VÍDEO] '{Text}' ({Format}, {Duration.TotalSeconds:0}s) arquivo='{FilePath}' em {SendDate:HH:mm}";
}

//
// ======= INTERFACE E CANAIS =======
//

interface IChannel
{
    string Name { get; }
    string Recipient { get; }
    void Send(Message message);
}

class WhatsAppChannel : IChannel
{
    public string Name => "WhatsApp";
    public string Recipient { get; }

    public WhatsAppChannel(string phoneNumber)
    {
        if (!Regex.IsMatch(phoneNumber ?? "", @"^\+?\d{10,15}$"))
            throw new ArgumentException("Número de telefone inválido.");
        Recipient = phoneNumber;
    }

    public void Send(Message message)
    {
        Console.WriteLine($"[{Name}] Enviando para {Recipient}: {message.BuildPayloadPreview()}");
    }
}

class TelegramChannel : IChannel
{
    public string Name => "Telegram";
    public string Recipient { get; }

    public TelegramChannel(string recipient)
    {
        bool isUser = Regex.IsMatch(recipient ?? "", @"^@[\w\d_]{3,}$");
        bool isPhone = Regex.IsMatch(recipient ?? "", @"^\+?\d{10,15}$");
        if (!isUser && !isPhone)
            throw new ArgumentException("Telegram aceita @usuario ou telefone.");
        Recipient = recipient;
    }

    public void Send(Message message)
    {
        Console.WriteLine($"[{Name}] Enviando para {Recipient}: {message.BuildPayloadPreview()}");
    }
}

class FacebookChannel : IChannel
{
    public string Name => "Facebook";
    public string Recipient { get; }

    public FacebookChannel(string username)
    {
        if (!Regex.IsMatch(username ?? "", @"^@?[\w\.]{3,}$"))
            throw new ArgumentException("Usuário inválido.");
        Recipient = username.StartsWith("@") ? username : "@" + username;
    }

    public void Send(Message message)
    {
        Console.WriteLine($"[{Name}] Enviando para {Recipient}: {message.BuildPayloadPreview()}");
    }
}

class InstagramChannel : IChannel
{
    public string Name => "Instagram";
    public string Recipient { get; }

    public InstagramChannel(string username)
    {
        if (!Regex.IsMatch(username ?? "", @"^@?[\w\.]{3,}$"))
            throw new ArgumentException("Usuário inválido.");
        Recipient = username.StartsWith("@") ? username : "@" + username;
    }

    public void Send(Message message)
    {
        Console.WriteLine($"[{Name}] Enviando para {Recipient}: {message.BuildPayloadPreview()}");
    }
}

//
// ======= FACTORY E ENUM =======
//

enum ChannelType
{
    WhatsApp,
    Telegram,
    Facebook,
    Instagram
}

static class ChannelFactory
{
    public static IChannel Create(ChannelType type, string recipient)
        => type switch
        {
            ChannelType.WhatsApp => new WhatsAppChannel(recipient),
            ChannelType.Telegram => new TelegramChannel(recipient),
            ChannelType.Facebook => new FacebookChannel(recipient),
            ChannelType.Instagram => new InstagramChannel(recipient),
            _ => throw new ArgumentException("Canal inválido.")
        };
}

//
// ======= PROGRAMA PRINCIPAL =======
//

class Program
{
    static void Main()
    {
        // Criação das mensagens
        Message m1 = new TextMessage("Olá, mundo!");
        Message m2 = new PhotoMessage("Foto do dia", "foto.jpg", "jpg");
        Message m3 = new FileMessage("Contrato", "contrato.pdf", "pdf");
        Message m4 = new VideoMessage("Trailer", "video.mp4", "mp4", TimeSpan.FromSeconds(15));

        // Criação dos canais
        IChannel whatsapp = ChannelFactory.Create(ChannelType.WhatsApp, "+5511999998888");
        IChannel telegram = ChannelFactory.Create(ChannelType.Telegram, "@clarinha");
        IChannel instagram = ChannelFactory.Create(ChannelType.Instagram, "clara.dev");
        IChannel facebook = ChannelFactory.Create(ChannelType.Facebook, "@clara.dev");

        // Envio das mensagens
        whatsapp.Send(m1);
        whatsapp.Send(m2);
        telegram.Send(m3);
        instagram.Send(m4);
        facebook.Send(m1);

        Console.WriteLine("\n✅ Envio de mensagens concluído com sucesso!");
    }
}

