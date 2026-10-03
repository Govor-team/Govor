---
icon: wifi
---

# ChatsHub — сообщения

Подключение: /hubs/chats, Bearer access token. Серверный класс называется ChatsHub. Команды возвращают HubResult, события приходят независимо от ответа команды.

## Команды

| Имя    | Запрос                             | Успешный Result        |
| ------ | ---------------------------------- | ---------------------- |
| Send   | MessageRequest                     | UserMessageResponse    |
| Read   | { messageId }                      | MessageReadResponse    |
| Edit   | { messageId, newEncryptedContent } | MessageEditResponse    |
| Remove | { messageId, requestType }         | MessageRemovedResponse |

MessageRequest:

```json
{
  "recipientId":"11111111-1111-1111-1111-111111111111",
  "recipientType":0,
  "encryptedContent":"<содержимое>",
  "replyToMessageId":null,
  "mediaAttachments":[]
}
```

RecipientType.User=0 означает **личный чат**, recipientId — PrivateChat.Id. Group=1 означает ChatGroup.Id. Вложение: { mediaId, encryptedKey }.

Содержимое или вложение обязательно. Хаб ограничивает текст 50 000 символами, хотя атрибут DTO указывает 100 000. SignalR не выполняет MVC DataAnnotations автоматически; транспортный лимит входящего сообщения отдельно ограничивает размер JSON. При null encryptedContent с вложением возможна ошибка на Length.

## События

| Имя события (точно) | Payload                |
| ------------------- | ---------------------- |
| ReceiveMessage      | UserMessageResponse    |
| MessageEdited       | MessageEditResponse    |
| MessageRemoved      | MessageRemovedResponse |
| MessageReaded       | MessageReadResponse    |

Опечатка MessageReaded является текущим именем wire-контракта. Константа MessageSent есть, но отправка этого события закомментирована; не ожидайте его.

UserMessageResponse: messageId, senderId, recipientId, recipientType, encryptedContent, replyToMessageId, sentAt, isEdited, mediaAttachments. mediaAttachments — список MediaFile, не HTTP MediaAttachmentResponse; навигация MediaFile после Send явно не загружается и элементы могут быть null.

MessageEditResponse: messageId, editorId, recipientId, recipientType, newEncryptedContent, editedAt. MessageReadResponse: viewId, messageId, readerId, recipientId, whenWas, recipientType. MessageRemovedResponse: messageId, senderId, recipientId, requestType, recipientType.

## Семантика и ошибки

Edit проверяет автора и срок из MessageEditingOptions (по умолчанию 15 минут при Enabled=true), но не применяет лимит текста Send. Многие сервисные ошибки хаб превращает в InvalidOperationException и возвращает status=500, теряя исходный код.

RemoveMessageRequestType: HideForMe=0, ForceRemove=1. **HideForMe сейчас не реализован**: если автор вызывает Remove, сервис физически удаляет сообщение независимо от флага. Групповое удаление сравнивает DeleterId с ID группы вместо автора/прав администратора.

Read проверяет наличие у пользователя любого личного чата вместо участия в чате сообщения. Для групп прочтение не реализовано; хаб может упасть на First при отсутствии MessageView. Параллельные Read могут конфликтовать по уникальному индексу (MessageId, UserId).

Уведомления Edit/Remove/Read в личных чатах отправляются инициатору и группе с именем ID чата, хотя второй адресат должен быть участником. Поэтому собеседник может не получить событие.

Send для личного чата проверяет только его существование, не участие отправителя. Исправление доступа обязательно.

## Доставка

Успех сохранения и доставка события — разные этапы. После reconnect загрузите историю через HTTP, объединяйте ответ Send и ReceiveMessage по messageId. Без clientMessageId/idempotency-key повтор Send после сбоя может создать дубль.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; сервер с БД и Firebase в рамках диагностики не запускался.

Источник: [Govor.API/Hubs/ChatsHub.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Hubs/ChatsHub.cs).
