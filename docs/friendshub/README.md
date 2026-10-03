# FriendsHub — заявки

Подключение: /hubs/friends с access token, содержащим userId. Атрибут Authorize у класса отсутствует; HubUserAccessor читает claim и бросает исключение при его отсутствии. Это не следует считать полноценной endpoint-политикой авторизации: её нужно добавить явно.

## Команды

| Команда | Аргумент | Успех |
| --- | --- | --- |
| SendRequest | Guid targetUserId | status 201, result null |
| AcceptRequest | Guid friendshipId | status 200, result null |
| RejectRequest | Guid friendshipId | status 200, result null |

SendRequest запрещает заявку себе и повтор при Pending/Accepted/Blocked. После Rejected запись переиспользуется. Accept разрешён только адресату и только Pending; после сохранения дружбы создаётся личный чат. Reject разрешён адресату для Pending/Rejected.

## Все события

| Событие | Кому |
| --- | --- |
| FriendRequestReceived | Адресату новой заявки |
| YourFriendRequestReceived | Отправителю |
| FriendRequestAccepted | Принимающему адресату |
| YourFriendRequestAccepted | Отправителю исходной заявки |
| FriendRequestRejected | Отклоняющему адресату |
| YourFriendRequestRejected | Отправителю исходной заявки |

Каждое событие передаёт **FriendshipDto**, не строку Guid:

```json
{"id":"11111111-1111-1111-1111-111111111111","requesterId":"22222222-2222-2222-2222-222222222222","addresseeId":"33333333-3333-3333-3333-333333333333","status":0}
```

Status: Pending=0, Accepted=1, Rejected=2, Blocked=3.

## Ошибки и восстановление

Сервисные Failure в SendRequest преобразуются в status 500; Accept/Reject — в 400. Поэтому не обещайте 409/403/404 для всех соответствующих бизнес-ошибок.

Дружба сохраняется до создания чата/рассылки; сбой может оставить частично завершённый сценарий. На reconnect перечитывайте /api/friends, /requests, /responses и /api/user/private-chats. Клиенту нужны дедупликация и обновление cache по id.

---
Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; сервер с БД и Firebase в рамках диагностики не запускался.

Источник: [Govor.API/Hubs/FriendsHub.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Hubs/FriendsHub.cs).
