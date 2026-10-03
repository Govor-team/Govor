---
description: >-
  Controller for managing token refresh operations, allowing users to refresh
  their access tokens using a valid refresh token.
---

# Обновление токенов

## POST /api/auth/token/refresh

Доступ анонимный. JSON:

```json
{"refreshToken":"<последний сохранённый refresh token>"}
```

Сервис хеширует переданную строку и ищет UserSession по RefreshTokenHash. Проверяет IsRevoked и ExpiresAt, выпускает новую пару, заменяет хеш и продлевает ExpiresAt. CreatedAt также перезаписывается, поэтому после refresh это не исходная дата открытия сессии.

Успех 200:

```json
{"refreshToken":"<new token>","accessToken":"<new token>"}
```

| Случай                     | Текущий ответ                    |
| -------------------------- | -------------------------------- |
| Пустая строка              | 400, errorCode Auth.EmptyToken   |
| Хеш не найден              | 400, errorCode Auth.InvalidToken |
| Истекшая/отозванная сессия | 400, errorCode Auth.InvalidToken |

Не документируйте неверный refresh как гарантированный 401: текущий Error.Failure преобразуется в 400.

## Клиент

Сохраните оба новых токена и сериализуйте refresh-запросы. Ошибка сети не должна автоматически удалять авторизацию пользователя. При подтверждённо недействительном refresh требуется повторный вход.

Ротация сейчас не имеет защиты от конкурентного использования и истории повторного применения. Детали — в разделе «Аутентификация».

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.Application/Users/UserSessions/UserSessionRefresher.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.Application/Users/UserSessions/UserSessionRefresher.cs).
