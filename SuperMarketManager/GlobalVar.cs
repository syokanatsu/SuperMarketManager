using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarketManager
{
    class GlobalVar
    {
        public static Loginer login = null;
    }

    class Loginer
    {
        private string username;
        private int id;

        public string Username { get => username; set => username = value; }
        public int Id { get => id; set => id = value; }

        public virtual bool GetPermission()
        {
            return false;
        }
    }

    class SuperAdmin : Loginer
    {
        public override bool GetPermission()
        {
            return true;
        }
    }

    class Admin : Loginer
    {
        public override bool GetPermission()
        {
            return false;
        }
    }
}
