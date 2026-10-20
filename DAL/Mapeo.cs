using System;
using System.Data;
using BE;

namespace DAL
{
    // Columnas y lectura de juegos y sorteos, compartidas por las consultas que los traen
    // junto con otras tablas (jugadas, ventas, pagos).
    internal static class Mapeo
    {
        public const string ColumnasJuego = @"
            j.Id AS JuegoId, j.Nombre AS JuegoNombre, j.Codigo AS JuegoCodigo, j.CantidadNumeros,
            j.NumeroMin, j.NumeroMax, j.CantidadSorteados, j.UsaRango, j.UsaLoteriaTurno, j.Precio,
            j.Activo AS JuegoActivo";

        // Un sorteo con su juego, loteria y turno (loteria y turno pueden venir en NULL).
        public const string ColumnasSorteo = @"
            s.Id AS SorteoId, s.Fecha AS SorteoFecha, s.Estado AS SorteoEstado,
            l.Id AS LoteriaId, l.Nombre AS LoteriaNombre, l.Codigo AS LoteriaCodigo, l.Activa AS LoteriaActiva,
            t.Id AS TurnoId, t.Nombre AS TurnoNombre, t.Activo AS TurnoActivo, " + ColumnasJuego;

        public const string JoinsSorteo = @"
            JOIN JUEGOS j ON j.Id = s.IdJuego
            LEFT JOIN LOTERIAS l ON l.Id = s.IdLoteria
            LEFT JOIN TURNOS t ON t.Id = s.IdTurno";

        // Nombre completo del usuario, a partir del alias u de USUARIOS.
        public const string NombreUsuario = "LTRIM(RTRIM(ISNULL(u.Nombre, '') + ' ' + ISNULL(u.Apellido, '')))";

        public static Juego LeerJuego(IDataRecord r)
        {
            return new Juego
            {
                Id = (int)r["JuegoId"],
                Nombre = (string)r["JuegoNombre"],
                Codigo = (string)r["JuegoCodigo"],
                CantidadNumeros = (int)r["CantidadNumeros"],
                NumeroMin = (int)r["NumeroMin"],
                NumeroMax = (int)r["NumeroMax"],
                CantidadSorteados = (int)r["CantidadSorteados"],
                UsaRango = (bool)r["UsaRango"],
                UsaLoteriaTurno = (bool)r["UsaLoteriaTurno"],
                Precio = r["Precio"] == DBNull.Value ? (decimal?)null : (decimal)r["Precio"],
                Activo = (bool)r["JuegoActivo"]
            };
        }

        public static Sorteo LeerSorteo(IDataRecord r)
        {
            return new Sorteo
            {
                Id = (int)r["SorteoId"],
                Fecha = (DateTime)r["SorteoFecha"],
                Estado = (string)r["SorteoEstado"],
                Juego = LeerJuego(r),
                Loteria = r["LoteriaId"] == DBNull.Value ? null : new Loteria
                {
                    Id = (int)r["LoteriaId"],
                    Nombre = (string)r["LoteriaNombre"],
                    Codigo = (string)r["LoteriaCodigo"],
                    Activa = (bool)r["LoteriaActiva"]
                },
                Turno = r["TurnoId"] == DBNull.Value ? null : new Turno
                {
                    Id = (int)r["TurnoId"],
                    Nombre = (string)r["TurnoNombre"],
                    Activo = (bool)r["TurnoActivo"]
                }
            };
        }
    }
}
