Escenario 1
1. Modelo de Datos (Jugador.cs)
Se implementó la clase base vinculada a Firestore, asegurando que los nombres de los campos
coincidan exactamente con la colección jugadores solicitada en el proyecto:
    Mapeo de Atributos: Se utilizaron [FirestoreProperty] para sincronizar nombres en español (nombreUsuario, puntosGlobales, fechaRegistro) con propiedades en C# .
    Tipos de Datos: Implementación de Timestamp para fechas y bool para estados de conexión y actividad.

2.Capa de Transferencia (JugadorDto.cs)
Se crearon los objetos necesarios para el intercambio de datos entre el cliente y el servidor:
  RegisterDto: Captura los datos obligatorios para crear una cuenta (Nombre, Correo, Contraseña, etc.).
  LoginDto: Estructura simple para el proceso de autenticación.
  JugadorDto: Expone información pública como PuntosGlobales y TorneoGanados, ocultando datos sensibles como la contraseña.
  ActualizarPerfilDto: Restringe la edición únicamente a nombre, apellido, edad y país.
  
3. Lógica de Negocio (IAuthService.cs y AuthService.cs)
Se desarrolló el motor del sistema cumpliendo con las validaciones de seguridad obligatorias:
  Registro: Implementación de validación de unicidad para correo y nombreUsuario (NickName).
  Autenticación (JWT): Generación de tokens con claims específicos (correo, rol, jugadorId) y validez de 24 horas.
  Estado de Conexión: Lógica para actualizar conectado: true y ultimaConexion al momento del login exitoso.

5. Controlador (Escenario1Controller.cs)
Se expusieron los endpoints requeridos con manejo apropiado de códigos HTTP:
  POST /api/Escenario1/registro: Crea jugadores con rol "jugador" por defecto.
  POST /api/Escenario1/login: Valida credenciales y devuelve el token JWT.
  GET /api/Escenario1/jugadores/{id}: Retorna el perfil público accesible para todos los usuarios autenticados.
  PUT /api/Escenario1/jugadores/{id}/perfil: Implementa seguridad a nivel de recurso; solo el propietario o un administrador puede realizar cambios.
  


Escenario 2: Gestión de Videojuegos
Responsable: Integrante #2
1. Modelo de Datos (Juego.cs)
Se implementó la clase base vinculada a Firestore para la colección juegos, cumpliendo con la estructura solicitada en el examen:
•	Mapeo de Atributos:
Se utilizaron anotaciones [FirestoreProperty] para asegurar la correcta sincronización con los campos en Firestore (titulo, desarrollador, genero, plataformas, etc.). 
•	Identificador del Documento:
Uso de [FirestoreDocumentId] para manejar el Id generado automáticamente por Firestore. 
•	Valores por Defecto:
Se inicializan automáticamente: 
o	jugadoresActivos = 0 
o	torneoActivos = 0 
o	estado = "disponible" 
o	puntuacionPromedio = 0.0 
o	fechaAgreg = DateTime.UtcNow 
•	Tipos de Datos:
Uso de List<string> para plataformas, DateTime para fechas y double/int para métricas numéricas. 
2. Capa de Transferencia (JuegoDto.cs)
Se creó el DTO para controlar los datos que entran desde el cliente hacia la API:
•	Validaciones: 
o	[Required] en todos los campos obligatorios 
o	[MinLength(20)] en la descripción 
•	Propósito: 
o	Evitar exponer directamente el modelo de base de datos 
o	Validar datos antes de procesarlos en el backend 
•	Campos Controlados:
Solo se reciben datos necesarios para la creación/actualización: 
o	Titulo 
o	Desarrollador 
o	Genero 
o	Plataformas 
o	FechaLanzamiento 
o	Descripcion 
3. Lógica de Negocio (IJuegoService.cs y JuegoService.cs)
Se implementó la lógica principal del sistema cumpliendo todas las reglas del escenario:
Agregar Juego
•	Validación de título único en Firestore 
•	Validación de plataformas permitidas: PC, PS5, Xbox, Switch 
•	Validación de descripción mínima de 20 caracteres 
•	Inicialización de valores por defecto 
Listar Juegos
•	Retorna únicamente juegos con estado "disponible" 
•	Permite filtros por: 
o	género 
o	plataforma 
o	desarrollador 
Actualizar Juego
•	Solo permite actualizar: 
o	descripción 
o	puntuaciónPromedio 
o	estado 
•	Validaciones: 
o	descripción ≥ 20 caracteres 
o	puntuación entre 0 y 5 
o	estado válido: 
	disponible 
	mantenimiento 
	descontinuado 
