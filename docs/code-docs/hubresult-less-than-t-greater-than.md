---
description: >-
  Description: A generic class used to encapsulate the result of SignalR hub
  operations, providing a standardized way to return status, data, and error
  messages. It supports various HTTP-like status cod
---

# Ответы, ошибки и сериализация

## HTTP

ResultExtensions преобразует Result\<T, Error>: успешный T → 200 со значением; Unit → 204 без тела.

| ErrorType                      | HTTP                         |
| ------------------------------ | ---------------------------- |
| Validation                     | 400                          |
| NotFound                       | 404                          |
| Conflict                       | 409                          |
| Unauthorized                   | 401                          |
| Forbidden                      | 403                          |
| Остальные, включая ServerError | 400 (текущий дефект mapping) |

Сервисная ошибка:

```json
{"status":404,"title":"Not Found","detail":"<описание>","errorCode":"<код>"}
```

errors добавляется при наличии. Автоматическая MVC-валидация, middleware авторизации и ручные BadRequest/StatusCode могут возвращать другую форму. Единого тела ошибки для всех маршрутов сейчас нет.

В контроллерах встречается Forbid(ex.Message): строка трактуется как authentication scheme, а не тело ответа; результат может быть 500 вместо 403.

## SignalR HubResult

```json
{"status":200,"result":null,"errorMessage":null}
```

status — число внутри ответа команды, не HTTP-статус соединения. Значения: 200 Success, 201 Created, 204 NoContent, 400 BadRequest, 401 Unauthorized, 404 NotFound, 409 Conflict, 422 UnprocessableEntity, 500 ServerError.

Static factories: Ok(result), Created(result), NoContent(), BadRequest(message, details), NotFound(message, details), Unauthorized(message), Conflict(message), UnprocessableEntity(message), Error(message).

Каждый хаб сам преобразует бизнес-ошибки; наличие enum не гарантирует точный status для каждого Failure. HubExceptionFilter возвращает Task.FromResult(errorResult) вместо объекта результата из InvokeMethodAsync; исключения вне локальных catch требуют исправления и проверки сериализации.

## JSON-контракт

Стандартные ASP.NET настройки используют camelCase; enum передаются числами, Guid — строками, byte\[] — base64, даты — ISO 8601. StringEnumConverter явно не настроен.

HTTP MessageResponse.id и SignalR UserMessageResponse.messageId отличаются. HTTP и SignalR вложения также имеют разные структуры. Клиент должен иметь явное преобразование; не подменяйте один DTO другим.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; сервер с БД и Firebase в рамках диагностики не запускался.

Источник: [Govor.API/Common/Extensions/ResultExtensions.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Common/Extensions/ResultExtensions.cs).
