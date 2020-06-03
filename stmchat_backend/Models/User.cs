//登录验证相关，这个刘子暄搞

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace stmchat_backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Key { get; set; }

        public void HashMyPassword()
        {
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);
            var hashed = new HMACSHA256(key).ComputeHash(new UTF8Encoding().GetBytes(this.Password));
            this.Password = System.Convert.ToBase64String(hashed);
            this.Key = System.Convert.ToBase64String(key);
        }

        public bool CheckPassword(string incoming)
        {
            var key = System.Convert.FromBase64String(this.Key);
            var hashed = new HMACSHA256(key).ComputeHash(new UTF8Encoding().GetBytes(incoming));
            var realPassword = System.Convert.FromBase64String(this.Password);
            Console.WriteLine(System.Convert.ToBase64String(hashed));
            return SlowByteEq(realPassword, hashed);
        }

        private static bool SlowByteEq(byte[] byteArray, byte[] other)
        {
            if (byteArray.Length != other.Length) return false;
            var eq = true;
            for (int i = 0; i < byteArray.Length; i++)
            {
                eq = eq && byteArray[i] == other[i];
            }

            return eq;
        }
    }
}