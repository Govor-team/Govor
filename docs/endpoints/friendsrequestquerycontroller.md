# Списки заявок в друзья

Оба маршрута требуют Bearer token и возвращают 200 с FriendshipDto[].

| Метод | Маршрут | Назначение |
| --- | --- | --- |
| GET | /api/friends/requests | Входящие заявки |
| GET | /api/friends/responses | Исходящие записи, кроме Accepted (включая Pending, Rejected, Blocked) |

```json
[{
  "id":"11111111-1111-1111-1111-111111111111",
  "requesterId":"22222222-2222-2222-2222-222222222222",
  "addresseeId":"33333333-3333-3333-3333-333333333333",
  "status":0
}]
```

FriendshipStatus сериализуется числом: Pending=0, Accepted=1, Rejected=2, Blocked=3. id — идентификатор заявки/дружбы; именно он передаётся в AcceptRequest и RejectRequest.

При отсутствии записей — пустой массив. Ошибки контроллера — 500 с {"error":"Internal server error."}. Для восстановления после reconnect перечитайте списки: события не являются журналом гарантированной доставки.

---
Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/Friends/FriendsRequestQueryController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/Friends/FriendsRequestQueryController.cs).
