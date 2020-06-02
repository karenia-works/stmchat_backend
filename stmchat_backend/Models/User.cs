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
        public string id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string key { get; set; }
        public void hashMyPassword()
        {
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);
            var hashed = new HMACSHA256(key).ComputeHash(new UTF8Encoding().GetBytes(this.password));
            this.password = System.Convert.ToBase64String(hashed);
            this.key = System.Convert.ToBase64String(key);
        }
        public bool checkPassword(string incoming)
        {
            var key = System.Convert.FromBase64String(this.key);
            var hashed = new HMACSHA256(key).ComputeHash(new UTF8Encoding().GetBytes(incoming));
            var realPassword = System.Convert.FromBase64String(this.password);
            Console.WriteLine(System.Convert.ToBase64String(hashed));
            return slowByteEq(realPassword, hashed);
        }

        private static bool slowByteEq(byte[] byteArray, byte[] other)
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