Eliminar Juego
•	Validaciones de integridad: 
o	No permite eliminar si tiene torneos activos 
o	No permite eliminar si tiene jugadores activos 
Obtener Estadísticas
•	Retorna: 
o	jugadoresActivos 
o	torneoActivos 
o	puntuacionPromedio 

Obtener Todos los Juegos
•	Método adicional para listar todos los juegos sin filtro de estado 
4. Controlador (JuegosController.cs)
Se expusieron los endpoints cumpliendo con seguridad y manejo de errores:

POST /api/juegos
•	Crea un nuevo juego 
•	Solo accesible para rol admin 
•	Valida: 
o	título único 
o	plataformas válidas 
o	descripción mínima 
GET /api/juegos
•	Lista juegos disponibles 
•	Acceso: usuarios autenticados 
•	Permite filtros: 
o	género 
o	plataforma 
o	desarrollador 
GET /api/juegos/todos
•	Retorna todos los juegos sin filtro 
•	Acceso: usuarios autenticados 

PUT /api/juegos/{id}
•	Actualiza un juego 
•	Solo admin 
•	Valida estado, puntuación y descripción 

DELETE /api/juegos/{id}
•	Elimina un juego 
•	Solo admin 
•	Bloquea eliminación si: 
o	tiene jugadores activos 
o	tiene torneos activos 

GET /api/juegos/{id}/estadisticas
•	Retorna estadísticas del juego 
•	Acceso: usuarios autenticados 

5. Manejo de Errores y Respuestas HTTP
Se implementó manejo adecuado de errores:
•	200 OK: operaciones exitosas 
•	201 Created: creación de juego 
•	400 BadRequest: errores de validación 
•	404 NotFound: recurso no encontrado 
•	409 Conflict: duplicados o conflictos 
•	500 InternalServerError: errores internos 

6. Seguridad
•	Uso de [Authorize] para endpoints protegidos 
•	Uso de [Authorize(Roles = "admin")] para operaciones críticas 
•	Integración con JWT (Escenario 1)



Escenario 3
Módulo: Creación y Gestión de Torneos (Escenario 3)
Responsable: Integrante 3
1. Descripción General
El presente documento detalla la implementación del módulo de torneos, el cual gestiona el ciclo de vida completo de un torneo, desde su registro hasta su finalización. El desarrollo se ha realizado cumpliendo con estrictas reglas de negocio, validación de datos de entrada y control de acceso basado en roles.

2. Tecnologías y Arquitectura
Framework: ASP.NET Core 8.0
Base de Datos: Google Cloud Firestore
Autenticación y Seguridad: JSON Web Tokens (JWT)
Patrón de Diseño: Arquitectura en N-Capas (Controladores, Servicios, Interfaces, DTOs y Modelos de Dominio)
Validación: DataAnnotations integrados en los Data Transfer Objects (DTOs)

3. Estructura de Datos (Firestore)
Colección: 
torneos
El sistema almacena los documentos con la siguiente estructura de campos:

Campo	Tipo / Descripción
nombre	string
juego	string – ID del documento del juego
organizador	string – ID del usuario creador
descripcion	string
estado	string
formato	string
maxParticipantes	number
participantesActuales	number
precioInscripcion	number
premioTotal	number
fechaInicio	timestamp
fechaFin	timestamp
fechaLimiteInscripcion	timestamp
minNivel	number
maxNivel	number
requiereEquipo	boolean
tamanioEquipo	number
fechaCreacion	timestamp
reglasModificadas	boolean

