
# Autenticación y Registro - MathRacer API

---

## Flujo de autenticación (Frontend → Backend)

1. El frontend autentica al usuario usando Firebase Authentication (email/password o Google).
2. Obtiene el `idToken` JWT de Firebase:
  ```js
  const idToken = await firebase.auth().currentUser.getIdToken();
  ```
3. Envía el `idToken` en el header HTTP en cada request protegido:
  ```http
  Authorization: Bearer <idToken>
  ```
4. El backend valida el token usando Firebase Admin SDK y extrae el UID/email.
5. Si el token es válido, procesa la solicitud y responde con el perfil del usuario.
6. Si el token es inválido o falta, responde con error 401 y mensaje claro.

---

## Configuración de credenciales Firebase (Backend)

1. Descarga tu archivo de credenciales desde la consola de Firebase.
2. Colócalo en la raíz del proyecto con el nombre `firebase-credentials.json`.
3. No necesitas configurar ninguna variable de entorno ni editar `launchSettings.json`.
4. El backend detecta automáticamente el archivo en la raíz.

**Importante:** Verifica que el archivo esté en `.gitignore` para evitar subirlo al repositorio.

## Endpoints de autenticación


### 1. Registro de usuario
**POST** `/api/player/register`

Headers:
- `Authorization: Bearer <idToken>` (obligatorio)

Body (JSON):
```json
{
  "username": "JuanPerez",
  "email": "juan@example.com",
  "password": "123456",
  "uid": "firebase-uid-123" // opcional, si el usuario se registra con Google
}
```
Respuesta exitosa (201):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: email duplicado, datos inválidos o faltantes
- 401 Unauthorized: token inválido o no enviado
- 500 Internal Server Error: error de base de datos

---


### 2. Login de usuario
**POST** `/api/player/login`

Headers:
- `Authorization: Bearer <idToken>` (obligatorio)

Body (JSON):
```json
{
  "email": "juan@example.com",
  "password": "123456"
}
```
Respuesta exitosa (200):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: datos inválidos o faltantes
- 401 Unauthorized: credenciales inválidas o token inválido/no enviado
- 500 Internal Server Error: error de base de datos

---


### 3. Login/Registro con Google
**POST** `/api/player/google`

Headers:
- `Authorization: Bearer <idToken>` (obligatorio)

Body (JSON):
```json
{
  "username": "JuanPerez", // opcional, solo si es registro
  "email": "juan@example.com" // opcional, solo si es registro
}
```
Respuesta exitosa (200):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: datos insuficientes o token faltante
- 401 Unauthorized: token inválido/no enviado
- 500 Internal Server Error: error de base de datos

---


## Ejemplos de uso con curl

Registro:
```bash
curl -X POST http://localhost:5153/api/player/register \
  -H "Authorization: Bearer <idToken>" \
  -H "Content-Type: application/json" \
  -d '{"username": "JuanPerez", "email": "juan@example.com", "password": "123456", "uid": "firebase-uid-123"}'
```

Login:
```bash
curl -X POST http://localhost:5153/api/player/login \
  -H "Authorization: Bearer <idToken>" \
  -H "Content-Type: application/json" \
  -d '{"email": "juan@example.com", "password": "123456"}'
```

Google:
```bash
curl -X POST http://localhost:5153/api/player/google \
  -H "Authorization: Bearer <idToken>" \
  -H "Content-Type: application/json" \
  -d '{"username": "JuanPerez", "email": "juan@example.com"}'
```

---


## Recomendaciones para el frontend

- Autenticar al usuario con Firebase y obtener el idToken antes de llamar a cualquier endpoint protegido.
- Enviar el idToken por header en cada request (Authorization: Bearer <idToken>).
- Validar los datos antes de enviarlos al backend.
- Manejar errores 400/401 mostrando mensajes claros y redirigiendo al login si es necesario.
- No enviar el token por body ni por query string.

---

## Notas técnicas
- El backend valida el token usando Firebase Admin SDK.
- El modelo de respuesta incluye: `id`, `name`, `email`, `lastLevelId`, `points`, `coins`.
- El campo `Uid` reemplaza a `FirebaseUid` en la base de datos y en los modelos.
- Si el token es inválido o falta, el backend responde con error 401 y mensaje claro, sin stacktrace ni detalles internos.
- El archivo de credenciales de Firebase se configura automáticamente en el backend.
---


