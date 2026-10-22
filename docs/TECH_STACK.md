# Технології

Версії взято з `package.json` і `package-lock.json` (встановлені версії), `unity/Packages/manifest.json` і `unity/ProjectSettings/ProjectVersion.txt`.

## Спільне для бота й сайту

| Що | Версія | Навіщо |
|---|---|---|
| Node.js | 24 (перевірено на 24.16.0) | Середовище виконання. З версії 24 Node запускає `.ts`-файли сам (type stripping), тому збірки немає |
| TypeScript | 7.0.2 | Мова обох проєктів у режимі `strict`. `tsc` лише перевіряє типи (`npm run typecheck`), нічого не компілює |
| npm | входить у Node.js | Залежності; у кожного проєкту свій `package.json` |
| @types/node | 26.6.5 | Типи для `node:fs`, `node:crypto`, `process.loadEnvFile` |
| better-sqlite3 | 13.0.3 | SQLite з синхронним API і prepared statements; база — один файл, сервер не потрібен |

Через type stripping у `tsconfig.json` увімкнено `erasableSyntaxOnly`, `verbatimModuleSyntax` і `allowImportingTsExtensions`. Тобто імпорти пишуться з `.ts`, імпорти типів — через `import type`, а `enum` і namespace з кодом заборонені.

## Telegram-бот (`bot/`)

| Що | Версія | Навіщо |
|---|---|---|
| grammY | 1.46.0 | Бібліотека Telegram Bot API: long polling, композери (`filter`/`drop`), клавіатури (`Keyboard`, `InlineKeyboard`), `forwardMessage`/`copyMessage` |
| better-sqlite3 | 13.0.3 | Таблиці `clients` і `messages`: блокування, статистика, зв'язка відповіді оператора з клієнтом |
| @types/better-sqlite3 | ^9.6.0 | Типи для better-sqlite3 |

## Сайт (`web/`)

| Що | Версія | Навіщо |
|---|---|---|
| Express | 5.3.0 | HTTP-сервер, маршрути, middleware. У версії 5 помилки async-обробників автоматично йдуть в обробник помилок |
| EJS | 7.0.1 | HTML-шаблони з рендером на сервері; `<%= %>` екранує дані |
| express-session | 1.19.0 | Сесії: дані на сервері, у cookie `sid` лише підписаний ідентифікатор |
| bcrypt | 6.0.0 | Хешування паролів (10 раундів, сіль вбудована) |
| better-sqlite3 | 13.0.3 | Таблиця `users` |
| @types/express, @types/express-session, @types/bcrypt | ^5.0.6, ^1.19.0, ^6.0.0 | Типи |
| CSS | — | Звичайний CSS на змінних (`:root`), світла й темна тема, адаптивність без бібліотек |
| JavaScript у браузері | — | Два невеликі файли без фреймворків: `register.js` (галочки вимог), `theme.js` (перемикач теми) |

## Unity-гра (`unity/`)

| Що | Версія | Навіщо |
|---|---|---|
| Unity | 6000.6.2f1 | Рушій; шаблон Universal 3D. Відкривається через Unity Hub |
| C# | (з Unity) | Скрипти гри й Editor-скрипти генерації |
| Universal Render Pipeline | 17.6.0 | Рендер, пост-обробка (Bloom, Vignette, Color Adjustments, Tonemapping), Global Volume |
| Input System | 1.20.0 | Ввід через project-wide actions (`Assets/InputSystem_Actions.inputactions`); активна лише нова система вводу |
| uGUI + TextMeshPro | 2.6.0 | Інтерфейс (Canvas, кнопки, повзунки) і текст; кирилиця через динамічний fallback-шрифт |
| Scripting backend | Mono | Збірка Windows `.exe` |

Інші пакети в `manifest.json` (AI Navigation, Timeline, Visual Scripting, Test Framework, IDE-інтеграції) прийшли з шаблону; код гри їх не використовує. Сторонніх пакетів і зовнішніх ассетів немає: меші, матеріали, текстури, звуки й музика генеруються кодом.