4. Especificación de Endpoints
Todas las rutas base de este módulo se exponen bajo:
/api/torneos

Método	Ruta	Nombre	Acceso	Descripción
POST	/api/torneos	Crear Torneo	organizador, admin	Registra un nuevo torneo. Valida fechas, cupos y formato. Inicializa participantes en 0, estado en "próximo" y reglas modificadas en false.
GET	/api/torneos	Listar Torneos	Público	Retorna lista paginada de torneos en estado "próximo" o "en progreso". Ordenados ascendentemente por fecha de inicio. Soporta filtros opcionales.
PUT	/api/torneos/{id}	Actualizar Torneo	organizador, admin	Modifica un torneo existente. Solo el creador o un admin pueden hacerlo. El torneo debe estar en estado "próximo".
DELETE	/api/torneos/{id}	Cancelar Torneo	organizador, admin	Cambia el estado del torneo a "cancelado". Solo permitido si está en estado "próximo".
PATCH	/api/torneos/{id}/cambiar-estado	Cambiar Estado	organizador, admin	Gestiona la máquina de estados: próximo → en progreso → finalizado.



4.1. Crear Torneo
Ruta: POST /api/torneos
Acceso: Protegido — Roles permitidos: organizador, admin
Registra un nuevo torneo en la base de datos. Valida la coherencia de las fechas, la cantidad de cupos permitidos y el formato de la competición. Inicializa los participantes actuales en 0, el estado en "próximo" y la bandera de reglas modificadas en falso.
Cuerpo de la Petición (JSON): Requiere los campos definidos en TorneoCreateDto.

4.2. Listar y Filtrar Torneos
Ruta: GET /api/torneos
Acceso: Público
Retorna una lista paginada de todos los torneos que se encuentran en estado "próximo" o "en progreso". Los resultados se ordenan de manera ascendente por la fecha de inicio.
Parámetros de Consulta (Opcionales): juego, estado, minPrecio, maxPrecio, minNivelReq, maxNivelReq, page, pageSize.

4.3. Actualizar Torneo
Ruta: PUT /api/torneos/{id}
Acceso: Protegido — Roles permitidos: organizador, admin
Modifica los detalles de un torneo existente. Esta operación está restringida únicamente al usuario creador del torneo o a un administrador del sistema.
Condiciones: Solo es posible modificar torneos en estado "próximo". No se permite reducir el número máximo de participantes a un valor inferior al de los participantes actualmente inscritos.

4.4. Cancelar Torneo
Ruta: DELETE /api/torneos/{id}
Acceso: Protegido — Roles permitidos: organizador, admin
Actualiza el estado del torneo a "cancelado".
Condiciones: Solo el organizador propietario o un administrador pueden ejecutar esta acción, y únicamente si el torneo se encuentra en estado "próximo".

4.5. Cambiar Estado del Torneo
Ruta: PATCH /api/torneos/{id}/cambiar-estado
Acceso: Protegido — Roles permitidos: organizador, admin
Gestiona la máquina de estados transicionales del torneo. Permite el avance de "próximo" a "en progreso", y de "en progreso" a "finalizado".

5. Reglas de Negocio y Manejo de Errores
El sistema implementa respuestas HTTP estandarizadas (400, 403, 404, 500) basadas en las siguientes validaciones:

•Validación de Fechas: La fecha de inicio debe ser posterior a la fecha actual. La fecha límite de inscripción debe ser estricta y cronológicamente anterior a la fecha de inicio (Error 400).
•Validación de Participantes: La capacidad máxima de participantes debe ser superior a 2 (Error 400).
•Validación de Formatos: Los únicos valores aceptados para el formato son "individual", "equipos" o "royale" (Error 400).
•Validación de Dominio: Los campos numéricos relacionados con precios, premios y niveles están restringidos a valores positivos.
•Validación de Identidad y Propiedad: El sistema extrae el identificador del usuario directamente del token JWT provisto en la cabecera de la petición. Si un usuario intenta modificar o cancelar un torneo que no fue creado por él (y no posee privilegios de administrador), la solicitud es denegada (Error 403).

