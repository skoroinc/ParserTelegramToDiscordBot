# 🌐 TelegramToDiscordBot

![GitHub repo size](https://img.shields.io/github/repo-size/your-repo/TelegramToDiscordBot?style=for-the-badge)
![GitHub contributors](https://img.shields.io/github/contributors/your-repo/TelegramToDiscordBot?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/your-repo/TelegramToDiscordBot?style=for-the-badge)

TelegramToDiscordBot — это кроссплатформенный бот для перенаправления сообщений из **Telegram** в **Discord** с поддержкой фильтрации ключевых слов, форматирования текста и динамической настройки.

---

## 🚀 Возможности

- 📤 Автоматическое перенаправление сообщений из Telegram в Discord.
- 📝 Поддержка форматирования текста Telegram (жирный, курсив, ссылки).
- 🔍 Фильтрация сообщений на основе списка ключевых слов.
- ⚙️ Управление ключевыми словами через команды в Telegram (`/add_keyword`, `/remove_keyword`, `/list_keywords`).
- 🔄 Легкая настройка через файл `appsettings.json`.

---

## 🏗️ Структура проекта

```plaintext
TelegramToDiscordBot
├── Interfaces
│   ├── IDiscordService.cs          // Интерфейс для работы с Discord
│   ├── IMessageFilter.cs           // Интерфейс фильтрации сообщений
│   ├── IMessageFormatter.cs        // Интерфейс форматирования сообщений
│   ├── ITelegramService.cs         // Интерфейс для работы с Telegram
├── Models
│   ├── FileDetails.cs              // Модель для обработки файлов
├── Services
│   ├── DiscordService.cs           // Реализация взаимодействия с Discord
│   ├── TelegramService.cs          // Реализация взаимодействия с Telegram
├── Handlers
│   ├── KeywordCommandHandler.cs    // Логика обработки команд Telegram
├── Utilities
│   ├── ConfigurationLoader.cs      // Загрузка конфигурации из appsettings.json
│   ├── DatabaseInitializer.cs      // Инициализация базы данных
├── Core
│   ├── MessageProcessor.cs         // Основная логика обработки сообщений
│   ├── MessageFilter.cs            // Реализация фильтрации сообщений
│   ├── MessageFormatter.cs         // Форматирование текста из Telegram в Markdown
├── Configuration
│   ├── appsettings.json            // Файл конфигурации
├── StartBot.cs                     // Точка входа
```
---

## 📂 Детали структуры
### 1. **Interfaces**
* **`IDiscordService`**

Определяет методы для отправки сообщений в Discord.
```csharp
Task SendMessageAsync(string message);
```

* **`IMessageFilter`**

Фильтрует сообщения на основе ключевых слов.
```csharp
bool ShouldFilter(string message);
```

* **`IMessageFormatter`**

Форматирует сообщения с учетом стиля Telegram.
```csharp
string Format(string text, MessageEntity[]? entities);
```

* **`ITelegramService`**

Определяет методы для взаимодействия с Telegram API.
```csharp
Task StartReceivingAsync();
```
---
### 2. **Models**

* **`FileDetails`**


Модель для работы с файлами, содержит метаданные о них:
```csharp
public string FileName { get; set; }
public long FileSize { get; set; }
```
---
### 3. **Services**
* **`DiscordService`**


Реализует логику отправки сообщений в Discord с использованием Webhook URL.
```csharp
public async Task SendMessageAsync(string message)
```

* **`TelegramService`**


Отвечает за прием и обработку сообщений из Telegram.
```csharp
public async Task StartReceivingAsync();
```
---
### 4. **Handlers**
* **`KeywordCommandHandler`**

Обрабатывает команды для управления ключевыми словами:
 * `/add_keyword <keyword>` — добавляет ключевое слово.
 * `/remove_keyword <keyword>` — удаляет ключевое слово.
 * `/list_keywords — выводит` — выводит список всех ключевых слов.
```csharp
public static async Task HandleCommandAsync(ITelegramBotClient botClient, Message message)
```
---
### 5. **Core**

* **`MessageProcessor`**
Основной компонент обработки сообщений: фильтрует, форматирует и перенаправляет их.

* **`MessageFilter`**
Сравнивает сообщения с заданным списком ключевых слов.

* **`MessageFormatter`**
Форматирует текст Telegram в Markdown для Discord.
---
### **6.** **Configuration**
* **`appsettings.json`**
Файл конфигурации, содержащий:

```json
{
  "Telegram": {
    "BotToken": "your-telegram-bot-token"
  },
  "Discord": {
    "WebhookUrl": "your-discord-webhook-url"
  },
  "Keywords": {
    "Filter": ["example1", "example2"]
  }
}
```
---
## ⚙️ Установка
### 1. **Клонирование репозитория:**
```
git clone https://github.com/your-repo/TelegramToDiscordBot.git
cd TelegramToDiscordBot
```

### 2. **Настройка конфигурации:**
   
* Файл `appsettings.json` находится в папке `Configuration`. 
* Настройте поля `BotToken` и `WebhookUrl`.

### 3. **Сборка и запуск:**
```
dotnet build
dotnet run
```
---

## 🛠️ Использование

### Команды Telegram
* **`/add_keyword <keyword>`** — Добавить новое ключевое слово.
* **`/remove_keyword <keyword>`** — Удалить ключевое слово.
* **`/list_keywords`** — Показать текущие ключевые слова.

### Фильтрация сообщений

Сообщения, содержащие ключевые слова из `appsettings.json`, будут автоматически удалены из пересылаемого контента.

---
## 📖 Дополнительно
### Форматирование текста

**`TelegramToDiscordBot`** поддерживает преобразование стилей текста из Telegram в Markdown, включая:

* Жирный текст → **bold**
* Курсивный текст → *italic*
* Ссылки → [text](https://example.com)

### 🤝 Вклад в проект
Если у вас есть идеи для улучшения проекта:

1. Форкните репозиторий.
2. Создайте новую ветку: `git checkout -b feature/your-feature`.
3. Внесите изменения и сделайте коммит: `git commit -m "Add your feature"`.
4. Создайте пул-реквест.

### 📜 Лицензия
Проект распространяется под лицензией MIT. Подробнее см. [LICENSE](https://github.com/skoroinc/ParserTelegramToDiscordBot/blob/main/LICENSE).

### Разработано с ❤️ и C#.