Вбудовані механізми Unity, на яких тримається гра:

- `CharacterController` — рух;
- `Physics.SphereCast` — стеля над присілим персонажем і камера біля стін;
- `JsonUtility` + `Application.persistentDataPath` — збереження;
- `PlayerPrefs` — налаштування;
- `AudioSource`/`AudioListener` — звук;
- Editor API: `EditorSceneManager`, `AssetDatabase`, `SerializedObject`, `UnityEventTools`, `ModelImporter`, `PrefabUtility` — генерація сцен і ассетів.

## 3D-модель (`model/`)

| Що | Версія | Навіщо |
|---|---|---|
| Blender | 5.2.2 LTS | Моделювання й експорт; запускається у фоні: `blender -b --python-exit-code 1 -P model/make_chair.py` |
| Python API Blender (`bpy`, `bmesh`) | з Blender 5.2 | Скрипт будує деталі, модифікатор Bevel, матеріали Principled BSDF, перевіряє розміри й експортує |
| FBX | — | Формат обміну з Unity (Forward −Z, Up Y, `bake_space_transform`, `FBX_SCALE_ALL`) |

## Інструменти

| Що | Навіщо |
|---|---|
| Git + GitHub | Гілка `feature/<назва>` на кожну частину → Pull Request → merge у `main` (PR #1–#4). Коміти вручну |
| Windows 10, PowerShell, Git Bash | Робоче середовище; запуск Unity в batchmode через `Start-Process -PassThru` + `WaitForExit()` |
| Unity Hub | Встановлення редактора й відкриття проєкту |
| Telegram, @BotFather | Створення бота й токена; перевірка з двох акаунтів |
| Браузер | Перевірка сайту, зокрема окремий тестовий екземпляр на порту 3001 |

## ШІ-інструменти

| Інструмент | Як використовувався |
|---|---|
| **Claude Code** (агент для програмування від Anthropic) | За моїми технічними завданнями писав код бота, сайту, Unity-скриптів і Blender-скрипта. Перед використанням API відкривав документацію, запускав перевірки (typecheck, тестовий екземпляр сайту, Unity в batchmode, Blender у фоні) і показував реальний вивід. Коміти не робив: це моє правило |
| **Claude** (чат на claude.ai) | Запропонував підхід до Unity-частини: Editor-скрипти, що збирають сцени з коду, batchmode для перевірки, процедурні звуки й меші замість ассетів. Давав інструкції, пропозиції й підказки, як це правильно написати кодом (зі слів автора) |
| Gemini Pro | Підказки під час роботи над моїми попередніми проєктами Vocabulary Trainer і Save Me (зі слів автора); у цьому проєкті не використовувався |

Детальніше про розподіл ролей — у розділі «Роль ШІ» в [PROCESS.md](PROCESS.md).

## Використано мої власні попередні проєкти

| Проєкт | Стек | Що перегукується з цим проєктом |
|---|---|---|
| [Vocabulary Trainer](https://github.com/Uplab-Education/vocab-trainer-practice-2026) (командна навчальна практика) | Next.js, React, Tailwind, Drizzle + PostgreSQL, TypeScript, Node 24 | Макет сайту (бічна панель + робоча область, плашка з ініціалами й роллю), відтінки кольорів і типографіка, ролі `user`/`admin`, підхід до автентифікації (cookie `httpOnly` + `sameSite=lax`), нормалізація email і повідомлення «невірний email або пароль». У WebTest це переписано на Express + EJS + звичайний CSS, з реальною базою, bcrypt, серверними сесіями й CSRF |
| [Save Me](https://github.com/TrutRoman/SaveMe) (командна 2D-гра на Unity) | Unity, C#, Rigidbody2D, старий Input Manager | Ідеї руху: біг через Shift, стрибок лише на землі, пауза через `Time.timeScale`, гучність у `PlayerPrefs`. У «Тестік гейм» це 3D на `CharacterController` з Input System, без статичних прапорців паузи |

Порівняння по файлах — у [PROCESS.md](PROCESS.md#використано-мої-власні-попередні-проєкти).
