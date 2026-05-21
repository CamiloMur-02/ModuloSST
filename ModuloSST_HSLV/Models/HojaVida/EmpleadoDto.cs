namespace ModuloSST_HSLV.Models.HojaVida
{
    /// <summary>
    /// Objeto de transferencia de datos del empleado.
    /// Se usa para devolver la información del funcionario
    /// al frontend vía AJAX desde EmpleadoController.
    /// </summary>
    public class EmpleadoDto
    {
        public int? IdEmpleado { get; set; }
        public string Cedula { get; set; }
        public string NombreCompleto { get; set; }
        public string Genero { get; set; }
        public int Edad { get; set; }
        public string Cargo { get; set; }
        public string Contratista { get; set; }
        public string TiempoPrestacion { get; set; }
        public int? IdProceso { get; set; }
        public string NombreProceso { get; set; }
    }
}