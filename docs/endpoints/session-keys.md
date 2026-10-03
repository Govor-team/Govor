# Ключи сессий — экспериментальный API

RequireHttps и роль User/Admin обязательны.

| Метод | Маршрут | Назначение |
| --- | --- | --- |
| POST | /api/session/keys | Привязать публичные ключи текущей сессии |
| GET | /api/session/users/{userId}/keys | Получить ключи друга |
| POST | /api/session/keys/rotate | Пополнить одноразовые ключи |
| GET | /api/session/keys/remaining | { remaining: число } |
| POST | /api/session/keys/{preKeyId}/used | Пометить собственный ключ использованным |

byte[] в JSON передаются строкой base64. UploadKeysRequest содержит identityKey, signedPreKey, signedPreKeySignature и oneTimePreKeys (массив base64). За запрос допускается максимум 100 одноразовых ключей. Сервис запрещает повторную привязку ключей к сессии и проверяет владельца.

RotateOneTimePreKeysRequest содержит newOneTimePreKeys. В реализации ротация удаляет использованные записи и добавляет новые, не ограничивая общий размер набора.

## Неработающие и небезопасные части

_oneTimePreKeysRotator не передан в конструктор SessionKeysController: rotate и used падают при обращении к null.

remaining считает все записи, включая IsUsed. Reader выдаёт все OneTimePreKeys и возвращает EF-сущности UserCryptoSession вместо PublicSessionKeysDto. Include(UserSession) может включить внутренние поля, а двусторонние навигации могут привести к циклу сериализации. SignedPreKey не включён явным Include.

Доступ к чужим ключам ограничен дружбой, но Forbid("текст") ошибочно используется как имя authentication scheme. Атомарной выдачи одного неиспользованного prekey потребителю нет: endpoint used относится к текущей сессии владельца.

## Статус E2EE

Это заготовка хранения ключей, не законченный криптографический протокол. Нет доказанной реализации обмена/проверки подписей, одноразового потребления, ratchet и поддержки шифрования на клиенте. Не используйте этот раздел как обещание сквозного шифрования.

---
Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/Authentication/SessionKeysController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/Authentication/SessionKeysController.cs).

