using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Evaluación
    /// 
    /// Representa el registro de documentos asociados a evaluaciones del sistema SST
    /// (inspecciones, análisis de puesto, riesgo psicosocial, bioseguridad, etc.).
    /// 
    /// Reglas de negocio:
    /// - El TipoEvaluacion es obligatorio y sus valores válidos se controlan en el controlador.
    /// - Cada registro debe tener un archivo asociado (NombreArchivo y RutaArchivo).
    /// - Funciona como repositorio documental independiente dentro del sistema.
    /// </summary>
    [Table("Evaluacion")]
    public class Evaluacion
    {
        #region [1] Clave primaria

        [Key]
        public int IdEvaluacion { get; set; }

        #endregion

        #region [2] Datos de la evaluación

        /// <summary>
        /// Tipo de evaluación registrada.
        /// Ejemplos:
        /// - Inspección de áreas y/o puesto de trabajo
        /// - Evaluación de Riesgo Psicosocial
        /// - Análisis de puesto de trabajo
        /// - Registro de inspección de bioseguridad
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar el tipo de evaluación.")]
        [StringLength(60)]
        [Display(Name = "Tipo de Evaluación")]
        public string TipoEvaluacion { get; set; }

        /// <summary>
        /// Nombre original del archivo cargado por el usuario.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; }

        /// <summary>
        /// Ruta física o virtual donde se almacena el archivo en el servidor.
        /// </summary>
        [Required]
        [StringLength(500)]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; }

        /// <summary>
        /// Fecha en la que se realizó la carga del archivo.
        /// </summary>
        [Required]
        [Display(Name = "Fecha de Carga")]
        public DateTime FechaCarga { get; set; }

        #endregion

        #region [3] Auditoría

        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación.
        /// </summary>
        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        #endregion

        #region [4] Relaciones

        // Este modelo no define relaciones de navegación directas.
        // Funciona como repositorio documental independiente dentro del sistema.

        #endregion
    }
}