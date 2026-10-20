using System;
using BE;
using BLL;

class Program
{
    static void Main()
    {
        try {
            UsuarioBLL bll = new UsuarioBLL();
            bool ok = bll.Login(""t001"", ""1234"");
            Console.WriteLine(""Login success: "" + ok);
        } catch (Exception ex) {
            Console.WriteLine(""Error: "" + ex.ToString());
        }
    }
}
