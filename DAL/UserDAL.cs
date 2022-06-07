using System;
using System.Linq;
namespace DAL
{
    public class UserDAL
    {
        /*public static int AddOrUpdate(Entity.User user)
        {
            using (var data = new bakaContext())
            {
                var user1 = data.Users.FirstOrDefault(item => item.Name == user.Name);
                if (user1 == null)
                {
                    user1 = new User();
                    data.Users.Add(user1);
                }
                user1.Name = user.Name;
                user1.Password = user.Password;
                user1.Total = user.Total;
                data.SaveChanges();
                return user.Id;
            }
        }*/
    }
}
