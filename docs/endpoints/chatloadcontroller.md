---
description: Controller for loading chat messages for users and groups with pagination.
---

# История сообщений

Требуется роль User/Admin.

| Метод | Маршрут                        | Идентификатор                          |
| ----- | ------------------------------ | -------------------------------------- |
| GET   | /api/user/{userId}/messages    | В userId передаётся **ID PrivateChat** |
| GET   | /api/groups/{groupId}/messages | ID ChatGroup                           |

Query: StartMessageId (Guid?, необязателен), Before=20, After=2. Before и After неотрицательные, сумма ≤100.

Без StartMessageId возвращаются последние Before сообщений, упорядоченные по SentAt по возрастанию; After не используется. С якорем возвращаются Before старых, якорное сообщение и After новых: максимум 101 запись. Неизвестный якорь — \[].

Успех 200, MessageResponse\[]:

```json
[{
  "id":"11111111-1111-1111-1111-111111111111",
  "senderId":"22222222-2222-2222-2222-222222222222",
  "recipientId":"33333333-3333-3333-3333-333333333333",
  "recipientType":0,
  "encryptedContent":"<строка>",
  "sentAt":"2026-10-03T00:00:00Z",
  "isEdited":false,
  "editedAt":null,
  "replyToMessageId":null,
  "mediaAttachments":[],
  "reactions":[],
  "messageViews":[]
}]
```

У HTTP-ответа поле id; у SignalR UserMessageResponse — messageId. Вложения HTTP имеют id, messageId, mediaFileId.

## Известные проблемы

Личный загрузчик принимает currentUser, но не проверяет участие в указанном чате. Это критичная ошибка доступа. Для группы участие проверяется; неучастнику возвращается \[].

Пагинация сравнивает только SentAt: сообщения с одинаковой датой могут выпадать вокруг якоря. Reactions и MessageViews не включаются явным Include, поэтому эти массивы не являются надёжным источником состояния. Нужны проверки доступа и стабильный курсор (SentAt, Id).

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.Application/Messages/MessagesLoader.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.Application/Messages/MessagesLoader.cs).
