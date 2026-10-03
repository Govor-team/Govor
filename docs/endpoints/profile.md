# Профиль и аватар

Требуется роль User/Admin.

| Метод | Маршрут                    | Успех                                       |
| ----- | -------------------------- | ------------------------------------------- |
| GET   | /api/profile/download/me   | 200, UserProfileDto текущего пользователя   |
| GET   | /api/profile/download/{id} | 200, UserProfileDto указанного пользователя |
| POST  | /api/profile/avatar        | 200, MediaUploadResult                      |

UserProfileDto: id, username, description (nullable), iconId (nullable), isOnline. Отсутствующий пользователь — 404 ProblemDetails.

Загрузка аватара: multipart/form-data с FromFile, Type, MimeType. В отличие от MediaController, отдельный лимит 20 MB не задан; действуют ограничения хоста. Контроллер сохраняет медиа с OwnerType.Avatar и устанавливает IconId.

Описание изменяется через ProfileHub.SetDescription. ProfileHub.SetAvatar принимает существующий iconId и рассылает AvatarUpdated.

## Ограничения

REST UploadAvatar не рассылает AvatarUpdated. Клиентам может потребоваться отдельное обновление данных. В ProfileHub.SetAvatar проверяется существование файла, но не его загрузчик/тип/владелец.

Хаб игнорирует Result от SetDescription и SetNewIcon, поэтому успех true не всегда подтверждает изменение. MappingProfile дважды регистрирует UserProfile → UserProfileDto; это нужно устранить и проверить isOnline.

Чтение чужого профиля не применяет PrivacyUserSettings. Наличие моделей приватности не означает действующую политику доступа.

***

Проверено по исходникам на 03.10.2026, commit `4603be9`. Описано текущее поведение; успешная сборка не означает проверку работающего сервера.

Источник: [Govor.API/Controllers/ProfileController.cs](https://github.com/Govor-team/Govor/blob/4603be9f714135e0af72d505d17f3d1709834bea/Govor.API/Controllers/ProfileController.cs).
