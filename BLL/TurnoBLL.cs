using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class TurnoBLL
    {
        private TurnoDAL dal = new TurnoDAL();

        public List<Turno> ObtenerActivos()
        {
            return dal.ObtenerActivos();
        }
    }
}
