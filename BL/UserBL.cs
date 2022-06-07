using System;

namespace BL
{
    public class UserBL
    {
        /*public static int AddOrUpdate(Entity.User user)
        {
            return DAL.UserDAL.AddOrUpdate(user);
            
        }*/
        public void Registration(Entity_new.Authorization auth)
        {
            new DAL.AuthorizationDAL().Registration(auth);

        }
    }
}
