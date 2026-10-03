# Push-уведомления

## POST /api/pushes/token/register

Требуется роль User/Admin. JSON:

```json
{"token":"<FCM registration token>","platform":"Android"}
```

Сервис связывает токен с userId и sid текущего access token, добавляет или обновляет запись. Успех Unit — 204. Пустой token/platform отклоняется 400; тело BadRequest(ModelState) не всегда содержит полезное объяснение.

При отзыве сессии связанные токены деактивируются. В DI зарегистрирован FirebasePushProvider; credentials читаются при старте приложения.

## Отправка

Личные сообщения вызывают отправку push собеседнику. title — имя отправителя; body — первые 40 символов EncryptedContent. Data содержит chatId и isGroup ("false"). Это текущая реализация: сервер не расшифровывает содержимое.

Для E2EE тело push должно быть нейтральным. Для текущего MAUI, отправляющего обычный текст, этот body раскрывает превью сообщения внешнему push-провайдеру.

## Доставка

Сохранение сообщения, SignalR и Firebase не образуют единую транзакцию. Ошибка после SaveChanges может дать ошибку Send, хотя сообщение уже записано. Автоматическое повторение без идентификатора клиентской операции может создать дубль. Нужны outbox, дедупликация и обработка недействительных FCM-токенов.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Hubs/Infrastructure/ChatNotificationService.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Hubs/Infrastructure/ChatNotificationService.cs).
