# Sistema de Gestión de Incidentes - Net Guard GT

> **Repositorio GitHub:** [Examen_Analisis](https://github.com/0004analaura/Examen_Analisis)  
> **Proyecto API:** `NetGuardGT.Api` · **Pruebas:** `NetGuardGT.Tests` · **Solución:** `NetGuardGT.sln`

## Descripción del proyecto

Prototipo funcional de una **API REST** para la gestión de incidentes de red de **Net Guard GT**, empresa de telecomunicaciones en Guatemala. El sistema centraliza el registro, asignación, seguimiento y reporte de incidentes, reemplazando el flujo manual basado en Excel y llamadas telefónicas.

Incluye:
- API REST con ASP.NET Core
- Base de datos SQLite con Entity Framework Core
- Pruebas unitarias con xUnit
- Documentación Swagger
- Preparación para despliegue en Render

## Contexto del problema

Net Guard GT opera **45 sitios de red** en Guatemala con **12 técnicos** especializados (fibra óptica, microondas y sistemas eléctricos). Se reportan aproximadamente **80 incidentes mensuales**, gestionados manualmente, lo que provoca:

- Pérdida de información
- Incidentes sin seguimiento
- Dificultad para reportes de cumplimiento de SLA
- Sobrecarga de técnicos

Este prototipo demuestra una solución digital con reglas de negocio automatizadas.

## Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| C# | Lenguaje principal |
| ASP.NET Core Web API | Backend REST |
| Entity Framework Core | ORM y acceso a datos |
| SQLite | Base de datos embebida |
| xUnit | Pruebas unitarias |
| Swagger | Documentación y prueba de endpoints |
| Docker | Despliegue en Render |

## Reglas de negocio implementadas

1. **SLA por severidad:** Baja 48h, Media 24h, Crítica 8h, Urgente 4h.
2. **Carga máxima:** Un técnico no puede tener más de 3 incidentes activos (Asignado, En Progreso, Escalado).
3. **Flujo de estados:** Registrado → Asignado → En Progreso → Resuelto → Cerrado (sin saltos).
4. **Escalado:** Incidentes Críticos/Urgentes en estado Registrado por más de 2 horas se escalan.
5. **Especialidad:** Solo técnicos con especialidad coincidente pueden atender un tipo de incidente.
6. **Reasignación:** Permitida antes de cerrar; libera al técnico anterior.
7. **Historial:** Cada cambio de estado queda registrado con fecha, observación y técnico.
8. **Incidentes cerrados:** No pueden modificarse.

## Historias de usuario

### HU-01 — Registro de incidente

**Como** operador del sistema,  
**quiero** registrar un nuevo incidente de red,  
**para** llevar control formal de las fallas reportadas en los sitios de Net Guard GT.

**Criterios de aceptación:**
- El sistema debe permitir ingresar título, descripción, tipo, severidad y sitio afectado.
- El incidente debe crearse con estado inicial **Registrado**.
- El sistema debe asignar automáticamente el SLA según la severidad seleccionada.

---

### HU-02 — Consulta de incidentes

**Como** coordinador de soporte,  
**quiero** consultar la lista de incidentes registrados,  
**para** dar seguimiento al estado actual de cada caso.

**Criterios de aceptación:**
- El sistema debe mostrar ID, título, tipo, severidad, estado, sitio, técnico asignado, fecha de registro y SLA.
- Los incidentes deben mostrarse actualizados.
- Si un incidente no tiene técnico asignado, debe mostrarse como **Sin asignar** o con guion.

---

### HU-03 — Registro de técnicos

**Como** administrador del sistema,  
**quiero** registrar técnicos con su especialidad,  
**para** asignarlos correctamente a los incidentes según el tipo de falla.

**Criterios de aceptación:**
- El sistema debe permitir registrar nombre y especialidad del técnico.
- Las especialidades deben incluir áreas como fibra óptica, microondas y sistemas eléctricos.
- El técnico registrado debe estar disponible para futuras asignaciones.

---

### HU-04 — Asignación de técnico

**Como** coordinador de soporte,  
**quiero** asignar un técnico a un incidente registrado,  
**para** que el caso sea atendido por una persona responsable.

**Criterios de aceptación:**
- El incidente debe estar en estado **Registrado**.
- El técnico debe tener una especialidad compatible con el tipo de incidente.
- Al asignar el técnico, el incidente debe cambiar a estado **Asignado**.

---

### HU-05 — Validación de carga de trabajo

**Como** coordinador de soporte,  
**quiero** evitar asignar más de tres incidentes activos a un mismo técnico,  
**para** reducir la sobrecarga de trabajo.

**Criterios de aceptación:**
- El sistema debe contar los incidentes activos del técnico.
- Si el técnico ya tiene tres incidentes activos, el sistema debe rechazar la asignación.
- El sistema debe mostrar un mensaje indicando que el técnico ya alcanzó el límite permitido.

---

### HU-06 — Validación por especialidad

**Como** coordinador de soporte,  
**quiero** asignar incidentes únicamente a técnicos con especialidad coincidente,  
**para** asegurar que el problema sea atendido por personal capacitado.

**Criterios de aceptación:**
- Un incidente de fibra óptica solo debe asignarse a técnicos de fibra óptica.
- Un incidente de microondas solo debe asignarse a técnicos de microondas.
- Un incidente de sistemas eléctricos solo debe asignarse a técnicos de sistemas eléctricos.

---

### HU-07 — Cambio de estado del incidente

**Como** técnico asignado,  
**quiero** actualizar el estado de un incidente,  
**para** reflejar el avance real de la atención del caso.

**Criterios de aceptación:**
- El sistema debe permitir cambiar de **Asignado** a **En Progreso**.
- El sistema debe permitir cambiar de **En Progreso** a **Resuelto**.
- El sistema no debe permitir saltos inválidos como **Registrado** a **Resuelto**.

---

### HU-08 — Cierre de incidente

**Como** coordinador de soporte,  
**quiero** cerrar un incidente resuelto,  
**para** finalizar oficialmente el seguimiento del caso.

**Criterios de aceptación:**
- Solo se debe poder cerrar un incidente que esté en estado **Resuelto**.
- Al cerrar el incidente, su estado debe cambiar a **Cerrado**.
- Un incidente cerrado no debe poder modificarse nuevamente.

---

### HU-09 — Reasignación de incidente

**Como** coordinador de soporte,  
**quiero** reasignar un incidente a otro técnico,  
**para** continuar la atención cuando el técnico anterior no pueda resolverlo.

**Criterios de aceptación:**
- El incidente debe poder reasignarse siempre que no esté **Cerrado**.
- El nuevo técnico debe cumplir con la especialidad requerida.
- El nuevo técnico no debe superar el límite de tres incidentes activos.

---

### HU-10 — Escalamiento de incidente

**Como** sistema,  
**quiero** escalar automáticamente incidentes críticos o urgentes sin atención durante más de dos horas,  
**para** evitar incumplimientos en los tiempos de respuesta.

**Criterios de aceptación:**
- El sistema debe revisar incidentes con severidad **Crítica** o **Urgente**.
- Si el incidente sigue en estado **Registrado** después de dos horas, debe cambiar a **Escalado**.
- El cambio a **Escalado** debe quedar registrado en el historial.

## Endpoints de la API

### Técnicos
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/tecnicos` | Listar técnicos |
| GET | `/api/tecnicos/{id}` | Obtener técnico |
| POST | `/api/tecnicos` | Crear técnico |
| PUT | `/api/tecnicos/{id}` | Actualizar técnico |
| DELETE | `/api/tecnicos/{id}` | Eliminar técnico |

### Incidentes
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/incidentes` | Listar incidentes |
| GET | `/api/incidentes/{id}` | Obtener incidente |
| POST | `/api/incidentes` | Crear incidente |
| PUT | `/api/incidentes/{id}/asignar/{tecnicoId}` | Asignar técnico |
| PUT | `/api/incidentes/{id}/reasignar/{tecnicoId}` | Reasignar técnico |
| PUT | `/api/incidentes/{id}/estado` | Cambiar estado |
| PUT | `/api/incidentes/escalar` | Revisar y escalar incidentes |
| GET | `/api/incidentes/{id}/historial` | Historial de estados |

### Reportes
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/reportes/incidentes` | Resumen general |
| GET | `/api/reportes/incidentes-por-estado` | Conteo por estado |
| GET | `/api/reportes/incidentes-por-tecnico` | Conteo por técnico |
| GET | `/api/reportes/incidentes-escalados` | Lista de escalados |

## Cómo ejecutar el proyecto

### Requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Pasos

Clonar el repositorio **[Examen_Analisis](https://github.com/0004analaura/Examen_Analisis)** y ejecutar desde la raíz:

```bash
git clone https://github.com/0004analaura/Examen_Analisis.git
cd Examen_Analisis
dotnet restore NetGuardGT.sln
dotnet run --project NetGuardGT.Api/NetGuardGT.Api.csproj
```

La API inicia en `http://localhost:8080` (o el puerto definido en `PORT`).

- **Swagger:** http://localhost:8080/swagger

La base de datos SQLite (`netguard.db`) se crea automáticamente con datos de prueba.

## Cómo ejecutar las pruebas unitarias

```bash
dotnet test NetGuardGT.Tests/NetGuardGT.Tests.csproj
```

Se ejecutan 10 pruebas que validan las reglas de negocio principales.

## Cómo probar la API

Usa **Swagger** en http://localhost:8080/swagger para probar todos los endpoints de forma visual: técnicos, incidentes, asignaciones, cambios de estado y reportes.

## Despliegue en Render

### Opción 1: Docker

1. Subir el repositorio **[Examen_Analisis](https://github.com/0004analaura/Examen_Analisis)** a GitHub.
2. En [Render](https://render.com), crear un **Web Service**.
3. Conectar el repositorio.
4. Seleccionar **Docker** como entorno.
5. Render detectará el `Dockerfile` en la raíz.
6. Render asigna automáticamente la variable `PORT`; la app la usa en `Program.cs`.

### Opción 2: .NET nativo

1. Crear Web Service en Render.
2. **Build command:** `dotnet publish NetGuardGT.Api/NetGuardGT.Api.csproj -c Release -o out`
3. **Start command:** `cd out && dotnet NetGuardGT.Api.dll`
4. Variable de entorno: `ASPNETCORE_ENVIRONMENT=Production`

### Notas
- SQLite en Render es efímera (se reinicia al redeploy). Para producción real se recomienda PostgreSQL.
- Swagger está habilitado en producción para facilitar pruebas del examen.

## Uso de IA

La inteligencia artificial (Cursor / asistente de código) fue utilizada como **apoyo** en este proyecto para:

- Estructurar la solución en capas (Controllers, Services, Data, DTOs).
- Generar ideas para organizar las reglas de negocio.
- Revisar la lógica de transiciones de estado y escalación.
- Mejorar la documentación (README).
- Acelerar la creación de pruebas unitarias y documentación.

Todas las decisiones de negocio, validaciones y ajustes finales fueron revisados para cumplir los requisitos del examen.

## Estructura del proyecto

Repositorio GitHub: **[Examen_Analisis](https://github.com/0004analaura/Examen_Analisis)**

```
Examen_Analisis/                    ← repositorio GitHub
├── NetGuardGT.sln
├── NetGuardGT.Api/                  ← proyecto API
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   └── DTOs/
├── NetGuardGT.Tests/                ← proyecto de pruebas
├── Dockerfile
└── README.md
```