## Resumen de la implementación

Todos los endpoints de autenticación (`/api/player/register`, `/api/player/login`, `/api/player/google`) aceptan el token de Firebase por el header HTTP:

```
Authorization: Bearer <idToken>
```

El backend valida este token usando Firebase Admin SDK. Si el token es válido, se extrae el UID y el email del usuario autenticado. El campo `Uid` se usa para asociar el usuario en la base de datos. El frontend debe obtener el token desde Firebase Authentication y enviarlo en cada request protegido.

---

## Endpoints

### 1. Registro de usuario
**POST** `/api/player/register`

Headers:
- `Authorization: Bearer <firebase_id_token>` (opcional, pero recomendado si el usuario se registra con Google)

Body (JSON):
```json
{
  "username": "JuanPerez",
  "email": "juan@example.com",
  "password": "123456",
  "uid": "firebase-uid-123" // opcional
}
```
Respuesta exitosa (201):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: email duplicado, datos inválidos o faltantes
- 500 Internal Server Error: error de base de datos

---

### 2. Login de usuario
**POST** `/api/player/login`

Headers:
- `Authorization: Bearer <firebase_id_token>` (opcional, si el usuario ya está autenticado en Firebase)

Body (JSON):
```json
{
  "email": "juan@example.com",
  "password": "123456"
}
```
Respuesta exitosa (200):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: datos inválidos o faltantes
- 401 Unauthorized: credenciales inválidas o token inválido
- 500 Internal Server Error: error de base de datos

---

### 3. Login/Registro con Google (Firebase)
**POST** `/api/player/google`

Headers:
- `Authorization: Bearer <firebase_id_token>` (obligatorio)

Body (JSON):
```json
{
  "username": "JuanPerez", // opcional, solo si es registro
  "email": "juan@example.com" // opcional, solo si es registro
}
```
Respuesta exitosa (200):
```json
{
  "id": 123,
  "name": "JuanPerez",
  "email": "juan@example.com",
  "lastLevelId": 1,
  "points": 0,
  "coins": 0
}
```
Errores:
- 400 Bad Request: token faltante en el header o datos insuficientes
- 401 Unauthorized: token inválido
- 500 Internal Server Error: error de base de datos

---

## Ejemplos de uso con curl

Registro:
```bash
curl -X POST http://localhost:5153/api/player/register \
  -H "Authorization: Bearer <firebase_id_token>" \
  -H "Content-Type: application/json" \
  -d '{"username": "JuanPerez", "email": "juan@example.com", "password": "123456"}'
```

Login:
```bash
curl -X POST http://localhost:5153/api/player/login \
  -H "Authorization: Bearer <firebase_id_token>" \
  -H "Content-Type: application/json" \
  -d '{"email": "juan@example.com", "password": "123456"}'
```

Google:
```bash
curl -X POST http://localhost:5153/api/player/google \
  -H "Authorization: Bearer <firebase_id_token>" \
  -H "Content-Type: application/json" \
  -d '{"username": "JuanPerez", "email": "juan@example.com"}'
```

---


## Flujo recomendado para el frontend

1. Autenticar al usuario usando Firebase Authentication (email/password o Google).
2. Obtener el `idToken` JWT de Firebase.
3. Enviar el `idToken` en el header `Authorization: Bearer <idToken>` en cada request protegido (registro, login, google).
4. El backend valida el token y extrae el UID/email para asociar la sesión y los datos del usuario.
5. El backend responde con el perfil del usuario o el error correspondiente.

---


## Notas técnicas
- El backend valida el token usando Firebase Admin SDK.
- El modelo de respuesta incluye: `id`, `name`, `email`, `lastLevelId`, `points`, `coins`.
- El campo `Uid` reemplaza a `FirebaseUid` en la base de datos y en los modelos.
- El token debe enviarse siempre por header, nunca por body.
- Si el token es inválido o falta, el backend responde con error 400 o 401.
- El frontend debe manejar los errores y mostrar mensajes claros al usuario.

---


## Ejemplo de integración frontend-backend

