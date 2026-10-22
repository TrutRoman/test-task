# Документація до презентації

Матеріали для презентації результатів тестового завдання. Повні інструкції з налаштування й перевірки лежать у головному [README.md](../README.md), тут лише короткий зміст.

| Файл | Що всередині |
|---|---|
| [PROCESS.md](PROCESS.md) | Як виконувалась кожна частина: етапи, ключові рішення з альтернативами, труднощі, перевірка, мій внесок, роль ШІ, використані попередні проєкти |
| [TECH_STACK.md](TECH_STACK.md) | Мови, бібліотеки, застосунки й версії та навіщо кожен; ШІ-інструменти |
| [SOURCES.md](SOURCES.md) | Документація, якою користувались, з посиланнями, і мої попередні проєкти |
| [screenshots/](screenshots) | Скриншоти сайту, гри й моделі |

## Як запустити кожну частину

Докладно, з налаштуванням `.env`, — у [README.md](../README.md). Коротко:

**Telegram-бот** (Node.js 24):

```bash
cd bot
cp .env.example .env    # BOT_TOKEN, OPERATOR_ID, DB_PATH, LOG_PATH
npm install
npm start
```

**Сайт** (Node.js 24), далі відкрити `http://localhost:<PORT>`:

```bash
cd web
cp .env.example .env    # PORT, SESSION_SECRET, DB_PATH, LOG_PATH, ADMIN_EMAIL, ADMIN_PASSWORD
npm install
npm run seed
npm start
```

**Гра** (Unity 6000.6.2f1): Unity Hub → Add project from disk → папка `unity/` → сцена `Assets/Scenes/MainMenu` → Play. Керування: WASD, Shift — біг, Ctrl — присідання, Space — стрибок, F5 — збереження, Esc — пауза.

**Стілець** (Blender 5.2): відкрити `model/chair.blend`. Щоб перебудувати модель з коду:

```bash
blender -b --python-exit-code 1 -P model/make_chair.py
```

В Unity: **Tools → Setup Chair Test**, потім сцена `Assets/Scenes/CollisionTest` → Play.

## Скриншоти

| Частина | Файли |
|---|---|
| Бот | `bot-operator.png` (чат оператора: переслане повідомлення, дані клієнта з кнопкою «Заблокувати», статистика, останні клієнти), `bot-client.png` (чат клієнта: `/start`, питання з фото, відповідь оператора) |
| Сайт | `site-home-guest.png`, `site-register-validation.png`, `site-register-errors.png`, `site-profile.png`, `site-courses.png`, `site-admin-403.png`, `site-login-error.png`, `site-admin.png`, `site-home-dark.png`, `site-mobile.png` |
| Гра | `unity-main-menu.png`, `unity-game.png`, `unity-run.png`, `unity-crouch.png`, `unity-jump.png`, `unity-pause.png`, `unity-settings.png` |
| Стілець | `chair-blender.png`, `chair-bevel-closeup.png`, `unity-collision-test.png` (сцена CollisionTest: куби на сидінні, персонаж біля стільця), `unity-chair-colliders.png` (префаб `Chair` в Unity Editor: шість деталей, на кожній Box Collider) |

Бота знято під час перевірки з двох акаунтів. Ім'я, username і Telegram ID клієнта на скриншоті оператора розмиті. Скриншот клієнта зроблено на першій версії бота (14.10), тому вітальний текст там коротший, ніж у поточному коді. Сайт знято на окремому тестовому екземплярі з тестовими користувачами. Гру — із зібраної версії `.exe`, стілець — рендером `chair.blend` у Blender, а сцену CollisionTest і префаб стільця — в Unity Editor.