6. Instrucciones para Ejecución de Pruebas (Swagger)
Para ejecutar pruebas mediante la interfaz de Swagger, siga los pasos a continuación:

1.Autenticarse en el sistema utilizando el endpoint correspondiente de la API para obtener un token JWT válido. El usuario empleado debe contar con los roles "organizador" o "admin".
2.Copiar el token generado.
3.En la interfaz de Swagger, seleccionar la opción "Authorize".
4.Ingresar la credencial utilizando el esquema Bearer (formato: Bearer [Token_JWT]).
5.Proceder con la ejecución de los endpoints documentados en la sección 4.



Escenario 4 - Participaciones en Torneos
1. Descripción General El presente módulo permite gestionar las participaciones de los jugadores en los torneos, incluyendo la inscripción, actualización de resultados, listado de participantes y abandono de torneos. Cumple con reglas de negocio estrictas y control de acceso basado en roles.
2. Tecnologías y Arquitectura - Framework: ASP.NET Core 8.0 - Base de Datos: Google Cloud Firestore - Autenticación: JSON Web Tokens (JWT) - Patrón de Diseño: Arquitectura en N-Capas (Controladores, Servicios, Interfaces, DTOs, Modelos)
3. Estructura de Datos (Firestore) Colección: participaciones
Campo	Tipo / Descripción
torneoId	string - ID del torneo
jugadorId	string - ID del jugador
estado	string - “inscrito”, “eliminado”, “abandonado”
victorias	number
derrotas	number
puntosObtenidos	number
partidasJugadas	number
posicionActual	number
fechaInscripcion	timestamp
fechaEliminacion	timestamp
4. DTOs - InscripcionDTO: contiene el campo pagado (bool) para confirmar pago. - ResultadoPartidaDTO: contiene victoria (bool) y puntosPartida (int). - ParticipanteDTO: muestra nombre, nombreUsuario, nivel, victorias, derrotas, posicion. - MisTorneosDTO: muestra torneoId, estado, posicion, puntos, victorias, derrotas, fechaInscripcion.
5. Interfaces y Servicios - IParticipacionService: define los métodos principales: - InscribirseTorneo (inscripción de jugador) - ObtenerParticipantes (listado de participantes con paginación) - ActualizarResultado (actualizar resultado de partida) - ObtenerMisTorneos (torneos del jugador) - AbandonarTorneo (permite abandonar torneo en estado “inscrito”)
ParticipacionService: implementación de la interfaz, interactúa con Firestore y aplica las reglas de negocio.
6. Especificación de Endpoints Todas las rutas base: /api
Método	Ruta	Descripción	Acceso
POST	/torneos/{id}/inscribirse	Inscribir jugador en torneo, validando estado, nivel y pago	Autenticado
GET	/torneos/{id}/participantes	Listar participantes con nombre, nivel, victorias, derrotas y posición	Autenticado
PUT	/torneos/{id}/participantes/{idParticipacion}/actualizar-resultado	Registrar resultado de una partida	Organizador/Admin
GET	/jugador/mis-torneos	Listar torneos en los que participa el jugador	Autenticado
DELETE	/torneos/{id}/participantes/{idParticipacion}/abandonar	Abandonar torneo en estado “inscrito”	Autenticado
7. Reglas de Negocio y Validaciones - Inscripción solo en torneos en estado “próximo” y con cupos disponibles. - El jugador debe estar activo, cumplir el rango de nivel y confirmar pago si aplica. - No se permite inscripción múltiple para el mismo torneo. - Actualización de resultados solo por organizador o admin. - En torneos de eliminación directa, perder una partida cambia estado a “eliminado”. - Solo se puede abandonar torneo si está en estado “inscrito” y el torneo no ha iniciado.
8. Ejecución de Pruebas (Swagger) 1. Autenticarse y obtener token JWT. 2. Pegar token en la opción “Authorize” de Swagger con esquema Bearer. 3. Ejecutar endpoints según documentación. 4. Verificar respuestas, incluyendo errores HTTP 400, 403, 404 y 500 según validaciones.
Fin del Documento





