---
description: >-
  Description: Controller for managing friendship-related operations, including
  searching for users and retrieving the list of friends for the current user.
---

# Друзья и поиск

Bearer token обязателен; отдельная роль в атрибуте не задана.

| Метод | Маршрут                       | Результат                     |
| ----- | ----------------------------- | ----------------------------- |
| GET   | /api/friends                  | UserDto\[] друзей             |
| GET   | /api/friends/search?query=Арт | UserDto\[] результатов поиска |

```json
[{
  "id":"11111111-1111-1111-1111-111111111111",
  "username":"Артём",
  "description":"",
  "wasOnline":"2026-10-03T00:00:00Z",
  "iconId":"00000000-0000-0000-0000-000000000000",
  "isOnline":false
}]
```

Пустой/пробельный query возвращает 400 строкой "Query cannot be empty". Успех — 200; серверные ошибки — 500 с объектом error. Не смешивайте UserDto и UserProfileDto: у второго нет wasOnline.

Изменение заявок выполняется через FriendsHub. HTTP-контроллер не содержит команды отправки, принятия и отклонения.

Статус isOnline зависит от состояния процесса и подключения к presence. Он не является гарантией доставки сообщений или фоновой доступности устройства.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/Friends/FriendshipController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/Friends/FriendshipController.cs).