```js
// Ejemplo usando fetch en el frontend
const idToken = await firebase.auth().currentUser.getIdToken();
const response = await fetch('http://localhost:5153/api/player/register', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${idToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ username: 'JuanPerez', email: 'juan@example.com', password: '123456', uid: 'firebase-uid-123' })
});
const data = await response.json();
```

---

## Resumen para refactorización del frontend
- Autenticar con Firebase y obtener el idToken.
- Enviar el idToken por header en todos los endpoints de autenticación.
- Usar los modelos y flujos documentados arriba para adaptar el frontend.
# Endpoints de Autenticación# Endpoints de Autenticación y Registro



## Registro## Registro con Email/Password

POST /api/players/register**POST** `/api/players/register`

- username

- emailHeaders:

- uid (opcional)- `Authorization: Bearer <firebase_id_token>`



## LoginBody (JSON):

POST /api/players/login```json

- email{

  "username": "TuNombre"

## Google Sign-In}

POST /api/players/google```

- idToken

- username (opcional)Respuesta:

- email (opcional)- 201 Created con los datos del usuario

- 401 Unauthorized si el token es inválido

## Ejemplo de uso- 409 Conflict si el usuario ya existe

```bash

curl -X POST http://localhost:5153/api/players/register -H "Content-Type: application/json" -d '{"username":"Juan","email":"juan@example.com"}'## Login con Email/Password

```**POST** `/api/players/login`


Headers:
- `Authorization: Bearer <firebase_id_token>`

Respuesta:
- 200 OK con los datos del usuario
- 401 Unauthorized si el token es inválido
- 404 Not Found si el usuario no existe

## Login/Registro con Google
**POST** `/api/players/google`

Headers:
- `Authorization: Bearer <firebase_id_token>`

Respuesta:
- 200 OK con los datos del usuario (si no existe, lo crea automáticamente)
- 401 Unauthorized si el token es inválido

## Ejemplo de uso con curl

Registro:
```bash
curl -X POST http://localhost:5153/api/players/register \
  -H "Authorization: Bearer <firebase_id_token>" \
  -H "Content-Type: application/json" \

  ## Endpoints de Autenticación y Registro

  Todos los endpoints de autenticación requieren el header:
  ```
  Authorization: Bearer <firebase_id_token>
  ```

  ### Registro con Email/Password
  **POST** `/api/player/register`

  Headers:
  - `Authorization: Bearer <firebase_id_token>` (obligatorio)

  Body (JSON):
  ```json
  {
    "username": "JuanPerez",
    "email": "juan@example.com",
    "password": "123456",
    "uid": "firebase-uid-123" // opcional
  }
  ```

  ### Login con Email/Password
  **POST** `/api/player/login`

  Headers:
  - `Authorization: Bearer <firebase_id_token>` (obligatorio)

  Body (JSON):
  ```json
  {
    "email": "juan@example.com",
    "password": "123456"
  }
  ```

  ### Login/Registro con Google
  **POST** `/api/player/google`

  Headers:
  - `Authorization: Bearer <firebase_id_token>` (obligatorio)

  Body (JSON):
  ```json
  {
    "username": "JuanPerez", // opcional, solo si es registro
    "email": "juan@example.com" // opcional, solo si es registro
  }
  ```

  ## Ejemplo de uso con curl

  Registro:
  ```bash
  curl -X POST http://localhost:5153/api/player/register \
    -H "Authorization: Bearer <firebase_id_token>" \
    -H "Content-Type: application/json" \
    -d '{"username": "JuanPerez", "email": "juan@example.com", "password": "123456", "uid": "firebase-uid-123"}'
  ```

  Login:
  ```bash
  curl -X POST http://localhost:5153/api/player/login \
    -H "Authorization: Bearer <firebase_id_token>" \
    -H "Content-Type: application/json" \
    -d '{"email": "juan@example.com", "password": "123456"}'
  ```

  Google:
  ```bash
  curl -X POST http://localhost:5153/api/player/google \
    -H "Authorization: Bearer <firebase_id_token>" \
    -H "Content-Type: application/json" \
    -d '{"username": "JuanPerez", "email": "juan@example.com"}'
  ```

  > Reemplaza `<firebase_id_token>` por el token real que te da Firebase al autenticarte.