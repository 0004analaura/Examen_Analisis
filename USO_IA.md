# Uso de Inteligencia Artificial — Examen_Analisis / NetGuardGT.Api

## Prompts utilizados

1. **Prompt principal del examen:**
   > Desarrollar un proyecto completo para examen final de Análisis de Sistemas: API REST para gestión de incidentes de Net Guard GT con ASP.NET Core, EF Core, SQLite, reglas de negocio específicas, pruebas xUnit, README, despliegue en Render y documentación Swagger.

2. **Prompts de refinamiento durante el desarrollo:**
   - Renombrar el proyecto a `NetGuardGT.Api` y `NetGuardGT.Tests`.
   - Corregir endpoints de reportes con `GroupBy` en memoria para SQLite.
   - Eliminar interfaz web y usar Swagger como herramienta de prueba.
   - Agregar historias de usuario al README.
   - Preparar despliegue en Render con Docker.

## Reflexión personal

*(Completar manualmente antes de entregar)*

La IA fue útil para acelerar la estructura base del proyecto y asegurar que no se omitieran requisitos del enunciado (endpoints, pruebas, documentación). Sin embargo, las reglas de negocio del dominio (telecomunicaciones, SLA, especialidades de técnicos) requieren comprensión humana para validar que la implementación refleje correctamente el caso de Net Guard GT.

Ejemplo de reflexión:
> "Utilicé IA para generar el esqueleto del proyecto y las pruebas unitarias. Revisé manualmente que las transiciones de estado coincidieran con el flujo del examen, ajusté los tiempos de SLA y personalicé las historias de usuario. La API la probé con Swagger verificando asignaciones, escalamiento y reportes."

## Correcciones realizadas

*(Completar manualmente — ejemplos sugeridos)*

- Verificar que los enums de especialidad coincidan con los tipos de incidente.
- Probar manualmente la asignación con 3 incidentes activos.
- Confirmar que el endpoint de escalación funciona con incidentes críticos sin atención.
- Corregir reportes que fallaban con `GroupBy` en Entity Framework Core + SQLite.
- Revisar ortografía y datos de técnicos de ejemplo en la base de datos inicial.

## Qué partes fueron ajustadas manualmente

*(Completar manualmente antes de entregar)*

| Archivo | Qué revisar |
|---------|-------------|
| `README.md` | Historias de usuario y datos personales |
| `USO_IA.md` | Reflexión personal y correcciones |
| `Data/DbInitializer.cs` | Datos de prueba (nombres, sitios) |
| `Services/IncidenteService.cs` | Lógica de negocio (validar comprensión) |
| `Services/ReglasNegocio.cs` | Transiciones de estado y SLA |

## Herramientas de IA utilizadas

- **Cursor** — asistente de código para estructurar, generar y revisar el proyecto.

## Declaración de uso responsable

La IA se utilizó como **apoyo** para estructurar el proyecto, generar ideas, revisar lógica y mejorar documentación. Las decisiones finales, validaciones de reglas de negocio, pruebas y ajustes fueron revisados manualmente para cumplir los requisitos del examen.
