# ProfileHub — изменения профиля

Подключение: /hubs/profiles; требуется Authorize.

| Метод          | Аргумент           | Успех                              |
| -------------- | ------------------ | ---------------------------------- |
| SetDescription | string description | HubResult, status=200, result=true |
| SetAvatar      | Guid iconId        | HubResult, status=200, result=true |

Описание не может быть null, переносы строк нормализуются, длина после нормализации ≤500; пустая строка допустима для очистки. Avatar требует непустой Guid и существующую запись медиа.

| Событие            | Payload                 |
| ------------------ | ----------------------- |
| DescriptionUpdated | { userId, description } |
| AvatarUpdated      | { userId, iconId }      |

Получатели: пользователь, друзья и потенциальные друзья согласно IFriendshipService. Группы имеют префикс user:.

## Ограничения

Result от записи профиля не проверяется хабом, поэтому true не всегда подтверждает запись. Ошибка рассылки логируется, но не меняет успешный ответ.

SetAvatar не проверяет владельца файла. REST UploadAvatar не отправляет AvatarUpdated. Политика приватности не применяется к этой рассылке. После reconnect перечитайте профиль; события не гарантированно сохраняются.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; сервер с БД и Firebase в рамках диагностики не запускался.

Источник: [Govor.API/Hubs/ProfileHub.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Hubs/ProfileHub.cs).
