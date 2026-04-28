using System.Collections.Generic;

namespace ModuloSST_HSLV.Helpers
{
    /// <summary>
    /// Catálogo de causas y subcausas de accidentes/incidentes
    /// del Hospital Susana López de Valencia.
    /// </summary>
    public static class CausasHelper
    {
        public static Dictionary<string, string[]> ObtenerCausasSubcausas()
        {
            return new Dictionary<string, string[]>
            {
                { "Manejo inadecuado de cortopunzantes", new[] {
                    "Falta de autocuidado",
                    "Reencapuchar agujas",
                    "Manejo inseguro durante procedimientos (suturas, punciones)",
                    "Falta de solicitud de apoyo en pacientes con alteraciones",
                    "Manejo inadecuado por terceros",
                    "Segregación inadecuada de residuos cortopunzantes",
                    "Disposición incorrecta de residuos" } },
                { "Posturas y movimientos inadecuados / ergonomía", new[] {
                    "Posturas inadecuadas", "Movimientos inadecuados",
                    "Sobreesfuerzo", "Fuerza excesiva en labores",
                    "Movimientos en falso", "Levantamiento incorrecto",
                    "Falta de autocuidado en manipulación de cargas" } },
                { "Caídas y resbalones", new[] {
                    "Piso húmedo / superficies resbalosas", "Falta de señalización",
                    "Baja iluminación", "Desniveles sin demarcar",
                    "Pisada en falso / torsión", "Caídas en gradas",
                    "Cerámica o piso liso", "Exceso de confianza" } },
                { "Movilización inadecuada de pacientes", new[] {
                    "Técnica incorrecta", "Falta de apoyo del personal",
                    "No solicitar ayuda",
                    "Uso de elementos desgastados (sábanas, camillas)" } },
                { "Fallas en equipos, mobiliario e infraestructura", new[] {
                    "Mobiliario defectuoso (sillas, gabinetes, pasamanos)",
                    "Falta de mantenimiento de equipos biomédicos",
                    "Fallas en ambulancia (piso, camilla)" } },
                { "Uso inadecuado o falta de EPP y dispositivos", new[] {
                    "Uso incorrecto de dispositivos de seguridad",
                    "Falta de aseguramiento del EPP",
                    "Herramientas mal utilizadas" } },
                { "Condiciones ambientales inseguras", new[] {
                    "Iluminación inadecuada", "Pisos en mal estado" } },
                { "Falta de autocuidado (general)", new[] {
                    "Exceso de confianza",
                    "Actividades rutinarias sin precaución",
                    "Correr, caminar sin cuidado",
                    "No asegurar barandas" } },
                { "Procedimientos inseguros", new[] {
                    "Falta de adherencia a protocolos",
                    "Falta de insumos adecuados",
                    "Administración insegura de medicamentos",
                    "Procedimientos clínicos inseguros" } },
                { "Accidentes de tránsito", new[] { "Volcamiento" } },
                { "Actividades deportivas", new[] {
                    "Deportes de alto impacto (fútbol, voleibol y baloncesto)" } },
                { "Fatiga y condiciones fisiológicas", new[] {
                    "Fatiga laboral", "Sudoración excesiva" } },
                { "Exposición a agentes biológicos", new[] {
                    "Picadura por (insectos, larvas, etc)",
                    "Mordedura por (insectos, larvas, etc)" } },
                { "Factores externos / terceros", new[] {
                    "Acciones inseguras de terceros",
                    "Bloqueos violentos",
                    "Pacientes agresivos (falta de contención adecuada)" } }
            };
        }
    }
}