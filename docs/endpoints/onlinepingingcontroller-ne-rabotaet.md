# HTTP-присутствие — ограничения

Требуется роль User/Admin.

| Метод | Маршрут | Текущее поведение |
| --- | --- | --- |
| PATCH | /api/online/ping | Вызывает PingHandlerService, успех 200 без тела |
| GET | /api/online/status/{userId} | Неработоспособен: 500 |

В конструкторе не присваиваются _userOnlineStore и _presenceReader. GetStatus обращается к null; catch возвращает 500 "Internal server error.".

Задуманное тело status — { isOnline, lastSeen }, но сейчас его нельзя считать рабочим контрактом.

Для событий online/offline подключайтесь к /hubs/presence. Ping и наличие SignalR-подключения — разные механизмы; требуется единая политика lastSeen/TTL. Не считайте HTTP ping подтверждением активного presence-соединения.

---
Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/OnlinePingingController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/OnlinePingingController.cs).
