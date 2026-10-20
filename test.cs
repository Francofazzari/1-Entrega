using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("1234"));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            Console.WriteLine(sb.ToString());
        }
    }
}
