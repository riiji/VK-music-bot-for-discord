# Установка Windows
1. скачать [ffmpeg](https://ffmpeg.zeranoe.com/builds/win32/static/ffmpeg-4.2-win32-static.zip),[opus и libsodium](https://discord.foxbot.me/binaries/) и засунуть рядом с ботом
2. ввести логин, пароль от вк, а также токен и префикс бота в config.json

# Config.json

**Login** - логин аккаунта от ВК

**Password** - пароль акккаунта от ВК

**Token** - токен бота в дискорде

**Prefix** - префикс команд бота(по умолчанию "!")

**StarsCount** - количество звездочек в сообщении плеера(по умолчанию "8")

**GetPlaylistCount** - количество получаемых песен в getplaylist(по умолчанию "20")

**ColorValue** - Цвет боковой стенки сообщения(по умолчанию "5614830"), если хотите изменить, то берите цвет в hex и переводите в dec

# Команды
**!play [vkid] [startindex=0] [forceupdate=false]** - запуск песни
- vkid(int) - id профиля вк для парса плейлиста(доступ должен быть из под аккаунта в который мы залогинились)
- startindex(int) - номер песни в плейлисте для запуска (опционально)
- forceupdate(bool) - обновление плейлиста из ВК (опционально)

**!stop** - остановка песни

**!geplaylist [vkid] [numberlist=0]** - получить плейлист в профиле

- vkid(int) - id профиля вк для парса плейлиста(доступ должен быть из под аккаунта в который мы залогинились)
- numberlist(int) - номер страницы плейлиста

# План развития
- убрать всю хрень из кода(весь код)

# Credits
Спасибо большое за библиотеки
- [vknet](https://github.com/vknet/vk) и [Discord.net](https://github.com/discord-net/Discord.Net)
