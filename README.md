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
