# ChatsHub — сообщения

Подключение: `/hubs/chats`, Bearer access token и активная сессия. Команды возвращают `HubResult<T>` (status, result, errorMessage); события приходят независимо от ответа команды. Этот раздел описывает рабочую копию с исправлениями от 03.10.2026.

## Команды

| Имя | Аргументы | Успешный result |
| --- | --- | --- |
| Send | MessageRequest | UserMessageResponse |
| Read | `{ messageId }` | MessageReadResponse |
| ReadChat | chatId, ReadChatRequest | ChatReadResponse |
| Edit | `{ messageId, newEncryptedContent }` | MessageEditResponse |
| Remove | `{ messageId, requestType }` | MessageRemovedResponse |
| React | messageId, `{ reactionId }` | MessageReactionsChangedResponse |
| RemoveReaction | messageId | MessageReactionsChangedResponse |

```json
{
  "recipientId": "11111111-1111-1111-1111-111111111111",
  "recipientType": 0,
  "encryptedContent": "<содержимое>",
  "replyToMessageId": null,
  "mediaAttachments": []
}
```

RecipientType.User=0 означает личный чат: recipientId — PrivateChat.Id. Group=1 означает ChatGroup.Id. Вложение: `{ mediaId, encryptedKey }`. Содержимое или вложение обязательно; Send ограничивает текст 50 000 символами. SignalR не выполняет MVC DataAnnotations автоматически. DTO проверки дополняются сервисами доступа.

Send/Read/React и чтение истории проверяют участие в конкретном чате. В группах заблокированный участник не имеет доступа. Reply и вложения не позволяют ссылаться на чужие закрытые чаты. Edit проверяет автора, доступ и временной лимит редактирования. Remove проверяет автора/права администратора группы; HideForMe пока не реализован и возвращает ошибку, ForceRemove удаляет сообщение.

[ReadChat и три сценария прочтения](../endpoints/message-read.md): видимые messageIds, граница upToMessageId либо весь чат. [React/RemoveReaction, паки и настройки канала](../endpoints/reactions.md) используют тот же сервис, что HTTP API.

## События

| Имя события (точно) | Payload |
| --- | --- |
| ReceiveMessage | UserMessageResponse |
| MessageSent | UserMessageResponse, подтверждение устройствам отправителя |
| MessageEdited | MessageEditResponse |
| MessageRemoved | MessageRemovedResponse |
| MessageReaded | MessageReadResponse |
| ChatRead | ChatReadResponse |
| MessageReactionsChanged | MessageReactionsChangedResponse |
| ChannelReactionPolicyChanged | ChannelReactionPolicyResponse |

Опечатка MessageReaded сохранена для совместимости. При подключении сервер автоматически добавляет соединение в группу его сессии. События чата направляются активным сессиям участников; заблокированные участники групп исключаются. Не требуется вручную подписываться на SignalR-группу чата. Отзыв сессии прекращает её участие в маршрутизации и проверяется для команд; истечение токена закрывает соединение.

UserMessageResponse: messageId, senderId, recipientId, recipientType, encryptedContent, replyToMessageId, sentAt, isEdited, mediaAttachments. История через HTTP также содержит реакции и reactionsVersion. После получения сообщения загрузите снимок реакций при необходимости.

MessageEditResponse: messageId, editorId, recipientId, recipientType, newEncryptedContent, editedAt.
MessageReadResponse: viewId, messageId, readerId, recipientId, whenWas, recipientType.
MessageRemovedResponse: messageId, senderId, recipientId, requestType, recipientType.

## Доставка и восстановление

Сохранение в БД и доставка события — разные этапы. После reconnect загрузите историю через HTTP; объединяйте ответ Send и ReceiveMessage по messageId. Для реакций принимайте более новую version и заменяйте counts целиком, затем восстанавливайте состояние видимых сообщений через GET.

SignalR-события не имеют долговременной очереди. Push сообщений использует серверную очередь и повторные попытки; для реакций и политик каналов push не отправляется. Реальный push требует корректной конфигурации Firebase и зарегистрированного FCM-токена активной сессии. Повтор Send после неоднозначного сетевого сбоя всё ещё может создать дубликат: clientMessageId/idempotency-key для сообщений не реализован.
