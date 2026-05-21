using System;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models.HojaVida;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador de servicio compartido — Consulta de Empleados
    /// 
    /// Provee endpoints AJAX consumidos por los 6 módulos SST:
    ///  - Inspección de Bioseguridad
    ///  - Matriz EPP
    ///  - Reporte de Accidente de Trabajo
    ///  - Reporte de Enfermedad Laboral
    ///  - Reporte de Incidente
    ///  - Pausas Activas
    /// 
    /// Fuente de datos: BD externa HOJAVIDA_EST (solo lectura).
    /// No realiza escrituras sobre ninguna base de datos.
    /// </summary>
    public class EmpleadoController : Controller
    {
        #region [1] Campos y Dependencias

        /// <summary>Contexto de la BD externa HOJAVIDA_EST.</summary>
        private readonly HojaVidaContext hvDb = new HojaVidaContext();

        #endregion

        #region [2] Endpoints AJAX

        /// <summary>
        /// [2.1] Busca un empleado por cédula y retorna sus datos en JSON.
        /// 
        /// Construye el objeto EmpleadoDto con:
        ///  - Nombre completo (apellidos + nombres)
        ///  - Género (1=Masculino, 2=Femenino)
        ///  - Edad calculada desde fechanacimiento
        ///  - Cargo desde Hojavida_cargo
        ///  - Contratista desde Hojavida_sindicato
        ///  - Meses de prestación calculados desde fechaingreso
        ///  - Proceso desde Hojavida_proceso
        /// 
        /// GET: /Empleado/BuscarPorCedula?cedula=12345678
        /// </summary>
        /// <param name="cedula">Número de identificación del empleado.</param>
        public JsonResult BuscarPorCedula(string cedula)
        {
            // [2.1.1] Validar parámetro
            if (string.IsNullOrWhiteSpace(cedula))
                return Json(
                    new { encontrado = false, mensaje = "La cédula no puede estar vacía." },
                    JsonRequestBehavior.AllowGet);

            // [2.1.2] Buscar funcionario por cédula
            var func = hvDb.Funcionarios
                .FirstOrDefault(f => f.iden == cedula.Trim());

            if (func == null)
                return Json(
                    new { encontrado = false, mensaje = "No se encontró ningún empleado con esa cédula." },
                    JsonRequestBehavior.AllowGet);

            // [2.1.3] Cargo
            var cargo = func.oidcargo.HasValue
                ? hvDb.Cargos.FirstOrDefault(c => c.oid == func.oidcargo.Value)
                : null;

            // [2.1.4] Datos laborales — tomar el registro más reciente
            var laboral = hvDb.DatosLaborales
                .Where(d => d.oidfuncionario == func.oid)
                .OrderByDescending(d => d.fechaingreso)
                .FirstOrDefault();

            // [2.1.5] Sindicato (contratista)
            var sindicato = (laboral != null && laboral.sindicato.HasValue)
                ? hvDb.Sindicatos.FirstOrDefault(s => s.oid == laboral.sindicato.Value)
                : null;

            // [2.1.6] Proceso
            var proceso = (laboral != null && laboral.oidproceso.HasValue)
                ? hvDb.Procesos.FirstOrDefault(p => p.oid == laboral.oidproceso.Value)
                : null;

            // [2.1.7] Calcular edad
            int edad = 0;
            if (func.fechanacimiento.HasValue)
            {
                var hoy = DateTime.Today;
                var fnac = func.fechanacimiento.Value;
                edad = hoy.Year - fnac.Year;
                if (fnac.Date > hoy.AddYears(-edad)) edad--;
            }

            // [2.1.8] Calcular meses de prestación de servicio
            string meses = "0";
            if (laboral != null && laboral.fechaingreso.HasValue)
            {
                var hoy = DateTime.Today;
                var ingres = laboral.fechaingreso.Value;
                int totalMeses = (hoy.Year - ingres.Year) * 12
                               + (hoy.Month - ingres.Month);
                if (totalMeses < 0) totalMeses = 0;
                meses = totalMeses.ToString();
            }

            // [2.1.9] Resolver género
            string genero = func.sexo == 1 ? "Masculino"
                          : func.sexo == 2 ? "Femenino"
                          : "Otro";

            // [2.1.10] Construir nombre completo (Apellido1 Apellido2 Nombre1 Nombre2)
            string nombreCompleto = string.Join(" ",
                new[]
                {
                    func.primerapellido,
                    func.segundoapellido,
                    func.primernombre,
                    func.segundonombre
                }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            // [2.1.11] Construir DTO de respuesta
            var dto = new EmpleadoDto
            {
                IdEmpleado = func.oid,
                Cedula = func.iden,
                NombreCompleto = nombreCompleto,
                Genero = genero,
                Edad = edad,
                Cargo = cargo != null ? cargo.cargo : "",
                Contratista = sindicato != null ? sindicato.nombre : "",
                TiempoPrestacion = meses,
                IdProceso = laboral != null ? laboral.oidproceso : null,
                NombreProceso = proceso != null ? proceso.proceso : ""
            };

            return Json(
                new { encontrado = true, empleado = dto },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// [2.2] Retorna la lista de subprocesos asociados a un proceso.
        /// Usado para poblar el select de subproceso tras el autocompletado.
        /// 
        /// GET: /Empleado/SubprocesosPorProceso?idProceso=3
        /// </summary>
        /// <param name="idProceso">ID del proceso (oid de Hojavida_proceso).</param>
        public JsonResult SubprocesosPorProceso(int idProceso)
        {
            var subprocesos = hvDb.Subprocesos
                .Select(s => new
                {
                    id = s.oid,
                    nombre = s.gdsubpronom
                })
                .OrderBy(s => s.nombre)
                .ToList();
            return Json(subprocesos, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region [3] Liberación de Recursos

        /// <summary>
        /// [3.1] Libera los recursos del contexto de HojaVida.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) hvDb.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}