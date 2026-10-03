---
description: How to work with jwt tokens
icon: user
---

# Аутентификация

## Последовательность

1. `POST /api/auth/register` либо `POST /api/auth/login` возвращает `{ refreshToken, accessToken }`.
2. HTTP-запросы передают `Authorization: Bearer <accessToken>`.
3. Для SignalR задайте AccessTokenProvider, который возвращает актуальный access token. Program.cs также принимает query `access_token` на путях /hubs.
4. `POST /api/auth/token/refresh` возвращает новую пару; клиент сохраняет новый refresh token.
5. Отзыв сессий выполняется методами /api/Session/close.

Access token содержит `userId`, `sid` и роль User/Admin. Подпись HS256 проверяется, срок проверяется, issuer и audience не проверяются. Токены возвращаются JSON; сервер не устанавливает HttpOnly-cookie.

## Ограничения реализации

Отзыв сессии блокирует refresh, но Program.cs не проверяет IsRevoked при каждом использовании access token. Уже открытые SignalR-соединения явно не разрываются. Немедленный logout на всех устройствах не гарантирован.

Refresh JWT содержит userId и tokenType, но не случайный jti или sid. При выпуске одному пользователю в одну секунду возможны одинаковые токены, конфликтующие с уникальным RefreshTokenHash. Ротация не защищена атомарным compare-and-swap от параллельных запросов.

Не используйте refresh token как access token. В bearer-валидации нет явного требования tokenType=access; методы с одним Authorize требуют исправления разделения типов токенов.

На MAUI рекомендовано хранить refresh token в SecureStorage и выполнять обновление последовательно. Это рекомендация клиенту, а не свойство сервера.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.Application/Authentication/JWT/JwtService.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.Application/Authentication/JWT/JwtService.cs).
