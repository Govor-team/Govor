# Администрирование и незавершённые группы

## Административные маршруты

Следующая таблица показывает фактические атрибуты, а не требуемую безопасную политику.

| Метод | Маршрут | Защита сейчас |
| --- | --- | --- |
| POST | /api/admin/InviteUser/Invitation | Без Authorize |
| GET | /api/admin/InviteUser | Без Authorize |
| GET | /api/admin/InviteUser/GetAllActiveInvitations | Без Authorize |
| GET | /api/admin/InviteUser/{id} | Без Authorize |
| GET | /api/admin/Users/all | Любой аутентифицированный пользователь |
| GET | /api/admin/Users/{id} | Любой аутентифицированный пользователь |
| GET | /api/admin/Users/user/{id}/setpassword/{password} | Любой аутентифицированный пользователь |

CreateInvitationRequest: endDate, maxParticipants, isAdmin, description. Генератор возвращает строковый код в JSON (200). Списки приглашений возвращают id, description, isAdmin, maxParticipants, code, createdAt, endAt, isActive, participantCount.

Users/all фактически ограничен 50 записями без стабильной пагинации. Ответ UserResponse включает passwordHash, что недопустимо для клиентского контракта. Смена пароля выполняется GET и кладёт пароль в URL.

Эти маршруты нужно закрыть ролью Admin до публичного использования. Создание административного приглашения вместе с регистрацией позволяет получить роль Admin. Не подключайте их к пользовательскому интерфейсу.

## Группы

GET /invite/{code} задуман как вход в группу и требует Authorize. IGroupService не зарегистрирован в AddServices, а _currentUser не присваивается. Маршрут не является рабочим сценарием вступления.

GroupsHub пуст и не подключён MapHub. Административный FriendshipsController содержит только закомментированные действия, поэтому рабочих endpoints у него нет.

Наличие моделей ChatGroup/GroupMembership/GroupInvitation и проверки участия в групповом Send не подтверждает готовый продуктовый сценарий групп и каналов.

---
Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/AdminStuff/UsersController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/AdminStuff/UsersController.cs).

