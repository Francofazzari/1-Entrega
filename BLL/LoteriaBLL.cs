using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class LoteriaBLL
    {
        private LoteriaDAL dal = new LoteriaDAL();

        public List<Loteria> ObtenerActivas()
        {
            return dal.ObtenerActivas();
        }
    }
}
