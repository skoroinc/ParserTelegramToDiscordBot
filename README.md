
# ParserTelegramToDiscordBot

## Описание

Этот бот парсит сообщения из Telegram-бота и пересылает их в Discord-канал. Он позволяет синхронизировать сообщения с указанного Telegram-бота и отправлять их в выбранный Discord-канал в реальном времени.

## Возможности

- Парсинг сообщений из Telegram-бота.
- Пересылка сообщений в Discord-канал.
- Легкая настройка через файл `JSON`.

## Требования

Перед запуском бота вам понадобятся:

- .NET 6.0 или новее.
- Токен Telegram-бота.
- Токен Discord-бота.
- ID канала в Discord для отправки сообщений.

## Настройка

Настройте бота в файле `appsettings.json`:

```json
{
  "TelegramToken": "your-telegram-bot-token",
  "DiscordToken": "your-discord-bot-token",
  "DiscordChannelId": 123456789012345678,
  "MaxFileSize": 52428800
}
```

Замените `your-telegram-bot-token` и `your-discord-bot-token` на реальные токены для ваших ботов в Telegram и Discord соответственно. Укажите `DiscordChannelId` для ID канала в Discord.

## Установка

1. Клонируйте репозиторий:

```bash
git clone https://github.com/skoroinc/ParserTelegramToDiscordBot.git
```

2. Перейдите в каталог проекта и восстановите зависимости:

```bash
cd ParserTelegramToDiscordBot
dotnet restore
```

3. Постройте проект:

```bash
dotnet build
```

4. Запустите бота:

```bash
dotnet run
```

## Лицензия

MIT License. См. [LICENSE](LICENSE) для деталей.