Escenario 5
Descripción
Este módulo implementa endpoints avanzados para consultas complejas, generación de reportes analíticos y sistemas de clasificación de jugadores dentro de la plataforma.
Incluye rankings globales, reportes personalizados, estadísticas de torneos y análisis de tendencias.
Tecnologías sugeridas
Backend: Node.js / Spring Boot / .NET 
Base de datos: PostgreSQL / MySQL 
Autenticación: JWT 
ORM: Sequelize / Hibernate / Entity Framework

 Endpoints
1.  Clasificación global por juego
GET /api/clasificaciones/{juegoId}
Retorna el ranking global de jugadores para un juego específico.

 Parámetros:
juegoId (path): ID del juego
page (query): número de página (default: 1)
limit (query): máximo 50 registros por página
nivelMin (query): filtro opcional
nivelMax (query): filtro opcional

 Respuesta:
{
  "pagina": 1,
  "total": 200,
  "data": [
    {
      "posicion": 1,
      "nombre": "JugadorPro",
      "puntos": 5000,
      "nivel": 45,
      "ratioVictoria": 0.78,
      "totalPartidas": 300,
      "rachaActual": 10
    }
  ]
}

2.  Clasificación del jugador autenticado

GET /api/jugador/clasificacion/{juegoId}
Retorna la posición del jugador autenticado en un juego específico.

 Acceso:
Solo usuario autenticado
 Respuesta:
{
  "posicion": 15,
  "puntos": 3200,
  "nivel": 30,
  "medallas": 5,
  "logrosDesbloqueados": 12
}

3.  Torneos populares

GET /api/reportes/torneos-populares
Retorna los 10 torneos con más inscripciones en los últimos 30 días.

Acceso:
Organizadores
Administradores
Respuesta:
[
  {
    "nombre": "Torneo Pro 2026",
    "juego": "Valorant",
    "inscripciones": 150,
    "premioTotal": 2000,
    "estado": "Activo"
  }
]
4.  Jugadores destacados

GET /api/reportes/jugadores-destacados
Retorna los 20 jugadores con mayor puntaje global.

 Acceso:
Usuarios autenticados
Respuesta:
[
  {
    "nombre": "TopPlayer",
    "puntosGlobales": 10000,
    "torneosGanados": 25,
    "juegosParticipados": 8
  }
]
5.  Mi desempeño

GET /api/reportes/mi-desempeno/{juegoId}
Genera un reporte completo del rendimiento del jugador autenticado.

 Acceso:
Solo usuario autenticado
 Respuesta:
{
  "nivelActual": 25,
  "posicionRanking": 40,
  "progresoSiguienteNivel": "70%",
  "ratioVictoria": 0.65,
  "rachaActual": 4,
  "medallas": 3,
  "mejoresTorneos": [
    {
      "nombre": "Copa Elite",
      "puntuacion": 500
    }
  ]
}
6.  Tendencias del sistema

GET /api/reportes/tendencias
Retorna métricas analíticas clave del sistema.

Acceso:
Solo administradores
 Respuesta:
{
  "juegosPopulares": ["Valorant", "FIFA", "Fortnite"],
  "generosActivos": ["FPS", "Deportes"],
  "horaPico": "20:00 - 22:00"
}

Reglas de negocio
Paginación obligatoria en clasificaciones (máx. 50 por página)
Control de acceso basado en roles (Jugador, Organizador, Administrador)
 Reportes de torneos basados en últimos 30 días
Ranking ordenado por posición (1 = mejor jugador)
