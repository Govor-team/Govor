---
description: >-
  Controller for managing user sessions, including retrieving and closing
  sessions.
---

# Сессии

Все методы требуют Bearer access token с ролью User или Admin.

| Метод  | Маршрут                        | Успех                                |
| ------ | ------------------------------ | ------------------------------------ |
| GET    | /api/Session/all               | 200, SessionDto\[]                   |
| DELETE | /api/Session/close/{sessionId} | 204, без тела                        |
| DELETE | /api/Session/close             | 204, закрыть sid текущего токена     |
| DELETE | /api/Session/close/all         | 204, закрыть все сессии пользователя |

```json
[{
  "id":"11111111-1111-1111-1111-111111111111",
  "deviceInfo":"Android test device",
  "createdAt":"2026-10-03T00:00:00Z",
  "expiresAt":"2026-10-10T00:00:00Z",
  "isRevoked":false
}]
```

Список фильтруется по UserId и !IsRevoked, но не по ExpiresAt; истекшие сессии могут присутствовать. CreatedAt меняется при refresh.

Закрытие чужой, неизвестной или уже отозванной сессии возвращает 404 с errorCode UserSession.NotFoundOrUnauthorized. Guid.Empty возвращает 400. CloseAll без активных сессий тоже успешен.

Отзыв деактивирует связанные push-токены. Успех Unit преобразуется в 204, а не в 200.

## Ограничение logout

IsRevoked проверяется при refresh; bearer middleware не сверяет текущий sid с БД. Access token может работать до окончания срока, а существующее соединение SignalR не закрывается этим контроллером.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/SessionController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/SessionController.cs).
