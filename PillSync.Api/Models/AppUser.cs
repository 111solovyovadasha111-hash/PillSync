using Microsoft.AspNetCore.Identity;

namespace PillSync.Api.Models;

// Наследуемся от стандартного IdentityUser.
// Это даст нам Id (string), Email, PasswordHash и другие нужные поля.
public class AppUser : IdentityUser
{
    // Пока здесь ничего не добавляем. 
    // В будущем сюда можно добавить, например, полное имя, дату рождения и т.д.
}