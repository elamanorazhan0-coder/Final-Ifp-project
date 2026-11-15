# Деплой AutoCare

AutoCare — серверное приложение ASP.NET Core Blazor Server (.NET 10 + SQLite).
Netlify размещает только статические сайты и JavaScript/TypeScript-функции и **не может запускать .NET**,
поэтому само приложение нужно развернуть на хостинге с поддержкой Docker или .NET.
На Netlify остаётся лёгкая страница-заглушка из папки `netlify-site/`.

## Вариант 1: Render (бесплатно, через Docker)

1. Зайдите на https://render.com и войдите через GitHub.
2. **New → Blueprint** и выберите репозиторий `Final-Ifp-project` — Render прочитает `render.yaml`
   (или **New → Web Service → Docker**, Dockerfile в корне).
3. Дождитесь сборки и откройте выданный адрес, например `https://autocare.onrender.com`.
4. Войдите как `admin@autocare.local` / `AutoCare123!`.

## Вариант 2: Railway / Fly.io / Azure App Service

- Railway и Fly.io автоматически находят `Dockerfile`. Порт берётся из переменной `PORT` (по умолчанию 8080).
- Azure App Service: создайте Web App с runtime .NET 10 и опубликуйте командой
  `dotnet publish -c Release` (или из Visual Studio → Publish).

## Проверка Docker локально

```bash
docker build -t autocare .
docker run -p 8080:8080 autocare
# открыть http://localhost:8080
```

## Важно про SQLite

База `Data/app.db` создаётся и заполняется демо-данными при старте. На бесплатных тарифах диск контейнера
временный: после перезапуска данные сбрасываются к демо-набору. Для постоянного хранения подключите
постоянный диск (Render Disk / Railway Volume) с путём `/app/Data` или замените SQLite на PostgreSQL.

## Связать с адресом Netlify (необязательно)

Чтобы `autocarecarserviceplatform.netlify.app` сразу открывал рабочее приложение, раскомментируйте
redirect в `netlify.toml` и укажите адрес вашего деплоя.
