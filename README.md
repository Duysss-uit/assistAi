# AssistAI

A CLI-based AI agent built with C# that can search the web, scrape pages, and open your browser — all powered by LLMs via OpenRouter.

## Features

- **Web Search** — Search DuckDuckGo and get results
- **Web Scraper** — Extract text content from any URL
- **Open Browser** — Open URLs in your default browser
- **Read/Write Files** — Read and write local files
- **Bash** — Execute shell commands
- **Model Selector** — Choose from multiple free LLMs

## Setup

1. Install [.NET 10 SDK](https://dotnet.microsoft.com/download)
2. Clone and navigate to the project:
   ```bash
   git clone https://github.com/Duysss-uit/assistAi.git
   cd assistAi
   ```
3. Create a `.env` file:
   ```env
   OPENROUTER_API_KEY=your-api-key-here
   ```
4. Get a free API key at [openrouter.ai](https://openrouter.ai)

## Usage

```bash
dotnet run
```

Select a model, then start chatting:

```
Select model:
  1. step-3.5-flash
  2. trinity-large-preview
  ...
Enter number: 2

User: Search the web for C# tutorials
AI: (searches DuckDuckGo and summarizes results)
```

### Slash Commands

| Command | Description |
|---------|-------------|
| `/model` | Switch to a different model |
| `/clear` | Clear the console |
| `/exit` | Exit the program |
| `/help` | Show this help message |

## Tech Stack

- **C# / .NET 10** — Core language and runtime
- **OpenRouter API** — LLM gateway (supports multiple models)
- **HtmlAgilityPack** — HTML parsing for web scraping
- **DotNetEnv** — Environment variable management
