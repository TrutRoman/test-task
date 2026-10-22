# Джерела

Тут перелічено лише те, що справді використовувалось. Під час генерації коду Claude Code відкривав офіційну документацію, і ці сторінки видно в журналі робочих сесій. Нижче вони згруповані за частинами, з одним реченням про те, що з кожної взято. Усі посилання перевірено 21.10.2026: вони відкриваються.

## Node.js і TypeScript

- [Node.js — Modules: TypeScript](https://nodejs.org/api/typescript.html) — як Node 24 запускає `.ts` без збірки (type stripping) і які обмеження це накладає на синтаксис.
- [Node.js — process.loadEnvFile](https://nodejs.org/api/process.html#processloadenvfilepath) — читання `.env` без пакета dotenv (`config.ts`, `env.ts`).

## Telegram-бот

- [grammY — Getting Started](https://grammy.dev/guide/getting-started) — створення бота й запуск long polling (`bot.start`).
- [grammY — Filter Queries](https://grammy.dev/guide/filter-queries) — фільтри `message`, `callbackQuery`, `chatType("private")`.
- [grammY — Middleware](https://grammy.dev/guide/middleware) — порядок обробників: перший, що спрацював, зупиняє ланцюжок.
- [grammY — Composer (API reference)](https://grammy.dev/ref/core/composer) — `filter`/`drop` для розділення оператора й клієнта.
- [grammY — Context](https://grammy.dev/guide/context) — `ctx.forwardMessage`, `ctx.copyMessage`, `ctx.reply`, `ctx.match`.
- [grammY — Commands](https://grammy.dev/guide/commands) — обробка `/start`.
- [grammY — Keyboards](https://grammy.dev/plugins/keyboard) — постійна клавіатура (`Keyboard`, `persistent`, `resized`) і inline-кнопки (`InlineKeyboard`).
- [grammY — Error Handling](https://grammy.dev/guide/errors) — `bot.catch`, щоб помилка в одному оновленні не зупиняла бота.
- [grammY — Reliability](https://grammy.dev/advanced/reliability) — коректна зупинка на SIGINT/SIGTERM (`bot.stop`).
- [Telegram Bot API — forwardMessage](https://core.telegram.org/bots/api#forwardmessage) — у методу немає `reply_markup`, тому кнопка блокування в окремому повідомленні.
- [Telegram Bot API — getUpdates](https://core.telegram.org/bots/api#getupdates) — як дізнатися свій Telegram ID без сторонніх ботів (інструкція в README).
- [Telegram — From BotFather to «Hello World»](https://core.telegram.org/bots/tutorial) і [Bot Features — BotFather](https://core.telegram.org/bots/features#creating-a-new-bot) — створення бота й токена (інструкція в README).

## Сайт

- [Express — Migrating to Express 5](https://expressjs.com/en/guide/migrating-5.html) — відмінності версії 5, зокрема автоматична передача помилок async-обробників.
- [Express 5 — API reference](https://expressjs.com/en/5x/api.html) — `express.urlencoded`, `res.render`, `res.redirect`, `app.locals`.
- [Express — Using template engines](https://expressjs.com/en/guide/using-template-engines.html) — підключення EJS (`view engine`, `views`).
- [Express — Error handling](https://expressjs.com/en/guide/error-handling.html) — обробник помилок з чотирма аргументами в кінці ланцюжка.
- [express-session](https://expressjs.com/en/resources/middleware/session.html) — параметри cookie (`httpOnly`, `sameSite`, `maxAge`), `resave`/`saveUninitialized`, `regenerate`, `destroy`, обмеження `MemoryStore`.
- [EJS](https://ejs.co/) і [EJS на GitHub](https://github.com/mde/ejs) — синтаксис `<%= %>` (з екрануванням) і `<%- include %>`.
- [better-sqlite3 — API](https://github.com/WiseLibs/better-sqlite3/blob/master/docs/api.md) — `prepare`, `run`, `get`, `all`, `lastInsertRowid`, `changes`.
- [better-sqlite3 — Performance](https://github.com/WiseLibs/better-sqlite3/blob/master/docs/performance.md) — рекомендації щодо продуктивності (режим WAL); у проєкті лишено налаштування за замовчуванням.
- [bcrypt (node.bcrypt.js)](https://github.com/kelektiv/node.bcrypt.js) — `hash`/`compare`, кількість раундів, обмеження на 72 байти пароля.

## Unity

- [Command-line arguments (Editor)](https://docs.unity3d.com/6000.6/Documentation/Manual/EditorCommandLineArguments.html) — `-batchmode`, `-quit`, `-executeMethod`, `-logFile`, `-buildWindows64Player`.
- [Player command-line arguments](https://docs.unity3d.com/6000.6/Documentation/Manual/PlayerCommandLineArguments.html) — запуск збірки у вікні (`-screen-fullscreen 0`, `-screen-width`).
- [CharacterController](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CharacterController.html) — `Move`, `isGrounded`, `velocity`, `height`/`center`, `stepOffset`.
- [Physics.SphereCast](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics.SphereCast.html) — перевірка стелі перед вставанням і відведення камери від стін.
- [Input System — Project-wide actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/ProjectWideActions.html) — `InputSystem.actions.FindAction("Player/…")`.
- [Input System — UI support](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/UISupport.html) — `InputSystemUIInputModule` для кнопок меню.
- [JsonUtility.ToJson](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/JsonUtility.ToJson.html) і [JSON Serialization](https://docs.unity3d.com/6000.6/Documentation/Manual/json-serialization.html) — збереження `SaveData` з `Vector3` у `save.json`.
- [EditorBuildSettings.scenes](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/EditorBuildSettings-scenes.html) — додавання сцен у Build Settings зі скрипта.
- [UnityEventTools.AddPersistentListener](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Events.UnityEventTools.AddPersistentListener.html) — підключення кнопок і повзунків до методів так, щоб зв'язок зберігся в сцені.
- [AssetDatabase.CreateAsset](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/AssetDatabase.CreateAsset.html) — збереження згенерованих матеріалів і мешів як ассетів.
- [TextureImporter.spriteBorder](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/TextureImporter-spriteBorder.html) — 9-slice спрайт із заокругленими кутами для UI.
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html) і [Dynamic fonts](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/FontAssetsDynamicFonts.html) — TMP Essential Resources і динамічний fallback-шрифт для кирилиці.
- [URP — Post-processing](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/integration-with-post-processing.html) і [Tonemapping](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/post-processing-tonemapping.html) — Global Volume, ефекти, режим Neutral.
- [Skybox shaders](https://docs.unity3d.com/6000.6/Documentation/Manual/skybox-shaders.html) — градієнтне небо.
- [AudioListener.volume](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/AudioListener-volume.html) — загальна гучність для повзунка налаштувань.
- [AudioMixer](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Audio.AudioMixer.html) — розглянуто як альтернативу, відкинуто: міксер не можна створити з коду.
- [AudioSource.PlayOneShot](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/AudioSource.PlayOneShot.html) — короткі звуки без переривання один одного.
- [AudioImporter](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/AudioImporter.html) — імпорт згенерованих WAV у PCM, щоб музична петля не мала паузи на стику.

## Blender і 3D-модель

- [Blender 5.2 Python API — Export Scene Operators](https://docs.blender.org/api/5.2/bpy.ops.export_scene.html) — параметри `export_scene.fbx` (осі, масштаб, `bake_space_transform`, модифікатори).
- [Blender 5.2 Python API — BevelModifier](https://docs.blender.org/api/5.2/bpy.types.BevelModifier.html) — ширина, сегменти, `limit_method` для фаски.
- [Blender Manual — FBX](https://docs.blender.org/manual/en/latest/files/import_export/fbx.html) — як Blender переводить Z-up у Y-up і одиниці виміру.
- [Unity — Model Import Settings (Model tab)](https://docs.unity3d.com/6000.6/Documentation/Manual/FBXImporter-Model.html) і [ModelImporter](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ModelImporter.html) — Scale Factor, Convert Units, вимкнення камер, світла, анімації й автоколайдерів.
- [Rigidbody.collisionDetectionMode](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody-collisionDetectionMode.html) — `ContinuousDynamic` для кубів, що падають на тонке сидіння.
- [PrefabUtility.SaveAsPrefabAsset](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html) — збереження префаба стільця з колайдерами.

## ШІ-інструменти

- [Claude Code](https://claude.com/product/claude-code) ([документація](https://docs.claude.com/en/docs/claude-code/overview)) — агент для програмування, яким згенеровано код за моїми вимогами.
- [Claude](https://claude.ai) — чат-асистент, що запропонував підхід до Unity-частини й давав підказки. (Посилання відкривається в браузері; автоматична перевірка отримує 403 через захист від ботів.)

## Власні попередні проєкти

- [Vocabulary Trainer](https://github.com/Uplab-Education/vocab-trainer-practice-2026) — командна навчальна практика (Next.js, PostgreSQL). Звідти — референс макету й стилю сайту (бічна панель, плашка користувача з роллю, відтінки й шрифти) і підхід до автентифікації та валідації.
- [Save Me](https://github.com/TrutRoman/SaveMe) — командна 2D-гра на Unity. Звідти — ідеї руху персонажа (біг через Shift, стрибок лише на землі) і паузи через `Time.timeScale`.

Детальне порівняння — у [PROCESS.md](PROCESS.md#використано-мої-власні-попередні-проєкти).

## Уточнити

- Джерела, які я відкривав сам (документація, відео, статті), окрім перелічених вище, — дописати, якщо такі були.
