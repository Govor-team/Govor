# Table of contents

* [Govor.API — обзор](README.md)
* [Запуск и конфигурация](getting-started.md)
* [Архитектура и границы](architecture.md)
* [Диагностика и порядок исправлений](diagnostics.md)

## Endpoints

* [Аутентификация](endpoints/authentication/README.md)
  * [Регистрация и вход](endpoints/authentication/authcontroller.md)
  * [Обновление токенов](endpoints/authentication/refreshcontroller.md)
  * [Ключи сессий — экспериментальный API](endpoints/authentication/session-keys.md)
* [Сессии](endpoints/sessioncontroller.md)
* [Друзья и поиск](endpoints/friendshipcontroller.md)
* [Списки заявок в друзья](endpoints/friendsrequestquerycontroller.md)
* [Личные чаты](endpoints/private-chats.md)
* [Медиа](endpoints/mediacontroller.md)
* [История сообщений](endpoints/chatloadcontroller.md)
* [HTTP-присутствие — ограничения](endpoints/onlinepingingcontroller-ne-rabotaet.md)
* [Профиль и аватар](endpoints/profile.md)
* [Push-уведомления](endpoints/push.md)
* [Администрирование и незавершённые группы](endpoints/admin-and-groups.md)

## SignalR

* [PresenceHub — online и offline](signalr/presencehub.md)
* [ChatsHub — сообщения](signalr/chathub.md)
* [ProfileHub — изменения профиля](signalr/profilehub.md)
* [FriendsHub — заявки](signalr/friendshub/README.md)
  * [Клиент SignalR на C# для MAUI](signalr/friendshub/friendshub-client-java.md)

## Code Docs

* [Ответы, ошибки и сериализация](code-docs/hubresult-less-than-t-greater-than.md)
