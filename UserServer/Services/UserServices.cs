﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserServer.Exceptions;

namespace UserServer.Services
{
    public class UserServices : MarshalByRefObject
    {
        private static List<User> Users { get; set; }

        public UserServices()
        {
            if(Users == null)
            {
                Users = new List<User>();
                User testUser = new User();
                testUser.Nickname = "rickrolls";
                testUser.AvatarRoute = "c:/notporn/homework/tests.mp4";
                Users.Add(testUser);
            }
        }

        private List<User> GetUsers()
        {
            if(Users == null)
            {
                Users = new List<User>();
            }
            return Users;
        }

        public List<User> GetAll()
        {
            return GetUsers();
        }

        public void Add(User user)
        {
            List<User> users = GetUsers();
            users.Add(user);
        }

        public User Get(string nickname)
        {
            User user = GetUsers().FirstOrDefault<User>(u => u.Nickname == nickname);
            if(user == null)
            {
                throw new ElementNotFoundException("User does not exist");
            }
            return user;
        }

        public void Delete(string nickname)
        {
            User user = Get(nickname);
            if(user == null)
            {
                throw new ElementNotFoundException("User does not exist");
            }
            GetUsers().Remove(user);
        }

        public void Update(User updatedUser, string nickname)
        {
            Delete(nickname);
            Add(updatedUser);
        }
    }

    public class User
    {
        public string Nickname { get; set; }
        public string AvatarRoute { get; set; }
    }
}